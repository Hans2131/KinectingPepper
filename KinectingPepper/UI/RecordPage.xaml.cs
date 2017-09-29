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
    public partial class RecordPage : Page
    {
        private MultiSourceFrameReader _reader;

        private ECameraType _selectedCamera = ECameraType.Color;

        public RecordPage()
        {
            InitializeComponent();

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
                            skeletonViewer.UpdateFrameCounter();
                            skeletonViewer.KinectImage = frameParser.ParseToBitmap(colorFrame);
                        }
                    }
                    break;
                case ECameraType.Depth:
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
                case ECameraType.Infrared:
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

            using (BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    Body[] bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies);

                    List<Body> trackedBodies = bodies.Where(x => x.IsTracked).ToList();

                    //choose body?
                    if (trackedBodies.Any())
                    {
                        skeletonViewer.RenderBodies(trackedBodies, _selectedCamera);
                    }
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
            Enum.TryParse<ECameraType>(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }
    }
}
