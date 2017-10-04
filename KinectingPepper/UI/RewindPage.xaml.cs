using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Kinect;
using Kinect_ing_Pepper.Enums;
using Kinect_ing_Pepper.Business;
using Kinect_ing_Pepper.Models;
using System.Windows.Media;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class RewindPage : Page
    {
        private ECameraType _selectedCamera = ECameraType.Color;
        private readonly Frame navigationFrame;
        private int _currentFrameNumber = 50;

        private List<BodyFrameWrapper> _framesFromDisk;
        private bool _playBackFrames = false;

        public RewindPage(Frame navigationFrame)
        {
            InitializeComponent();
            this.navigationFrame = navigationFrame;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void cbxCameraType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Enum.TryParse<ECameraType>(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }

        private void selectFile_Click(object sender, RoutedEventArgs e)
        {
            _framesFromDisk = PersistFrames.Instance.DeserializeFromXML(@"C:\Users\Hans\Documents\Kinect Data\BodyFrames 3-10-2017 15 09 01 Arm sidewards.xml");
            _playBackFrames = true;

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            //bodyViewer.TestDrawLine();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_playBackFrames)
            {
                KinectHelper.Instance.TryStartKinect();

                BodyFrameWrapper currentBodyFrame = _framesFromDisk[_currentFrameNumber];
                bodyViewer.RenderBodies(currentBodyFrame.TrackedBodies, ECameraType.Color);
                _playBackFrames = false;
            }

            if(_currentFrameNumber == _framesFromDisk.Count - 1)
            {
                _playBackFrames = false;
            } else
            {

            }
        }

        private void resetPlayer_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            _playBackFrames = false;
        }
        private void pausePlayer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void navigateToRecordPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.NavigationService.GoBack();
        }
    }
}
