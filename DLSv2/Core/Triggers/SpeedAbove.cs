using DLSv2.Utils;
using System;

namespace DLSv2.Core.Triggers
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
