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
        private List<JointRow> jList;
        private int frameCounter;
        private BodyWrapper trackedBody;
        public CSV_saver()
        {
            frameCounter = 1;
        }

        public void saveCSV()
        {
            StreamWriter sw = new StreamWriter("joints_csv.csv");
            var csv = new CsvWriter(sw);
            csv.WriteRecords(jList);
            csv.Dispose();
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
                foreach (var jw in bw.Joints)
                {
                    var jr = new JointRow(jw, bfw.RelativeTimeString, this.frameCounter, bw.TrackingId, jw.TrackingState);
                    jList.Add(jr);
                    if(frameCounter == 99)
                    {
                        Console.WriteLine(jr.ToString());
                    }
                }

            }
            this.frameCounter++;

        }
    }
}
