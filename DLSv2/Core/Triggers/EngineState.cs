using System;

namespace DLSv2.Core.Triggers
{
    internal class EngineState : Trigger
    {
        public override BaseCondition GetBaseCondition(string arguments)
        {
            if (arguments == null) return null;
            switch (arguments.ToLower())
            {
                case "on":
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.Vehicle.IsEngineOn));
                case "off":
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => !mV.Vehicle.IsEngineOn));
                default:
                    return null;
            }
        }
    }
}
