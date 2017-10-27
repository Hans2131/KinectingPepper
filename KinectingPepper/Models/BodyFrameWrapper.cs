using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Xml.Serialization;

namespace Kinect_ing_Pepper.Models
{
    public class BodyFrameWrapper
    {
        public System.Numerics.Vector4 FloorClipPlane;

        private TimeSpan _relativeTime;
        [XmlIgnore]
        public TimeSpan RelativeTime
        {
            get { return _relativeTime; }
            set { _relativeTime = value; }
        }

        public long RelativeTimeTicks
        {
            get { return _relativeTime.Ticks; }
            set { _relativeTime = new TimeSpan(value); }
        }

        private string _relativeTimeString;
        public string RelativeTimeString
        {
            get
            {
                _relativeTimeString = RelativeTime.ToString();
                return _relativeTimeString;
            }
            set { _relativeTimeString = value; }
        }

        public List<BodyWrapper> TrackedBodies;

        public BodyFrameWrapper() { }

        public BodyFrameWrapper(BodyFrame bodyFrame)
        {
            FloorClipPlane = new System.Numerics.Vector4(bodyFrame.FloorClipPlane.X, bodyFrame.FloorClipPlane.Y, bodyFrame.FloorClipPlane.Z, bodyFrame.FloorClipPlane.W);
            RelativeTime = bodyFrame.RelativeTime;

            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);
            TrackedBodies = bodies.Where(x => x.IsTracked).Select(x => new BodyWrapper(x)).ToList();
        }
    }
}
