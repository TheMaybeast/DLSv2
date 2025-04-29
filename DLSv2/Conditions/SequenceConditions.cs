using System.Xml.Serialization;

namespace DLSv2.Conditions;

using Core;
using System.Linq;

public class SequenceBeatCondition : VehicleCondition
{
    [XmlAttribute("sequence")]
    public string Sequence
    {
        get => string.Concat(boolSequence.Select(x => x ? '1' : '0'));

        set
        {
            boolSequence = value.Where(c => (c == '1' || c == '0')).Select(c => c == '1').ToArray();
        }
    }

    internal bool[] boolSequence;

    protected override bool Evaluate(ManagedVehicle veh)
    {
        return 
            boolSequence.Length > 0 
            && veh.sirenInstance.TotalSirenBeats >= 0 
            && boolSequence[veh.sirenInstance.TotalSirenBeats % boolSequence.Length];
    }
}

