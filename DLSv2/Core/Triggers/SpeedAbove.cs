using DLSv2.Utils;
using System;

namespace DLSv2.Core.Triggers
{
    internal class SpeedAbove : Trigger
    {
        public override BaseCondition GetBaseCondition(string arguments)
        {
            if (arguments == null) return null;
            int speed = arguments.ToInt32();
            return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.Vehicle.Speed > speed));
        }
    }
}
