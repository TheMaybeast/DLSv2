using DLSv2.Core;
using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
using DLSv2.Utils;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace DLSv2.Threads
{
    class PlayerController
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
                            ModeManager.Update(currentManaged);
                        }

                        // Registers ControlGroup keys for DLS vehicles
                        if (!registeredKeys && veh.IsDLS())
                        {
                            foreach (ControlGroup cG in ControlGroupManager.ControlGroups[veh.Model].Values)
                            {
                                Keys key = Settings.INI.ReadEnum("Keyboard", cG.Name, Keys.None);
                                if (key != Keys.None)
                                {
                                    ControlsManager.RegisterKey(key, (modified, args) =>
                                    {
                                        if (modified) ControlGroupManager.PreviousInControlGroup(currentManaged, cG.Name);
                                        else ControlGroupManager.NextInControlGroup(currentManaged, cG.Name);
                                    });
                                }
                            }
                            registeredKeys = true;
                        }

                        // Adds Brake Light Functionality
                        if (!currentManaged.Blackout && NativeFunction.Natives.IS_VEHICLE_STOPPED<bool>(veh))
                            NativeFunction.Natives.SET_VEHICLE_BRAKE_LIGHTS(veh, true);

                        if (!Game.IsPaused)
                        {
                            // Siren Controls
                            if (veh.HasSiren)
                            {
                                // Toggle Aux Siren
                                if (Controls.IsDLSControlDown(DLSControls.SIREN_AUX))
                                {
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
                                if (currentManaged.CurrentModes.Count > 0 || currentManaged.LightsOn)
                                {
                                    if (Controls.IsDLSControlDown(DLSControls.SIREN_TOGGLE))
                                    {
                                        NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
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
                                        SirenController.MoveDownStage(currentManaged);
                                    // Move Up Siren Stage
                                    else if (Controls.IsDLSControlDown(DLSControls.SIREN_CYCLE))
                                        SirenController.MoveUpStage(currentManaged);
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
                                NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

                                if (currentManaged.IndStatus == IndStatus.Left)
                                    currentManaged.IndStatus = IndStatus.Off;
                                else
                                    currentManaged.IndStatus = IndStatus.Left;

                                GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                            }

                            // Right Indicator
                            if (Controls.IsDLSControlDown(DLSControls.LIGHT_INDR))
                            {
                                NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

                                if (currentManaged.IndStatus == IndStatus.Right)
                                    currentManaged.IndStatus = IndStatus.Off;
                                else
                                    currentManaged.IndStatus = IndStatus.Right;

                                GenericLights.SetIndicator(veh, currentManaged.IndStatus);
                            }

                            // Hazards
                            if (Controls.IsDLSControlDown(DLSControls.LIGHT_HZRD))
                            {
                                NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

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
                            }
                        }
                    }
                }
                else
                {
                    ControlsManager.ClearKeys();
                    registeredKeys = false;
                }                    
                GameFiber.Yield();
            }
        }
    }
}
