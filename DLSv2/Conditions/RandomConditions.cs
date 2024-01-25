using Rage;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DLSv2.Conditions;

using Core;

public class RandomCondition : VehicleCondition
{
    private static Dictionary<ManagedVehicle, bool> cachedResults = new Dictionary<ManagedVehicle, bool>();

    [XmlAttribute("chance")]
    public int PercentChance { get; set; }

    public bool GetRandomResult() => PercentChance >= MathHelper.GetRandomInteger(0, 100);

    protected override bool Evaluate(ManagedVehicle veh)
    {
        if (!cachedResults.TryGetValue(veh, out bool result))
        {
            result = GetRandomResult();
            cachedResults.Add(veh, result);
        }

        return result;
    }
}