using DLSv2.Core;
using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
using DLSv2.Utils;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DLSv2.Threads
{
    class PlayerManager
    {
        private static Vehicle prevVehicle;
        private static ManagedVehicle currentManaged;

        public static bool registeredKeys = false;

        internal static void MainLoop()
        {
            while (true)
            {
                Ped playerPed = Game.LocalPlayer.Character;
                if (playerPed.IsInAnyVehicle(false) && playerPed.CurrentVehicle.GetPedOnSeat(-1) == playerPed
                    && playerPed.CurrentVehicle.IsDLS())
                {
                    Vehicle veh = playerPed.CurrentVehicle;
                    OldControlsManager.DisableControls();

                    // Registers new Vehicle
                    if (currentManaged == null || prevVehicle != veh)
                    {
                        currentManaged = veh.GetActiveVehicle();
                        prevVehicle = veh;
                        veh.IsInteriorLightOn = false;
                        OldControlsManager.ClearKeys();
                        LightController.Update(currentManaged);
                    }

                    // Registers keys
                    if (!registeredKeys)
                    {
                        // Light Control group and modes keys
                        foreach (ControlGroup cG in ControlGroupManager.ControlGroups[veh.Model].Values)
                        {
                            bool hasToggle = cG.Toggle != null && Settings.INI.DoesSectionExist(cG.Toggle);
                            bool hasCycle = cG.Cycle != null && Settings.INI.DoesSectionExist(cG.Cycle);

                            if (hasToggle && hasCycle)
                            {
                                OldControlsManager.RegisterInput(cG.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                    LightController.Update(currentManaged);
                                });

                                OldControlsManager.RegisterInput(cG.Cycle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    if (modified) ControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                    else ControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                    LightController.Update(currentManaged);
                                });
                            }
                            else if (hasToggle && !hasCycle)
                            {
                                OldControlsManager.RegisterInput(cG.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name, true);
                                    LightController.Update(currentManaged);
                                });
                            }
                            else if (!hasToggle && hasCycle)
                            {
                                OldControlsManager.RegisterInput(cG.Cycle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    if (modified) ControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                    else ControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                    LightController.Update(currentManaged);
                                });
                            }

                            foreach (ModeSelection mode in cG.Modes)
                            {
                                if (mode.Toggle == null || !Settings.INI.DoesSectionExist(mode.Toggle)) continue;
                                OldControlsManager.RegisterInput(mode.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    int index = cG.Modes.IndexOf(mode);
                                    if (currentManaged.LightControlGroups[cG.Name].Item1 && currentManaged.LightControlGroups[cG.Name].Item2 == index)
                                        ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                    else
                                        ControlGroupManager.SetControlGroupIndex(currentManaged, cG.Name, index);
                                    LightController.Update(currentManaged);
                                });
                            }
                        }

                        // Audio Control group and modes keys
                        foreach (AudioControlGroup cG in AudioControlGroupManager.ControlGroups[veh.Model].Values)
                        {
                            bool hasToggle = cG.Toggle != null && Settings.INI.DoesSectionExist(cG.Toggle);
                            bool hasCycle = cG.Cycle != null && Settings.INI.DoesSectionExist(cG.Cycle);

                            if (hasToggle && hasCycle)
                            {
                                OldControlsManager.RegisterInput(cG.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    AudioControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                    AudioController.Update(currentManaged);
                                });

                                OldControlsManager.RegisterInput(cG.Cycle, (pressed, modified, args) =>
                                {
                                    if (!pressed || !currentManaged.AudioControlGroups[cG.Name].Item1) return;
                                    OldControlsManager.PlayInputSound();
                                    if (modified) AudioControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                    else AudioControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                    AudioController.Update(currentManaged);
                                });
                            }
                            else if (hasToggle && !hasCycle)
                            {
                                OldControlsManager.RegisterInput(cG.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    AudioControlGroupManager.ToggleControlGroup(currentManaged, cG.Name, true);
                                    AudioController.Update(currentManaged);
                                });
                            }
                            else if (!hasToggle && hasCycle)
                            {
                                OldControlsManager.RegisterInput(cG.Cycle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    OldControlsManager.PlayInputSound();
                                    if (modified) AudioControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name, true);
                                    else AudioControlGroupManager.NextInControlGroup(currentManaged, cG.Name, cycleOnly: true);
                                    AudioController.Update(currentManaged);
                                });
                            }

                            foreach (AudioModeSelection mode in cG.Modes)
                            {
                                if (mode.Toggle != null && Settings.INI.DoesSectionExist(mode.Toggle))
                                {
                                    OldControlsManager.RegisterInput(mode.Toggle, (pressed, modified, args) =>
                                    {
                                        if (!pressed) return;
                                        OldControlsManager.PlayInputSound();
                                        int index = cG.Modes.IndexOf(mode);
                                        if (currentManaged.AudioControlGroups[cG.Name].Item1 && currentManaged.AudioControlGroups[cG.Name].Item2 == index)
                                            AudioControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                        else
                                            AudioControlGroupManager.SetControlGroupIndex(currentManaged, cG.Name, index);
                                        AudioController.Update(currentManaged);
                                    });
                                }
                                
                                if (mode.Hold != null && Settings.INI.DoesSectionExist(mode.Hold))
                                {
                                    OldControlsManager.RegisterInput(mode.Hold, (pressed, modified, args) =>
                                    {
                                        if (pressed)
                                        {
                                            int index = cG.Modes.IndexOf(mode);
                                            if (currentManaged.AudioControlGroups[cG.Name].Item1 && currentManaged.AudioControlGroups[cG.Name].Item2 != index)
                                            {
                                                currentManaged.AudioCGManualing[cG.Name] = new Tuple<bool, int>(true, currentManaged.AudioControlGroups[cG.Name].Item2);
                                                AudioControlGroupManager.SetControlGroupIndex(currentManaged, cG.Name, index);
                                                AudioController.Update(currentManaged);                                                
                                            }
                                            else if (!currentManaged.AudioControlGroups[cG.Name].Item1)
                                            {
                                                currentManaged.AudioCGManualing[cG.Name] = new Tuple<bool, int>(true, -1);
                                                AudioControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                                AudioControlGroupManager.SetControlGroupIndex(currentManaged, cG.Name, index);
                                                AudioController.Update(currentManaged);
                                            }                                      
                                        }
                                        else
                                        {
                                            if (currentManaged.AudioCGManualing[cG.Name].Item1)
                                            {
                                                if (currentManaged.AudioCGManualing[cG.Name].Item2 == -1)
                                                    AudioControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                                else
                                                    AudioControlGroupManager.SetControlGroupIndex(currentManaged, cG.Name, currentManaged.AudioCGManualing[cG.Name].Item2);
                                                currentManaged.AudioCGManualing[cG.Name] = new Tuple<bool, int>(false, 0);
                                                AudioController.Update(currentManaged);
                                            }
                                        }
                                        
                                    }, true);
                                }
                            }
                        }

                        // Toggle Keys Locked
                        OldControlsManager.RegisterInput("LOCKALL", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            OldControlsManager.PlayInputSound();
                            OldControlsManager.KeysLocked = !OldControlsManager.KeysLocked;
                        });

                        // Kills all lights and audio
                        OldControlsManager.RegisterInput("KILLALL", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            OldControlsManager.PlayInputSound();
                            // Clears light modes
                            foreach (string key in currentManaged.LightModes.Keys.ToList())
                                currentManaged.LightModes[key] = false;

                            // Clears light control groups
                            foreach (string key in currentManaged.LightControlGroups.Keys.ToList())
                                currentManaged.LightControlGroups[key] = new Tuple<bool, int>(false, 0);

                            // Updates lights
                            LightController.Update(currentManaged);

                            // Clears audio control groups
                            foreach (string key in currentManaged.AudioControlGroups.Keys.ToList())
                                currentManaged.AudioControlGroups[key] = new Tuple<bool, int>(false, 0);

                            // Updates audio
                            AudioController.Update(currentManaged);
                        });

                        // Interior Light
                        OldControlsManager.RegisterInput("INTLT", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            OldControlsManager.PlayInputSound();
                            currentManaged.InteriorLight = !currentManaged.InteriorLight;
                            GenericLights.SetInteriorLight(veh, currentManaged.InteriorLight);
                        });

                        // Indicator Left
                        OldControlsManager.RegisterInput("INDL", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            OldControlsManager.PlayInputSound();
                            if (currentManaged.IndStatus == VehicleIndicatorLightsStatus.LeftOnly)
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Off;
                            else
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.LeftOnly;

                            GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                        });

                        // Indicator Right
                        OldControlsManager.RegisterInput("INDR", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            OldControlsManager.PlayInputSound();
                            if (currentManaged.IndStatus == VehicleIndicatorLightsStatus.RightOnly)
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Off;
                            else
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.RightOnly;

                            GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                        });

                        // Hazards
                        OldControlsManager.RegisterInput("HZRD", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            OldControlsManager.PlayInputSound();
                            if (currentManaged.IndStatus == VehicleIndicatorLightsStatus.Both)
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Off;
                            else
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Both;

                            GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                        });

                        registeredKeys = true;
                    }

                    // Dev Mode UI
                    if (Settings.DEVMODE)
                    {
                        string controlGroups = "CGs: ";
                        List<ControlGroup> cGs = ControlGroupManager.ControlGroups[veh.Model].Values.ToList();
                        foreach (ControlGroup cG in cGs)
                        {
                            if (currentManaged.LightControlGroups[cG.Name].Item1)
                            {
                                controlGroups += "~g~" + cG.Name + " (";
                                List<string> cGModes = ControlGroupManager.ControlGroups[veh.Model][cG.Name].Modes[currentManaged.LightControlGroups[cG.Name].Item2].Modes;
                                foreach (string mode in cGModes)
                                {
                                    controlGroups += mode;
                                    if (cGModes.IndexOf(mode) != cGModes.Count - 1) controlGroups += " + ";
                                }
                                controlGroups += ")";
                            }
                            else
                                controlGroups += "~r~" + cG.Name;

                            if (cGs.IndexOf(cG) != cGs.Count - 1) controlGroups += "~w~~s~, ";
                        }

                        NativeFunction.Natives.SET_TEXT_FONT(4);
                        NativeFunction.Natives.SET_TEXT_SCALE(1.0f, 0.6f);
                        NativeFunction.Natives.BEGIN_TEXT_COMMAND_DISPLAY_TEXT("STRING");
                        NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(controlGroups);
                        NativeFunction.Natives.END_TEXT_COMMAND_DISPLAY_TEXT(0, 0);
                    }

                    // Adds Brake Light Functionality
                    if (Settings.BRAKELIGHTS && NativeFunction.Natives.IS_VEHICLE_STOPPED<bool>(veh))
                        NativeFunction.Natives.SET_VEHICLE_BRAKE_LIGHTS(veh, true);
                }
                else if (registeredKeys)
                    OldControlsManager.ClearKeys();
                GameFiber.Yield();
            }
        }
    }
}
