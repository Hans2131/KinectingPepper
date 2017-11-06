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
using Microsoft.Win32;

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
            if (_playBackFrames)
            {
                _playBackFrames = false;
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                _currentFrameNumber = 0;
                _timeLastFrameRender = DateTime.MinValue;
                _framesFromDisk = null;
            }

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = @"C:\images\Pepper\";
            openFileDialog1.Filter = "XML files (*.xml)|*.xml";

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    _framesFromDisk = PersistFrames.Instance.DeserializeFromXML(openFileDialog1.FileName);
                    _playBackFrames = true;

                    slrFrameProgress.Maximum = 0;
                    slrFrameProgress.Maximum = _framesFromDisk.Count - 1;

                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
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

                            txtFrameTime.Text = (_currentFrameNumber + 1).ToString();
                            slrFrameProgress.Value = _currentFrameNumber;

                            bodyViewer.RenderBodies(_framesFromDisk[_currentFrameNumber].TrackedBodies, ECameraType.Color);
                        }
                    }
                }
            }
        }

        private void startPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (!_playBackFrames)
            {
                _playBackFrames = true;
            }
        }
        private void pausePlayer_Click(object sender, RoutedEventArgs e)
        {
            StopPlaying();
        }

        private void navigateToRecordPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.NavigationService.GoBack();
        }

        private void slrFrameProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_framesFromDisk != null)
            {
                int currentValue = (int)slrFrameProgress.Value;
                if (currentValue != _currentFrameNumber)
                {
                    StopPlaying();
                    _currentFrameNumber = currentValue;

                    txtFrameTime.Text = (_currentFrameNumber + 1).ToString();
                    bodyViewer.RenderBodies(_framesFromDisk[_currentFrameNumber].TrackedBodies, ECameraType.Color);
                }
            }
        }

        private void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.D)
            {
                StopPlaying();

                if (_currentFrameNumber < _framesFromDisk.Count - 1)
                {
                    _currentFrameNumber++;
                }
            }

            if (e.Key == System.Windows.Input.Key.A)
            {
                StopPlaying();

                if (_currentFrameNumber > 0)
                {
                    _currentFrameNumber--;
                }
            }

            txtFrameTime.Text = (_currentFrameNumber + 1).ToString();
            slrFrameProgress.Value = _currentFrameNumber;
            bodyViewer.RenderBodies(_framesFromDisk[_currentFrameNumber].TrackedBodies, ECameraType.Color);
        }

        private void StopPlaying()
        {
            if (_playBackFrames)
            {
                _playBackFrames = false;
                _timeLastFrameRender = DateTime.MinValue;
            }
        }
    }
}
