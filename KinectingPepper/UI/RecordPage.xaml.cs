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
using System.Drawing;
using System.Diagnostics;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class RecordPage : Page
    {
        private readonly RewindPage rewindPage;
        private readonly Frame navigationFrame;
        private ECameraType _selectedCamera = ECameraType.Depth;

        private MultiSourceFrameReader _reader;
        private List<BodyFrameWrapper> _recordedBodyFrames = new List<BodyFrameWrapper>();
        private bool _recordingStarted = false;
        private VideoWriter _videoWriter = null;
        private Task _videoProcessingTask = null;

        private PathNameGenerator generator = new PathNameGenerator();
        private FrameParser _frameParser = new FrameParser();

        private DateTime _timeRecordingStart = DateTime.MinValue;
        private string _fileNameBase = "";

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
            cbxCameraType.SelectedIndex = 1;
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

            btnStartRecording.IsEnabled = true;
            btnStopRecording.IsEnabled = false;
        }
        private int counter = 0;
        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            bool recordFrame = _recordingStarted;

            WriteableBitmap colorWBitmap = null;
            WriteableBitmap depthWBitmap = null;

            MultiSourceFrame frame = e.FrameReference.AcquireFrame();

            using (DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    depthWBitmap = _frameParser.ParseToWriteableBitmap(depthFrame);
                    if (_selectedCamera == ECameraType.Depth)
                    {
                        bodyViewer.UpdateFrameCounter();
                        bodyViewer.KinectImage = depthWBitmap;
                    }
                }
            }

            using (BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    BodyFrameWrapper bodyFrameWrapper = new BodyFrameWrapper(bodyFrame);

                    if (bodyFrameWrapper.TrackedBodies.Any())
                    {
                        bodyViewer.RenderBodies(bodyFrameWrapper.TrackedBodies, _selectedCamera);

                        if (recordFrame)
                        {
                            _recordedBodyFrames.Add(bodyFrameWrapper);
                            if (depthWBitmap != null)
                            {
                                Bitmap videoFrame = _frameParser.ParseToBitmap(depthWBitmap);
                                _videoWriter.WriteVideoFrame(videoFrame);
                            }
                            else
                            {
                                Logger.Instance.LogMessage("DepthVideoFrame missing at: " + _recordedBodyFrames.Count.ToString());
                            }
                        }
                    }
                }
                else
                {
                    bodyViewer.DeleteUntrackedBodies(null);
                }
            }

            if (frame != null)
            {
                if (_selectedCamera == ECameraType.Color)
                {
                    using (ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame())
                    {
                        if (colorFrame != null)
                        {
                            colorWBitmap = _frameParser.ParseToWriteableBitmap(colorFrame);

                            bodyViewer.UpdateFrameCounter();
                            bodyViewer.KinectImage = colorWBitmap;
                        }
                    }
                }
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

                _timeRecordingStart = DateTime.Now;
                _fileNameBase = _timeRecordingStart.ToShortDateString() + " " + _timeRecordingStart.ToLongTimeString().Replace(":", "_");

                string depthFileName = generator.FolderPathName + "/" + "Depth " + _fileNameBase + ".mp4";
                _videoWriter = new VideoWriter();
                _videoWriter.CreateVideoFile(depthFileName, Constants.DepthWidth, Constants.DepthHeight);
                _recordingStarted = true;

                Logger.Instance.LogMessage("Recording started in: " + generator.FolderPathName);
            }
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
                _videoWriter.CloseFile();

                btnRestartKinect.IsEnabled = true;
                btnRewindPageNavigation.IsEnabled = true;
                btnStopRecording.IsEnabled = false;
                btnStartRecording.IsEnabled = true;

                if (_recordedBodyFrames.Any())
                {
                    string xmlFileName = generator.FolderPathName + "/" + _fileNameBase + ".xml";
                    DiskIOManager.Instance.SerializeToXML(_recordedBodyFrames, xmlFileName);

                    //reset recorded frames
                    _recordedBodyFrames = new List<BodyFrameWrapper>();

                    Logger.Instance.LogMessage("Xml saved as: " + xmlFileName);
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