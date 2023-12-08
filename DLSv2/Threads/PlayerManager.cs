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
        private static ManagedVehicle currentManaged;

        public static bool registeredKeys;

        internal static void MainLoop()
        {
            while (true)
            {
                Ped playerPed = Game.LocalPlayer.Character;
                if (playerPed.IsInAnyVehicle(false) && playerPed.CurrentVehicle.Driver == playerPed
                    && playerPed.CurrentVehicle.GetDLS() != null)
                {
                    Vehicle veh = playerPed.CurrentVehicle;
                    ControlsManager.DisableControls();

                    // Registers new Vehicle
                    if (currentManaged == null || prevVehicle != veh)
                    {
                        currentManaged = veh.GetManagedVehicle();
                        prevVehicle = veh;
                        veh.IsInteriorLightOn = false;
                        ControlsManager.ClearInputs();
                        registeredKeys = false;
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
                            currentManaged.AudioControlGroups[key] = (false, 0);

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

        [ConsoleCommand]
        private static void DebugCurrentModes()
        {
            if (currentManaged == null)
            {
                ("No current managed DLS vehicle").ToLog(true);
                return;
            }

            if (!currentManaged.Vehicle)
            {
                ("Current managed DLS vehicle is invalid").ToLog(true);
            }

            currentManaged.Vehicle.DebugCurrentModes();
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