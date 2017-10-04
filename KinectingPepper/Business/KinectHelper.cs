using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_ing_Pepper.Business
{
    public class KinectHelper
    {
        private static KinectHelper _instance;

        public static KinectHelper Instance
        {
            get
            {
                if (_instance == null) _instance = new KinectHelper();
                return _instance;
            }
            set { _instance = value; }
        }

        private KinectSensor _kinectSensor;

        public KinectSensor KinectSensor
        {
            get
            {
                if(_kinectSensor == null) _kinectSensor = KinectSensor.GetDefault();
                return _kinectSensor;
            }
        }

        private CoordinateMapper _coordinateMapper;

        public CoordinateMapper CoordinateMapper
        {
            get
            {
                //if (_coordinateMapper == null && KinectSensor != null) _coordinateMapper = KinectSensor.CoordinateMapper;
                KinectSensor.CoordinateMapper

                return _coordinateMapper;
            }
        }


        public bool TryStartKinect()
        {
            if (KinectSensor != null)
            {
                KinectSensor.Open();
            }

            return HasStarted();
        }

        public bool HasStarted()
        {
            return _kinectSensor != null && _kinectSensor.IsOpen;
        }

        public void StopKinect()
        {
            if (_kinectSensor != null) _kinectSensor.Close();
        }
    }
}
