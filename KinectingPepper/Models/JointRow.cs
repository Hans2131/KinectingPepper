using Kinect_ing_Pepper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_ing_Pepper.Business
{
    class JointRow
    {
        public int frameNum;
        public string jointName;
        public float x, y, z;
        public float orX, orY, orZ, orW;
        public TimeSpan time;

        public JointRow(JointWrapper jw, TimeSpan time, int frameNum)
        {
            this.frameNum = frameNum;
            this.x = jw.Position.X;
            this.y = jw.Position.Y;
            this.z = jw.Position.Z;
            this.jointName = jw.JointType.ToString();
            this.orW = jw.Orientation.W;
            this.orX = jw.Orientation.X;
            this.orY = jw.Orientation.Y;
            this.orZ = jw.Orientation.Z;
        }
    }
}
