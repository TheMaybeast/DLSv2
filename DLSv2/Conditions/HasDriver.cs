using System;
using DLSv2.Core;

namespace DLSv2.Conditions
{
    internal class HasDriver : VehicleCondition
    {
        bool mustHaveDriver;

        public override void Init(ManagedVehicle managedVehicle, string args)
        {
            base.Init(managedVehicle, args);
            if (arguments == "true") mustHaveDriver = true;
            else if (arguments == "false") mustHaveDriver = false;
            else throw new ArgumentException("HasDriver argument must be \"true\" or \"false\"");
        }

        public override bool Evaluate() => Vehicle.HasDriver == mustHaveDriver;
    }
}