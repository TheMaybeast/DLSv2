using System;
using System.Xml.Serialization;

namespace DLSv2.Core.Triggers
{
    internal class AudioControlGroupCondition : VehicleCondition
    {

        [XmlAttribute("Name")]
        public string ControlGroupName { get; set; }

        public override bool Evaluate(ManagedVehicle veh) => veh.AudioControlGroups.ContainsKey(ControlGroupName) && veh.AudioControlGroups[ControlGroupName].Item1;
    }

    internal class AudioModeActiveCondition : VehicleCondition
    {
        [XmlAttribute("Name")]
        public string AudioModeName { get; set; }

        public override bool Evaluate(ManagedVehicle veh) => veh.ActiveAudioModes.Contains(AudioModeName);
    }
}
