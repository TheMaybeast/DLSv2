using DLSv2.Core;
using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
using DLSv2.Utils;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DLSv2.Threads
{
    class PlayerManager
    {
        private static Vehicle prevVehicle;
        private static ManagedVehicle currentManaged;

        public static bool actv_manu;
        public static bool actv_horn;

        public static bool registeredKeys;

        internal static void MainLoop()
        {
            while (true)
            {
                Ped playerPed = Game.LocalPlayer.Character;
                if (playerPed.IsInAnyVehicle(false))
                {
                    Vehicle veh = playerPed.CurrentVehicle;
                    // Inside here basic vehicle functionality will be available
                    // eg. Indicators and Internal Lights
                    if (veh.GetPedOnSeat(-1) == playerPed)
                    {
                        Controls.DisableControls();

                        // Registers new Vehicle
                        if (currentManaged == null || prevVehicle != veh)
                        {
                            currentManaged = veh.GetActiveVehicle();
                            prevVehicle = veh;
                            veh.IsInteriorLightOn = false;
                            ControlsManager.ClearKeys();
                            registeredKeys = false;
                            if (veh.IsDLS())
                                LightController.Update(currentManaged);
                        }

                        // Registers ControlGroup keys for DLS vehicles
                        if (!registeredKeys && veh.IsDLS())
                        {
                            foreach (ControlGroup cG in ControlGroupManager.ControlGroups[veh.Model].Values)
                            {
                                bool hasToggle = cG.Toggle != null && Settings.INI.DoesSectionExist(cG.Toggle);
                                bool hasCycle = cG.Cycle != null && Settings.INI.DoesSectionExist(cG.Cycle);

                                if (hasToggle && hasCycle)
                                {
                                    ControlsManager.RegisterInput(new Input
                                    {
                                        Name = cG.Toggle,
                                        Key = Settings.INI.ReadEnum(cG.Toggle, "Key", Keys.None),
                                        ControllerButton = Settings.INI.ReadEnum(cG.Toggle, "ControllerButton", ControllerButtons.None)
                                    }, (modified, args) =>
                                    {
                                        ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                        LightController.Update(currentManaged);
                                    });

                                    ControlsManager.RegisterInput(new Input
                                    {
                                        Name = cG.Cycle,
                                        Key = Settings.INI.ReadEnum(cG.Cycle, "Key", Keys.None),
                                        ControllerButton = Settings.INI.ReadEnum(cG.Cycle, "ControllerButton", ControllerButtons.None)
                                    }, (modified, args) =>
                                    {
                                        if (modified) ControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                        else ControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                        LightController.Update(currentManaged);
                                    });
                                }
                                else if (hasToggle && !hasCycle)
                                {
                                    ControlsManager.RegisterInput(new Input
                                    {
                                        Name = cG.Toggle,
                                        Key = Settings.INI.ReadEnum(cG.Toggle, "Key", Keys.None),
                                        ControllerButton = Settings.INI.ReadEnum(cG.Toggle, "ControllerButton", ControllerButtons.None)
                                    }, (modified, args) =>
                                    {
                                        ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name, true);
                                        LightController.Update(currentManaged);
                                    });
                                }
                                else if (!hasToggle && hasCycle)
                                {
                                    ControlsManager.RegisterInput(new Input
                                    {
                                        Name = cG.Cycle,
                                        Key = Settings.INI.ReadEnum(cG.Cycle, "Key", Keys.None),
                                        ControllerButton = Settings.INI.ReadEnum(cG.Cycle, "ControllerButton", ControllerButtons.None)
                                    }, (modified, args) =>
                                    {
                                        if (modified) ControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                        else ControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                        LightController.Update(currentManaged);
                                    });
                                }

                                foreach (ModeSelection mode in cG.Modes)
                                {
                                    if (mode.Toggle == null || !Settings.INI.DoesSectionExist(mode.Toggle)) continue;
                                    ControlsManager.RegisterInput(new Input
                                    {
                                        Name = mode.Toggle,
                                        Key = Settings.INI.ReadEnum(mode.Toggle, "Key", Keys.None),
                                        ControllerButton = Settings.INI.ReadEnum(mode.Toggle, "ControllerButton", ControllerButtons.None)
                                    }, (modified, args) =>
                                    {
                                        int index = cG.Modes.IndexOf(mode);
                                        if (currentManaged.ControlGroups[cG.Name].Item1 && currentManaged.ControlGroups[cG.Name].Item2 == index)
                                            ControlGroupManager.ToggleControlGroup(currentManaged, cG.Name);
                                        else
                                            ControlGroupManager.SetControlGroupIndex(currentManaged, cG.Name, index);
                                        LightController.Update(currentManaged);
                                    });
                                }
                            }
                            registeredKeys = true;
                        }
                        
                        if (Settings.SET_DEVMODE && veh.IsDLS())
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

                        if (!Game.IsPaused)
                        {
                            // Toggle Keys Locked
                            if (Controls.IsDLSControlDown(DLSControls.GEN_LOCKALL))
                            {
                                ControlsManager.PlayInputSound();
                                Controls.KeysLocked = !Controls.KeysLocked;
                            }

                            // Kills all lights
                            if (Controls.IsDLSControlDown(DLSControls.GEN_KILLALL))
                            {
                                ControlsManager.PlayInputSound();

                                // Clears modes
                                foreach (string key in currentManaged.Modes.Keys.ToList())
                                    currentManaged.Modes[key] = false;

                                // Clears control groups
                                foreach (string key in currentManaged.ControlGroups.Keys.ToList())
                                    currentManaged.ControlGroups[key] = new Tuple<bool, int>(false, 0);

                                // Updates lights
                                LightController.Update(currentManaged);
                            }

                            // Siren Controls
                            if (veh.HasSiren)
                            {
                                // Toggle Aux Siren
                                if (Controls.IsDLSControlDown(DLSControls.SIREN_AUX))
                                {
                                    ControlsManager.PlayInputSound();
                                    if (currentManaged.AuxOn)
                                    {
                                        SoundManager.ClearTempSoundID(currentManaged.AuxID);
                                        currentManaged.AuxOn = false;
                                    }
                                    else
                                    {
                                        currentManaged.AuxID = SoundManager.TempSoundID();
                                        currentManaged.AuxOn = true;
                                        List<Tone> sirenTones = SirenController.SirenTones.ContainsKey(veh.Model) ? SirenController.SirenTones[veh.Model] : SirenController.DefaultSirenTones;
                                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(currentManaged.AuxID, sirenTones[0].ToneHash, currentManaged.Vehicle, 0, 0, 0);
                                    }
                                }

                                // Siren Switches
                                if (currentManaged.LightsOn)
                                {
                                    if (Controls.IsDLSControlDown(DLSControls.SIREN_TOGGLE))
                                    {
                                        ControlsManager.PlayInputSound();
                                        switch (currentManaged.SirenOn)
                                        {
                                            case true:
                                                SirenController.KillSirens(currentManaged);
                                                break;
                                            case false:
                                                currentManaged.SirenOn = true;
                                                SirenController.Update(currentManaged);
                                                break;
                                        }
                                    }
                                }
                                if (currentManaged.SirenOn)
                                {
                                    // Move Down Siren Stage
                                    if (Controls.IsDLSControlDownWithModifier(DLSControls.SIREN_CYCLE))
                                    {
                                        ControlsManager.PlayInputSound();
                                        SirenController.MoveDownStage(currentManaged);
                                    }                                        
                                    // Move Up Siren Stage
                                    else if (Controls.IsDLSControlDown(DLSControls.SIREN_CYCLE))
                                    {
                                        ControlsManager.PlayInputSound();
                                        SirenController.MoveUpStage(currentManaged);
                                    }
                                }

                                // Manual                                                              
                                if (!currentManaged.SirenOn)
                                {
                                    if (Controls.IsDLSControlDown(DLSControls.SIREN_MAN))
                                        actv_manu = true;
                                    else
                                        actv_manu = false;
                                }
                                else
                                    actv_manu = false;

                                // Horn
                                if (Controls.IsDLSControlDown(DLSControls.SIREN_HORN))
                                    actv_horn = true;
                                else
                                    actv_horn = false;

                                // Manage Horn and Manual siren
                                int hman_state = 0;
                                if (actv_horn && !actv_manu)
                                    hman_state = 1;
                                else if (!actv_horn && actv_manu)
                                    hman_state = 2;
                                else if (actv_horn && actv_manu)
                                    hman_state = 3;

                                SirenController.SetAirManuState(currentManaged, hman_state);
                            }

                            // Left Indicator
                            if (Controls.IsDLSControlDown(DLSControls.LIGHT_INDL))
                            {
                                ControlsManager.PlayInputSound();

                                if (currentManaged.IndStatus == IndStatus.Left)
                                    currentManaged.IndStatus = IndStatus.Off;
                                else
                                    currentManaged.IndStatus = IndStatus.Left;

                                GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                            }

                            // Right Indicator
                            if (Controls.IsDLSControlDown(DLSControls.LIGHT_INDR))
                            {
                                ControlsManager.PlayInputSound();

                                if (currentManaged.IndStatus == IndStatus.Right)
                                    currentManaged.IndStatus = IndStatus.Off;
                                else
                                    currentManaged.IndStatus = IndStatus.Right;

                                GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                            }

                            // Hazards
                            if (Controls.IsDLSControlDown(DLSControls.LIGHT_HZRD))
                            {
                                ControlsManager.PlayInputSound();

                                if (currentManaged.IndStatus == IndStatus.Hazard)
                                    currentManaged.IndStatus = IndStatus.Off;
                                else
                                    currentManaged.IndStatus = IndStatus.Hazard;

                                GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                            }

                            // Interior Light
                            if (Controls.IsDLSControlDown(DLSControls.LIGHT_INTLT))
                            {
                                currentManaged.InteriorLight = !currentManaged.InteriorLight;
                                GenericLights.SetInteriorLight(veh, currentManaged.InteriorLight);

                                ControlsManager.PlayInputSound();
                            }
                        }
                    }
                }
                else if (registeredKeys)
                {
                    ControlsManager.ClearKeys();
                    registeredKeys = false;
                }                    
                GameFiber.Yield();
            }
        }
    }
}
