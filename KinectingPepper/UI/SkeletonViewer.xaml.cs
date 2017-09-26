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

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonView.xaml
    /// </summary>
    public partial class SkeletonViewer : UserControl
    {
        private int _frameCounter = 0;
        private DateTime _lastFPSSample = DateTime.MinValue;

        #region Dependancy Properties

        public ImageSource KinectImage
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("KinectImage", typeof(ImageSource), typeof(SkeletonViewer), new FrameworkPropertyMetadata(null));

        public string FPSDescription
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register("FPSDescription", typeof(string), typeof(SkeletonViewer), new FrameworkPropertyMetadata(null));

        #endregion

        #region Constructor

        public SkeletonViewer()
        {
            InitializeComponent();

            DataContext = this;

            FPSDescription = "Not counted!";
            _lastFPSSample = DateTime.Now;
        }

        public void UpdateFrameCounter()
        {
            TimeSpan currentTimeSpan = DateTime.Now - _lastFPSSample;

            if (currentTimeSpan.TotalMilliseconds > 1000)
            {
                FPSDescription = "FPS = " + _frameCounter.ToString();
                _frameCounter = 0;
                _lastFPSSample = DateTime.Now;
            }
            else
            {
                _frameCounter++;
            }
        }

        #endregion
    }
}
