using System;

namespace DLSv2.Core.Triggers
{
    internal class AudioControlGroupActive : Trigger
    {
        public override BaseCondition GetBaseCondition(string arguments)
        {
            if (arguments == null) return null;
            return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) =>
            {
                if (mV.AudioControlGroups.ContainsKey(arguments))
                    return mV.AudioControlGroups[arguments].Item1;
                else
                    return false;
            }));
        }
    }
}