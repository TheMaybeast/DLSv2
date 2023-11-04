using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
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

            // Adds Light Control Groups and Modes
            foreach (ControlGroup cG in ControlGroupManager.ControlGroups[vehicle.Model].Values)
                LightControlGroups.Add(cG.Name, new Tuple<bool, int>(false, 0));

            foreach (Mode mode in ModeManager.Modes[vehicle.Model].Values)
            {
                LightModes.Add(mode.Name, false);
                foreach (TriggerRaw trigger in mode.Triggers)
                {
                    BaseCondition condition = trigger.GetBaseCondition();
                    if (condition == null) continue;

                    condition.ConditionChangedEvent += (sender, args) =>
                    {
                        ModeManager.SetStandaloneModeStatus(this, mode.Name, args.ConditionMet);
                        LightController.Update(this);
                    };

                    if (condition.GetType() == typeof(VehicleCondition))
                        VehicleConditions.Add((VehicleCondition)condition);
                }
            }

            // Adds Audio Control Groups and Modes
            foreach (AudioControlGroup cG in AudioControlGroupManager.ControlGroups[vehicle.Model].Values)
            {
                AudioControlGroups.Add(cG.Name, new Tuple<bool, int>(false, 0));
                AudioCGManualing.Add(cG.Name, new Tuple<bool, int>(false, 0));
            }                

            foreach (AudioMode mode in AudioModeManager.Modes[vehicle.Model].Values)
                AudioModes.Add(mode.Name, false);

            if (vehicle)
            {
                bool temp = vehicle.IsSirenOn;
                vehicle.IsSirenOn = false;
                vehicle.IsSirenOn = temp;
                vehicle.ClearSiren();
            }
        }

        // General
        public Vehicle Vehicle { get; set; }

        // Lights
        public bool LightsOn { get; set; } = false;
        public bool Blackout { get; set; } = false;
        public bool InteriorLight { get; set; } = false;
        public VehicleIndicatorLightsStatus IndStatus { get; set; } = VehicleIndicatorLightsStatus.Off;
        public uint CurrentELHash { get; set; }
        public Dictionary<string, Tuple<bool, int>> LightControlGroups = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, bool> LightModes = new Dictionary<string, bool>();

        // Sirens
        public bool SirenOn { get; set; } = false;
        public Dictionary<string, int> SoundIds = new Dictionary<string, int>();
        public Dictionary<string, Tuple<bool, int>> AudioControlGroups = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, Tuple<bool, int>> AudioCGManualing = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, bool> AudioModes = new Dictionary<string, bool>();
        public List<string> ActiveAudioModes = new List<string>();
    }
}
