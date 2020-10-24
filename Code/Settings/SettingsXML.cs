using System;
using System.IO;
using System.Xml.Serialization;


namespace ABLC
{
    /// <summary>
    /// XML settings file.
    /// </summary>
    [XmlRoot(ElementName = "AdvancedBuildingLevelControl", Namespace = "", IsNullable = false)]
    public class ABLCSettingsFile
    {
        [XmlIgnore]
        private static readonly string SettingsFileName = "AdvancedBuildingLevelControl.xml";

        // Version.
        [XmlAttribute("Version")]
        public int version = 0;

        // Language.
        [XmlElement("Language")]
        public string language
        {
            get
            {
                return Translations.Language;
            }
            set
            {
                Translations.Language = value;
            }
        }


        // Panel position.
        [XmlElement("PanelOnRight")]
        public bool onRight { get => ModSettings.onRight; set => ModSettings.onRight = value; }

        // Show panel.
        [XmlElement("ShowPanel")]
        public bool showPanel { get => ModSettings.showPanel; set => ModSettings.showPanel = value; }


        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void LoadSettings()
        {
            // Check to see if configuration file exists.
            if (File.Exists(SettingsFileName))
            {
                // Read it.
                using (StreamReader reader = new StreamReader(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ABLCSettingsFile));
                    if (!(xmlSerializer.Deserialize(reader) is ABLCSettingsFile gbrSettingsFile))
                    {
                        Debugging.Message("couldn't deserialize settings file");
                    }
                }
            }
            else
            {
                Debugging.Message("no settings file found");
            }
        }


        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void SaveSettings()
        {
            try
            {
                // Pretty straightforward.  Serialisation is within GBRSettingsFile class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ABLCSettingsFile));
                    xmlSerializer.Serialize(writer, new ABLCSettingsFile());
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }
    }
}