using Kinect_ing_Pepper.Business;
using Kinect_ing_Pepper.Enums;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Kinect_ing_Pepper.Models
{
    public class BodyDrawing
    {
        private double _jointRadius = 10;
        private double _jointLineThickness = 5;
        private Brush _ellipseBrush = Brushes.DarkBlue;
        private Brush _ellipseFillBrush = Brushes.Blue;

        private double _boneLineThickness = 5;
        private Brush _boneBrush = Brushes.Green;

        private Brush _inferredBrush = Brushes.LawnGreen;
        private double _inferredThickness = 1;

        public ulong TrackingId { get; set; }

        //public Dictionary<JointType, Point> JointPoints { get; set; }

        public Dictionary<JointType, Ellipse> JointEllipses { get; set; }

        public Dictionary<Tuple<JointType, JointType>, Line> BoneLines { get; set; }

        public BodyDrawing(BodyWrapper body, ECameraType cameraType)
        {
            TrackingId = body.TrackingId;
            
            JointEllipses = new Dictionary<JointType, Ellipse>();
            BoneLines = new Dictionary<Tuple<JointType, JointType>, Line>();

            foreach (KeyValuePair<JointType, JointWrapper> joint in body.JointsDictionary)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = _jointRadius * 2,
                    Height = _jointRadius * 2,
                    Fill = _ellipseFillBrush,
                    StrokeThickness = _jointLineThickness,
                    Stroke = _ellipseBrush
                };

                Canvas.SetLeft(ellipse, joint.Value.GetCameraPoint(cameraType).X - _jointRadius);
                Canvas.SetTop(ellipse, joint.Value.GetCameraPoint(cameraType).Y - _jointRadius);

                JointEllipses.Add(joint.Key, ellipse);
            }

            foreach (Tuple<JointType, JointType> bone in BodyHelper.Instance.BodyHierarchy)
            {
                Line line = new Line
                {
                    StrokeThickness = _boneLineThickness,
                    Stroke = _boneBrush,
                    X1 = body.JointsDictionary[bone.Item1].GetCameraPoint(cameraType).X,
                    Y1 = body.JointsDictionary[bone.Item1].GetCameraPoint(cameraType).Y,
                    X2 = body.JointsDictionary[bone.Item2].GetCameraPoint(cameraType).X,
                    Y2 = body.JointsDictionary[bone.Item2].GetCameraPoint(cameraType).Y
                };

                BoneLines.Add(bone, line);
            }
        }

        //Bones and joints are hidden when inferred and nottracked to avoid spacing of line of the screen
        public void Update(BodyWrapper body, ECameraType cameraType)
        {
            foreach (KeyValuePair<JointType, JointWrapper> joint in body.JointsDictionary)
            {               
                Ellipse ellipse = JointEllipses[joint.Key];

                Canvas.SetLeft(ellipse, joint.Value.GetCameraPoint(cameraType).X - _jointRadius);
                Canvas.SetTop(ellipse, joint.Value.GetCameraPoint(cameraType).Y - _jointRadius);

                if (joint.Value.TrackingState == TrackingState.Inferred)
                {
                    //ellipse.Stroke = _inferredBrush;
                    //ellipse.Fill = _inferredBrush;
                    //ellipse.StrokeThickness = _inferredThickness;
                    ellipse.Visibility = Visibility.Hidden;
                }
                else if (joint.Value.TrackingState == TrackingState.NotTracked)
                {
                    ellipse.Visibility = Visibility.Hidden;
                }
                else
                {
                    //ellipse.Stroke = _ellipseFillBrush;
                    //ellipse.StrokeThickness = _boneLineThickness;
                    //ellipse.Fill = _ellipseFillBrush;
                    ellipse.Visibility = Visibility.Visible;
                }
            }

            foreach (Tuple<JointType, JointType> bone in BodyHelper.Instance.BodyHierarchy)
            {
                Line line = BoneLines[bone];
                line.X1 = body.JointsDictionary[bone.Item1].GetCameraPoint(cameraType).X;
                line.Y1 = body.JointsDictionary[bone.Item1].GetCameraPoint(cameraType).Y;
                line.X2 = body.JointsDictionary[bone.Item2].GetCameraPoint(cameraType).X;
                line.Y2 = body.JointsDictionary[bone.Item2].GetCameraPoint(cameraType).Y;

                if (body.JointsDictionary[bone.Item1].TrackingState == TrackingState.Inferred || body.JointsDictionary[bone.Item2].TrackingState == TrackingState.Inferred)
                {
                    //line.Stroke = _inferredBrush;
                    //line.StrokeThickness = _inferredThickness;
                    line.Visibility = Visibility.Visible;
                }
                else if (body.JointsDictionary[bone.Item1].TrackingState == TrackingState.NotTracked || body.JointsDictionary[bone.Item2].TrackingState == TrackingState.NotTracked)
                {
                    line.Visibility = Visibility.Hidden;
                }
                else
                {
                    //line.Stroke = _boneBrush;
                    //line.StrokeThickness = _boneLineThickness;
                    line.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
