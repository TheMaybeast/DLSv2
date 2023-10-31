using DLSv2.Utils;
using System;

namespace DLSv2.Core.Triggers
{
    internal class HasDriver : Trigger
    {
        public override BaseCondition GetBaseCondition(string arguments)
        {
            if (arguments == null) return null;
            bool hasDriver = arguments.ToBoolean();
            if (hasDriver)
                return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.Vehicle.HasDriver));
            else
                return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => !mV.Vehicle.HasDriver));
        }
    }
}