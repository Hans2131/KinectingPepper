using AForge.Video.FFMPEG;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kinect_ing_Pepper.Business
{
    public class VideoWriter
    {
        private VideoFileWriter _writer;

        public void CreateVideoFile(string fileName, int width, int height)
        {
            _writer = new VideoFileWriter();
            _writer.Open(fileName, width, height, 25, VideoCodec.MPEG4);
        }

        public void WriteVideoFrame(Bitmap videoFrame)
        {
            _writer.WriteVideoFrame(videoFrame);
        }

        public void CloseFile()
        {
            _writer.Close();
            _writer.Dispose();
        }
    }
}
