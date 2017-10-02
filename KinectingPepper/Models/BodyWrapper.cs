using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using System.Numerics;

namespace Kinect_ing_Pepper.Models
{
    public class BodyWrapper
    {

        public bool IsRestricted;
        public bool IsTracked;
        public ulong TrackingId;

        public Dictionary<JointType, JointWrapper> Joints;

        public Point Lean;
        public TrackingState LeanTrackingState { get; }

        public HandState HandRightState { get; }
        public TrackingConfidence HandLeftConfidence { get; }
        public HandState HandLeftState { get; }
        public TrackingConfidence HandRightConfidence { get; }

        //include these?
        //public FrameEdges ClippedEdges { get; }
        //public IReadOnlyDictionary<Expression, DetectionResult> Expressions { get; }

        //Don't remove empty constructor, neccesary for serialization
        public BodyWrapper() { }

        public BodyWrapper(Body body)
        {
            Lean = new Point(body.Lean.X, body.Lean.Y);
            IsRestricted = body.IsRestricted;
            IsTracked = body.IsTracked;
            TrackingId = body.TrackingId;
            Joints = body.Joints.Select(x => new JointWrapper(x.Value)).ToDictionary(x => x.JointType);

            Joints.ToList().ForEach(x => x.Value.Orientation = ToVector4(body.JointOrientations[x.Key]));

            System.Numerics.Vector4 ToVector4(JointOrientation jointOrientation)
            {
                return new System.Numerics.Vector4(jointOrientation.Orientation.X, jointOrientation.Orientation.Y,
                                                    jointOrientation.Orientation.Z, jointOrientation.Orientation.W);
            }
        }

    }
}
