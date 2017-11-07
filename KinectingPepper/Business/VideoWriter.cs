﻿using AForge.Video.FFMPEG;
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
        private ConcurrentQueue<WriteableBitmap> _recordQueue = new ConcurrentQueue<WriteableBitmap>();
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

                    WriteableBitmap videoFrame;
                    if (_recordQueue.TryDequeue(out videoFrame))
                    {                      
                        try
                        {
                            Bitmap bitmap = _frameParser.ParseToBitmap(videoFrame);

                            await _writerSemaphore.WaitAsync();
                            await Task.Run(() => _writer.WriteVideoFrame(bitmap));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
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
            }).ConfigureAwait(false);
        }

        public void EnqueueFrame(WriteableBitmap videoFrame)
        {
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
            } catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                _writerSemaphore.Dispose();
            }
        }
    }
}
