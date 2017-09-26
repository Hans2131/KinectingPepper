using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using Kinect_ing_Pepper.Enums;
using System.Diagnostics;
using Kinect_ing_Pepper.Business;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class SkeletonPage : Page
    {
        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;

        private ECameraTypes _selectedCamera = ECameraTypes.Color;

        public SkeletonPage()
        {
            InitializeComponent();

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }

            cbxCameraType.ItemsSource = Enum.GetValues(typeof(ECameraTypes)).Cast<ECameraTypes>();
            cbxCameraType.SelectedIndex = 0;
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            FrameParser frameParser = new FrameParser();
            MultiSourceFrame frame = e.FrameReference.AcquireFrame();

            switch (_selectedCamera)
            {
                case ECameraTypes.Color:
                    // Color
                    using (ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame())
                    {
                        if (colorFrame != null)
                        {
                            skeletonViewer.UpdateFrameCounter();
                            skeletonViewer.KinectImage = frameParser.ParseToBitmap(colorFrame);
                        }
                    }
                    break;
                case ECameraTypes.Depth:
                    // Depth
                    using (DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame())
                    {
                        if (depthFrame != null)
                        {
                            skeletonViewer.UpdateFrameCounter();
                            skeletonViewer.KinectImage = frameParser.ParseToBitmap(depthFrame);
                        }
                    }
                    break;
                case ECameraTypes.Infrared:
                    using (InfraredFrame infraredFrame = frame.InfraredFrameReference.AcquireFrame())
                    {
                        if (infraredFrame != null)
                        {
                            skeletonViewer.UpdateFrameCounter();
                            skeletonViewer.KinectImage = frameParser.ParseToBitmap(infraredFrame);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _reader.Dispose();

            _sensor.Close();
        }

        private void cbxCameraType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Enum.TryParse<ECameraTypes>(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }
    }
}
