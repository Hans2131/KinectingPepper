using System;
using System.Runtime.InteropServices;

namespace Kinect_ing_Pepper.MediaSink
{    
     /*class implemented for saving data to mp4 file
     * Current Directory for savefile is C:\images\Pepper\out.mp4
     * Input is RGB32 as X8 R8 G8 B8
     * 
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
}
