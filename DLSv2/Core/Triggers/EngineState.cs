using System;

namespace DLSv2.Core.Triggers
{
    internal class EngineState : VehicleOnOffCondition
    {
        public override bool GetVehState() => Vehicle.IsEngineOn;
    }
}
