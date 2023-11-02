using System;

namespace DLSv2.Core.Triggers
{
    /*internal class Horn : Trigger
    {
        public override BaseCondition GetBaseCondition(string arguments)
        {
            if (arguments == null) return null;
            switch (arguments.ToLower())
            {
                case "on":
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.AirManuState == 1));
                case "off":
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.AirManuState != 1));
                default:
                    return null;
            }
        }
    }*/
}
