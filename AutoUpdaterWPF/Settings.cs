using System;
using System.IO;
using System.Xml.Serialization;

namespace AutoUpdaterWPFedition
{
    public static class Settings
    {
        private static SettingsFields _fields = new SettingsFields();
        private const string XmlName = "UpdateSettings.xml";

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
                Version version;
                if (!Version.TryParse(_fields.SkipVersion, out version))
                {
                    version = new Version(0,0,0,0);
                }
                return version;
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
            TextWriter writer = new StreamWriter(XmlName);
            ser.Serialize(writer, _fields);
            writer.Close();
        }
        private static void ReadXml()
        {
            if (File.Exists(XmlName))
            {
                XmlSerializer ser = new XmlSerializer(typeof(SettingsFields));
                TextReader reader = new StreamReader(XmlName);
                _fields = ser.Deserialize(reader) as SettingsFields;
                reader.Close();
            }
        }
        public class SettingsFields
        {
            public bool UpdeteIsEnable = false;
            public DateTime RemindLater = DateTime.MinValue;
            public string SkipVersion = "0.0.0.0";
        }
    }
}
