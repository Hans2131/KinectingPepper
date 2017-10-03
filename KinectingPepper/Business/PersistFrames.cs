using Kinect_ing_Pepper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kinect_ing_Pepper.Business
{
    public class PersistFrames
    {
        private static PersistFrames _instance;

        public static PersistFrames Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PersistFrames();

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
                XmlSerializer deserializer = new XmlSerializer(typeof(List<BodyFrameWrapper>));
                TextReader reader = new StreamReader(filePath);
                List<BodyFrameWrapper> bodyFrames = (List<BodyFrameWrapper>)deserializer.Deserialize(reader); ;
                reader.Close();

                return bodyFrames;
            }
            return null;
        }
    }
}
