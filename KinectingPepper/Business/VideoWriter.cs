using AForge.Video.FFMPEG;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Kinect_ing_Pepper.Business
{
    public class VideoWriter
    {
        private VideoFileWriter _writer;
        private bool _isStartedAsync = false;
        private ConcurrentQueue<Bitmap> _recordQueue = new ConcurrentQueue<Bitmap>();
        private SemaphoreSlim _writerSemaphore = new SemaphoreSlim(1);
        private bool _isAsync = false;

        public VideoWriter(bool isAsync)
        {
            _isAsync = isAsync;
        }

        public void Start(string fileName, int width, int height)
        {
            if (_isAsync)
            {
                Task task = ProcessVideoFramesAsync(fileName, width, height);
                task.ContinueWith((antecedent) =>
                {
                    CloseWriter();
                });
            }
            else
            {
                _writer = new VideoFileWriter();
                _writer.Open(fileName, width, height, 25, VideoCodec.MPEG4);
            }
        }


        public async Task ProcessVideoFramesAsync(string fileName, int width, int height)
        {
            Debug.WriteLine("Async started");

            _writer = new VideoFileWriter();
            _writer.Open(fileName, width, height, 25, VideoCodec.MPEG4);

            _isStartedAsync = true;
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
                            await Task.Run(() =>
                            {
                                _writer.WriteVideoFrame(videoFrame);
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                        finally
                        {
                            _writerSemaphore.Release();
                        }

                        
                    }
                    else
                    {
                        await Task.Delay(100);
                        if (_recordQueue.IsEmpty && _isStartedAsync == false)
                        {
                            break;
                        }
                    }
                }
            });
        }

        public void Stop()
        {
            if (_isAsync)
            {
                _isStartedAsync = false;
                Debug.WriteLine("Async stopped");
            }
            else
            {
                CloseWriter();
            }
        }

        public void WriteVideoFrame(Bitmap videoFrame)
        {
            if (_isAsync)
            {
                _recordQueue.Enqueue(videoFrame);
            }
            else
            {
                _writer.WriteVideoFrame(videoFrame);
            }
        }

        private void CloseWriter()
        {
            if (_isAsync)
            {
                try
                {
                    _writerSemaphore.Wait();
                    _writer.Close();
                    _writer.Dispose();

                    Debug.WriteLine("Writer closed");
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
            else
            {
                _writer.Close();
                _writer.Dispose();
            }
        }
    }
}
