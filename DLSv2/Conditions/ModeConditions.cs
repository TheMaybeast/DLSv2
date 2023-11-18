using System.Xml.Serialization;
using DLSv2.Core;

namespace DLSv2.Conditions
{
    public class AudioControlGroupCondition : VehicleCondition
    {

        [XmlAttribute("Name")]
        public string ControlGroupName { get; set; }

        [XmlAttribute("Active")]
        public bool GroupEnabled { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.AudioControlGroups.ContainsKey(ControlGroupName) && veh.AudioControlGroups[ControlGroupName].Item1 == GroupEnabled;
    }

    public class AudioModeCondition : VehicleCondition
    {
        [XmlAttribute("Name")]
        public string AudioModeName { get; set; }

        [XmlAttribute("Active")]
        public bool ModeEnabled { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.ActiveAudioModes.Contains(AudioModeName) == ModeEnabled;
    }

    public class LightControlGroupCondition : VehicleCondition
    {
        [XmlAttribute("Name")]
        public string ControlGroupName { get; set; }

        [XmlAttribute("Active")]
        public bool GroupEnabled { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.LightControlGroups.ContainsKey(ControlGroupName) && veh.LightControlGroups[ControlGroupName].Item1 == GroupEnabled;
    }

    public class LightModeCondition : VehicleCondition
    {
        [XmlAttribute("Name")]
        public string LightModeName { get; set; }

        [XmlAttribute("Active")]
        public bool ModeEnabled { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.ActiveLightModes.Contains(LightModeName) == ModeEnabled;
    }
}
