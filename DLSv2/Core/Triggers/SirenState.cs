using System;

namespace DLSv2.Core.Triggers
{
    internal class SirenState : VehicleCondition
    {
        public override bool Evaluate()
        {
            switch ((arguments ?? "").ToLower())
            {
                case "on":
                    return MV.SirenOn;
                case "off":
                    return !MV.SirenOn;
                case "manual":
                    return MV.AirManuState == 2;
                default:
                    throw new ArgumentException("SirenState argument must be \"on\", \"off\", or \"manual\"");
            }
        }
    }
}