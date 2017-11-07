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
    public class VideoWriter : IDisposable
    {

        private VideoFileWriter _writer;
        private bool _isStarted = false;
        private ConcurrentQueue<Bitmap> _recordQueue = new ConcurrentQueue<Bitmap>();
        private SemaphoreSlim _writerSemaphore = new SemaphoreSlim(1);
        private FrameParser _frameParser = new FrameParser();

        public void TestVideoWriter(List<Bitmap> videoFrames, string fileName, int width, int height)
        {
            VideoFileWriter writer = new VideoFileWriter();
            writer.Open(fileName, width, height, 25, VideoCodec.MPEG4);

            foreach (Bitmap frame in videoFrames)
            {
                writer.WriteVideoFrame(frame);
            }
            writer.Close();
        }

        public async Task ProcessVideoFramesAsync(string fileName, int width, int height)
        {
            Debug.WriteLine("Async started");

            _writer = new VideoFileWriter();
            _writer.Open(fileName, width, height, 25, VideoCodec.MPEG4);

            _isStarted = true;
            await Task.Run(async () =>
            {
                while (true)
                {

                    Bitmap videoFrame;
                    if (_recordQueue.TryDequeue(out videoFrame))
                    {
                        try
                        {
                            await _writerSemaphore.WaitAsync();
                            await Task.Run(() => _writer.WriteVideoFrame(videoFrame));
                            Debug.WriteLine("Video frame written, in queue: " + _recordQueue.Count.ToString());
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                        finally
                        {
                            _writerSemaphore.Release();
                        }

                        if (_recordQueue.IsEmpty && _isStarted == false)
                        {
                            Debug.WriteLine("Close writer");
                            _writer.Close();
                            break;
                        }
                    }
                }
            });
        }

        public void EnqueueFrame(Bitmap videoFrame)
        {
            //WriteableBitmap frameCopy = videoFrame.Clone();
            _recordQueue.Enqueue(videoFrame);
        }

        public void Finish()
        {
            _isStarted = false;
            Debug.WriteLine("Async stopped");
        }

        public void Dispose()
        {
            try
            {
                _writerSemaphore.Wait();
                _writer.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                _writerSemaphore.Dispose();
            }
        }

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
