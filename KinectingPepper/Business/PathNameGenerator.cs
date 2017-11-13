using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_ing_Pepper.Business
{
    class PathNameGenerator
    {
        private string _folderPathName = "";
        public string FolderPathName
        {
            get
            {
                if (_folderPathName == "")
                {
                    CreateFolder();
                }
                return _folderPathName;
            }

        }

        private string _fileNameBase = "";
        public string FileNameBase
        {
            get
            {
                return _folderPathName;
            }
        }

        private string _defaultPackage = "C:/images/Pepper/";

        public void CreateFolder()
        {
            DateTime dateTime = DateTime.Now;
            string dateTimeString = dateTime.ToString();
            dateTimeString = dateTimeString.Replace(":", "_");
            string path = _defaultPackage + dateTimeString;
            path = path.Replace(" ", "_");
            System.IO.Directory.CreateDirectory(path);
            _folderPathName = path;
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

        public void SetFileNameBase()
        {
            DateTime timeRecordingStart = DateTime.Now;
            _fileNameBase = timeRecordingStart.ToShortDateString() + " " + timeRecordingStart.ToLongTimeString().Replace(":", "_");
        }
    }
}
