using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Kinect;
using Kinect_ing_Pepper.Enums;
using Kinect_ing_Pepper.Business;
using System.IO;
using System.Xml.Serialization;
using Kinect_ing_Pepper.Models;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class RewindPage : Page
    {
        private ECameraType _selectedCamera = ECameraType.Color;
        private readonly Frame navigationFrame;

        public RewindPage(Frame navigationFrame)
        {
            InitializeComponent();
            this.navigationFrame = navigationFrame;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void cbxCameraType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Enum.TryParse<ECameraType>(cbxCameraType.SelectedValue.ToString(), out _selectedCamera);
        }

        private void selectFile_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void resetPlayer_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void pausePlayer_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void navigateToRecordPage_Click(object sender, RoutedEventArgs e)
        {
            navigationFrame.NavigationService.GoBack();
        }

        private void xmlDirToCSV(DirectoryInfo di)
        {
            foreach(var subDir in di.GetDirectories())
            {
                foreach(var file in subDir.GetFiles())
                {
                    if (file.Name.EndsWith(".xml"))
                    {
                        var csvName = file.FullName.Remove(file.FullName.Length - 3) + "csv";

                        var serializer = new XmlSerializer(typeof(List<BodyFrameWrapper>));
                        var reader = new StreamReader(file.FullName);
                        var bfws = (List<BodyFrameWrapper>)serializer.Deserialize(reader);
                        var csvSaver = new CSV_saver();
                        foreach(var bfw in bfws)
                        {
                            csvSaver.saveSkeletonFrame(bfw);
                        }
                        csvSaver.saveCSV(csvName);


                    }
                }
            }

        }

        private void csv_Click(object sender, RoutedEventArgs e)
        {
            foreach (var dr in System.Environment.GetLogicalDrives())
            {
                var di = new System.IO.DriveInfo(dr);
                if (di.IsReady)
                {
                    var rootDir = di.RootDirectory;


                    var subDirs = rootDir.GetDirectories();

                    foreach (var dirInfo in subDirs)
                    {

                        if (dirInfo.Name.Equals("xml_to_csv"))
                        {
                            xmlDirToCSV(dirInfo);

                        }
                    }
                }
            }

        }
    }
}
