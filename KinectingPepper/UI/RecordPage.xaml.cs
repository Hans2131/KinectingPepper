using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Kinect;
using Kinect_ing_Pepper.Enums;
using Kinect_ing_Pepper.Business;
using System.Xml.Serialization;
using System.IO;
using Kinect_ing_Pepper.Models;
using System.Windows.Media.Imaging;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class RecordPage : Page
    {
        private MultiSourceFrameReader _reader;
        private ECameraType _selectedCamera = ECameraType.Color;
        private readonly RewindPage rewindPage;
        private readonly Frame navigationFrame;
        private List<BodyFrameWrapper> _recordedBodyFrames = new List<BodyFrameWrapper>();
        private bool _recordBodyFrames = false;
        private PathNameGenerator generator = new PathNameGenerator();

        public RecordPage(Frame navigationFrame)
        {
            InitializeComponent();
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
            if(_reader != null) _reader.Dispose();
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
            
            FrameParser frameParser = new FrameParser();
            MultiSourceFrame frame = e.FrameReference.AcquireFrame();

            switch (_selectedCamera)
            {
                case ECameraType.Color:
                    // Color
                    using (ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame())
                    {
                        if (colorFrame != null)
                        {
                            bodyViewer.UpdateFrameCounter();
                            WriteableBitmap wb = frameParser.ParseToBitmap(colorFrame);
                            bodyViewer.KinectImage = wb;
                            MediaSink.General.RGBToBuf(ref wb);
                        }
                    }
                    break;
                case ECameraType.Depth:
                    // Depth
                    using (DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame())
                    {
                        if (depthFrame != null)
                        {

                            MediaSink.General.DepthToBuf(depthFrame);
                            bodyViewer.UpdateFrameCounter();
                            bodyViewer.KinectImage = frameParser.ParseToBitmap(depthFrame);
                        }
                    }
                    break;
                case ECameraType.Infrared:
                    using (InfraredFrame infraredFrame = frame.InfraredFrameReference.AcquireFrame())
                    {
                        if (infraredFrame != null)
                        {
                            bodyViewer.UpdateFrameCounter();
                            bodyViewer.KinectImage = frameParser.ParseToBitmap(infraredFrame);
                        }
                    }
                    break;
                default:
                    break;
            }
        

            using (BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    BodyFrameWrapper bodyFrameWrapper = new BodyFrameWrapper(bodyFrame);

                    //choose body to record? why not safe all..
                    if (bodyFrameWrapper.TrackedBodies.Any())
                    {
                        bodyViewer.RenderBodies(bodyFrameWrapper.TrackedBodies, _selectedCamera);

                        if (_recordBodyFrames)
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
        }
        

        private void cbxCameraType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Enum.TryParse(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }

        private void RestartKinectButton_Click(object sender, RoutedEventArgs e)
        {
            if (KinectHelper.Instance.HasStarted())
            {
                KinectHelper.Instance.StopKinect();
            }
            RestartKinect();
        }

        private void startRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            btnRestartKinect.IsEnabled = false;
            btnRewindPageNavigation.IsEnabled = false;
            btnStopRecording.IsEnabled = true;
            btnStartRecording.IsEnabled = false;
            if (cbxCameraType.SelectedIndex == 0)
            {
                MediaSink.RGBMediaSink.SetPath(generator.CreateFilePathName("RGB").ToArray());
                MediaSink.RGBMediaSink.Start();
                cbxCameraType.IsEnabled = false;
            }
            if (cbxCameraType.SelectedIndex == 1)
            {
                string pathName = generator.CreateFilePathName("Depth");
                MediaSink.DepthMediaSink.SetPath(pathName.ToArray());
                MediaSink.DepthMediaSink.Start();
                cbxCameraType.IsEnabled = false;
            }
                                    
            _recordBodyFrames = true;
        }

        private void newPersonButton_Click(object sender, RoutedEventArgs e)
        {
            generator.CreateFolder();
        }

        private void stopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            btnRestartKinect.IsEnabled = true;
            btnRewindPageNavigation.IsEnabled = true;
            btnStopRecording.IsEnabled = false;
            btnStartRecording.IsEnabled = true;
            DateTime dateTime = DateTime.Now;

            if (_recordedBodyFrames.Any())
            {
                _recordBodyFrames = false;
                PersistFrames.Instance.SerializeToXML(_recordedBodyFrames, generator.folderPathName +"/"+
                    dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString().Replace(":", " ") + ".xml");

                _recordedBodyFrames = new List<BodyFrameWrapper>();
            }
            cbxCameraType.IsEnabled = true;
            MediaSink.RGBMediaSink.Stop();
            MediaSink.DepthMediaSink.Stop();
            btnStartRecording.IsEnabled = false;
            btnStopRecording.IsEnabled = true;
            List<BodyFrameWrapper> framesFromDisk = PersistFrames.Instance.DeserializeFromXML(generator.folderPathName + "/XmlTest.xml");
        }

        private void navigateToRewindPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.Navigate(rewindPage);
        }
    }
}