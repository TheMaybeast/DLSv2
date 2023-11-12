﻿using DLSv2.Utils;
using Rage;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core.Lights
{
    internal class ModeManager
    {
        public static Dictionary<Model, Dictionary<string, Mode>> Modes = new Dictionary<Model, Dictionary<string, Mode>>();

        public static List<Mode> GetStandaloneModes(ManagedVehicle managedVehicle)
        {
            // Safety checks
            if (managedVehicle == null) return null;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !Modes.ContainsKey(vehicle.Model)) return null;

            List<Mode> modes = new List<Mode>();

            foreach (string modeName in managedVehicle.StandaloneLightModes.Where(pair => pair.Value == true).Select(pair => pair.Key))
            {
                // Skips if CG does not exist
                if (!Modes[vehicle.Model].ContainsKey(modeName)) continue;

                modes.Add(Modes[vehicle.Model][modeName]);
            }

            return modes;
        }

        public static void SetStandaloneModeStatus(ManagedVehicle managedVehicle, Mode mode, bool status)
        {
            Vehicle vehicle = managedVehicle.Vehicle;

            // If invalid mode, disregards
            if (!Modes[vehicle.Model].ContainsKey(mode.Name)) return;

            bool requirementsMet = true;

            // Check if requirements are met when turning on
            // TODO: Turn off mode if requirements are no longer met?
            if (status)
            {
                foreach (BaseCondition requirement in mode.Requirements.Conditions)
                {
                    requirementsMet = requirementsMet && requirement.Update(managedVehicle);
                }
            }

            // Set status if requirements are met
            managedVehicle.StandaloneLightModes[mode.Name] = status && requirementsMet;
        }

        public static void ApplyModes(ManagedVehicle managedVehicle, List<Mode> modes)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !Modes.ContainsKey(vehicle.Model)) return;

            // Generate EL name and hash
            string modesName = "";
            modes.ForEach(i => modesName += (i.ToString() + " | "));
            string name = vehicle.Model.Name + " | " + modesName;
            uint key = Game.GetHashKey(name);

            EmergencyLighting eL;

            if (Entrypoint.ELUsedPool.Count > 0 && Entrypoint.ELUsedPool.ContainsKey(key))
            {
                eL = Entrypoint.ELUsedPool[key];
                ("Allocated \"" + name + "\" (" + key + ") for " + vehicle.Handle + " from Used Pool").ToLog();
            }
            else if (Entrypoint.ELAvailablePool.Count > 0)
            {
                eL = Entrypoint.ELAvailablePool[0];
                Entrypoint.ELAvailablePool.Remove(eL);
                ("Removed \"" + eL.Name + "\" from Available Pool").ToLog();
                ("Allocated \"" + name + "\" (" + key + ") for " + vehicle.Handle + " from Available Pool").ToLog();
            }
            else
            {
                if (EmergencyLighting.GetByName(name) == null)
                {
                    eL = vehicle.EmergencyLighting.Clone();
                    eL.Name = name;
                    ("Created \"" + name + "\" (" + key + ") for " + vehicle.Handle).ToLog();
                }
                else
                {
                    eL = EmergencyLighting.GetByName(name);
                    ("Allocated \"" + name + "\" (" + key + ") for " + vehicle.Handle + " from Game Memory").ToLog();
                }
            }

            SirenApply.ApplySirenSettingsToEmergencyLighting(Mode.GetEmpty(vehicle).SirenSettings, eL);

            bool shouldYield = false;

            foreach (Mode mode in modes)
            {
                SirenApply.ApplySirenSettingsToEmergencyLighting(mode.SirenSettings, eL);

                // Sets the extras for the specific mode
                // Set enabled extras first, then disabled extras second, because <extraIncludes> in vehicles.meta 
                // can cause enabling one extra to enable other linked extras. By disabling second, we turn back off 
                // any extras that are explictly set to be turned off. 
                foreach (Extra extra in mode.Extra.OrderByDescending(e => e.Enabled))
                   if (vehicle.HasExtra(extra.ID)) vehicle.SetExtra(extra.ID, extra.Enabled.ToBoolean());

                // Sets modkits for the specific mode
                foreach (ModKit kit in mode.ModKits)
                    if (vehicle.HasModkitMod(kit.Type) && vehicle.GetModkitModCount(kit.Type) > kit.Index)
                        vehicle.SetModkitModIndex(kit.Type, kit.Index);

                // Sets the yield setting
                if (mode.Yield.Enabled) shouldYield = true;
            }

            vehicle.ShouldVehiclesYieldToThisVehicle = shouldYield;
            
            if (!Entrypoint.ELUsedPool.ContainsKey(key))
                Entrypoint.ELUsedPool.Add(key, eL);
            managedVehicle.CurrentELHash = key;

            managedVehicle.Vehicle.EmergencyLightingOverride = eL;
        }
    }
}
