using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Kinect;

namespace Kinect_ing_Pepper.MediaSink
{
    
    /*class implemented for saving data to mp4 file
    * Current Directory for savefile is C:\images\Pepper\[RGB/Depth]Out.mp4
    * Input is RGB32 as X8 R8 G8 B8
    * !!! Data Processing will go from bottom-left to top-right !!!
    */
    public unsafe class RGBMediaSink
    {

        const int WIDTH = 1920;
        const int HEIGHT = 1080;
        const int NEWWIDTH = 640;
        const int NEWHEIGHT = 480;        
        const int COLORS = 3;
        const int Commpresstride = (HEIGHT) / (NEWHEIGHT);
        const int skiptonext = HEIGHT * COLORS - ((HEIGHT) % (Commpresstride));
        const int Commpresheight = (WIDTH) / (NEWWIDTH);
        const int extraframe = NEWWIDTH / ((WIDTH % NEWWIDTH) + 1);
        const int TOTALI = NEWHEIGHT * NEWWIDTH * COLORS;
        const int extendx = (Commpresstride) * COLORS;
        const int extendy1 = (Commpresheight + 1) * HEIGHT * COLORS;
        const int extendy = (Commpresheight) * HEIGHT * COLORS;

        static char[] buffer = new char[NEWWIDTH * NEWHEIGHT * COLORS + 1];

        static public void ProcessBitmap(IntPtr b)
        {
            char* buf = (char*)b;
            for (int i = 0, j = 0, t = 0, y = 0; i < TOTALI; i += 3)
            {
                buffer[i + 0] = buf[j + 3 + y];  //R
                buffer[i + 1] = buf[j + 2 + y];  //G
                buffer[i + 2] = buf[j + 1 + y];  //B
                j += extendx;
                if (j > skiptonext)
                {
                    t++;
                    if (t == extraframe)
                    {
                        j = 0; y += extendy1; t = 0;
                    }
                    else
                    {
                        j = 0;
                        y += extendy;
                    }
                }
            }
            fixed(char* x = &buffer[0])
            Process(x);
        }

        static public bool IsRunning()
        {
            return running;
        }
        
        static private bool running = false;

        static public void Start()
        {
            running = true;
            init();
        }
        static public void Stop()
        {
            running = false;
            ShutDown();
        }
        //Call to start the SinkWriter
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "RGBInit")]
        private static extern void init();
        //Call to finish and close SinkWriter
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "RGBShutdown")]
        private static extern void ShutDown();
        //returns true if SinkWriter is active
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "RGBIsActive")]
        public static extern bool IsActive();
        //process the given data
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "RGBProcess")]
        private static extern void Process(char* buffer);        
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "RGBSetPath")]
        public static extern int SetPath(char[] path);
    }


    public unsafe class DepthMediaSink
    {
        const int NEWWIDTH = 640;
        const int NEWHEIGHT = 480;
        const int DEPTHWIDTH = 512;
        const int DEPTHHEIGHT = 424;
        const int AREA = NEWWIDTH * NEWHEIGHT;
        const int Dextendx = (int)((double)(NEWWIDTH) / (NEWWIDTH - DEPTHWIDTH) + 0.5);
        const int Dextendy = (int)((double)(NEWHEIGHT) / (NEWHEIGHT - DEPTHHEIGHT) + 0.5);

        static char[]bufferdepth = new char[NEWHEIGHT*NEWWIDTH];
        static UInt32* buff;

        static public bool IsRunning()
        {
            return running;
        }
        static private bool running = false;
        static public void Start()
        {
            running = true;
            init();
            buff = GetBuffer();
        }
        static public void Stop()
        {
            running = false;
            ShutDown();
        }

        static public void ProcessBitmap(IntPtr b)
        {
            char* buffer = (char*)b;
            int i = 0, j = 0, y = 0;
            while (i < AREA)
            {
                for (int x = 0; x < NEWWIDTH; x++)
                {
                    bufferdepth[i] = buffer[j];
                    j++; i++;
                    if (j % (Dextendx - 1) == 0)
                    {
                        bufferdepth[i] = bufferdepth[i - 1];
                        i++; x++;
                    }
                }
                y++;
                if (y % (Dextendy - 1) == 0)
                {
                    for (int x = 0; x < NEWWIDTH; x++, i++)
                    {
                        bufferdepth[i] = bufferdepth[i - NEWHEIGHT];
                    }
                    y++;
                }
            }
            for(int I = 0,J=NEWWIDTH*(NEWHEIGHT-1); I <AREA; I++)
            {
                buff[I] = bufferdepth[J];
                J++;
                if (J % NEWWIDTH == 0)
                    J -= NEWWIDTH * 2;
            }
            WriteFrame();
        }    


        //Call to start the SinkWriter - DONT CALL
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "DInit")]
        private static extern void init();
        //Call to finish and close SinkWriter - DONT CALL
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "DShutdown")]
        public static extern void ShutDown();
        //returns true if SinkWriter is active - DONT CALL
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "DIsActive")]
        public static extern bool IsActive();
        //returns location of the FrameBuffer - DONT CALL
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "DGetBuffer")]
        public static extern UInt32* GetBuffer();
        //Call to process the FrameBuffer, returns 0 on success
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "DWriteFrame")]
        public static extern int WriteFrame();
        [DllImport("SinkWriter_CLI.dll", EntryPoint = "DSetPath")]
        public static extern int SetPath(char[] path);
    }
}
