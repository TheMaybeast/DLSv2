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

            foreach (ControlGroup cG in ControlGroupManager.ControlGroups[vehicle.Model].Values)
                ControlGroups.Add(cG.Name, new Tuple<bool, int>(false, 0));

            foreach (Mode mode in ModeManager.Modes[vehicle.Model].Values)
            {
                Modes.Add(mode.Name, false);
                foreach (TriggerRaw trigger in mode.Triggers)
                {
                    BaseCondition condition = trigger.GetCondition();
                    if (condition == null) continue;

                    if (condition is VehicleCondition vehCondition)
                    {
                        vehCondition.Init(this, trigger.Argument);
                    } else if (condition is GlobalCondition globalCond)
                    {
                        globalCond.Init(trigger.Argument);
                    }

                    condition.ConditionChangedEvent += (sender, args) =>
                    {
                        ModeManager.SetStandaloneModeStatus(this, mode.Name, args.ConditionMet);
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

        // General
        public Vehicle Vehicle { get; set; }

        // Control Groups and Modes Status
        public Dictionary<string, Tuple<bool, int>> ControlGroups = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, bool> Modes = new Dictionary<string, bool>();

        // Lights
        public bool LightsOn { get; set; } = false;
        public bool Blackout { get; set; } = false;
        public bool InteriorLight { get; set; } = false;
        public VehicleIndicatorLightsStatus IndStatus { get; set; } = VehicleIndicatorLightsStatus.Off;
        public uint CurrentELHash { get; set; }

        // Sirens
        public bool SirenOn { get; set; } = false;
        public int SirenToneIndex { get; set; } = 0;
        public bool AuxOn { get; set; } = false;
        public int AuxID { get; set; } = 999;
        public int SoundId { get; set; } = 999;
        public int? AirManuState { get; set; } = null;
        public int? AirManuID { get; set; } = null;
    }
}
