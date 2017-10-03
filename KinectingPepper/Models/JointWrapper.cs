using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Kinect_ing_Pepper.Models
{
    public class JointWrapper
    {
        public JointType JointType;
        public Vector3 Position;
        public System.Numerics.Vector4 Orientation;
        public TrackingState TrackingState;

        public JointWrapper() { }

        public JointWrapper(Joint joint)
        {
            JointType = joint.JointType;
            Position = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
            TrackingState = joint.TrackingState;
        }
    }
}
