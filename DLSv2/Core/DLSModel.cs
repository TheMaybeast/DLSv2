using System.Collections.Generic;
using System.Xml.Serialization;

namespace DLSv2.Core
{
    [XmlRoot("Model")]
    public class DLSModel
    {
        [XmlElement("SoundSettings")]
        public SoundSettings SoundSettings;

        [XmlElement("LightStages")]
        public List<LightStage> LightStages;
    }

    public class SoundSettings
    {
        [XmlElement("Tones")]
        public string Tones;

        [XmlElement("Horn")]
        public string Horn = "sirens_airhorn";

        [XmlElement("AirHornInterruptsSiren")]
        public string AirHornInterruptsSiren = "false";

        [XmlIgnore]
        public List<string> SirenTones = new List<string> { "sirens_slow_dir", "fast_9mvv0vf", "", "" };
    }

    public class LightStage
    {
        [XmlElement("Yield")]
        public string Yield = "true";

        [XmlElement("ForceSiren")]
        public string ForceSiren = "0";

        [XmlElement("Extras")]
        public List<Extra> Extra;

        [XmlElement("SirenSetting")]
        public SirenSetting SirenSetting;
    }

    public class Extra
    {
        [XmlAttribute("ID")]
        public string ID;

        [XmlAttribute("Enabled")]
        public string Enabled;
    }
}
