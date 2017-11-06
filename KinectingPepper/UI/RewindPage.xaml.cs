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
using System.IO;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class RewindPage : Page
    {
        private ECameraType _selectedCamera = ECameraType.Depth;
        private readonly Frame navigationFrame;

        private List<BodyFrameWrapper> _skeletonFrames;
        private List<ImageSource> _videoFrames;

        private bool _playBackFrames = false;
        private int _currentFrameNumber = 0;
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
                _skeletonFrames = null;
            }

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = @"C:\images\Pepper\";
            openFileDialog1.Filter = "XML files (*.xml)|*.xml";

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    string fullXMLPath = openFileDialog1.FileName;
                    //string videoFileName = "Depth_" + Path.GetFileName(fullPath).Replace(".xml", ".mp4");
                    //string[] splitted = videoFileName.Split(new char[] { ' ' });
                    //videoFileName = splitted[0] + " " + splitted[1] + "_" + splitted[2] + "_" + splitted[3];
                    //string fullVideoUri = Path.Combine(Path.GetDirectoryName(fullPath), videoFileName);
                    openFileDialog1.FileName = "";
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(fullXMLPath);
                    openFileDialog1.Filter = "MP4 files (*.mp4)|*.mp4";

                    if (openFileDialog1.ShowDialog() == true)
                    {
                        string fullVideoUri = openFileDialog1.FileName;
                        _videoFrames = IOKinectData.Instance.GetFramesFromVideo(fullVideoUri);
                    }

                    _skeletonFrames = IOKinectData.Instance.DeserializeFromXML(fullXMLPath);
                    _playBackFrames = true;

                    slrFrameProgress.Maximum = 0;
                    slrFrameProgress.Maximum = _skeletonFrames.Count - 1;

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
                if (_currentFrameNumber == _skeletonFrames.Count - 1)
                {
                    _currentFrameNumber = 0;
                    _timeLastFrameRender = DateTime.MinValue;
                }
                else
                {
                    if (_timeLastFrameRender == DateTime.MinValue)
                    {
                        _timeLastFrameRender = DateTime.Now;
                        UpdateImaging();
                    }
                    else
                    {
                        TimeSpan timePast = DateTime.Now - _timeLastFrameRender;
                        TimeSpan expectedTimeSpan = _skeletonFrames[_currentFrameNumber + 1].RelativeTime - _skeletonFrames[_currentFrameNumber].RelativeTime;

                        if (timePast >= expectedTimeSpan)
                        {
                            _currentFrameNumber++;
                            _timeLastFrameRender = DateTime.Now;
                            _timeLastFrameRender += timePast - expectedTimeSpan;

                            UpdateImaging();
                        }
                    }
                }
            }
        }

        private void startPlayer_Click(object sender, RoutedEventArgs e)
        {
            _playBackFrames = true;
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
            if (_skeletonFrames != null)
            {
                int currentValue = (int)slrFrameProgress.Value;
                if (currentValue != _currentFrameNumber)
                {
                    StopPlaying();

                    _currentFrameNumber = currentValue;

                    UpdateImaging();
                }
            }
        }

        private void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.D)
            {
                StopPlaying();
                if (_currentFrameNumber < _skeletonFrames.Count - 1)
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
            UpdateImaging();
        }

        private void StopPlaying()
        {
            if (_playBackFrames)
            {
                _playBackFrames = false;
                _timeLastFrameRender = DateTime.MinValue;
            }
        }

        private void UpdateImaging()
        {
            txtFrameTime.Text = (_currentFrameNumber + 1).ToString();
            slrFrameProgress.Value = _currentFrameNumber;

            bodyViewer.KinectImage = _videoFrames[_currentFrameNumber];
            bodyViewer.RenderBodies(_skeletonFrames[_currentFrameNumber].TrackedBodies, _selectedCamera);
        }
    }
}
