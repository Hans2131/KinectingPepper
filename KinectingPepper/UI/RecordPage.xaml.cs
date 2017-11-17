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
        private VideoWriter _depthVideoWriter = null;

        private PathNameGenerator generator = new PathNameGenerator();
        private FrameParser _frameParser = new FrameParser();

        public RecordPage(Frame navigationFrame)
        {
            InitializeComponent();

            Logger.Instance.Init(logList);
            Logger.Instance.LogMessage("Application started!");

            rewindPage = new RewindPage(navigationFrame);
            this.navigationFrame = navigationFrame;

            RestartKinect();
            cbxCameraType.ItemsSource = Enum.GetValues(typeof(ECameraType)).Cast<ECameraType>();
            cbxCameraType.SelectedIndex = 1;

            txtPersonNumber.Text = Properties.Settings.Default.PersonNumber.ToString();
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

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            WriteableBitmap depthWBitmap = null;
            WriteableBitmap colorWBitmap = null;
            Bitmap colorBitmap = null;
            Bitmap depthBitmap = null;

            MultiSourceFrame frame = e.FrameReference.AcquireFrame();
            if (frame != null)
            {
                using (DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame())
                {
                    if (depthFrame != null)
                    {
                        _frameParser.ParseToBitmaps(depthFrame, out depthBitmap, out depthWBitmap);

                        if (_selectedCamera == ECameraType.Depth)
                        {
                            if (_selectedCamera == ECameraType.Depth)
                            {
                                bodyViewer.UpdateFrameCounter();
                                bodyViewer.KinectImage = depthWBitmap;
                            }

                        }
                    }
                }

                if (_selectedCamera == ECameraType.Color)
                {
                    using (ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame())
                    {
                        if (colorFrame != null)
                        {
                            _frameParser.ParseToBitmaps(colorFrame, out colorBitmap, out colorWBitmap);

                            if (_selectedCamera == ECameraType.Color)
                            {
                                bodyViewer.UpdateFrameCounter();
                                bodyViewer.KinectImage = colorWBitmap;
                            }
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

                            if (_recordingStarted)
                            {
                                _recordedBodyFrames.Add(bodyFrameWrapper);
                                if (depthWBitmap != null)
                                {
                                    _depthVideoWriter.WriteVideoFrame(depthBitmap);

                                    //writing colorframes does work but should be deactivated by default
                                    //_colorVideoWriter.WriteVideoFrame(colorBitmap);
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


                generator.SetFileNameBase();
                string depthFileName = generator.FolderPathName + "/" + "Depth " + generator.FileNameBase + ".mp4";
                _depthVideoWriter = new VideoWriter(true);
                _depthVideoWriter.Start(depthFileName, Constants.DepthWidth, Constants.DepthHeight);

                _recordingStarted = true;

                Logger.Instance.LogMessage("Recording started in: " + generator.FolderPathName);
            }
        }

        private void NewPersonButton_Click(object sender, RoutedEventArgs e)
        {
            int personNumber;
            if (!int.TryParse(txtPersonNumber.Text, out personNumber))
            {
                Logger.Instance.LogMessage("Invalid personnumber!");
            }
            else
            {
                if (personNumber == Properties.Settings.Default.PersonNumber)
                {
                    personNumber++;

                }
                Properties.Settings.Default.PersonNumber = personNumber;
                Properties.Settings.Default.Save();
                txtPersonNumber.Text = personNumber.ToString();
            }


            generator.CreateFolder();
            Logger.Instance.LogMessage("New person started in folder: " + generator.FolderPathName);
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_recordingStarted)
            {
                _recordingStarted = false;

                _depthVideoWriter.Stop();

                btnRestartKinect.IsEnabled = true;
                btnRewindPageNavigation.IsEnabled = true;
                btnStopRecording.IsEnabled = false;
                btnStartRecording.IsEnabled = true;

                if (_recordedBodyFrames.Any())
                {
                    //string csvPath = generator.FolderPathName + "/" + generator.FileNameBase + ".csv";
                    //CSVWriter csvSaver = new CSVWriter();
                    //csvSaver.SaveSkeletonFrames(_recordedBodyFrames, csvPath);

                    string xmlFileName = generator.FolderPathName + "/" + generator.FileNameBase + ".xml";
                    DiskIOManager.Instance.SerializeToXML(_recordedBodyFrames, xmlFileName);

                    //reset recorded frames
                    _recordedBodyFrames = new List<BodyFrameWrapper>();

                    Logger.Instance.LogMessage("XML&CSV saved in: " + generator.FolderPathName + " as " + generator.FileNameBase);
                }

                cbxCameraType.IsEnabled = true;
            }
        }

        private void NavigateToRewindPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.Navigate(rewindPage);
        }
    }
}