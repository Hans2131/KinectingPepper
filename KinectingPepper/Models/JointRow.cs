using Kinect_ing_Pepper.Models;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_ing_Pepper.Models
{
    public class JointRow
    {
        public int frameNum { get; set; }
        public string jointName { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float orX { get; set; }
        public float orY { get; set; }
        public float orZ { get; set; }
        public float orW { get; set; }
        public string time { get; set; }
        public ulong trackingId { get; set; }
        public TrackingState trackState { get; set; }

        public JointRow(JointWrapper jw, string time, int frameNum, ulong trackingId, TrackingState state )
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
            this.time = time;
            this.trackingId = trackingId;
            this.trackState = state;
        }
        private PropertyInfo[] _PropertyInfos = null;

        public override string ToString()
        {
            if (_PropertyInfos == null)
                _PropertyInfos = this.GetType().GetProperties();

            var sb = new StringBuilder();

            foreach (var info in _PropertyInfos)
            {
                var value = info.GetValue(this, null) ?? "(null)";
                sb.AppendLine(info.Name + ": " + value.ToString());
            }

            return sb.ToString();
        }
    }
}
