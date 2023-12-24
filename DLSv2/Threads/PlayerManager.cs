using System.Collections.Generic;
using System.Linq;
using Rage;
using Rage.Native;
using Rage.Attributes;
using Rage.ConsoleCommands;

namespace DLSv2.Threads
{
    using Core;
    using Rage.ConsoleCommands.AutoCompleters;
    using Utils;

    class PlayerManager
    {
        private static Vehicle prevVehicle;
        internal static HashSet<Vehicle> cachedPlayerVehicles = new();

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
                        ManagedVehicle.ActivePlayerVehicle.UpdateLights();
                        veh.IsSirenSilent = true;
                    }

                    if (!ManagedVehicle.ActivePlayerVehicle.SirenOn && !veh.IsSirenSilent)
                    {
                        ManagedVehicle.ActivePlayerVehicle.AudioControlGroups.First().Value.Toggle();
                        ManagedVehicle.ActivePlayerVehicle.UpdateAudio();
                    }
                    else if (ManagedVehicle.ActivePlayerVehicle.SirenOn && veh.IsSirenSilent)
                    {
                        // Clears audio control groups
                        foreach (var cG in ManagedVehicle.ActivePlayerVehicle.AudioControlGroups.Values.ToList())
                            cG.Disable();

                        // Updates audio
                        ManagedVehicle.ActivePlayerVehicle.UpdateAudio();
                    }

                    // Dev Mode UI
                    if (Settings.DEVMODE)
                    {
                        var controlGroups = "CGs: ";
                        var cGs = ManagedVehicle.ActivePlayerVehicle.LightControlGroups.Values.ToList();
                        foreach (var cG in cGs)
                        {
                            if (cG.Enabled)
                            {
                                controlGroups += "~g~" + cG.BaseControlGroup.Name + " (";
                                var cGModes = cG.BaseControlGroup.Modes[cG.Index].Modes;
                                foreach (var mode in cG.BaseControlGroup.Modes[cG.Index].Modes)
                                {
                                    controlGroups += mode;
                                    if (cGModes.IndexOf(mode) != cGModes.Count - 1) controlGroups += " + ";
                                }
                                controlGroups += ")";
                            }
                            else
                                controlGroups += "~r~" + cG.BaseControlGroup.Name;

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

        /*[ConsoleCommand]
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

        ConsoleCommand]
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
        }*/
    }
}