using System.Collections.Generic;
using System.Xml.Serialization;

namespace DLSv2.Core
{
    [XmlRoot("Model")]
    public class DLSModel
    {
        [XmlAttribute("vehicles")]
        public string Vehicles;

        [XmlElement("SoundSettings")]
        public SoundSettings SoundSettings;

        [XmlArray("LightStages")]
        [XmlArrayItem("Stage")]
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
        public List<string> SirenTones = new List<string>();
    }

    public class LightStage
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlElement("Yield")]
        public string Yield = "true";

        [XmlElement("ForceSiren")]
        public string ForceSiren = "0";

        [XmlElement("Extras")]
        public List<Extra> Extra;

        [XmlElement("SirenSettings")]
        public SirenSetting SirenSettings;
    }

    public class Extra
    {
        [XmlAttribute("ID")]
        public string ID;

        [XmlAttribute("Enabled")]
        public string Enabled;
    }
}
