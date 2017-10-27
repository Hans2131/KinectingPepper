using Kinect_ing_Pepper.Enums;
//using Kinect_ing_Pepper.Models;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

/*namespace Kinect_ing_Pepper.Business
{
    public class BodyHelper
    {
        private static BodyHelper _instance;

        public static BodyHelper Instance
        {
            get
            {
                if (_instance == null) _instance = new BodyHelper();
                return _instance;
            }
        }

        private List<Tuple<JointType, JointType>> _bodyHierarchy;

        public List<Tuple<JointType, JointType>> BodyHierarchy
        {
            get
            {
                if (_bodyHierarchy == null) _bodyHierarchy = CreateBodyHierarchy();
                return _bodyHierarchy;
            }
        }

        public List<BodyDrawing> BodyDrawings { get; } = new List<BodyDrawing>();

        private List<Tuple<JointType, JointType>> CreateBodyHierarchy()
        {
            List<Tuple<JointType, JointType>> boneList = new List<Tuple<JointType, JointType>>();

            //Skeleton base
            boneList.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.SpineMid));

            //Right leg
            boneList.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            //Left leg
            boneList.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            //Spine
            boneList.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineShoulder));
            boneList.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.Neck));

            //Head
            boneList.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.Head));

            //Right arm
            boneList.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));
            boneList.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));

            //Left arm
            boneList.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));
            boneList.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));

            return boneList;
        }

        public Point MapCameraToSpace(Vector3 position, ECameraType cameraType)
        {
            CameraSpacePoint jointPosition = new CameraSpacePoint();
            jointPosition.X = position.X;
            jointPosition.Y = position.Y;
            jointPosition.Z = position.Z;

            switch (cameraType)
            {
                case ECameraType.Color:
                    ColorSpacePoint colorPoint = KinectHelper.Instance.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);
                    return new Point(float.IsInfinity(colorPoint.X) ? 0.0 : colorPoint.X, float.IsInfinity(colorPoint.X) ? 0.0 : colorPoint.Y);
                case ECameraType.Depth:
                    DepthSpacePoint depthPoint = KinectHelper.Instance.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);
                    return new Point(float.IsInfinity(depthPoint.X) ? 0.0 : depthPoint.X, float.IsInfinity(depthPoint.X) ? 0.0 : depthPoint.Y);
                case ECameraType.Infrared:
                    DepthSpacePoint depthPoint1 = KinectHelper.Instance.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);
                    return new Point(float.IsInfinity(depthPoint1.X) ? 0.0 : depthPoint1.X, float.IsInfinity(depthPoint1.X) ? 0.0 : depthPoint1.Y);
                default:
                    return new Point();
            }
        }
    }
}
*/