using System.Collections.Generic;
using System.Linq;
using Rage;
using Rage.Native;
using Rage.Attributes;
using Rage.ConsoleCommands;

namespace DLSv2.Threads
{
    using Core;
    using Core.Lights;
    using Core.Sound;
    using Rage.ConsoleCommands.AutoCompleters;
    using Utils;

    class PlayerManager
    {
        private static Vehicle prevVehicle;
        internal static HashSet<Vehicle> cachedPlayerVehicles = new HashSet<Vehicle>();

        internal static void MainLoop()
        {
            while (true)
            {
                Ped playerPed = Game.LocalPlayer.Character;
                if (playerPed.IsInAnyVehicle(false) && playerPed.CurrentVehicle.Driver == playerPed
                    && playerPed.CurrentVehicle.GetDLS() != null)
                {
                    Vehicle veh = playerPed.CurrentVehicle;
                    cachedPlayerVehicles.Add(veh);

                    ControlsManager.DisableControls();

                    // Registers new Vehicle
                    if (ManagedVehicle.ActivePlayerVehicle == null || prevVehicle != veh)
                    {
                        ManagedVehicle.ActivePlayerVehicle = veh.GetManagedVehicle();
                        ManagedVehicle.ActivePlayerVehicle.RegisterInputs();
                        prevVehicle = veh;
                        veh.IsInteriorLightOn = false;
                        LightController.Update(ManagedVehicle.ActivePlayerVehicle);
                        veh.IsSirenSilent = true;
                    }

                    if (!ManagedVehicle.ActivePlayerVehicle.SirenOn && !veh.IsSirenSilent)
                    {
                        AudioControlGroupManager.ToggleControlGroup(ManagedVehicle.ActivePlayerVehicle, ManagedVehicle.ActivePlayerVehicle.AudioControlGroups.First().Key);
                        AudioController.Update(ManagedVehicle.ActivePlayerVehicle);
                    }
                    else if (ManagedVehicle.ActivePlayerVehicle.SirenOn && veh.IsSirenSilent)
                    {
                        // Clears audio control groups
                        foreach (string key in ManagedVehicle.ActivePlayerVehicle.AudioControlGroups.Keys.ToList())
                            ManagedVehicle.ActivePlayerVehicle.AudioControlGroups[key] = (false, 0);

                        // Updates audio
                        AudioController.Update(ManagedVehicle.ActivePlayerVehicle);
                    }

                    // Dev Mode UI
                    if (Settings.DEVMODE)
                    {
                        string controlGroups = "CGs: ";
                        List<ControlGroup> cGs = ControlGroupManager.ControlGroups[veh.Model].Values.ToList();
                        foreach (ControlGroup cG in cGs)
                        {
                            if (ManagedVehicle.ActivePlayerVehicle.LightControlGroups[cG.Name].Item1)
                            {
                                controlGroups += "~g~" + cG.Name + " (";
                                List<string> cGModes = ControlGroupManager.ControlGroups[veh.Model][cG.Name].Modes[ManagedVehicle.ActivePlayerVehicle.LightControlGroups[cG.Name].Item2].Modes;
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

                foreach (var v in cachedPlayerVehicles.ToArray())
                {
                    if (!v || (v.HasDriver && !v.Driver.IsPlayer)) cachedPlayerVehicles.Remove(v);
                }

                GameFiber.Yield();
            }
        }

        [ConsoleCommand]
        private static void DebugCurrentModes()
        {
            if (ManagedVehicle.ActivePlayerVehicle == null)
            {
                ("No current managed DLS vehicle").ToLog(true);
                return;
            }

            if (!ManagedVehicle.ActivePlayerVehicle.Vehicle)
            {
                ("Current managed DLS vehicle is invalid").ToLog(true);
            }

            ManagedVehicle.ActivePlayerVehicle.Vehicle.DebugCurrentModes();
        }

        [ConsoleCommand]
        private static void DebugCurrentModesAll()
        {
            foreach (var managedVehicle in Entrypoint.ManagedVehicles.Values)
            {
                if (!managedVehicle.Vehicle) continue;
                managedVehicle.Vehicle.DebugCurrentModes();
            }
        }

        [ConsoleCommand]
        private static void DebugCurrentModesForVehicle([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterVehicle))] Vehicle vehicle)
        {
            if (!vehicle)
            {
                ("No valid vehicle specified").ToLog(true);
                return;
            }

            if (vehicle.GetDLS() == null)
            {
                ("Selected vehicle is not managed by DLS").ToLog(true);
                return;
            }

            vehicle.DebugCurrentModes();
        }
    }
}