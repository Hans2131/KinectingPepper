using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_ing_Pepper.Business
{
    public class CSV_saver
    {
        private DateTime startTime { get; set; }
        private List<Tuple<Joint, TimeSpan, int>> jointList { get; set; }
        private int frameCounter;
        public CSV_saver()
        {
            frameCounter = 1;
        }

        public void saveCSV()
        {
        }

        public void saveSkeletonFrame(Body body, DateTime datetime)
        {
            if (jointList == null)
            {
                jointList = new LinkedList<Tuple<Joint, TimeSpan>>;
                startTime = datetime;
            }
            foreach (var joint in body.Joints)
            {
                var relativeTime = datetime - startTime;
                Tuple tuple = new Tuple<Joint, TimeSpan>(joint, relativeTime, this.frameCounter);
                jointList.Add(tuple);
            }
            frameCounter++;
        }
    }
}
