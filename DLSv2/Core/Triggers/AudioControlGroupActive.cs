using System;

namespace DLSv2.Core.Triggers
{
    internal class AudioControlGroupActive : VehicleCondition
    {
        string audioCG;

        public override void Init(ManagedVehicle managedVehicle, string args)
        {
            base.Init(managedVehicle, args);

            if (!managedVehicle.AudioControlGroups.ContainsKey(args))
            {
                throw new ArgumentException("Audio Control Group must exist");
            }
            else
                audioCG = args;
        }

        public override bool Evaluate() => MV.AudioControlGroups[audioCG].Item1;
    }
}