using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinect_ing_Pepper.Models;
using System.IO;
using CsvHelper;

namespace Kinect_ing_Pepper.Business
{
    public class CSV_saver
    {
        private DateTime startTime;
        private List<JointRow> jList;
        private int frameCounter;
        private BodyWrapper trackedBody;
        public CSV_saver()
        {
            frameCounter = 1;
        }

        public void saveCSV()
        {
            TextWriter tw = new StreamWriter("joints_csv_" + DateTime.Now.ToString() + ".csv");
            var csv = new CsvWriter(tw);
            csv.WriteRecords(jList);
        }

        public void saveSkeletonFrame(BodyFrameWrapper bfw)
        {
            if (jList == null)
            {
                jList = new List<JointRow>();
                trackedBody = bfw.TrackedBodies[0];
            }

            foreach (var bw in bfw.TrackedBodies)
            {
                if(bw.TrackingId == trackedBody.TrackingId)
                {
                    foreach (var jw in bw.Joints)
                    {
                        jList.Add(new JointRow(jw, bfw.RelativeTime, this.frameCounter));
                    }
                }

            }
            this.frameCounter++;

            }
        }
    }
}
