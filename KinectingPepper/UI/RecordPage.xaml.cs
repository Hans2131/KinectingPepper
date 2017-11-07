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
        private bool _recordingStarted = false;
        private PathNameGenerator generator = new PathNameGenerator();

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

            WriteableBitmap cbmp = null;
            WriteableBitmap dbmp = null;

            FrameParser frameParser = new FrameParser();
            MultiSourceFrame frame = e.FrameReference.AcquireFrame();
            using (ColorFrame cf = (frame.ColorFrameReference.AcquireFrame())) {
                using (DepthFrame df = (frame.DepthFrameReference.AcquireFrame()))
                {
                    if (cf != null)
                    {
                        cbmp = frameParser.ParseToBitmap(cf);
                    }
                    if (df != null)
                    {
                        dbmp = frameParser.ParseToBitmap(df);
                    }

                    //Save data if needed
                    if (MediaSink.RGBMediaSink.IsRunning())
                    {
                        if (cbmp != null)
                        {
                            //cbmp.Unlock();
                            MediaSink.RGBMediaSink.ProcessBitmap(cbmp.BackBuffer);
                        }
                    }

                    if (MediaSink.DepthMediaSink.IsRunning())
                    {
                        if (dbmp != null)
                        {
                            //dbmp.Unlock();
                            MediaSink.DepthMediaSink.ProcessBitmap(dbmp.BackBuffer);
                        }
                    }

                    switch (_selectedCamera)
                    {

                        case ECameraType.Color:
                            // Color
                            bodyViewer.UpdateFrameCounter();
                            bodyViewer.KinectImage = cbmp;
                            break;
                        case ECameraType.Depth:
                            // Depth
                            bodyViewer.UpdateFrameCounter();
                            bodyViewer.KinectImage = dbmp;

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
                }
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
        }


        private void cbxCameraType_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void startRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (KinectHelper.Instance.KinectSensor.IsAvailable && !_recordingStarted)
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

                _recordingStarted = true;

                Logger.Instance.LogMessage("Recording started in: " + generator.FolderPathName);
            }
        }

        private void newPersonButton_Click(object sender, RoutedEventArgs e)
        {
            generator.CreateFolder();
            Logger.Instance.LogMessage("New person started in folder: " + generator.FolderPathName);
        }

        private void stopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_recordingStarted)
            {
                _recordingStarted = false;
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

                MediaSink.RGBMediaSink.Stop();
                MediaSink.DepthMediaSink.Stop();

                Logger.Instance.LogMessage("Recording stopped, files saved in " + generator.FolderPathName);
            }
        }

        private void navigateToRewindPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.Navigate(rewindPage);
        }
    }
}