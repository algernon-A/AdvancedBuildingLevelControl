// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ABLC
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using AlgernonCommons.XML;

    /// <summary>
    /// Class to hold global mod settings.
    /// </summary>
    [XmlRoot(ElementName = "AdvancedBuildingLevelControl")]
    public class ModSettings : SettingsXMLBase
    {
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = "AdvancedBuildingLevelControl.xml";

        // User settings directory.
        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        /// <summary>
        /// Gets or sets the data file format version.
        /// </summary>
        [XmlAttribute("Version")]
        public int Version { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the ABLC panel should appear on the right-hand side of the target WorldInfoPanel (instead of the left).
        /// </summary>
        [XmlElement("PanelOnRight")]
        public bool XMLOnRight { get => OnRight; set => OnRight = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the ABLC panel should be automatically shown when the target WorldInfoPanel is shown.
        /// </summary>
        [XmlElement("ShowPanel")]
        public bool XMLShowPanel { get => ShowPanel; set => ShowPanel = value; }

        /// <summary>
        /// Gets or sets a value indicating whether there is no abandonment for historical buildings.
        /// </summary>
        [XmlElement("NoAbandonHistorical")]
        public bool XMLNoAbandonHistorical { get => NoAbandonHistorical; set => NoAbandonHistorical = value; }

        /// <summary>
        /// Gets or sets a value indicating whether there is no abandonment for any buildings.
        /// </summary>
        [XmlElement("NoAbandonAny")]
        public bool XMLNoAbandonAny { get => NoAbandonAny; set => NoAbandonAny = value; }

        /// <summary>
        /// Gets or sets a value indicating whether building prfabs should be checked on load for illegal levels (and errors fixed).
        /// </summary>
        [XmlElement("LoadLevelCheck")]
        public bool XMLLoadLevelCheck { get => LoadLevelCheck; set => LoadLevelCheck = value; }

        /// <summary>
        /// Gets or sets a value indicating whether building models should display a random level +/- 1 level from intended target (for greater physical variation).
        /// </summary>
        [XmlElement("RandomLevels")]
        public bool XMLRandomLevels { get => RandomLevels; set => RandomLevels = value; }

        /// <summary>
        /// Gets or sets a value indicating whether non-historical buildings can still upgrade (as if they were historical) when there's no eligible target prefab.
        /// </summary>
        [XmlElement("UpgradeWithoutTarget")]
        public bool XMLUpgradeWithoutTarget { get => UpgradeWithoutTarget; set => UpgradeWithoutTarget = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the ABLC panel should appear on the right-hand side of the target WorldInfoPanel (instead of the left).
        /// </summary>
        [XmlIgnore]
        internal static bool OnRight { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the ABLC panel should be automatically shown when the target WorldInfoPanel is shown.
        /// </summary>
        [XmlIgnore]
        internal static bool ShowPanel { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether there is no abandonment for historical buildings.
        /// </summary>
        [XmlIgnore]
        internal static bool NoAbandonHistorical { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether there is no abandonment for any buildings.
        /// </summary>
        [XmlIgnore]
        internal static bool NoAbandonAny { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether building prfabs should be checked on load for illegal levels (and errors fixed).
        /// </summary>
        [XmlIgnore]
        internal static bool LoadLevelCheck { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether building models should display a random level +/- 1 level from intended target (for greater physical variation).
        /// </summary>
        [XmlIgnore]
        internal static bool RandomLevels { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether non-historical buildings can still upgrade (as if they were historical) when there's no eligible target prefab.
        /// </summary>
        [XmlIgnore]
        internal static bool UpgradeWithoutTarget { get; set; } = false;

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
        internal static void Save() => XMLFileUtils.Save<ModSettings>(Path.Combine(UserSettingsDir, SettingsFileName));
    }
}