using System;

namespace DLSv2.Core.Triggers
{
    internal class Horn : VehicleOnOffCondition
    {
        public override bool GetVehState() => MV.AirManuState == 1;
    }
}
