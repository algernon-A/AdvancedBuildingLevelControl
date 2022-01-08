using System;
using System.IO;
using System.Xml.Serialization;


namespace ABLC
{
    /// <summary>
    /// Class to hold global mod settings.
    /// </summary>
    [XmlRoot(ElementName = "AdvancedBuildingLevelControl", Namespace = "", IsNullable = false)]
    public class ModSettings
    {
        // Panel on right side of info panel, not left.
        [XmlIgnore]
        internal static bool onRight = false;

        // Show panel automatically on info panel show.
        [XmlIgnore]
        internal static bool showPanel = true;

        // No abandonment for historical buildings.
        [XmlIgnore]
        internal static bool noAbandonHistorical = false;

        // No abaondoment for any buildings.
        [XmlIgnore]
        internal static bool noAbandonAny = false;

        // Check building prfabs on load for illegal levels (and fix them).
        [XmlIgnore]
        internal static bool loadLevelCheck = true;

        [XmlIgnore]
        private static readonly string SettingsFileName = "AdvancedBuildingLevelControl.xml";

        // Version.
        [XmlAttribute("Version")]
        public int version = 0;

        // Language.
        [XmlElement("Language")]
        public string Language
        {
            get
            {
                return Translations.CurrentLanguage;
            }
            set
            {
                Translations.CurrentLanguage = value;
            }
        }


        // Panel position.
        [XmlElement("PanelOnRight")]
        public bool OnRight { get => ModSettings.onRight; set => ModSettings.onRight = value; }

        // Show panel.
        [XmlElement("ShowPanel")]
        public bool ShowPanel { get => ModSettings.showPanel; set => ModSettings.showPanel = value; }

        // No historical abandonment.
        [XmlElement("NoAbandonHistorical")]
        public bool NoAbandonHistorical { get => ModSettings.noAbandonHistorical; set => ModSettings.noAbandonHistorical = value; }

        // No abandonment at all.
        [XmlElement("NoAbandonAny")]
        public bool NoAbandonAny { get => ModSettings.noAbandonAny; set => ModSettings.noAbandonAny = value; }

        // Check building levels on game load.
        [XmlElement("LoadLevelCheck")]
        public bool LoadLevelCheck { get => ModSettings.loadLevelCheck; set => ModSettings.loadLevelCheck = value; }


        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            try
            {
                // Check to see if configuration file exists.
                if (File.Exists(SettingsFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(SettingsFileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                        if (!(xmlSerializer.Deserialize(reader) is ModSettings gbrSettingsFile))
                        {
                            Logging.Error("couldn't deserialize settings file");
                        }
                    }
                }
                else
                {
                    Logging.Message("no settings file found");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading XML settings file");
            }
        }


        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save()
        {
            try
            {
                // Pretty straightforward.  Serialisation is within GBRSettingsFile class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, new ModSettings());
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }
}