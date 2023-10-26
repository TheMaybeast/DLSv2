using DLSv2.Utils;
using Rage;
using Rage.Native;

namespace DLSv2.Threads
{
    class PlayerController
    {
        private static Vehicle prevVehicle;
        private static ManagedVehicle activeVehicle;
        public static bool actv_manu;
        public static bool actv_horn;

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
                        Game.DisableControlAction(0, (GameControl)Settings.CON_HZRD, true);
                        Game.DisableControlAction(0, (GameControl)Settings.CON_INDRIGHT, true);
                        Game.DisableControlAction(0, (GameControl)Settings.CON_INDLEFT, true);

                        // Registers new Vehicle
                        if (activeVehicle == null || prevVehicle != veh)
                        {
                            activeVehicle = veh.GetActiveVehicle();
                            prevVehicle = veh;
                            veh.IsInteriorLightOn = false;
                        }

                        // Adds Brake Light Functionality
                        if (!activeVehicle.Blackout && NativeFunction.Natives.IS_VEHICLE_STOPPED<bool>(veh))
                            NativeFunction.Natives.SET_VEHICLE_BRAKE_LIGHTS(veh, true);

                        if (veh.HasSiren)
                        {
                            Game.DisableControlAction(0, (GameControl)Settings.CON_TOGGLESIREN, true);
                            Game.DisableControlAction(0, (GameControl)Settings.CON_NEXTSIREN, true);
                            Game.DisableControlAction(0, (GameControl)Settings.CON_PREVSIREN, true);
                            Game.DisableControlAction(0, (GameControl)Settings.CON_AUXSIREN, true);
                            Game.DisableControlAction(0, (GameControl)Settings.CON_TOGGLELIGHTS, true);
                            Game.DisableControlAction(0, (GameControl)Settings.CON_INTLT, true);
                            Game.DisableControlAction(0, (GameControl)Settings.CON_HORN, true);

                            if (!Game.IsPaused)
                            {
                                // Toggle lighting
                                if(Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_TOGGLELIGHTS))
                                {
                                    switch (veh.IsSirenOn)
                                    {
                                        case true:
                                            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                            activeVehicle.LightsOn = false;
                                            veh.IsSirenOn = false;
                                            activeVehicle.SirenStage = SirenStage.Off;
                                            Sirens.Update(activeVehicle);
                                            break;
                                        case false:
                                            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                            activeVehicle.LightsOn = true;
                                            veh.IsSirenOn = true;
                                            veh.IsSirenSilent = true;
                                            break;
                                    }
                                }

                                // Toggle Aux Siren
                                if(Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_AUXSIREN))
                                {
                                    if (activeVehicle.AuxOn)
                                    {
                                        Sound.ClearTempSoundID(activeVehicle.AuxID);
                                        activeVehicle.AuxOn = false;
                                    }
                                    else
                                    {
                                        activeVehicle.AuxID = Sound.TempSoundID();
                                        activeVehicle.AuxOn = true;
                                        if (activeVehicle.SoundSet == null)
                                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVehicle.AuxID, "VEHICLES_HORNS_SIREN_1", activeVehicle.Vehicle, 0, 0, 0);
                                        else
                                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVehicle.AuxID, activeVehicle.SoundSet.SirenTones[0], activeVehicle.Vehicle, 0, 0, 0);
                                    }
                                }

                                // Siren Switches
                                if(activeVehicle.LightsOn)
                                {
                                    if(Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_TOGGLESIREN))
                                    {
                                        NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                        if (activeVehicle.SirenStage == SirenStage.Off)
                                            activeVehicle.SirenStage = SirenStage.One;
                                        else
                                            activeVehicle.SirenStage = SirenStage.Off;
                                        Sirens.Update(activeVehicle);
                                    }
                                }
                                if(activeVehicle.SirenStage > SirenStage.Off)
                                {
                                    // Move Up Siren Stage
                                    if(Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_NEXTSIREN))
                                        Sirens.MoveUpStage(activeVehicle);
                                    // Move Down Siren Stage
                                    if (Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_PREVSIREN))
                                        Sirens.MoveDownStage(activeVehicle);
                                }

                                // Interior Light
                                if (Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_INTLT))
                                {
                                    activeVehicle.InteriorLight = !activeVehicle.InteriorLight;
                                    veh.IsInteriorLightOn = activeVehicle.InteriorLight;
                                }

                                // Manual                                                              
                                if (activeVehicle.SirenStage == SirenStage.Off)
                                {
                                    if (Controls.IsDisabledControlPressed(0, (GameControl)Settings.CON_NEXTSIREN))
                                        actv_manu = true;
                                    else
                                        actv_manu = false;
                                }
                                else
                                    actv_manu = false;

                                // Horn
                                if (Controls.IsDisabledControlPressed(0, (GameControl)Settings.CON_HORN))
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

                                Sirens.SetAirManuState(activeVehicle, hman_state);
                            }
                        }

                        // Indicators
                        if (!Game.IsPaused)
                        {
                            // Left Indicator
                            if(Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_INDLEFT) && NativeFunction.Natives.xA571D46727E2B718<bool>(0))
                            {
                                if (activeVehicle.IndStatus == IndStatus.Left)
                                {
                                    activeVehicle.IndStatus = IndStatus.Off;
                                    NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                }
                                else
                                {
                                    activeVehicle.IndStatus = IndStatus.Left;
                                    NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                }
                                Lights.UpdateIndicator(activeVehicle);
                            }

                            // Right Indicator
                            if (Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_INDRIGHT) && NativeFunction.Natives.xA571D46727E2B718<bool>(0))
                            {
                                if (activeVehicle.IndStatus == IndStatus.Right)
                                {
                                    activeVehicle.IndStatus = IndStatus.Off;
                                    NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                }
                                else
                                {
                                    activeVehicle.IndStatus = IndStatus.Right;
                                    NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                }
                                Lights.UpdateIndicator(activeVehicle);
                            }

                            // Hazards
                            if (Controls.IsDisabledControlJustReleased(0, (GameControl)Settings.CON_HZRD) && NativeFunction.Natives.xA571D46727E2B718<bool>(0))
                            {
                                if (activeVehicle.IndStatus == IndStatus.Both)
                                {
                                    activeVehicle.IndStatus = IndStatus.Off;
                                    NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                }
                                else
                                {
                                    activeVehicle.IndStatus = IndStatus.Both;
                                    NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
                                }
                                Lights.UpdateIndicator(activeVehicle);
                            }
                        }
                    }
                }
                GameFiber.Yield();
            }
        }
    }
}
