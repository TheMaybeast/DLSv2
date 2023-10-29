using DLSv2.Utils;
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

            foreach (string modeName in managedVehicle.Modes.Where(pair => pair.Value == true).Select(pair => pair.Key))
            {
                // Skips if CG does not exist
                if (!Modes[vehicle.Model].ContainsKey(modeName)) continue;

                modes.Add(Modes[vehicle.Model][modeName]);
            }

            return modes;
        }

        public static void SetStandaloneModeStatus(ManagedVehicle managedVehicle, string mode, bool status)
        {
            Vehicle vehicle = managedVehicle.Vehicle;

            // If invalid mode, disregards
            if (!Modes[vehicle.Model].ContainsKey(mode)) return;

            managedVehicle.Modes[mode] = status;
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
                foreach (Extra extra in mode.Extra)
                   if (vehicle.HasExtra(extra.ID.ToInt32())) vehicle.SetExtra(extra.ID.ToInt32(), extra.Enabled.ToBoolean());

                // Sets the yield setting
                if (mode.Yield.Enabled.ToBoolean()) shouldYield = true;
            }

            vehicle.ShouldVehiclesYieldToThisVehicle = shouldYield;
            
            if (!Entrypoint.ELUsedPool.ContainsKey(key))
                Entrypoint.ELUsedPool.Add(key, eL);
            managedVehicle.CurrentELHash = key;

            managedVehicle.Vehicle.EmergencyLightingOverride = eL;
        }
    }
}
