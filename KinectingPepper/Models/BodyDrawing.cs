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
        private double _boneLineThickness = 5;
        //private Brush _jointBrush = new SolidColorBrush(Color.FromRgb(66, 134, 244));

        public ulong TrackingId { get; set; }

        public Dictionary<JointType, Point> JointPoints { get; set; }

        public Dictionary<JointType, Ellipse> JointEllipses { get; set; }

        public Dictionary<Tuple<JointType, JointType>, Line> BoneLines { get; set; }

        public BodyDrawing(BodyWrapper body, ECameraType cameraType)
        {
            TrackingId = body.TrackingId;

            JointPoints = new Dictionary<JointType, Point>();
            JointEllipses = new Dictionary<JointType, Ellipse>();
            BoneLines = new Dictionary<Tuple<JointType, JointType>, Line>();

            foreach (KeyValuePair<JointType, JointWrapper> joint in body.JointsDictionary)
            {
                Point point = BodyHelper.Instance.MapCameraToSpace(joint.Value.Position, cameraType);
                JointPoints.Add(joint.Key, point);

                Ellipse ellipse = new Ellipse
                {
                    Width = _jointRadius * 2,
                    Height = _jointRadius * 2,
                    Fill = Brushes.Blue,
                    StrokeThickness = _jointLineThickness,
                    Stroke = Brushes.DarkBlue
                };

                Canvas.SetLeft(ellipse, point.X - _jointRadius);
                Canvas.SetTop(ellipse, point.Y - _jointRadius);

                JointEllipses.Add(joint.Key, ellipse);
            }

            foreach (Tuple<JointType, JointType> bone in BodyHelper.Instance.BodyHierarchy)
            {
                Line line = new Line
                {
                    StrokeThickness = _boneLineThickness,
                    Stroke = Brushes.Green,
                    X1 = JointPoints[bone.Item1].X,
                    Y1 = JointPoints[bone.Item1].Y,
                    X2 = JointPoints[bone.Item2].X,
                    Y2 = JointPoints[bone.Item2].Y
                };

                BoneLines.Add(bone, line);
            }
        }

        public void Update(BodyWrapper body, ECameraType cameraType)
        {
            foreach (KeyValuePair<JointType, JointWrapper> joint in body.JointsDictionary)
            {
                JointPoints[joint.Key] = BodyHelper.Instance.MapCameraToSpace(joint.Value.Position, cameraType);

                Ellipse ellipse = JointEllipses[joint.Key];

                Canvas.SetLeft(ellipse, JointPoints[joint.Key].X - _jointRadius);
                Canvas.SetTop(ellipse, JointPoints[joint.Key].Y - _jointRadius);
            }

            foreach (Tuple<JointType, JointType> bone in BodyHelper.Instance.BodyHierarchy)
            {
                Line line = BoneLines[bone];
                line.X1 = JointPoints[bone.Item1].X;
                line.Y1 = JointPoints[bone.Item1].Y;
                line.X2 = JointPoints[bone.Item2].X;
                line.Y2 = JointPoints[bone.Item2].Y;
            }
        }
    }
}
