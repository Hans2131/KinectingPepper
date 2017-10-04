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
using Kinect_ing_Pepper.Business;
using Kinect_ing_Pepper.Models;
using System.Diagnostics;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonView.xaml
    /// </summary>
    public partial class BodyViewer : UserControl
    {
        private int _frameCounter = 0;
        private DateTime _lastFPSSample = DateTime.MinValue;
        private bool _canvasCleared = false;

        #region Dependancy Properties

        public ImageSource KinectImage
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("KinectImage", typeof(ImageSource), typeof(BodyViewer), new FrameworkPropertyMetadata(null));

        public string FPSDescription
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register("FPSDescription", typeof(string), typeof(BodyViewer), new FrameworkPropertyMetadata(null));

        #endregion

        #region Constructor

        public BodyViewer()
        {
            InitializeComponent();

            DataContext = this;

            FPSDescription = "Not counted!";
            _lastFPSSample = DateTime.Now;
        }
        #endregion

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

        public void RenderBodies(List<BodyWrapper> trackedBodies, ECameraType cameraType)
        {
            DeleteUntrackedBodies(trackedBodies);

            foreach (BodyWrapper body in trackedBodies)
            {
                RenderBody(body, cameraType);
            }
        }

        public void DeleteUntrackedBodies(List<BodyWrapper> trackedBodies)
        {
            if (trackedBodies == null)
            {
                BodyHelper.Instance.BodyDrawings.Clear();
                canvasSkeleton.Children.Clear();
                _canvasCleared = true;
            }
            else
            {
                int deletedCount = BodyHelper.Instance.BodyDrawings.RemoveAll(x => !trackedBodies.Any(tracked => tracked.TrackingId == x.TrackingId));

                if (deletedCount > 0)
                {
                    canvasSkeleton.Children.Clear();
                    _canvasCleared = true;
                }
            }
        }

        public void Clear()
        {
            canvasSkeleton.Children.Clear();
            KinectImage = null;
        }

        private void RenderBody(BodyWrapper body, ECameraType cameraType)
        {
            BodyDrawing bodyDrawing = BodyHelper.Instance.BodyDrawings.Where(x => x.TrackingId == body.TrackingId).FirstOrDefault();
            bool isNewBody = false;

            if (bodyDrawing == null)
            {
                bodyDrawing = new BodyDrawing(body, cameraType);
                BodyHelper.Instance.BodyDrawings.Add(bodyDrawing);
                isNewBody = true;
            }
            else
            {
                bodyDrawing.Update(body, cameraType);
            }

            if (_canvasCleared || isNewBody)
            {
                List<Ellipse> ellipses = bodyDrawing.JointEllipses.Select(x => x.Value).ToList();
                foreach (Ellipse ellipse in ellipses)
                {
                    canvasSkeleton.Children.Add(ellipse);
                }

                List<Line> lines = bodyDrawing.BoneLines.Select(x => x.Value).ToList();
                foreach (Line line in lines)
                {
                    canvasSkeleton.Children.Add(line);
                }

                _canvasCleared = false;
            }
        }
    }
}