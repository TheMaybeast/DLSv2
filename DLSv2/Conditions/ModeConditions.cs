using System.Xml.Serialization;
using DLSv2.Core;

namespace DLSv2.Conditions
{
    public class AudioControlGroupCondition : VehicleCondition
    {

        [XmlAttribute("name")]
        public string ControlGroupName { get; set; }

        [XmlAttribute("active")]
        public bool GroupEnabled { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.AudioControlGroups.ContainsKey(ControlGroupName) && veh.AudioControlGroups[ControlGroupName].Item1 == GroupEnabled;
    }

    public class AudioModeCondition : VehicleCondition
    {
        [XmlAttribute("name")]
        public string AudioModeName { get; set; }

        [XmlAttribute("active")]
        public bool ModeEnabled { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.ActiveAudioModes.Contains(AudioModeName) == ModeEnabled;
    }

    public class LightControlGroupCondition : VehicleCondition
    {
        [XmlAttribute("name")]
        public string ControlGroupName { get; set; }

        [XmlAttribute("active")]
        public bool GroupEnabled { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.LightControlGroups.ContainsKey(ControlGroupName) && veh.LightControlGroups[ControlGroupName].Item1 == GroupEnabled;
    }

    public class LightModeCondition : VehicleCondition
    {
        [XmlAttribute("name")]
        public string LightModeName { get; set; }

        [XmlAttribute("active")]
        public bool ModeEnabled { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.ActiveLightModes.Contains(LightModeName) == ModeEnabled;
    }
}
