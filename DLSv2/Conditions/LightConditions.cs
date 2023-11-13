using System.Xml.Serialization;
using DLSv2.Core;

namespace DLSv2.Conditions
{
    public class LightControlGroupCondition : VehicleCondition
    {

        [XmlAttribute("Name")]
        public string ControlGroupName { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.LightControlGroups.ContainsKey(ControlGroupName) && veh.LightControlGroups[ControlGroupName].Item1;
    }

    public class LightModeActiveCondition : VehicleCondition
    {
        [XmlAttribute("Name")]
        public string LightModeName { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.ActiveLightModes.Contains(LightModeName);
    }
}
