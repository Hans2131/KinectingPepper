using System;
using System.Runtime.InteropServices;

namespace Kinect_ing_Pepper.MediaSink
{
    public unsafe class Tester
    {
        //40 seconds
        //take timestamp before and after calling this function to measure the time required to process 30 seconds of RGB and Depth data
        public unsafe static void Test()
        {
            MediaSink.DepthMediaSink.Init();
            MediaSink.RGBMediaSink.Init();
            uint* x = MediaSink.DepthMediaSink.GetBuffer();
            uint* x2 = MediaSink.RGBMediaSink.GetBuffer();
            System.DateTime y = System.DateTime.Now;
            for (int j = 0; j < 900; j++)
            {
                for (int i = 0; i < 1920 * 1080; i++)
                {
                    x[i] = 0x00FFF000;
                    x2[i] = 0x00F000FF;
                }
                MediaSink.DepthMediaSink.WriteFrame();
                MediaSink.RGBMediaSink.WriteFrame();
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
        public static extern UInt32* GetBuffer();
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
        public static extern UInt32* GetBuffer();
        //Call to process the FrameBuffer, returns 0 on succes
        [DllImport("Depth_SinkWriter_CLI.dll", EntryPoint = "WriteFrame")]
        public static extern int WriteFrame();
    }
}
