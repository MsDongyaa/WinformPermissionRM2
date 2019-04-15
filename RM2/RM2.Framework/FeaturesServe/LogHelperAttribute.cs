using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace FrameWork.FeaturesServe
{
    public  class LogHelperAttribute : Attribute
    {
        public  string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public  void Write(string logString)
        {
            if (!Directory.Exists(LogPath))
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(LogPath);
            }

            string sFileName = Path.Combine(LogPath, "Log_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt");
            using (StreamWriter sw = File.AppendText(sFileName))
            {
                string sData = DateTime.Now.ToString("HH:mm:ss ") + logString + "\r\n";
                byte[] bytes = Encoding.Default.GetBytes(sData);
                sw.BaseStream.Write(bytes, 0, bytes.Length);
                sw.Flush();
            }
        }

    }
}
