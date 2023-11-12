using DLSv2.Core;
using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
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
                    ControlsManager.DisableControls();

                    // Registers new Vehicle
                    if (currentManaged == null || prevVehicle != veh)
                    {
                        currentManaged = veh.GetActiveVehicle();
                        prevVehicle = veh;
                        veh.IsInteriorLightOn = false;
                        ControlsManager.ClearInputs();
                        LightController.Update(currentManaged);
                        veh.IsSirenSilent = true;
                    }

                    // Registers keys
                    if (!registeredKeys)
                    {
                        currentManaged.RegisterInputs();
                        registeredKeys = true;
                    }

                    if (!currentManaged.SirenOn && !veh.IsSirenSilent)
                    {
                        AudioControlGroupManager.ToggleControlGroup(currentManaged, currentManaged.AudioControlGroups.First().Key);
                        AudioController.Update(currentManaged);
                    }
                    else if (currentManaged.SirenOn && veh.IsSirenSilent)
                    {
                        // Clears audio control groups
                        foreach (string key in currentManaged.AudioControlGroups.Keys.ToList())
                            currentManaged.AudioControlGroups[key] = new Tuple<bool, int>(false, 0);

                        // Updates audio
                        AudioController.Update(currentManaged);
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
                {
                    ControlsManager.ClearInputs();
                    registeredKeys = false;
                }
                    
                GameFiber.Yield();
            }
        }
    }
}
