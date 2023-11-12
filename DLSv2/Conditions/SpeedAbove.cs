using System;
using DLSv2.Core;

namespace DLSv2.Conditions
{
    internal class SpeedAbove : VehicleCondition
    {
        float speed;

        public override void Init(ManagedVehicle managedVehicle, string args)
        {
            base.Init(managedVehicle, args);

            if (!float.TryParse(args, out speed))
            {
                throw new ArgumentException("SpeedAbove argument must be a float");
            }
        }

        public override bool Evaluate() => Vehicle.Speed > speed;

    }
}
