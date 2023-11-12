using DLSv2.Utils;
using System;

/*
namespace DLSv2.Core.Triggers
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
*/