using System;
using System.IO;
using System.Xml.Serialization;

namespace AutoUpdaterWPFedition
{
    public static class Settings
    {
        private static SettingsFields _fields = new SettingsFields();
        private static readonly string _xmlName = "UpdateSettings.xml";

        public static bool UpdeteIsEnable
        {
            get
            {
                ReadXml();
                return _fields.UpdeteIsEnable;
            }
            set
            {
                _fields.UpdeteIsEnable = value;
                WriteXml();
            }
        }
        public static DateTime RemindLater
        {
            get
            {
                ReadXml();
                return _fields.RemindLater;
            }
            set
            {
                _fields.RemindLater = value;
                WriteXml();
            }
        }
        public static Version SkipVersion
        {
            get
            {
                ReadXml();
                return new Version(_fields.SkipVersion);
            }
            set
            {
                _fields.SkipVersion = value.ToString();
                WriteXml();
            }
        }

        private static void WriteXml()
        {
            XmlSerializer ser = new XmlSerializer(typeof(SettingsFields));
            TextWriter writer = new StreamWriter(_xmlName);
            ser.Serialize(writer, _fields);
            writer.Close();
        }
        private static void ReadXml()
        {
            if (File.Exists(_xmlName))
            {
                XmlSerializer ser = new XmlSerializer(typeof(SettingsFields));
                TextReader reader = new StreamReader(_xmlName);
                _fields = ser.Deserialize(reader) as SettingsFields;
                reader.Close();
            }
        }
        public class SettingsFields
        {
            public bool UpdeteIsEnable = false;
            public DateTime RemindLater = DateTime.MinValue;
            public string SkipVersion = "0,0,0,0";
        }
    }
}
