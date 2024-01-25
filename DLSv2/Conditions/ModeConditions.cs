using System.Xml.Serialization;

namespace DLSv2.Conditions;

using Core;

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
    public bool GroupEnabled { get; set; }

    protected override bool Evaluate(ManagedVehicle veh)
    {
        return veh.LightControlGroups.ContainsKey(ControlGroupName) &&
               veh.LightControlGroups[ControlGroupName].Enabled == GroupEnabled;
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