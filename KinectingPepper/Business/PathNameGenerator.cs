﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_ing_Pepper.Business
{
    class PathNameGenerator
    {
        public string FolderPathName { get; set; }
        private string _defaultPackage = "C:/images/Pepper/";

        public void CreateFolder()
        {
            DateTime dateTime = DateTime.Now;
            string dateTimeString = dateTime.ToString();
            dateTimeString = dateTimeString.Replace(":", "_");
            string path = _defaultPackage + dateTimeString;
            path = path.Replace(" ", "_");
            System.IO.Directory.CreateDirectory(path);
            this.FolderPathName = path; 
        }

        private string CreateFileName(string cameraType)
        {
            DateTime dateTime = DateTime.Now;
            string dateTimeString = dateTime.ToString().Replace(":", "_");
            return cameraType + "_" + dateTimeString + ".mpeg";
        }
        
        public string CreateFilePathName(string cameraType)
        {
            return FolderPathName + "/" + CreateFileName(cameraType);
        }
    }
}
