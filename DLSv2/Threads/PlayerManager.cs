using DLSv2.Core;
using DLSv2.Core.Lights;
using DLSv2.Utils;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    Controls.DisableControls();

                    // Registers new Vehicle
                    if (currentManaged == null || prevVehicle != veh)
                    {
                        currentManaged = veh.GetActiveVehicle();
                        prevVehicle = veh;
                        veh.IsInteriorLightOn = false;
                        ControlsManager.ClearKeys();
                        LightController.Update(currentManaged);
                    }

                    // Registers keys
                    if (!registeredKeys)
                    {
                        // Control group and modes keys
                        foreach (ControlGroup cG in ControlGroupManager.ControlGroups[veh.Model].Values)
                        {
                            bool hasToggle = cG.Toggle != null && Settings.INI.DoesSectionExist(cG.Toggle);
                            bool hasCycle = cG.Cycle != null && Settings.INI.DoesSectionExist(cG.Cycle);

                            if (hasToggle && hasCycle)
                            {
                                ControlsManager.RegisterInput(cG.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    ControlsManager.PlayInputSound();
                                    ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                    LightController.Update(currentManaged);
                                });

                                ControlsManager.RegisterInput(cG.Cycle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    ControlsManager.PlayInputSound();
                                    if (modified) ControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                    else ControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                    LightController.Update(currentManaged);
                                });
                            }
                            else if (hasToggle && !hasCycle)
                            {
                                ControlsManager.RegisterInput(cG.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    ControlsManager.PlayInputSound();
                                    ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name, true);
                                    LightController.Update(currentManaged);
                                });
                            }
                            else if (!hasToggle && hasCycle)
                            {
                                ControlsManager.RegisterInput(cG.Cycle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    ControlsManager.PlayInputSound();
                                    if (modified) ControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                    else ControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                    LightController.Update(currentManaged);
                                });
                            }

                            foreach (ModeSelection mode in cG.Modes)
                            {
                                if (mode.Toggle == null || !Settings.INI.DoesSectionExist(mode.Toggle)) continue;
                                ControlsManager.RegisterInput(mode.Toggle, (pressed, modified, args) =>
                                {
                                    if (!pressed) return;
                                    ControlsManager.PlayInputSound();
                                    int index = cG.Modes.IndexOf(mode);
                                    if (currentManaged.ControlGroups[cG.Name].Item1 && currentManaged.ControlGroups[cG.Name].Item2 == index)
                                        ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                    else
                                        ControlGroupManager.SetControlGroupIndex(currentManaged, cG.Name, index);
                                    LightController.Update(currentManaged);
                                });
                            }
                        }

                        // Toggle Keys Locked
                        ControlsManager.RegisterInput("LOCKALL", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            ControlsManager.PlayInputSound();
                            Controls.KeysLocked = !Controls.KeysLocked;
                        });

                        // Kills all lights
                        ControlsManager.RegisterInput("KILLALL", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            ControlsManager.PlayInputSound();
                            // Clears modes
                            foreach (string key in currentManaged.Modes.Keys.ToList())
                                currentManaged.Modes[key] = false;

                            // Clears control groups
                            foreach (string key in currentManaged.ControlGroups.Keys.ToList())
                                currentManaged.ControlGroups[key] = new Tuple<bool, int>(false, 0);

                            // Updates lights
                            LightController.Update(currentManaged);
                        });

                        // Interior Light
                        ControlsManager.RegisterInput("INTLT", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            ControlsManager.PlayInputSound();
                            currentManaged.InteriorLight = !currentManaged.InteriorLight;
                            GenericLights.SetInteriorLight(veh, currentManaged.InteriorLight);
                        });

                        // Indicator Left
                        ControlsManager.RegisterInput("INDL", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            ControlsManager.PlayInputSound();
                            if (currentManaged.IndStatus == VehicleIndicatorLightsStatus.LeftOnly)
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Off;
                            else
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.LeftOnly;

                            GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                        });

                        // Indicator Right
                        ControlsManager.RegisterInput("INDR", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            ControlsManager.PlayInputSound();
                            if (currentManaged.IndStatus == VehicleIndicatorLightsStatus.RightOnly)
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Off;
                            else
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.RightOnly;

                            GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                        });

                        // Hazards
                        ControlsManager.RegisterInput("HZRD", (pressed, modified, args) =>
                        {
                            if (!pressed) return;
                            ControlsManager.PlayInputSound();
                            if (currentManaged.IndStatus == VehicleIndicatorLightsStatus.Both)
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Off;
                            else
                                currentManaged.IndStatus = VehicleIndicatorLightsStatus.Both;

                            GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                        });

                        registeredKeys = true;
                    }

                    // Dev Mode UI
                    if (Settings.SET_DEVMODE)
                    {
                        string controlGroups = "CGs: ";
                        List<ControlGroup> cGs = ControlGroupManager.ControlGroups[veh.Model].Values.ToList();
                        foreach (ControlGroup cG in cGs)
                        {
                            if (currentManaged.ControlGroups[cG.Name].Item1)
                            {
                                controlGroups += "~g~" + cG.Name + " (";
                                List<string> cGModes = ControlGroupManager.ControlGroups[veh.Model][cG.Name].Modes[currentManaged.ControlGroups[cG.Name].Item2].Modes;
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

                        string modes = "Modes: ";
                        foreach (Mode mode in ModeManager.Modes[veh.Model].Values)
                        {
                            if (currentManaged.Modes[mode.Name])
                                modes += "~g~" + mode.Name;
                            else
                                modes += "~r~" + mode.Name;

                            modes += "~w~~s~, ";
                        }

                        NativeFunction.Natives.SET_TEXT_FONT(4);
                        NativeFunction.Natives.SET_TEXT_SCALE(1.0f, 0.6f);
                        NativeFunction.Natives.BEGIN_TEXT_COMMAND_DISPLAY_TEXT("STRING");
                        NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(modes);
                        NativeFunction.Natives.END_TEXT_COMMAND_DISPLAY_TEXT(0, 0.03f);
                    }

                    // Adds Brake Light Functionality
                    if (Settings.SET_BRAKELIGHTS && NativeFunction.Natives.IS_VEHICLE_STOPPED<bool>(veh))
                        NativeFunction.Natives.SET_VEHICLE_BRAKE_LIGHTS(veh, true);
                }
                else if (registeredKeys)
                    ControlsManager.ClearKeys();
                GameFiber.Yield();
            }
        }
    }
}
