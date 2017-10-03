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
        //System.Diagnostics.Stopwatch z = System.Diagnostics.Stopwatch.StartNew();
        
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
                    Body[] bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies);

                    List<Body> trackedBodies = bodies.Where(x => x.IsTracked).ToList();

                    //choose body?
                    if (trackedBodies.Any())
                    {
                        bodyViewer.RenderBodies(trackedBodies, _selectedCamera);
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
            Enum.TryParse<ECameraType>(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }

        private void startRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbxCameraType.SelectedIndex == 0)
            {
                MediaSink.RGBMediaSink.Start();
                cbxCameraType.IsEnabled = false;
            }
            if (cbxCameraType.SelectedIndex == 1)
            {
                MediaSink.DepthMediaSink.Start();
                cbxCameraType.IsEnabled = false;
            }
                                    
        }

        private void stopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            cbxCameraType.IsEnabled = true;
            MediaSink.RGBMediaSink.Stop();
            MediaSink.DepthMediaSink.Stop();
        }
    }
}
