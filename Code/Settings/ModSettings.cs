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
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = "AdvancedBuildingLevelControl.xml";

        // User settings directory.
        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

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

        // Randomise building model levels +/- 1 level from intended target (for greater physical variation).
        [XmlIgnore]
        internal static bool randomLevels = false;

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
        public bool OnRight { get => onRight; set => onRight = value; }

        // Show panel.
        [XmlElement("ShowPanel")]
        public bool ShowPanel { get => showPanel; set => showPanel = value; }

        // No historical abandonment.
        [XmlElement("NoAbandonHistorical")]
        public bool NoAbandonHistorical { get => noAbandonHistorical; set => noAbandonHistorical = value; }

        // No abandonment at all.
        [XmlElement("NoAbandonAny")]
        public bool NoAbandonAny { get => noAbandonAny; set => noAbandonAny = value; }

        // Check building levels on game load.
        [XmlElement("LoadLevelCheck")]
        public bool LoadLevelCheck { get => loadLevelCheck; set => loadLevelCheck = value; }

        // Randomise building model levels.
        [XmlElement("RandomLevels")]
        public bool LoadLevelRandomLevelsCheck { get => randomLevels; set => randomLevels = value; }



        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            try
            {
                // Attempt to read new settings file (in user settings directory).
                string fileName = Path.Combine(UserSettingsDir, SettingsFileName);
                if (!File.Exists(fileName))
                {
                    // No settings file in user directory; use application directory instead.
                    fileName = SettingsFileName;
                }

                // Check to see if configuration file exists.
                if (File.Exists(fileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                        if (!(xmlSerializer.Deserialize(reader) is ModSettings settingsFile))
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
                // Save into user local settings.
                using (StreamWriter writer = new StreamWriter(Path.Combine(UserSettingsDir, SettingsFileName)))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, new ModSettings());
                }

                // Cleaning up after ourselves - delete any old config file in the application directory.
                if (File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }
}