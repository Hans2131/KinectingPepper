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
    public class CSVWriter
    {
        private List<JointRow> jList;
        private int frameCounter;
        private BodyWrapper trackedBody;

        public CSVWriter()
        {
            frameCounter = 1;
        }

        public void SaveCSV(string filepath)
        {
            StreamWriter sw = new StreamWriter(filepath);
            var csv = new CsvWriter(sw);
            csv.WriteRecords(jList);
            csv.Dispose();
        }

        public void SaveSkeletonFrames(List<BodyFrameWrapper> skeletonFrames, string csvPath)
        {
            foreach (var frame in skeletonFrames)
            {
                SaveSkeletonFrame(frame);
            }
            SaveCSV(csvPath);
        }

        public void SaveSkeletonFrame(BodyFrameWrapper bfw)
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
                }

            }
            this.frameCounter++;
        }
    }
}
