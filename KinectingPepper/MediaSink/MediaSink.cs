using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Kinect;

namespace Kinect_ing_Pepper.MediaSink
{
    public unsafe class General
    {        
        public static bool RGBToBuf(ref System.Windows.Media.Imaging.WriteableBitmap frame)
        {                        
            if (frame == null || MediaSink.RGBMediaSink.processing == false) return true;            
            UInt32[] buf = new UInt32[1080 * 1920];
            byte* P = (byte*)frame.BackBuffer;            
            UInt32 x, y, z, q;
            for (int i = 0; i < 1080 * 1920; i++)
            {
                x = *P;P++;
                y = *P;P++;
                z = *P;P++;
                q = *P;P++;
                buf[i] = ((q << 24) + (z<< 16) + (y << 8) + (x));                
            }            
            return MediaSink.RGBMediaSink.ProcesData(ref buf);
        }

        public static bool DepthToBuf(DepthFrame frame)
        {
            if (frame == null) return true;
            if (!MediaSink.DepthMediaSink.processing) return true;
            UInt32[] b = new UInt32[512 * 424];
            ushort[] B = new ushort[512 * 424];
            frame.CopyFrameDataToArray(B);
            for (int i = 0; i < 512 * 424; i++)
                b[i] = B[i];
            B = null;
            return MediaSink.DepthMediaSink.ProcesData(ref b);           
        }
    }

    //test the time to process 1 minute of data
    public unsafe class Tester
    {        
        public unsafe static void Test2()
        {
            MediaSink.DepthMediaSink.Start();
            MediaSink.RGBMediaSink.Start();


            System.Diagnostics.Stopwatch stamp = System.Diagnostics.Stopwatch.StartNew();
            int frames = 0;
            System.Diagnostics.Stopwatch z = System.Diagnostics.Stopwatch.StartNew();
            z.Start();
            System.Diagnostics.Stopwatch z2 = System.Diagnostics.Stopwatch.StartNew();
            z2.Start();
            UInt32[] Buf = new UInt32[1920 * 1080 + 1];
            UInt32[] Buf2 = new UInt32[1920 * 1080 + 1];
            for (int i = 0; i < 1920 * 1080; i++)
            {
                Buf[i] = 0x00FF0000;
                Buf2[i] = 0x000FF000;
            }
            while (frames < 20 * 60 * 1)
            {
                stamp.Reset();
                stamp.Start();
                MediaSink.RGBMediaSink.ProcesData(ref Buf);
                MediaSink.DepthMediaSink.ProcesData(ref Buf2);
                //Buf = null; Buf2 = null;             
                frames++;
                while (stamp.ElapsedMilliseconds < (48)) ;

            }
            z2.Stop();
            MediaSink.DepthMediaSink.Stop();
            MediaSink.RGBMediaSink.Stop();
            z.Stop();
            Console.WriteLine("1 minute of footage processed in: {0:N} || {1:N} seconds total.", z2.ElapsedMilliseconds, z.ElapsedMilliseconds);
            return;
        }
    }
    
     /*class implemented for saving data to mp4 file
     * Current Directory for savefile is C:\images\Pepper\[RGB/Depth]Out.mp4
     * Input is RGB32 as X8 R8 G8 B8
     * !!! Data Processing will go from bottom-left to top-right !!!
     */
    public unsafe class RGBMediaSink
    {        
        private static System.Collections.Queue bufferlist = new System.Collections.Queue();
        public static bool processing=false;
        private static bool writeractive = false;
        //Call to process data to the buffer - DO CALL
        public static bool ProcesData(ref UInt32[] data) {
            if (!processing) return true;            
            bufferlist.Enqueue(data);
            return false;
        }
        //thread to write data to file - DONT CALL
        private static void Writer()
        {
            writeractive = true;
            UInt32** ppBuf = MediaSink.RGBMediaSink.GetBuffer();
            UInt32* pBuf; 
            while (processing)
            {
                while (bufferlist.Count > 0)
                {
                    UInt32 buffer;
                    UInt32[] buf = (UInt32[])bufferlist.Dequeue();
                    for(int i = 0,j=1920*1079; i < 1920 * 1080 / 2;i++)
                    {
                        buffer = buf[i];
                        buf[i] = buf[j];
                        buf[j] = buffer;
                        j++;
                        if(j%1920 == 0)
                        {
                            j -= (1920 * 2);
                        }

                    }
                    fixed (UInt32* b = &buf[0])
                        pBuf = b;
                    *ppBuf = pBuf;
                    MediaSink.RGBMediaSink.WriteFrame();
                    buf = null;
                }
            }
            writeractive = false;
            pBuf = null;
            ppBuf = null;
            
        }
        //call to start the writer | call before processdata
        public static void Start()
        {
            if (processing) return;            
            processing = true;
            init();
            System.Threading.Thread t = new System.Threading.Thread(Writer);
            t.Start();
        }
        //call to stop the writer | proper saving of files after both depth+rgb are stopped - DO CALL
        public static void Stop()
        {
            if (!processing) return;
            processing = false;
            while (writeractive) ;
            if (MediaSink.DepthMediaSink.processing)
                return;
            ShutDown();
            MediaSink.DepthMediaSink.ShutDown();
        }

        //Call to start the SinkWriter
        [DllImport("RGB_SinkWriter_CLI.dll", EntryPoint = "Init")]
        private static extern void init();
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
        private static System.Collections.Queue bufferlist = new System.Collections.Queue();
        public static bool processing = false;
        private static bool writeractive = false;
        //Call to process data to the buffer - DO CALL
        public static bool ProcesData(ref UInt32[] data)
        {
            if (!processing) return true;                                                            
            bufferlist.Enqueue(data);
            return false;
        }
        //thread to write data to file - DONT CALL
        private static void Writer()
        {
            writeractive = true;
            UInt32** ppBuf = MediaSink.DepthMediaSink.GetBuffer();
            UInt32* pBuf;
            while (processing)
            {
                while (bufferlist.Count > 0)
                {
                    UInt32[] buf = (UInt32[])bufferlist.Dequeue();
                    UInt32 buffer;
                    for (int i = 0, j = 512 * 421; i < 512 * 424 / 2; i++)
                    {
                        buffer = buf[i];
                        buf[i] = buf[j];
                        buf[j] = buffer;
                        j++;
                        if (j % 512 == 0)
                        {
                            j -= (512 * 2);
                        }

                    }
                    fixed (UInt32* b = &buf[0])
                        pBuf = b;
                    *ppBuf = pBuf;
                    MediaSink.DepthMediaSink.WriteFrame();
                    buf = null;
                }
            }
            writeractive = false;
            pBuf = null;
            ppBuf = null;

        }
        //call to start the writer | call before processdata - DO CALL
        public static void Start()
        {
            if (processing) return;
            processing = true;
            init();
            System.Threading.Thread t = new System.Threading.Thread(Writer);
            t.Start();
        }
        //call to stop the writer | proper saving of files after both depth+rgb are stopped  - DO CALL
        public static void Stop()
        {
            if (!processing) return;
            processing = false;
            while (writeractive) ;
            if (MediaSink.RGBMediaSink.processing)
                return;
            ShutDown();
            MediaSink.RGBMediaSink.ShutDown();
        }

        //Call to start the SinkWriter - DONT CALL
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "Init")]
        private static extern void init();
        //Call to finish and close SinkWriter - DONT CALL
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "Shutdown")]
        public static extern void ShutDown();
        //returns true if SinkWriter is active - DONT CALL
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "IsActive")]
        public static extern bool IsActive();
        //returns location of the FrameBuffer - DONT CALL
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "GetBuffer")]
        public static extern UInt32** GetBuffer();
        //Call to process the FrameBuffer, returns 0 on succes - DONT CALL
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "WriteFrame")]
        public static extern int WriteFrame();
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "SetPath")]
        public static extern int SetPath(char[] path);
    }
}
