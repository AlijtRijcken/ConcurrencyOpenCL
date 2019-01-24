using Cloo;
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
        // load the OpenCL program; this creates the OpenCL context
        static OpenCLProgram ocl = new OpenCLProgram("../../program.cl");
        // find the kernel named 'device_function' in the program
        OpenCLKernel kernel = new OpenCLKernel(ocl, "device_function");
        // create a regular buffer; by default this resides on both the host and the device
        OpenCLBuffer<uint> inBuffer, outBuffer;

        //COPYED FROM CPU VERSION
        // screen surface to draw to
        public Surface screen;
        // stopwatch
        Stopwatch timer = new Stopwatch();
        int generation = 0;
        // two buffers for the pattern: simulate reads '_out', writes to '_in'
        static uint[] _in;
        static uint[] _out;
        uint pw, ph; // note: pw is in uints; width in bits is 32 this value.
        long[] workSize = { 0, 0 };

        // mouse handling: dragging functionality
        uint xoffset = 0, yoffset = 0;
        bool lastLButtonState = false;
        int dragXStart, dragYStart, offsetXStart, offsetYStart;


        public void Init()
        {
            StreamReader sr = new StreamReader("C:/Users/Alijt Rijcken/Documents/GitHub/ConcurrencyOpenCL/CClab3/CClab3/samples/c4-orthogonal.rle");
            uint state = 0, n = 0, x = 0, y = 0;
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
                    _in = new uint[pw * ph];
                    _out = new uint[pw * ph];
                    workSize[0] = pw * 32;
                    workSize[1] = ph;
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

            inBuffer = new OpenCLBuffer<uint>(ocl, _in);
            outBuffer = new OpenCLBuffer<uint>(ocl, _out);

            kernel.SetArgument(0, inBuffer);
            kernel.SetArgument(1, outBuffer);
            kernel.SetArgument(2, pw);
            kernel.SetArgument(3, ph); 

            BitSet(20, 20);
            BitSet(21, 20);
            BitSet(22, 20);
            BitSet(20, 21);
            BitSet(21, 22);
            //informatie doorgeven naar de GPU - hele dure operatie. Je kan gaan files loaden op de GPU
            //Kopier de begin state één keer naar de GPU en daarna ga je de array's aanpassen. 
        }


        public void Tick()
        {
            // start timer
            timer.Restart();
            // run the simulation, 1 step
            

            inBuffer.CopyToDevice();
            kernel.Execute(workSize);
            outBuffer.CopyFromDevice();

            
            for (int i = 0; i < pw * ph; i++)
            {
                _in[i] = 0;
            }

            // visualize current state, DRAW FUNCTION -> GPU BONUS. 
            screen.Clear(0);
            for (uint y = 0; y < screen.height; y++)
                for (uint x = 0; x < screen.width; x++)
                    if (GetBit(x + xoffset, y + yoffset) == 1)
                    {
                        screen.Plot(x, y, 0xffffff);
                        
                    }

            uint w = pw * 32, h = ph;
            for (uint y = 1; y < h - 1; y++) for (uint x = 1; x < w - 1; x++)
                {
                    if (GetBit(x, y) == 1)
                        BitSet(x, y);
                }


            // report performance
            //Console.WriteLine("generation " + generation++ + ": " + timer.ElapsedMilliseconds + "ms");
        }

        // helper function for setting one bit in the pattern buffer
        void BitSet(uint x, uint y)
        {
            _in[y * pw + (x >> 5)] |= 1U << (int)(x & 31);
        }
        // helper function for getting one bit from the secondary pattern buffer
        uint GetBit(uint x, uint y)
        {
            return (_out[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U;
        }

        //COPYED FROM CPU VERSION
        //handles mouse movement
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

        //hij moet de scoller van de mouse meenemen
        //Console venster gaat niet veranderen, maar de inhoud word groter
        //maar een pixel blijft een pixel natuurlijk...
        //--> Template.cs staat de input handler.
        //Mouse positie geeft aan waarop je uiteindelijk moet inzoomen natuurlijk
        public void Zoom(int scrolling)
        {


        }
    } // class Game
} // namespace Template

