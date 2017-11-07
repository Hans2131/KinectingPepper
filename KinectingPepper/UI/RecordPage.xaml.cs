using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Kinect;
using Kinect_ing_Pepper.Enums;
using Kinect_ing_Pepper.Business;
using Kinect_ing_Pepper.Models;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Kinect_ing_Pepper.Utils;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class RecordPage : Page
    {
        private readonly RewindPage rewindPage;
        private readonly Frame navigationFrame;
        private ECameraType _selectedCamera = ECameraType.Color;

        private MultiSourceFrameReader _reader;
        private List<BodyFrameWrapper> _recordedBodyFrames = new List<BodyFrameWrapper>();
        private bool _recordingStarted = false;
        private VideoWriter _videoWriter = null;
        private Task _videoProcessingTask = null;

        private PathNameGenerator generator = new PathNameGenerator();
        private FrameParser _frameParser = new FrameParser();

        public RecordPage(Frame navigationFrame)
        {
            InitializeComponent();

            Logger.Instance.Init(logList);
            Logger.Instance.LogMessage("Application started!");

            generator.CreateFolder();
            rewindPage = new RewindPage(navigationFrame);
            this.navigationFrame = navigationFrame;

            RestartKinect();
            cbxCameraType.ItemsSource = Enum.GetValues(typeof(ECameraType)).Cast<ECameraType>();
            cbxCameraType.SelectedIndex = 0;
        }

        private void RestartKinect()
        {
            bodyViewer.Clear();
            if (_reader != null) _reader.Dispose();
            if (KinectHelper.Instance.TryStartKinect())
            {
                _reader = KinectHelper.Instance.KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }

            cbxCameraType.ItemsSource = Enum.GetValues(typeof(ECameraType)).Cast<ECameraType>();
            cbxCameraType.SelectedIndex = 0;
            btnStartRecording.IsEnabled = true;
            btnStopRecording.IsEnabled = false;
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame frame = e.FrameReference.AcquireFrame();
            if (frame != null)
            {
                ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame();

                DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame();

                BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame();

                ProcesKinectFrames(bodyFrame, colorFrame, depthFrame);
            }
        }

        private void ProcesKinectFrames(BodyFrame bodyFrame, ColorFrame colorFrame, DepthFrame depthFrame)
        {
            WriteableBitmap colorWBitmap = null;
            WriteableBitmap depthWBitmap = null;

            if (colorFrame != null)
            {
                colorWBitmap = _frameParser.ParseToWriteableBitmap(colorFrame);
            }

            if (depthFrame != null)
            {
                depthWBitmap = _frameParser.ParseToWriteableBitmap(depthFrame);

                if (_recordingStarted && _videoWriter != null)
                {
                    _videoWriter.EnqueueFrame(depthWBitmap);
                }
            }

            switch (_selectedCamera)
            {
                case ECameraType.Color:
                    // Color
                    bodyViewer.UpdateFrameCounter();
                    bodyViewer.KinectImage = colorWBitmap;
                    break;
                case ECameraType.Depth:
                    // Depth
                    bodyViewer.UpdateFrameCounter();
                    bodyViewer.KinectImage = depthWBitmap;

                    break;
                default:
                    break;
            }

            if (bodyFrame != null)
            {
                BodyFrameWrapper bodyFrameWrapper = new BodyFrameWrapper(bodyFrame);

                //choose body to record? why not safe all..
                if (bodyFrameWrapper.TrackedBodies.Any())
                {
                    bodyViewer.RenderBodies(bodyFrameWrapper.TrackedBodies, _selectedCamera);

                    if (_recordingStarted)
                    {
                        _recordedBodyFrames.Add(bodyFrameWrapper);
                    }
                }
            }
            else
            {
                bodyViewer.DeleteUntrackedBodies(null);
            }
        }


        private void CbxCameraType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Enum.TryParse(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }

        private void RestartKinectButton_Click(object sender, RoutedEventArgs e)
        {
            if (KinectHelper.Instance.KinectSensor.IsAvailable && !_recordingStarted)
            {
                KinectHelper.Instance.StopKinect();
            }
            RestartKinect();
        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (KinectHelper.Instance.KinectSensor.IsAvailable && !_recordingStarted)
            {
                btnRestartKinect.IsEnabled = false;
                btnRewindPageNavigation.IsEnabled = false;
                btnStopRecording.IsEnabled = true;
                btnStartRecording.IsEnabled = false;

                if (cbxCameraType.SelectedIndex == 0)
                {
                    cbxCameraType.IsEnabled = false;
                }
                if (cbxCameraType.SelectedIndex == 1)
                {
                    cbxCameraType.IsEnabled = false;
                }

                StartVideoWriter();
                _recordingStarted = true;

                Logger.Instance.LogMessage("Recording started in: " + generator.FolderPathName);
            }
        }

        private void StartVideoWriter()
        {
            if (_videoProcessingTask != null)
            {
                if (_videoProcessingTask.IsCompleted)
                {
                    _videoWriter.Dispose();
                    _videoProcessingTask.Dispose();
                }
                else
                {
                    _videoProcessingTask.Wait();
                    _videoWriter.Dispose();
                    _videoProcessingTask.Dispose();
                }
            }

            _videoWriter = new VideoWriter();
            _videoProcessingTask = _videoWriter.ProcessVideoFramesAsync(@"C:\images\Test\test3.mp4", Constants.DepthWidth, Constants.DepthHeight);
        }

        private void NewPersonButton_Click(object sender, RoutedEventArgs e)
        {
            generator.CreateFolder();
            Logger.Instance.LogMessage("New person started in folder: " + generator.FolderPathName);
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_recordingStarted)
            {
                _recordingStarted = false;
                _videoWriter.Finish();
                DateTime dateTime = DateTime.Now;

                btnRestartKinect.IsEnabled = true;
                btnRewindPageNavigation.IsEnabled = true;
                btnStopRecording.IsEnabled = false;
                btnStartRecording.IsEnabled = true;

                if (_recordedBodyFrames.Any())
                {
                    string filePath = generator.FolderPathName + "/" +
                        dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString().Replace(":", " ") + ".xml";
                    DiskIOManager.Instance.SerializeToXML(_recordedBodyFrames, filePath);

                    //reset recorded frames
                    _recordedBodyFrames = new List<BodyFrameWrapper>();

                    Logger.Instance.LogMessage("Xml saved as: " + filePath);
                }

                cbxCameraType.IsEnabled = true;

                Logger.Instance.LogMessage("Recording stopped, files saved in " + generator.FolderPathName);
            }
        }

        private void NavigateToRewindPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.Navigate(rewindPage);
        }
    }
}