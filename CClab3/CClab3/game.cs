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
        uint pw, ph, breedte; // note: pw is in uints; width in bits is 32 this value.
        long[] workSize = { 0, 0 };
        static uint scale = 3;

        // mouse handling: dragging functionality
        uint xoffset = 0, yoffset = 0;
        bool lastLButtonState = false;
        int dragXStart, dragYStart, offsetXStart, offsetYStart;


        public void Init()
        {
            StreamReader sr = new StreamReader("../../samples/turing_js_r.rle");
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
                    breedte = (UInt32.Parse(sub[1]));
                    ph = UInt32.Parse(sub[3]);
                    _in = new uint[pw * ph];
                    _out = new uint[pw * ph];
                    workSize[0] = breedte;
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
            kernel.SetArgument(4, breedte); 

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
            for (uint y = 0; y < screen.height / scale; y++)
                for (uint x = 0; x < screen.width / scale; x++)
                    if (GetBit(x + xoffset, y + yoffset) == 1)
                        if (scale > 1)
                        {
                            for (uint j = 0; ((j + 1) % scale) != 0; j++)
                                for (uint i = 0; ((i + 1) % scale) != 0; i++)
                                    screen.Plot((x * scale + i), (y * scale + j), 0xffffff);
                        }
                        else
                            screen.Plot(x, y, 0xffffff);

            for (uint y = 0; y < ph; y++)
                for (uint x = 0; x < breedte; x++)
                {
                    if (GetBit(x, y) == 1)
                        BitSet(x, y);
                }


            // report performance
            Console.WriteLine("generation " + generation++ + ": " + timer.ElapsedMilliseconds + "ms");
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
    } // class Game
} // namespace Template

