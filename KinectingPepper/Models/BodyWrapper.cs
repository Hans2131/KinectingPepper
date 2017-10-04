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
    [Serializable()]
    public class BodyWrapper
    {

        public bool IsRestricted;
        public bool IsTracked;
        public ulong TrackingId;

        //XML serializer can't serialize Dictionary, this property returns the dictionary as list
        public List<JointWrapper> Joints;

        private Dictionary<JointType, JointWrapper> _jointsDictionary;
        [XmlIgnore]
        public Dictionary<JointType, JointWrapper> JointsDictionary
        {
            get
            {
                if (Joints != null && Joints.Any() && _jointsDictionary == null)
                {
                    _jointsDictionary = Joints.ToDictionary(x => x.JointType);
                }

                return _jointsDictionary;
            }
        }

        public Point Lean;
        public TrackingState LeanTrackingState;

        public HandState HandRightState;
        public TrackingConfidence HandLeftConfidence;
        public HandState HandLeftState;
        public TrackingConfidence HandRightConfidence;

        //include these?
        //public FrameEdges ClippedEdges;
        //public IReadOnlyDictionary<Expression, DetectionResult> Expressions;

        //Don't remove empty constructor, neccesary for serialization
        public BodyWrapper() { }

        public BodyWrapper(Body body)
        {
            Lean = new Point(body.Lean.X, body.Lean.Y);
            IsRestricted = body.IsRestricted;
            IsTracked = body.IsTracked;
            TrackingId = body.TrackingId;

            Joints = body.Joints.Select(x => new JointWrapper(x.Value)).ToList();
            Joints.ForEach(x => x.Orientation = ToVector4(body.JointOrientations[x.JointType]));
            //_jointsDictionary = body.Joints.Select(x => new JointWrapper(x.Value)).ToDictionary(x => x.JointType);
            //_jointsDictionary.ToList().ForEach(x => x.Value.Orientation = ToVector4(body.JointOrientations[x.Key]));

            System.Numerics.Vector4 ToVector4(JointOrientation jointOrientation)
            {
                return new System.Numerics.Vector4(jointOrientation.Orientation.X, jointOrientation.Orientation.Y,
                                                    jointOrientation.Orientation.Z, jointOrientation.Orientation.W);
            }
        }
    }
}
