using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Kinect_ing_Pepper.Business;
using System.Windows;
using Kinect_ing_Pepper.Enums;

namespace Kinect_ing_Pepper.Models
{
    public class JointWrapper
    {
        public JointType JointType;
        public Vector3 Position;
        public Point PointInColorSpace;
        public Point PointInDepthSpace;
        public System.Numerics.Vector4 Orientation;
        public TrackingState TrackingState;

        public JointWrapper() { }

        public JointWrapper(Joint joint)
        {
            JointType = joint.JointType;
            Position = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
            PointInColorSpace = BodyHelper.Instance.MapCameraToSpace(Position, Enums.ECameraType.Color);
            PointInDepthSpace = BodyHelper.Instance.MapCameraToSpace(Position, Enums.ECameraType.Depth);
            TrackingState = joint.TrackingState;
        }

        public Point GetCameraPoint(ECameraType cameraType)
        {
            switch (cameraType)
            {
                case ECameraType.Color:
                    return PointInColorSpace;
                case ECameraType.Depth:
                    return PointInDepthSpace;
                case ECameraType.Infrared:
                    return PointInDepthSpace;
                default:
                    return PointInColorSpace;
            }
        }
    }
}
