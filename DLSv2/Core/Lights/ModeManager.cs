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

            foreach (string modeName in managedVehicle.StandaloneLightModes.Where(pair => pair.Value).Select(pair => pair.Key))
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
                foreach (BaseCondition requirement in mode.Requirements.NestedConditions)
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

            EmergencyLighting eL;
            var key = vehicle.Handle;

            if (Entrypoint.ELUsedPool.TryGetValue(key, out var elFromPool))
            {
                eL = elFromPool;
                ("Allocated \"" + eL.Name + "\" EL from Used Pool").ToLog();
            }
            else if (Entrypoint.ELAvailablePool.Count > 0)
            {
                eL = Entrypoint.ELAvailablePool[0];
                Entrypoint.ELAvailablePool.Remove(eL);
                eL.Name = "DLS_" + key;
                ("Allocated \"" + eL.Name + "\" (now \"" + key + "\") EL from Available Pool").ToLog();
            }
            else
            {
                if (EmergencyLighting.GetByName("DLS_" + key) == null)
                {
                    eL = vehicle.EmergencyLighting.Clone();
                    eL.Name = "DLS_" + key;
                    ("Created \"" + eL.Name + "\" EL").ToLog();
                }
                else
                {
                    eL = EmergencyLighting.GetByName("DLS_" + key);
                    ("Allocated \"" + eL.Name + "\" EL from Game Memory").ToLog();
                }
            }

            SirenApply.ApplySirenSettingsToEmergencyLighting(Mode.GetEmpty(vehicle).SirenSettings, eL);

            bool shouldYield = false;
            var extras = new Dictionary<int, bool>();

            foreach (Mode mode in modes)
            {
                SirenApply.ApplySirenSettingsToEmergencyLighting(mode.SirenSettings, eL);

                // Sets the extras for the specific mode
                // Set enabled extras first, then disabled extras second, because <extraIncludes> in vehicles.meta 
                // can cause enabling one extra to enable other linked extras. By disabling second, we turn back off 
                // any extras that are explicitly set to be turned off.
                foreach (var extra in mode.Extra.OrderByDescending(e => e.Enabled))
                    extras[extra.ID] = extra.Enabled;
                   

                // Sets modkits for the specific mode
                foreach (ModKit kit in mode.ModKits)
                    if (vehicle.HasModkitMod(kit.Type) && vehicle.GetModkitModCount(kit.Type) > kit.Index)
                        vehicle.SetModkitModIndex(kit.Type, kit.Index);

                // Sets the yield setting
                if (mode.Yield.Enabled) shouldYield = true;

                // Sets the indicators
                if (mode.Indicators != null)
                {
                    switch (mode.Indicators.ToLower())
                    {
                        case "off":
                            managedVehicle.IndStatus = VehicleIndicatorLightsStatus.Off;
                            GenericLights.SetIndicator(managedVehicle.Vehicle, managedVehicle.IndStatus);
                            break;
                        case "rightonly":
                            managedVehicle.IndStatus = VehicleIndicatorLightsStatus.RightOnly;
                            GenericLights.SetIndicator(managedVehicle.Vehicle, managedVehicle.IndStatus);
                            break;
                        case "leftonly":
                            managedVehicle.IndStatus = VehicleIndicatorLightsStatus.LeftOnly;
                            GenericLights.SetIndicator(managedVehicle.Vehicle, managedVehicle.IndStatus);
                            break;
                        case "both":
                            managedVehicle.IndStatus = VehicleIndicatorLightsStatus.Both;
                            GenericLights.SetIndicator(managedVehicle.Vehicle, managedVehicle.IndStatus);
                            break;
                    }
                }
            }

            foreach (var extra in extras)
            {
                if (!vehicle.HasExtra(extra.Key)) continue;
                if (!managedVehicle.ManagedExtras.ContainsKey(extra.Key)) managedVehicle.ManagedExtras[extra.Key] = vehicle.IsExtraEnabled(extra.Key);
                vehicle.SetExtra(extra.Key, extra.Value);
            }

            var extrasToDisable = managedVehicle.ManagedExtras.Keys.Where(x => !extras.ContainsKey(x)).ToList();
            foreach (var extra in extrasToDisable)
            {
                if (vehicle.HasExtra(extra)) vehicle.SetExtra(extra, managedVehicle.ManagedExtras[extra]);
                managedVehicle.ManagedExtras.Remove(extra);
            }

            vehicle.ShouldVehiclesYieldToThisVehicle = shouldYield;
            
            if (!Entrypoint.ELUsedPool.ContainsKey(key))
                Entrypoint.ELUsedPool.Add(key, eL);

            managedVehicle.Vehicle.EmergencyLightingOverride = eL;
        }
    }
}
