using System;

namespace DLSv2.Core.Triggers
{
    internal class AudioModeActive : Trigger
    {
        public override BaseCondition GetBaseCondition(string arguments)
        {
            if (arguments == null) return null;
            return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.ActiveAudioModes.Contains(arguments)));
        }
    }
}
