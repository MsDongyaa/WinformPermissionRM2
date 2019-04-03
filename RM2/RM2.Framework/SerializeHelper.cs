using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace FrameWork
{
    public class SerializeHelper
    {
        public static string XmlSerialize<T>(T t)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer xmlFormat = new XmlSerializer(typeof(T));
                xmlFormat.Serialize(stream, t);
                stream.Position = 0;
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                string result = sr.ReadToEnd();
                sr.Close();
                return result;
            }
        }

        public static T XmlDeserialize<T>(string xmlString)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(xmlString)))
            {
                XmlSerializer xmlFormat = new XmlSerializer(typeof(T));
                stream.Position = 0;
                T t = (T)xmlFormat.Deserialize(stream);
                return t; 
            }
        }

        public static string JsonSerialize<T>(T t) 
        {
            return JsonConvert.SerializeObject(t);
        }

        public static T JsonDeserialize<T>(string data) 
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
