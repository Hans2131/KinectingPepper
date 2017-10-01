using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Kinect_ing_Pepper.MediaSink
{
    //test the time to process 1 minute of data
    public unsafe class Tester
    {            
        static System.Collections.Queue bufferlist = new System.Collections.Queue();
        static System.Collections.Queue bufferlist2=new System.Collections.Queue();
        static bool done = false;
        static bool wait = false;
        public unsafe static void Test2()
        {                                  
            System.Threading.Thread thread = new System.Threading.Thread(Test2b);
            
            System.Diagnostics.Stopwatch stamp = System.Diagnostics.Stopwatch.StartNew();
            thread.Start();
            int frames = 0;            
            while (!wait) ;
            System.Diagnostics.Stopwatch z = System.Diagnostics.Stopwatch.StartNew();
            z.Start();
            System.Diagnostics.Stopwatch z2 = System.Diagnostics.Stopwatch.StartNew();
            z2.Start();
            while (frames < 20*60*1)
            {
                
                stamp.Reset();
                stamp.Start();
                UInt32[] Buf = new UInt32[1920 * 1080 + 1];
                UInt32[] Buf2 = new UInt32[1920 * 1080 + 1];                

                Buf[1920*1080] = 0;
                Buf2[1920*1080] = 0;
                bufferlist.Enqueue(Buf2);
                bufferlist2.Enqueue(Buf);
                frames++;                
                while (stamp.ElapsedMilliseconds<(48)) ;

            }            
            wait = false;
            z2.Stop();
            while (!wait) ;            
            done = true;
            thread.Join();
            z.Stop();
            Console.WriteLine("1 minute of footage processed in: {0:N} || {1:N} seconds total.", z2.ElapsedMilliseconds, z.ElapsedMilliseconds);
            return;
        }
        public unsafe static void Test2b()
        {
            MediaSink.DepthMediaSink.Init();
            MediaSink.RGBMediaSink.Init();
            UInt32** ppBuf = MediaSink.DepthMediaSink.GetBuffer();
            UInt32** ppBuf2 = MediaSink.RGBMediaSink.GetBuffer();
            UInt32* pBuf;
            while (!done)
            {
                if (bufferlist2.Count > 0)
                {
                    UInt32[] x = (UInt32[])bufferlist2.Dequeue();                    
                    fixed (UInt32* b = &x[0])
                        pBuf = b;
                    if (pBuf != null)
                    {
                        *ppBuf = pBuf;
                        MediaSink.DepthMediaSink.WriteFrame();                        
                        x = null;                        
                    }
                    UInt32[] y = (UInt32[])bufferlist.Dequeue();
                    fixed (UInt32* b = &y[0])
                        pBuf = b;
                    if (pBuf != null)
                    {
                        *ppBuf2 = pBuf;
                        MediaSink.RGBMediaSink.WriteFrame();
                        y = null;
                    }
                }
                else
                {
                    wait = true;
                }
                
            }
            MediaSink.DepthMediaSink.ShutDown();
            MediaSink.RGBMediaSink.ShutDown();
        }
    }

     /*class implemented for saving data to mp4 file
     * Current Directory for savefile is C:\images\Pepper\[RGB/Depth]Out.mp4
     * Input is RGB32 as X8 R8 G8 B8
     * !!! Data Processing will go from bottom-left to top-right !!!
     */
    public unsafe class RGBMediaSink
    {
        //Call to start the SinkWriter
        [DllImport("RGB_SinkWriter_CLI.dll", EntryPoint = "Init")]
        public static extern void Init();
        //Call to finish and close SinkWriter
        [DllImport("RGB_SinkWriter_CLI.dll", EntryPoint = "Shutdown")]
        public static extern void ShutDown();
        //returns true if SinkWriter is active
        [DllImport("RGB_SinkWriter_CLI.dll", EntryPoint = "IsActive")]
        public static extern bool IsActive();
        //returns location of the FrameBuffer
        [DllImport("RGB_SinkWriter_CLI.dll", EntryPoint = "GetBuffer")]
        public static extern UInt32** GetBuffer();
        //Call to process the FrameBuffer, returns 0 on succes
        [DllImport("RGB_SinkWriter_CLI.dll", EntryPoint = "WriteFrame")]
        public static extern int WriteFrame();
    }

         
    public unsafe class DepthMediaSink
    {
        //Call to start the SinkWriter
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "Init")]
        public static extern void Init();
        //Call to finish and close SinkWriter
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "Shutdown")]
        public static extern void ShutDown();
        //returns true if SinkWriter is active
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "IsActive")]
        public static extern bool IsActive();
        //returns location of the FrameBuffer    
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "GetBuffer")]
        public static extern UInt32** GetBuffer();
        //Call to process the FrameBuffer, returns 0 on succes
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "WriteFrame")]
        public static extern int WriteFrame();
    }
}
