using System.Xml.Serialization;

namespace DLSv2.Conditions;

using Core;
using System.Linq;

public class AudioControlGroupCondition : VehicleCondition
{

    [XmlAttribute("name")]
    public string ControlGroupName { get; set; }

    [XmlAttribute("active")]
    public bool GroupEnabled { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        return veh.AudioControlGroups.ContainsKey(ControlGroupName) &&
               veh.AudioControlGroups[ControlGroupName].Enabled == GroupEnabled;
    }
}

public class AudioModeCondition : VehicleCondition
{
    [XmlAttribute("name")]
    public string AudioModeName { get; set; }

    [XmlAttribute("active")]
    public bool ModeEnabled { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        return veh.AudioModes.ContainsKey(AudioModeName) &&
               veh.AudioModes[AudioModeName].Enabled == ModeEnabled;
    }
}

public class LightControlGroupCondition : VehicleCondition
{
    [XmlAttribute("name")]
    public string ControlGroupName { get; set; }

    [XmlAttribute("active")]
    public bool GroupEnabled { get; set; } = true;

    [XmlAttribute("any_mode")]
    public bool AnyModeInGroup { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        if (!veh.LightControlGroups.ContainsKey(ControlGroupName)) 
            return false;

        var cg = veh.LightControlGroups[ControlGroupName];

        if (!AnyModeInGroup)
            return cg.Enabled == GroupEnabled;

        // If allowed to check any mode in group
        return cg.BaseControlGroup.Modes.Any(m => m.Modes.Any(m => veh.LightModes[m].Enabled)) == GroupEnabled;
    }
}

public class LightModeCondition : VehicleCondition
{
    [XmlAttribute("name")]
    public string LightModeName { get; set; }

    [XmlAttribute("active")]
    public bool ModeEnabled { get; set; } = true;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        return veh.LightModes.ContainsKey(LightModeName) &&
               veh.LightModes[LightModeName].Enabled == ModeEnabled;
    }
}