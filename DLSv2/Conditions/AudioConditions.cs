using System.Xml.Serialization;
using DLSv2.Core;

namespace DLSv2.Conditions
{
    public class AudioControlGroupCondition : VehicleCondition
    {

        [XmlAttribute("Name")]
        public string ControlGroupName { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.AudioControlGroups.ContainsKey(ControlGroupName) && veh.AudioControlGroups[ControlGroupName].Item1;
    }

    public class AudioModeActiveCondition : VehicleCondition
    {
        [XmlAttribute("Name")]
        public string AudioModeName { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.ActiveAudioModes.Contains(AudioModeName);
    }
}
