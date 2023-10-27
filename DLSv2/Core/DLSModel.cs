using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DLSv2.Core
{
    [XmlRoot("Model")]
    public class DLSModel
    {
        [XmlAttribute("vehicles")]
        public string Vehicles;

        [XmlElement("SoundSettings", IsNullable = true)]
        public SoundSettings SoundSettings;

        [XmlArray("Modes")]
        [XmlArrayItem("Mode")]
        public List<Mode> Modes;

        [XmlArray("ControlGroups")]
        [XmlArrayItem("ControlGroup")]
        public List<ControlGroup> ControlGroups;
    }

    public class SoundSettings
    {
        [XmlArray("Tones", IsNullable = true)]
        [XmlArrayItem("Tone")]
        public List<Tone> Tones;

        [XmlElement("Horn", IsNullable = true)]
        public string Horn;
    }

    public class Tone
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("allowdual")]
        public string AllowDual;

        [XmlAttribute("man_alt")]
        public string ManualAlternate;

        [XmlAttribute("man_pause")]
        public string PauseOnManual;

        [XmlText]
        public string ToneHash;
    }

    public class Mode
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlElement("Yield", IsNullable = true)]
        public Yield Yield;

        [XmlElement("Siren", IsNullable = true)]
        public Siren Siren;

        [XmlArray("Extras", IsNullable = true)]
        [XmlArrayItem("Extra")]
        public List<Extra> Extra;

        [XmlElement("SirenSettings", IsNullable = true)]
        public SirenSetting SirenSettings;
    }

    public class Yield
    {
        [XmlAttribute("enabled")]
        public string Enabled;
    }

    public class Siren
    {
        [XmlAttribute("manual")]
        public string ManualEnabled;

        [XmlAttribute("full")]
        public string FullSirenEnabled;
    }

    public class Extra
    {
        [XmlAttribute("ID")]
        public string ID;

        [XmlAttribute("enabled")]
        public string Enabled;
    }

    public class ControlGroup
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlArray("Modes")]
        [XmlArrayItem("Mode")]
        public List<ModeSelection> Modes;

        public delegate void ControlGroupKeybindingEventHandler(bool modified, EventArgs args);
    }

    public class ModeSelection
    {
        [XmlText]
        public string ModesRaw;

        [XmlIgnore]
        public List<string> Modes;
    }
}
