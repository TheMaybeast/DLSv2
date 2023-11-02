using System;

namespace DLSv2.Core.Triggers
{
    /*internal class SirenState : Trigger
    {
        public override BaseCondition GetBaseCondition(string arguments)
        {
            if (arguments == null) return null;
            switch (arguments.ToLower())
            {
                case "on":
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.SirenOn));
                case "off":
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => !mV.SirenOn));
                case "manual":
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.AirManuState == 2));
                default:
                    return null;
            }
        }
    }*/
}