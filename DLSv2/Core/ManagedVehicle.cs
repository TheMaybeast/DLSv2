using DLSv2.Core.Lights;
using DLSv2.Core.Triggers;
using DLSv2.Utils;
using Rage;
using System;
using System.Collections.Generic;

namespace DLSv2.Core
{
    public class ManagedVehicle
    {
        public List<VehicleCondition> VehicleConditions = new List<VehicleCondition>();

        public ManagedVehicle(Vehicle vehicle)
        {
            Vehicle = vehicle;

            if (vehicle.IsDLS())
            {
                foreach (ControlGroup cG in ControlGroupManager.ControlGroups[vehicle.Model].Values)
                    ControlGroups.Add(cG.Name, new Tuple<bool, int>(false, 0));

                foreach (Mode mode in ModeManager.Modes[vehicle.Model].Values)
                {
                    Modes.Add(mode.Name, false);
                    foreach (Trigger trigger in mode.Triggers)
                    {
                        BaseCondition condition = ParseTrigger(trigger);
                        if (condition == null) continue;

                        condition.ConditionChangedEvent += (sender, args) =>
                        {
                            ModeManager.SetStandaloneModeStatus(this, "Testing", args.ConditionMet);
                            LightController.Update(this);
                        };

                        if (condition.GetType() == typeof(VehicleCondition))
                            VehicleConditions.Add((VehicleCondition)condition);
                    }
                }

                if (vehicle)
                {
                    bool temp = vehicle.IsSirenOn;
                    vehicle.IsSirenOn = false;
                    vehicle.IsSirenOn = temp;
                }
            }
        }

        // General
        public Vehicle Vehicle { get; set; }

        // Control Groups and Modes Status
        public Dictionary<string, Tuple<bool, int>> ControlGroups = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, bool> Modes = new Dictionary<string, bool>();

        // Lights
        public bool LightsOn { get; set; } = false;
        public bool Blackout { get; set; } = false;
        public bool InteriorLight { get; set; } = false;
        public IndStatus IndStatus { get; set; } = IndStatus.Off;
        public uint CurrentELHash { get; set; }

        // Sirens
        public bool SirenOn { get; set; } = false;
        public int SirenToneIndex { get; set; } = 0;
        public bool AuxOn { get; set; } = false;
        public int AuxID { get; set; } = 999;
        public int SoundId { get; set; } = 999;
        public int? AirManuState { get; set; } = null;
        public int? AirManuID { get; set; } = null;

        private BaseCondition ParseTrigger(Trigger trigger)
        {
            switch (trigger.Name)
            {
                case "SpeedAbove":
                    if (trigger.Argument == null) return null;
                    int speed = trigger.Argument.ToInt32();
                    return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.Vehicle.Speed > speed));
                case "SirenState":
                    if (trigger.Argument == null) return null;
                    switch (trigger.Argument.ToLower())
                    {
                        case "on":
                            return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.SirenOn));
                        case "off":
                            return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => !mV.SirenOn));
                        case "manual":
                            return new VehicleCondition(new Func<ManagedVehicle, bool>((mV) => mV.AirManuState == 2));
                        default:
                            return null;
                    }

                default:
                    return null;
            }
        }
    }

    public enum IndStatus
    {
        Left,
        Right,
        Hazard,
        Off
    }
}
