﻿using System;
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

        public RecordPage(Frame navigationFrame)
        {
            InitializeComponent();
            this.rewindPage = new RewindPage(navigationFrame);
            this.navigationFrame = navigationFrame;

            if (KinectHelper.Instance.TryStartKinect())
            {
                _reader = KinectHelper.Instance.KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }

            cbxCameraType.ItemsSource = Enum.GetValues(typeof(ECameraType)).Cast<ECameraType>();
            cbxCameraType.SelectedIndex = 0;
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
                            bodyViewer.KinectImage = frameParser.ParseToBitmap(colorFrame);
                        }
                    }
                    break;
                case ECameraType.Depth:
                    // Depth
                    using (DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame())
                    {
                        if (depthFrame != null)
                        {
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _reader.Dispose();

            KinectHelper.Instance.StopKinect();
        }

        private void cbxCameraType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Enum.TryParse(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }

        private void startRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            _recordBodyFrames = true;
        }

        private void stopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime dateTime = DateTime.Now;

            if (_recordedBodyFrames.Any())
            {
                _recordBodyFrames = false;
                PersistFrames.Instance.SerializeToXML(_recordedBodyFrames, @"C:\Users\Hans\Documents\Kinect Data\BodyFrames " +
                    dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString().Replace(":", " ") + ".xml");

                _recordedBodyFrames = new List<BodyFrameWrapper>();
            }

            List<BodyFrameWrapper> framesFromDisk = PersistFrames.Instance.DeserializeFromXML(@"C:\Users\Hans\Documents\Kinect Data\XmlTest.xml");
        }

        private void navigateToRewindPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.Navigate(rewindPage);
        }
    }
}