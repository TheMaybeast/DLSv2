﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using Rage.Native;
using Rage.Attributes;

namespace DLSv2.Threads
{
    using Core;
    using Conditions;
    using Core.Lights;
    using Core.Sound;
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

        [ConsoleCommand]
        private static void DebugCurrentModes()
        {
            if (currentManaged == null)
            {
                Game.LogTrivial("No current managed DLS vehicle");
                return;
            }

            if (!currentManaged.Vehicle)
            {
                Game.LogTrivial("Current managed DLS vehicle is invalid");
            }

            Game.LogTrivial("");
            Game.LogTrivial("--------------------------------------------------------------------------------");
            Game.LogTrivial($"Active modes for managed DLS vehicle {currentManaged.Vehicle.Model.Name}");
            Game.LogTrivial("");

            Game.LogTrivial("Light Control Groups:");
            foreach (var cg in currentManaged.LightControlGroups)
            {
                string modes = string.Join(" + ", ControlGroupManager.ControlGroups[currentManaged.Vehicle.Model][cg.Key].Modes[currentManaged.LightControlGroups[cg.Key].Item2].Modes);
                Game.LogTrivial($"  {boolToCheck(cg.Value.Item1)}\t{cg.Key}: ({cg.Value.Item2}) = {modes}");
            }

            Game.LogTrivial("");
            Game.LogTrivial("");
            Game.LogTrivial("Light Modes:"); 
            foreach (var slm in currentManaged.StandaloneLightModes)
            {
                string modeName = slm.Key;
                bool enabled = slm.Value;
                Mode mode = ModeManager.Modes[currentManaged.Vehicle.Model][modeName];
                Game.LogTrivial($"  {boolToCheck(enabled)}  {modeName}");

                if (mode.Triggers != null && mode.Triggers.NestedConditions.Count > 0)
                {
                    bool triggers = mode.Triggers.GetInstance(currentManaged).LastTriggered;
                    Game.LogTrivial($"       {boolToCheck(triggers)}  Triggers:");
                    logNestedConditions(currentManaged, mode.Triggers, 5);
                }
                
                if (mode.Requirements != null && mode.Requirements.NestedConditions.Count > 0)
                {
                    bool reqs = mode.Triggers.GetInstance(currentManaged).LastTriggered;
                    Game.LogTrivial($"       {boolToCheck(reqs)}  Requirements:");
                    logNestedConditions(currentManaged, mode.Requirements, 5);
                }
            }

            Game.LogTrivial("");
            Game.LogTrivial("");
            Game.LogTrivial("Active Light Modes:");
            foreach (var mode in currentManaged.ActiveLightModes)
            {
                Game.LogTrivial($"  {mode}");
            }

            Game.LogTrivial("");
            Game.LogTrivial("--------------------------------------------------------------------------------");
            Game.LogTrivial("");
        }

        private static string boolToCheck(bool state) => state ? "[x]" : "[ ]";

        private static void logNestedConditions(ManagedVehicle mv, GroupConditions group, int level = 0)
        {
            string indent = new string(' ', 2 * level);
            foreach (var condition in group.NestedConditions)
            {
                var inst = condition.GetInstance(mv);
                Game.LogTrivial($"{indent} - {boolToCheck(inst.LastTriggered)} {condition.GetType().Name}");
                if (condition is GroupConditions subGroup)
                {
                    logNestedConditions(mv, subGroup, level + 1);
                }
            }
        }
    }
}
