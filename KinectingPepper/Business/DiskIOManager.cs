using Accord.Video.FFMPEG;
using Kinect_ing_Pepper.Models;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Kinect_ing_Pepper.Business
{
    public class DiskIOManager
    {
        private static DiskIOManager _instance;
        private VideoFileReader _videoFileReader;

        public static DiskIOManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DiskIOManager();

                return _instance;
            }
        }

        public void SerializeToXML(List<BodyFrameWrapper> bodyFrames, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<BodyFrameWrapper>));
            using (TextWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, bodyFrames);
            }
        }

        public List<BodyFrameWrapper> DeserializeFromXML(string filePath)
        {
            if (File.Exists(filePath))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<BodyFrameWrapper>)); //throws ['System.IO.FileNotFoundException' in mscorlib.dll], no idea if solvable
                TextReader reader = new StreamReader(filePath);
                List<BodyFrameWrapper> bodyFrames = (List<BodyFrameWrapper>)deserializer.Deserialize(reader); ;
                reader.Close();

                return bodyFrames;
            }
            return null;
        }

        public List<ImageSource> GetFramesFromVideo(string fileName)
        {
            VideoFileReader reader = new VideoFileReader();
            reader.Open(fileName);

            FrameParser parser = new FrameParser();
            List<ImageSource> videoFrames = new List<ImageSource>();

            for (int i = 0; i < reader.FrameCount; i++)
            {
                Bitmap videoFrame = reader.ReadVideoFrame();
                ImageSource imageSource = parser.ImageSourceForBitmap(videoFrame);
                imageSource.Freeze();
                videoFrames.Add(imageSource);
                videoFrame.Dispose();
            }

            reader.Close();
            reader.Dispose();

            return videoFrames;
        }

        public void InitVideoReader(string fileName)
        {
            if (_videoFileReader != null && _videoFileReader.IsOpen)
            {
                CloseVideoReader();
            }
            _videoFileReader = new VideoFileReader();
            _videoFileReader.Open(fileName);
        }

        public ImageSource GetFrameByIndex(int index)
        {
            if (index < _videoFileReader.FrameCount && _videoFileReader.IsOpen)
            {
                Bitmap videoFrame = _videoFileReader.ReadVideoFrame(index);
                FrameParser parser = new FrameParser();
                ImageSource imageSource = parser.ImageSourceForBitmap(videoFrame);
                imageSource.Freeze();
                return imageSource;
            }
            else
            {
                return null;
            }
        }

        public void CloseVideoReader()
        {
            _videoFileReader.Close();
            _videoFileReader.Dispose();
        }
    }
}
