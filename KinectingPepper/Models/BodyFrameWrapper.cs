using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Kinect_ing_Pepper.Models
{
    public class BodyFrameWrapper
    {
        public System.Numerics.Vector4 FloorClipPlane;
        public TimeSpan RelativeTime;
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
