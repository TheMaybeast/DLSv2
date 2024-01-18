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

        public List<BaseCondition> Conditions = new();

        public ManagedVehicle(Vehicle vehicle)
        {
            if (!vehicle) return;

            Vehicle = vehicle;
            VehicleHandle = vehicle.Handle;

            Entrypoint.DLSModels.TryGetValue(vehicle.Model, out var _dlsModel);
            if (_dlsModel == null) return;
            dlsModel = _dlsModel;

            // Adds Light Control Groups and Modes
            foreach (var cG in dlsModel.ControlGroups)
                LightControlGroups.Add(cG.Name, new LightControlGroupInstance(cG));

            foreach (var mode in dlsModel.Modes)
                LightModes.Add(mode.Name, new LightModeInstance(mode));

            // Adds Audio Control Groups and Modes
            foreach (var cG in dlsModel.AudioSettings.AudioControlGroups)
                AudioControlGroups.Add(cG.Name, new AudioControlGroupInstance(cG));

            foreach (var mode in dlsModel.AudioSettings.AudioModes)
                AudioModes.Add(mode.Name, new AudioModeInstance(mode));

            // Adds Triggers
            foreach (var mode in LightModes.Values.Select(x => x.BaseMode))
            {
                var triggersAndRequirements = new AllCondition(mode.Requirements, mode.Triggers);
                
                Conditions.Add(triggersAndRequirements);

                // if triggers change (to true or false from the opposite), update the mode
                triggersAndRequirements.GetInstance(this).OnInstanceTriggered += (sender, condition, state) =>
                {
                    LightModes[mode.Name].EnabledByTrigger = state;
                    UpdateLights();
                };

                // if requirements become false, turn off the mode
                mode.Requirements.GetInstance(this).OnInstanceTriggered += (sender, condition, state) =>
                {
                    if (!state) LightModes[mode.Name].EnabledByTrigger = false;
                    UpdateLights();
                };
            }

            EmptyMode = vehicle.GetEmptyMode();
            DefaultMode = LightModes[dlsModel.DefaultModeName];

            var temp = vehicle.IsSirenOn;
            vehicle.IsSirenOn = false;
            vehicle.IsSirenOn = temp;

            VehicleOwner.OnIsPlayerVehicleChanged += SetIsPlayerOwned;

            SetIsPlayerOwned(vehicle, vehicle.IsPlayerVehicle());
        }

        private void SetIsPlayerOwned(Vehicle v, bool isPlayerOwned)
        {
            if (isPlayerOwned)
            {
                v.DisableSirenSounds();
                DefaultMode.EnabledByTrigger = false;
            } else
            {
                bool silent = v.IsSirenSilent;
                ClearAll();
                v.EnableSirenSounds();
                DefaultMode.EnabledByTrigger = true;
                v.IsSirenSilent = silent;
            }

            UpdateLights();
        }

        /// <summary>
        /// Vehicle Info
        /// </summary>
        public Vehicle Vehicle { get; }
        public DLSModel dlsModel { get; }
        public uint VehicleHandle { get; }
        public Dictionary<int, bool> ManagedExtras = new Dictionary<int, bool>(); // Managed Extras - ID, original state
        public Dictionary<int, int> ManagedPaint = new Dictionary<int, int>(); // Managed Paint Settings - Paint index, original color code
        private bool areLightsOn;

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
        public Dictionary<string, LightControlGroupInstance> LightControlGroups = new();
        public Dictionary<string, LightModeInstance> LightModes = new();
        public LightModeInstance DefaultMode;

        public LightMode EmptyMode;

        /// <summary>
        /// Sirens
        /// </summary>
        public bool SirenOn { get; set; } = false;
        public Dictionary<string, int> SoundIds = new();
        public Dictionary<string, AudioControlGroupInstance> AudioControlGroups = new();
        public Dictionary<string, AudioModeInstance> AudioModes = new();

        public void ClearAll()
        {
            // Clears light modes
            foreach (var mode in LightModes.Values)
                mode.Enabled = false;

            // Clears light control groups
            foreach (var cG in LightControlGroups.Values)
                cG.Disable();

            // Updates lights
            UpdateLights();

            // Clears audio control groups
            foreach (var cG in AudioControlGroups.Values)
                cG.Disable();

            // Updates audio
            UpdateAudio();
        }

        // Registers input
        public void RegisterInputs()
        {
            ControlsManager.ClearInputs();
            // Light Inputs
            foreach (var cG in LightControlGroups.Values)
            {
                string toggleKey = cG.BaseControlGroup.Toggle;
                string cycleKey = cG.BaseControlGroup.Cycle;
                string reverseCycleKey = cG.BaseControlGroup.ReverseCycle;

                bool hasToggle = ControlsManager.RegisterInput(toggleKey);
                bool hasCycle = ControlsManager.RegisterInput(cycleKey);

                if (hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[toggleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.Toggle();
                        UpdateLights();
                    };
                    ControlsManager.Inputs[cycleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.MoveToNext();
                        UpdateLights();
                    };
                }
                else if (hasToggle && !hasCycle)
                {
                    ControlsManager.Inputs[toggleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.Toggle(true);
                        UpdateLights();
                    };
                }
                else if (!hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[cycleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.MoveToNext();
                        UpdateLights();
                    };
                }

                if (ControlsManager.RegisterInput(reverseCycleKey))
                {
                    ControlsManager.Inputs[reverseCycleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.MoveToPrevious();
                        UpdateLights();
                    };
                }

                foreach (var mode in cG.BaseControlGroup.Modes)
                {
                    if (!ControlsManager.RegisterInput(mode.Toggle)) continue;
                    ControlsManager.Inputs[mode.Toggle].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        int index = cG.BaseControlGroup.Modes.IndexOf(mode);
                        if (cG.Enabled && cG.Index == index)
                            cG.Toggle();
                        else
                        {
                            cG.Index = index;
                            cG.Enabled = true;
                        }
                        UpdateLights();
                    };
                }
            }

            // Audio Control group and modes keys
            foreach (var cG in AudioControlGroups.Values)
            {
                string toggleKey = cG.BaseControlGroup.Toggle;
                string cycleKey = cG.BaseControlGroup.Cycle;
                string reverseCycleKey = cG.BaseControlGroup.ReverseCycle;

                bool hasToggle = ControlsManager.RegisterInput(toggleKey);
                bool hasCycle = ControlsManager.RegisterInput(cycleKey);

                if (hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[toggleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.Toggle();
                        UpdateAudio();
                    };
                    ControlsManager.Inputs[cycleKey].OnInputReleased += (sender, inputName) =>
                    {
                        if (!cG.Enabled) return;
                        ControlsManager.PlayInputSound();
                        cG.MoveToNext();
                        UpdateAudio();
                    };
                }
                else if (hasToggle && !hasCycle)
                {
                    ControlsManager.Inputs[toggleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.Toggle(true);
                        UpdateAudio();
                    };
                }
                else if (!hasToggle && hasCycle)
                {
                    ControlsManager.Inputs[cycleKey].OnInputReleased += (sender, inputName) =>
                    {
                        ControlsManager.PlayInputSound();
                        cG.MoveToNext(cycleOnly: true);
                        UpdateAudio();
                    };
                }

                if (ControlsManager.RegisterInput(reverseCycleKey))
                {
                    ControlsManager.Inputs[reverseCycleKey].OnInputReleased += (sender, inputName) =>
                    {
                        if (!cG.Enabled) return;
                        ControlsManager.PlayInputSound();
                        cG.MoveToPrevious();
                        UpdateAudio();
                    };
                }

                foreach (AudioModeSelection mode in cG.BaseControlGroup.Modes)
                {
                    if (ControlsManager.RegisterInput(mode.Toggle))
                    {
                        ControlsManager.Inputs[mode.Toggle].OnInputReleased += (sender, inputName) =>
                        {
                            ControlsManager.PlayInputSound();
                            int index = cG.BaseControlGroup.Modes.IndexOf(mode);
                            if (cG.Enabled && cG.Index == index)
                                cG.Toggle();
                            else
                            {
                                cG.Index = index;
                                cG.Enabled = true;
                            }
                            UpdateAudio();
                        };
                    }                    

                    if (ControlsManager.RegisterInput(mode.Hold))
                    {
                        ControlsManager.Inputs[mode.Hold].OnInputPressed += (sender, inputName) =>
                        {
                            int index = cG.BaseControlGroup.Modes.IndexOf(mode);
                            if (cG.Enabled && cG.Index != index)
                            {
                                cG.ManualingEnabled = true;
                                cG.ManualingIndex = cG.Index;
                                cG.Index = index;
                                UpdateAudio();
                            }
                            else if (!cG.Enabled)
                            {
                                cG.ManualingEnabled = true;
                                cG.ManualingIndex = -1;
                                cG.Toggle();
                                cG.Index = index;
                                UpdateAudio();
                            }
                        };

                        ControlsManager.Inputs[mode.Hold].OnInputReleased += (sender, inputName) =>
                        {
                            if (cG.ManualingEnabled)
                            {
                                if (cG.ManualingIndex == -1)
                                    cG.Toggle();
                                else
                                    cG.Index = cG.ManualingIndex;
                                cG.ManualingEnabled = false;
                                cG.ManualingIndex = 0;
                                UpdateAudio();
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
                    ClearAll();
                };
            }

            // Interior Light
            if (ControlsManager.RegisterInput("INTLT"))
            {
                ControlsManager.Inputs["INTLT"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    InteriorLight = !InteriorLight;
                    Vehicle.IsInteriorLightOn = InteriorLight;
                };
            }

            // Indicator Left
            if (ControlsManager.RegisterInput("INDL"))
            {
                ControlsManager.Inputs["INDL"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    IndStatus = IndStatus == VehicleIndicatorLightsStatus.LeftOnly ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.LeftOnly;
                    Vehicle.IndicatorLightsStatus = IndStatus;
                };
            }

            // Indicator Right
            if (ControlsManager.RegisterInput("INDR"))
            {
                ControlsManager.Inputs["INDR"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    IndStatus = IndStatus == VehicleIndicatorLightsStatus.RightOnly ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.RightOnly;
                    Vehicle.IndicatorLightsStatus = IndStatus;
                };
            }

            // Hazards
            if (ControlsManager.RegisterInput("HZRD"))
            {
                ControlsManager.Inputs["HZRD"].OnInputReleased += (sender, inputName) =>
                {
                    ControlsManager.PlayInputSound();

                    IndStatus = IndStatus == VehicleIndicatorLightsStatus.Both ? VehicleIndicatorLightsStatus.Off : VehicleIndicatorLightsStatus.Both;
                    Vehicle.IndicatorLightsStatus = IndStatus;
                };
            }
        }

        public void UpdateLights()
        {
            if (!Vehicle) return;

            // Start with no modes activated
            List<LightMode> modes = new();

            // Go through all modes in order, and enable modes based on trigger criteria
            foreach (var instance in LightModes.Values.Where(x => x.EnabledByTrigger))
                modes.Add(instance.BaseMode);

            // Go through all control groups in order, and enable modes based on control inputs
            foreach (var instance in LightControlGroups.Values.Where(x => x.Enabled))
            {
                // If group is exclusive, disables every mode if enabled previously
                if (instance.BaseControlGroup.Exclusive)
                {
                    List<string> cGExclusiveModes = new();
                    foreach (var modeSelection in instance.BaseControlGroup.Modes)
                        cGExclusiveModes.AddRange(modeSelection.Modes);

                    modes.RemoveAll(x => cGExclusiveModes.Contains(x.Name));
                }

                foreach (var modeName in instance.BaseControlGroup.Modes[instance.Index].Modes)
                    modes.Add(LightModes[modeName].BaseMode);
            }

            // Remove modes that are disabled
            foreach (var mode in modes.ToArray())
            {
                if (!mode.Requirements.GetInstance(this).LastTriggered) modes.RemoveAll(m => m == mode);
            }

            // Sort modes by the order they initially appear in the config file
            modes = modes.OrderBy(d => dlsModel.Modes.IndexOf(d)).ToList();

            // Disable modes that are not in the new list of enabled modes
            foreach (var item in LightModes.Values)
                if (item.Enabled && !modes.Contains(item.BaseMode))
                    item.Enabled = false;

            // If no active modes, clears EL and disables siren
            if (modes.Count == 0)
            {
                if (Vehicle.IsPlayerVehicle() || !Vehicle.IsSirenOn) LightsOn = false;
                this.ApplyLightModes(new List<LightMode>());
                //AudioController.KillSirens(managedVehicle);
                return;
            }

            // Turns on vehicle siren
            if (Vehicle.IsPlayerVehicle() || Vehicle.IsSirenOn) LightsOn = true;

            // Sets EL with appropriate modes
            this.ApplyLightModes(modes);
        }

        public void UpdateAudio()
        {
            if (!Vehicle) return;
            
            // Start with no modes activated
            List<AudioMode> modes = new();

            // Go through all control groups in order, and enable modes based on control inputs
            foreach (var instance in AudioControlGroups.Values.Where(x => x.Enabled))
            {
                // If group is exclusive, disables every mode if enabled previously
                if (instance.BaseControlGroup.Exclusive)
                {
                    List<string> cGExclusiveModes = new();
                    foreach (var modeSelection in instance.BaseControlGroup.Modes)
                        cGExclusiveModes.AddRange(modeSelection.Modes);

                    modes.RemoveAll(x => cGExclusiveModes.Contains(x.Name));
                }

                foreach (var modeName in instance.BaseControlGroup.Modes[instance.Index].Modes)
                    modes.Add(AudioModes[modeName].BaseMode);
            }

            var audioModes = modes.Select(x => x.Name).ToList();
            foreach (var item in AudioModes.Values)
                if (item.Enabled && !audioModes.Contains(item.BaseMode.Name))
                {
                    item.Enabled = false;
                    Audio.StopMode(this, item.BaseMode.Name);
                }
                    

            // If no active modes
            if (modes.Count == 0)
            {
                SirenOn = false;
                Vehicle.IsSirenSilent = true;
                return;
            }

            SirenOn = true;
            Vehicle.IsSirenSilent = false;

            modes = modes.OrderBy(d => dlsModel.AudioSettings.AudioModes.IndexOf(d)).ToList();

            // Sets EL with appropriate modes
            foreach (var audioMode in modes)
            {
                AudioModes[audioMode.Name].Enabled = true;
                Audio.PlayMode(this, audioMode);
            }
        }
    }
}
