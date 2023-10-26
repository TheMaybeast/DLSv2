using System.Collections.Generic;
using System.Xml.Serialization;

namespace DLSv2
{
    [XmlRoot("SirenSets")]
    public class SirenSets
    {
        [XmlElement("SirenSet")]
        public List<SirenSet> SirenSet;
    }

    public class SirenSet
    {
        [XmlElement("SoundSet")]
        public SoundSet SoundSet;
        [XmlElement("Vehicles")]
        public string Vehicles;
    }

    public class SoundSet
    {
        [XmlElement("Tones")]
        public string Tones;
        [XmlElement("Horn")]
        public string Horn = "SIRENS_AIRHORN";

        [XmlIgnore]
        public List<string> SirenTones = new List<string> { "VEHICLES_HORNS_SIREN_1", "VEHICLES_HORNS_SIREN_2", "", "" };
        [XmlIgnore]
        public List<SirenStage> AvailableSirenStages = new List<SirenStage> { SirenStage.One, SirenStage.Two };
    }
}
