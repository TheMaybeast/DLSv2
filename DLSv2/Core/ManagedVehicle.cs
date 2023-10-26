﻿using Rage;

namespace DLSv2.Core
{
    public class ManagedVehicle
    {
        public ManagedVehicle(Vehicle vehicle, bool lightsOn = false)
        {
            Vehicle = vehicle;

            if (vehicle)
            {
                bool temp = vehicle.IsSirenOn;
                vehicle.IsSirenOn = false;
                vehicle.IsSirenOn = temp;
            }
        }

        // General
        public Vehicle Vehicle { get; set; }


        // Lights
        public int LightStage { get; set; } = 0;
        public bool Blackout { get; set; } = false;
        public bool InteriorLight { get; set; } = false;
        public IndStatus IndStatus { get; set; } = IndStatus.Off;
        public uint CurrentELHash { get; set; }

        // Sirens
        public bool SirenOn { get; set; } = false;
        public int SirenStage { get; set; } = 0;
        public bool AuxOn { get; set; } = false;
        public int AuxID { get; set; } = 999;
        public int SoundId { get; set; } = 999;
        public int? AirManuState { get; set; } = null;
        public int? AirManuID { get; set; } = null;
    }

    public enum IndStatus
    {
        Left,
        Right,
        Hazard,
        Off
    }
}
