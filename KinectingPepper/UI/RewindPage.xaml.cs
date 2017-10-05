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
        private int _currentFrameNumber = 0;

        private List<BodyFrameWrapper> _framesFromDisk;
        private bool _playBackFrames = false;
        private DateTime _timeLastFrameRender = DateTime.MinValue;

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
            string filePath = @"C:\Users\Hans\Documents\Kinect Data\5-10-2017_09_01_38 P01\5-10-2017 09 03 27.xml";
            _framesFromDisk = PersistFrames.Instance.DeserializeFromXML(filePath);
            _playBackFrames = true;

            slrFrameProgress.Maximum = _framesFromDisk.Count;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_playBackFrames)
            {
                if (_currentFrameNumber == _framesFromDisk.Count - 1)
                {
                    //_playBackFrames = false;
                    _currentFrameNumber = 0;
                    _timeLastFrameRender = DateTime.MinValue;
                }
                else
                {
                    if (_timeLastFrameRender == DateTime.MinValue)
                    {
                        _timeLastFrameRender = DateTime.Now;
                        bodyViewer.RenderBodies(_framesFromDisk[_currentFrameNumber].TrackedBodies, ECameraType.Color);
                    }
                    else
                    {
                        TimeSpan timePast = DateTime.Now - _timeLastFrameRender;
                        TimeSpan expectedTimeSpan = _framesFromDisk[_currentFrameNumber + 1].RelativeTime - _framesFromDisk[_currentFrameNumber].RelativeTime;

                        if (timePast >= expectedTimeSpan)
                        {
                            _currentFrameNumber++;
                            _timeLastFrameRender = DateTime.Now;
                            _timeLastFrameRender += timePast - expectedTimeSpan;
                            slrFrameProgress.Value = _currentFrameNumber;
                            bodyViewer.RenderBodies(_framesFromDisk[_currentFrameNumber].TrackedBodies, ECameraType.Color);
                        }
                    }
                }
            }
        }

        private void resetPlayer_Click(object sender, RoutedEventArgs e)
        {
            //CompositionTarget.Rendering -= CompositionTarget_Rendering;
            //_playBackFrames = false;
            _currentFrameNumber = 0;
        }
        private void pausePlayer_Click(object sender, RoutedEventArgs e)
        {
            _playBackFrames = false;
        }

        private void navigateToRecordPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.NavigationService.GoBack();
        }
    }
}
