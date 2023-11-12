using DLSv2.Core;

namespace DLSv2.Conditions
{
    internal class EngineState : VehicleOnOffCondition
    {
        public override bool GetVehState() => Vehicle.IsEngineOn;
    }
}
