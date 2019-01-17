﻿using Cloo;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Template
{
    class Game
    {
        // when GLInterop is set to true, the fractal is rendered directly to an OpenGL texture
        bool GLInterop = false;
        // load the OpenCL program; this creates the OpenCL context
        static OpenCLProgram ocl = new OpenCLProgram( "../../program.cl" );
        // find the kernel named 'device_function' in the program
        OpenCLKernel kernel = new OpenCLKernel( ocl, "device_function" );
        // create a regular buffer; by default this resides on both the host and the device
        OpenCLBuffer<uint> Sbuffer;
        // create an OpenGL texture to which OpenCL can send data
        //OpenCLImage<int> image = new OpenCLImage<int>( ocl, 512, 512 );

        //COPYED FROM CPU VERSION
        // screen surface to draw to
        public Surface screen;
        // stopwatch
        Stopwatch timer = new Stopwatch();
        int generation = 0;
        // two buffers for the pattern: simulate reads 'second', writes to 'pattern'
        static uint[] pattern;
        static uint[] second;
        uint pw, ph; // note: pw is in uints; width in bits is 32 this value.

        // mouse handling: dragging functionality
        uint xoffset = 0, yoffset = 0;
        bool lastLButtonState = false;
        int dragXStart, dragYStart, offsetXStart, offsetYStart;

        long[] workSize = { 0, 0 };
        


        public void Init()
        {
            StreamReader sr = new StreamReader("../../samples/turing_js_r.rle");
            uint state = 0, n = 0, x = 0, y = 0;
            workSize[0] = screen.width; workSize[1] = screen.height;
            while (true)
            {
                String line = sr.ReadLine();
                if (line == null) break; // end of file
                int pos = 0;
                if (line[pos] == '#') continue; /* comment line */
                else if (line[pos] == 'x') // header
                {
                    String[] sub = line.Split(new char[] { '=', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    pw = (UInt32.Parse(sub[1]) + 31) / 32;
                    ph = UInt32.Parse(sub[3]);
                    pattern = new uint[pw * ph];
                    second = new uint[pw * ph];
                }
                else while (pos < line.Length)
                    {
                        Char c = line[pos++];
                        if (state == 0) if (c < '0' || c > '9') { state = 1; n = Math.Max(n, 1); } else n = (uint)(n * 10 + (c - '0'));
                        if (state == 1) // expect other character
                        {
                            if (c == '$') { y += n; x = 0; } // newline
                            else if (c == 'o') for (int i = 0; i < n; i++) BitSet(x++, y); else if (c == 'b') x += n;
                            state = n = 0;
                        }
                    }
            }
            // swap buffers
            for (int i = 0; i < pw * ph; i++) second[i] = pattern[i];
            Sbuffer = new OpenCLBuffer<uint>(ocl, second);
            kernel.SetArgument(0, Sbuffer);
            
            //informatie doorgeven naar de GPU - hele dure operatie. Je kan gaan files loaden op de GPU
            //Kopier de begin state één keer naar de GPU en daarna ga je de array's aanpassen. 

            //Draw code is op het moment op CPU, paste naar een image  -> draw op GPU
        }
        public void Tick()
        {
            // start timer
            timer.Restart();
            // run the simulation, 1 step
            //Simulate();
            Sbuffer.CopyToDevice();
            kernel.Execute(workSize);
            Sbuffer.CopyFromDevice();

            // visualize current state, DRAW FUNCTION -> GPU BONUS. 
            screen.Clear(0);
            for (uint y = 0; y < screen.height; y++)
                for (uint x = 0; x < screen.width; x++)
                    if (GetBit(x + xoffset, y + yoffset) == 1) screen.Plot(x, y, 0xffffff);
            // report performance
            Console.WriteLine("generation " + generation++ + ": " + timer.ElapsedMilliseconds + "ms");


        }

        // helper function for setting one bit in the pattern buffer
        void BitSet(uint x, uint y)
        {
            pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31);
        }
        // helper function for getting one bit from the secondary pattern buffer
        uint GetBit(uint x, uint y)
        {
            return (second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U;
        }

        // SIMULATE --> OpenCL Class ALLE BEREKENINGEN MOETEN NAAR DE GPU.
        // Takes the pattern in array 'second', and applies the rules of Game of Life to produce the next state
        // in array 'pattern'. At the end, the result is copied back to 'second' for the next generation.
        //void Simulate()
        //{
        //    // clear destination pattern
        //    for (int i = 0; i < pw * ph; i++) pattern[i] = 0;
        //    // process all pixels, skipping one pixel boundary
        //    uint w = pw * 32, h = ph;
        //    for (uint y = 1; y < h - 1; y++) for (uint x = 1; x < w - 1; x++)
        //        {
        //            // count active neighbors
        //            uint n = GetBit(x - 1, y - 1) + GetBit(x, y - 1) + GetBit(x + 1, y - 1) + GetBit(x - 1, y) +
        //                GetBit(x + 1, y) + GetBit(x - 1, y + 1) + GetBit(x, y + 1) + GetBit(x + 1, y + 1);
        //            if ((GetBit(x, y) == 1 && n == 2) || n == 3) BitSet(x, y);
        //        }
        //    // swap buffers
        //    for (int i = 0; i < pw * ph; i++) second[i] = pattern[i];
        //}

        //public void Render()
        //{
        //    // use OpenGL to draw a quad using the texture that was filled by OpenCL
        //    if (GLInterop)
        //    {
        //        GL.LoadIdentity();
        //        GL.BindTexture( TextureTarget.Texture2D, image.OpenGLTextureID );
        //        GL.Begin( PrimitiveType.Quads );
        //        GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
        //        GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  1.0f, -1.0f );
        //        GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  1.0f,  1.0f );
        //        GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
        //        GL.End();
        //    }
        //}


        //COPYED FROM CPU VERSION
        public void SetMouseState(int x, int y, bool pressed)
        {
            if (pressed)
            {
                if (lastLButtonState)
                {
                    int deltax = x - dragXStart, deltay = y - dragYStart;
                    xoffset = (uint)Math.Min(pw * 32 - screen.width, Math.Max(0, offsetXStart - deltax));
                    yoffset = (uint)Math.Min(ph - screen.height, Math.Max(0, offsetYStart - deltay));
                }
                else
                {
                    dragXStart = x;
                    dragYStart = y;
                    offsetXStart = (int)xoffset;
                    offsetYStart = (int)yoffset;
                    lastLButtonState = true;
                }
            }
            else lastLButtonState = false;
        }
    } // class Game
} // namespace Template

