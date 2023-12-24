using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
using DLSv2.Threads;
using DLSv2.Utils;
using Rage;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core
{
    public class ManagedVehicle
    {
        // Stores the current player's active vehicle.
        public static ManagedVehicle ActivePlayerVehicle { get; set; } = null;
        public bool IsActivePlayerVehicle { get => this == ActivePlayerVehicle; }

        public List<BaseCondition> Conditions = new List<BaseCondition>();

        public ManagedVehicle(Vehicle vehicle)
        {
            if (!vehicle) return;

            Vehicle = vehicle;
            VehicleHandle = vehicle.Handle;

            // Adds Light Control Groups and Modes
            foreach (var cG in ControlGroupManager.ControlGroups[vehicle.Model].Values)
                LightControlGroups.Add(cG.Name, (false, 0));             

            foreach (var mode in ModeManager.Modes[vehicle.Model].Values)
                StandaloneLightModes.Add(mode.Name, false);

            // Adds Audio Control Groups and Modes
            foreach (var cG in AudioControlGroupManager.ControlGroups[vehicle.Model].Values)
            {
                AudioControlGroups.Add(cG.Name, (false, 0));
                AudioCGManualing.Add(cG.Name, (false, 0));
            }                

            foreach (var mode in AudioModeManager.Modes[vehicle.Model].Values)
                AudioModes.Add(mode.Name, false);

            // Adds Triggers
            foreach (var mode in ModeManager.Modes[vehicle.Model].Values)
            {
                var triggersAndRequirements = new AllCondition(mode.Requirements, mode.Triggers);
                
                Conditions.Add(triggersAndRequirements);

                // if triggers change (to true or false from the opposite), update the mode
                triggersAndRequirements.GetInstance(this).OnInstanceTriggered += (sender, condition, state) =>
                {
                    ModeManager.SetStandaloneModeStatus(this, mode, state);
                    LightController.Update(this);
                };

                // if requirements become false, turn off the mode
                mode.Requirements.GetInstance(this).OnInstanceTriggered += (sender, condition, state) =>
                {
                    if (state) return;

                    ModeManager.SetStandaloneModeStatus(this, mode, state);
                    LightController.Update(this);
                };
            }

            EmptyMode = vehicle.GetEmptyMode();

            var temp = vehicle.IsSirenOn;
            vehicle.IsSirenOn = false;
            vehicle.IsSirenOn = temp;

            if (vehicle.IsPlayerVehicle())
                vehicle.ClearSiren();
            else
            {
                var modes = ModeManager.Modes[vehicle.Model];

                var defaultMode = vehicle.GetDLS().DefaultMode;
                if (defaultMode == null || !modes.Keys.Contains(defaultMode))
                    defaultMode = "DLS_DEFAULT_MODE";

                StandaloneLightModes[defaultMode] = true;
                
                LightController.Update(this);
            }
        }

        /// <summary>
        /// Vehicle Info
        /// </summary>
        public Vehicle Vehicle { get; set; }
        public uint VehicleHandle { get; set; }
        public Dictionary<int, bool> ManagedExtras = new Dictionary<int, bool>(); // Managed Extras - ID, original state

        private bool areLightsOn;

        internal bool isUpdating = false;

        /// <summary>
        /// Lights
        /// </summary>
        public bool LightsOn
        {
            get => areLightsOn;
            
            set
            {
                if (value)
                {
                    if (!Vehicle.IsSirenOn) Vehicle.IsSirenOn = true;
                    SyncManager.SyncSirens(Vehicle);
                } else
                {
                    Vehicle.IsSirenOn = false;
                }

                areLightsOn = value;
            }
        }
        public bool InteriorLight { get; set; }
        public VehicleIndicatorLightsStatus IndStatus { get; set; } = VehicleIndicatorLightsStatus.Off;
        public Dictionary<string, (bool Status, int Index)> LightControlGroups = new Dictionary<string, (bool, int)>();
        public Dictionary<string, bool> StandaloneLightModes = new Dictionary<string, bool>();
        public List<string> ActiveLightModes = new List<string>();

        public Mode EmptyMode;

        /// <summary>
        /// Sirens
        /// </summary>
        public bool SirenOn { get; set; } = false;
        public Dictionary<string, int> SoundIds = new Dictionary<string, int>();
        public Dictionary<string, (bool IsActive, int Index)> AudioControlGroups = new Dictionary<string, (bool, int)>();
        public Dictionary<string, (bool IsManualing, int Index)> AudioCGManualing = new Dictionary<string, (bool, int)>();
        public Dictionary<string, bool> AudioModes = new Dictionary<string, bool>();
        public List<string> ActiveAudioModes = new List<string>();

        // Registers input
        public void RegisterInputs()
        {
            ControlsManager.ClearInputs();
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
                                AudioCGManualing[cG.Name] = (true, AudioControlGroups[cG.Name].Item2);
                                AudioControlGroupManager.SetControlGroupIndex(this, cG.Name, index);
                                AudioController.Update(this);
                            }
                            else if (!AudioControlGroups[cG.Name].Item1)
                            {
                                AudioCGManualing[cG.Name] = (true, -1);
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
                                AudioCGManualing[cG.Name] = (false, 0);
                                AudioController.Update(this);
                            }
                        };
                    }
                }
            }

            // General Inputs
            // Toggle Keys Locked
            if (ControlsManager.RegisterInput("LOCKALL"))
            {
                ControlsManager.Inputs["LOCKALL"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();
                    ControlsManager.KeysLocked = !ControlsManager.KeysLocked;
                };
            }

            // Kills all lights and audio
            if (ControlsManager.RegisterInput("KILLALL"))
            {
                ControlsManager.Inputs["KILLALL"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    // Clears light modes
                    foreach (string key in StandaloneLightModes.Keys.ToList())
                        StandaloneLightModes[key] = false;

                    // Clears light control groups
                    foreach (string key in LightControlGroups.Keys.ToList())
                        LightControlGroups[key] = (false, 0);

                    // Updates lights
                    LightController.Update(this);

                    // Clears audio control groups
                    foreach (string key in AudioControlGroups.Keys.ToList())
                        AudioControlGroups[key] = (false, 0);

                    // Updates audio
                    AudioController.Update(this);
                };
            }

            // Interior Light
            if (ControlsManager.RegisterInput("INTLT"))
            {
                ControlsManager.Inputs["INTLT"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    InteriorLight = !InteriorLight;
                    GenericLights.SetInteriorLight(Vehicle, InteriorLight);
                };
            }

            // Indicator Left
            if (ControlsManager.RegisterInput("INDL"))
            {
                ControlsManager.Inputs["INDL"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    IndStatus = IndStatus == VehicleIndicatorLightsStatus.LeftOnly ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.LeftOnly;

                    GenericLights.SetIndicator(Vehicle, IndStatus);
                };
            }

            // Indicator Right
            if (ControlsManager.RegisterInput("INDR"))
            {
                ControlsManager.Inputs["INDR"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    IndStatus = IndStatus == VehicleIndicatorLightsStatus.RightOnly ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.RightOnly;

                    GenericLights.SetIndicator(Vehicle, IndStatus);
                };
            }

            // Hazards
            if (ControlsManager.RegisterInput("HZRD"))
            {
                ControlsManager.Inputs["HZRD"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    IndStatus = IndStatus == VehicleIndicatorLightsStatus.Both ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.Both;

                    GenericLights.SetIndicator(Vehicle, IndStatus);
                };
            }
        }
    }
}
