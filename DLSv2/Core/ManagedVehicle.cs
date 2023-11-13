﻿using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
using DLSv2.Threads;
using DLSv2.Utils;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core
{
    public class ManagedVehicle
    {
        public List<BaseCondition.ConditionInstance> Conditions = new List<BaseCondition.ConditionInstance>();

        public ManagedVehicle(Vehicle vehicle)
        {
            Vehicle = vehicle;
            VehicleHandle = vehicle.Handle;

            // Adds Light Control Groups and Modes
            foreach (ControlGroup cG in ControlGroupManager.ControlGroups[vehicle.Model].Values)
                LightControlGroups.Add(cG.Name, new Tuple<bool, int>(false, 0));             

            foreach (Mode mode in ModeManager.Modes[vehicle.Model].Values)
                StandaloneLightModes.Add(mode.Name, false);

            // Adds Audio Control Groups and Modes
            foreach (AudioControlGroup cG in AudioControlGroupManager.ControlGroups[vehicle.Model].Values)
            {
                AudioControlGroups.Add(cG.Name, new Tuple<bool, int>(false, 0));
                AudioCGManualing.Add(cG.Name, new Tuple<bool, int>(false, 0));
            }                

            foreach (AudioMode mode in AudioModeManager.Modes[vehicle.Model].Values)
                AudioModes.Add(mode.Name, false);

            // Adds Triggers
            foreach (Mode mode in ModeManager.Modes[vehicle.Model].Values)
            {
                foreach (var trigger in mode.Triggers.NestedConditions)
                {
                    BaseCondition.ConditionInstance instance = trigger.GetInstance(this);
                    Conditions.Add(instance);
                    instance.OnInstanceTriggered += (sender, condition, state) =>
                    {
                        ModeManager.SetStandaloneModeStatus(this, mode, state);
                        LightController.Update(this);
                    };
                }
            }

            if (vehicle)
            {
                bool temp = vehicle.IsSirenOn;
                vehicle.IsSirenOn = false;
                vehicle.IsSirenOn = temp;
                vehicle.ClearSiren();
            }
        }

        // Vehicle
        public Vehicle Vehicle { get; set; }
        public uint VehicleHandle { get; set; }

        // Managed Extras - ID, original state
        public Dictionary<int, bool> ManagedExtras = new Dictionary<int, bool>();

        // Lights
        public bool LightsOn { get; set; }
        public bool InteriorLight { get; set; }
        public VehicleIndicatorLightsStatus IndStatus { get; set; } = VehicleIndicatorLightsStatus.Off;
        public Dictionary<string, Tuple<bool, int>> LightControlGroups = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, bool> StandaloneLightModes = new Dictionary<string, bool>();
        public List<string> ActiveLightModes = new List<string>();

        // Sirens
        public bool SirenOn { get; set; } = false;
        public Dictionary<string, int> SoundIds = new Dictionary<string, int>();
        public Dictionary<string, Tuple<bool, int>> AudioControlGroups = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, Tuple<bool, int>> AudioCGManualing = new Dictionary<string, Tuple<bool, int>>();
        public Dictionary<string, bool> AudioModes = new Dictionary<string, bool>();
        public List<string> ActiveAudioModes = new List<string>();

        // Registers input
        public void RegisterInputs()
        {
            // Light Inputs
            foreach (ControlGroup cG in ControlGroupManager.ControlGroups[Vehicle.Model].Values)
            {
                bool hasToggle = ControlsManager.RegisterInput(cG.Toggle);
                bool hasCycle = ControlsManager.RegisterInput(cG.Cycle);

                if (hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[cG.Toggle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        ControlGroupManager.ToggleControlGroup(this, cG.Name);
                        LightController.Update(this);
                    };
                    ControlsManager.Inputs[cG.Cycle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        ControlGroupManager.NextInControlGroup(this, cG.Name);
                        LightController.Update(this);
                    };
                }
                else if (hasToggle && !hasCycle)
                {
                    ControlsManager.Inputs[cG.Toggle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        ControlGroupManager.ToggleControlGroup(this, cG.Name, true);
                        LightController.Update(this);
                    };
                }
                else if (!hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[cG.Cycle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        ControlGroupManager.NextInControlGroup(this, cG.Name);
                        LightController.Update(this);
                    };
                }

                if (ControlsManager.RegisterInput(cG.ReverseCycle))
                {
                    ControlsManager.Inputs[cG.ReverseCycle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        ControlGroupManager.PreviousInControlGroup(this, cG.Name);
                        LightController.Update(this);
                    };
                }

                foreach (ModeSelection mode in cG.Modes)
                {
                    if (!ControlsManager.RegisterInput(mode.Toggle)) continue;
                    ControlsManager.Inputs[mode.Toggle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        int index = cG.Modes.IndexOf(mode);
                        if (this.LightControlGroups[cG.Name].Item1 && this.LightControlGroups[cG.Name].Item2 == index)
                            ControlGroupManager.ToggleControlGroup(this, cG.Name);
                        else
                            ControlGroupManager.SetControlGroupIndex(this, cG.Name, index);
                        LightController.Update(this);
                    };
                }
            }

            // Audio Control group and modes keys
            foreach (AudioControlGroup cG in AudioControlGroupManager.ControlGroups[Vehicle.Model].Values)
            {
                bool hasToggle = ControlsManager.RegisterInput(cG.Toggle);
                bool hasCycle = ControlsManager.RegisterInput(cG.Cycle);

                if (hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[cG.Toggle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        AudioControlGroupManager.ToggleControlGroup(this, cG.Name);
                        AudioController.Update(this);
                    };
                    ControlsManager.Inputs[cG.Cycle].OnInputReleased += (sender, inputName) =>
                    {
                        if (!AudioControlGroups[cG.Name].Item1) return;
                        ControlsManager.PlayInputSound();
                        AudioControlGroupManager.NextInControlGroup(this, cG.Name);
                        AudioController.Update(this);
                    };
                }
                else if (hasToggle && !hasCycle)
                {
                    ControlsManager.Inputs[cG.Toggle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        AudioControlGroupManager.ToggleControlGroup(this, cG.Name, true);
                        AudioController.Update(this);
                    };
                }
                else if (!hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[cG.Cycle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        AudioControlGroupManager.NextInControlGroup(this, cG.Name, cycleOnly: true);
                        AudioController.Update(this);
                    };
                }

                if (ControlsManager.RegisterInput(cG.ReverseCycle))
                {
                    ControlsManager.Inputs[cG.ReverseCycle].OnInputReleased += (sender, inputName) =>
                    {
                        if (!AudioControlGroups[cG.Name].Item1) return;
                        ControlsManager.PlayInputSound();
                        AudioControlGroupManager.PreviousInControlGroup(this, cG.Name);
                        AudioController.Update(this);
                    };
                }

                foreach (AudioModeSelection mode in cG.Modes)
                {
                    if (ControlsManager.RegisterInput(mode.Toggle))
                    {
                        ControlsManager.Inputs[mode.Toggle].OnInputReleased += (sender, inputName) =>
                        {
                            ControlsManager.PlayInputSound();
                            int index = cG.Modes.IndexOf(mode);
                            if (AudioControlGroups[cG.Name].Item1 && AudioControlGroups[cG.Name].Item2 == index)
                                AudioControlGroupManager.ToggleControlGroup(this, cG.Name);
                            else
                                AudioControlGroupManager.SetControlGroupIndex(this, cG.Name, index);
                            AudioController.Update(this);
                        };
                    }                    

                    if (ControlsManager.RegisterInput(mode.Hold))
                    {
                        ControlsManager.Inputs[mode.Hold].OnInputPressed += (sender, inputName) =>
                        {
                            int index = cG.Modes.IndexOf(mode);
                            if (AudioControlGroups[cG.Name].Item1 && AudioControlGroups[cG.Name].Item2 != index)
                            {
                                AudioCGManualing[cG.Name] = new Tuple<bool, int>(true, AudioControlGroups[cG.Name].Item2);
                                AudioControlGroupManager.SetControlGroupIndex(this, cG.Name, index);
                                AudioController.Update(this);
                            }
                            else if (!AudioControlGroups[cG.Name].Item1)
                            {
                                AudioCGManualing[cG.Name] = new Tuple<bool, int>(true, -1);
                                AudioControlGroupManager.ToggleControlGroup(this, cG.Name);
                                AudioControlGroupManager.SetControlGroupIndex(this, cG.Name, index);
                                AudioController.Update(this);
                            }
                        };

                        ControlsManager.Inputs[mode.Hold].OnInputReleased += (sender, inputName) =>
                        {
                            if (AudioCGManualing[cG.Name].Item1)
                            {
                                if (AudioCGManualing[cG.Name].Item2 == -1)
                                    AudioControlGroupManager.ToggleControlGroup(this, cG.Name);
                                else
                                    AudioControlGroupManager.SetControlGroupIndex(this, cG.Name, AudioCGManualing[cG.Name].Item2);
                                AudioCGManualing[cG.Name] = new Tuple<bool, int>(false, 0);
                                AudioController.Update(this);
                            }
                        };
                    }
                }
            }

            // General Inputs
            // Toggle Keys Locked
            ControlsManager.RegisterInput("LOCKALL");
            ControlsManager.Inputs["LOCKALL"].OnInputReleased += (sender, inputName) =>
            {
                ControlsManager.PlayInputSound();
                ControlsManager.KeysLocked = !ControlsManager.KeysLocked;
            };

            // Kills all lights and audio
            ControlsManager.RegisterInput("KILLALL");
            ControlsManager.Inputs["KILLALL"].OnInputReleased += (sender, inputName) =>
            {
                ControlsManager.PlayInputSound();

                // Clears light modes
                foreach (string key in StandaloneLightModes.Keys.ToList())
                    StandaloneLightModes[key] = false;

                // Clears light control groups
                foreach (string key in LightControlGroups.Keys.ToList())
                    LightControlGroups[key] = new Tuple<bool, int>(false, 0);

                // Updates lights
                LightController.Update(this);

                // Clears audio control groups
                foreach (string key in AudioControlGroups.Keys.ToList())
                    AudioControlGroups[key] = new Tuple<bool, int>(false, 0);

                // Updates audio
                AudioController.Update(this);
            };

            // Interior Light
            ControlsManager.RegisterInput("INTLT");
            ControlsManager.Inputs["INTLT"].OnInputReleased += (sender, inputName) =>
            {
                ControlsManager.PlayInputSound();

                InteriorLight = !InteriorLight;
                GenericLights.SetInteriorLight(Vehicle, InteriorLight);
            };

            // Indicator Left
            ControlsManager.RegisterInput("INDL");
            ControlsManager.Inputs["INDL"].OnInputReleased += (sender, inputName) =>
            {
                ControlsManager.PlayInputSound();

                IndStatus = IndStatus == VehicleIndicatorLightsStatus.LeftOnly ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.LeftOnly;

                GenericLights.SetIndicator(Vehicle, IndStatus);
            };

            // Indicator Right
            ControlsManager.RegisterInput("INDR");
            ControlsManager.Inputs["INDR"].OnInputReleased += (sender, inputName) =>
            {
                ControlsManager.PlayInputSound();

                IndStatus = IndStatus == VehicleIndicatorLightsStatus.RightOnly ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.RightOnly;

                GenericLights.SetIndicator(Vehicle, IndStatus);
            };

            // Hazards
            ControlsManager.RegisterInput("HZRD");
            ControlsManager.Inputs["HZRD"].OnInputReleased += (sender, inputName) =>
            {
                ControlsManager.PlayInputSound();

                IndStatus = IndStatus == VehicleIndicatorLightsStatus.Both ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.Both;

                GenericLights.SetIndicator(Vehicle, IndStatus);
            };
        }
    }
}
