using DLSv2.Core.Sound;
using DLSv2.Utils;
using Rage;
using System.Collections.Generic;

namespace DLSv2.Core.Lights
{
    internal class ModeManager
    {
        public static Dictionary<Model, Dictionary<string, Mode>> Modes = new Dictionary<Model, Dictionary<string, Mode>>();

        public static void Update(ManagedVehicle managedVehicle)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !Modes.ContainsKey(vehicle.Model)) return;

            if (managedVehicle.CurrentModes.Count == 0)
            {
                managedVehicle.Vehicle.EmergencyLightingOverride = GetEL(managedVehicle, new List<Mode> { Mode.GetEmpty(vehicle) });
                managedVehicle.Vehicle.IsSirenOn = false;
                SirenController.KillSirens(managedVehicle);
                return;
            }

            managedVehicle.Vehicle.IsSirenOn = true;
            managedVehicle.Vehicle.IsSirenSilent = true;

            List<Mode> modes = new List<Mode>();

            foreach (string modeName in managedVehicle.CurrentModes)
            {
                // If invalid mode, continue loop
                if (!Modes[vehicle.Model].ContainsKey(modeName)) continue;
                modes.Add(Modes[vehicle.Model][modeName]);
            }

            managedVehicle.Vehicle.EmergencyLightingOverride = GetEL(managedVehicle, modes);
        }

        private static EmergencyLighting GetEL(ManagedVehicle managedVehicle, List<Mode> modes)
        {
            Vehicle veh = managedVehicle.Vehicle;
            string name = veh.Model.Name + " | " + string.Join(" | ", managedVehicle.CurrentModes.ToArray());
            uint key = Game.GetHashKey(name);
            EmergencyLighting eL;

            if (Entrypoint.ELUsedPool.Count > 0 && Entrypoint.ELUsedPool.ContainsKey(key))
            {
                eL = Entrypoint.ELUsedPool[key];
                ("Allocated \"" + name + "\" (" + key + ") for " + veh.Handle + " from Used Pool").ToLog();
            }
            else if (Entrypoint.ELAvailablePool.Count > 0)
            {
                eL = Entrypoint.ELAvailablePool[0];
                Entrypoint.ELAvailablePool.Remove(eL);
                ("Removed \"" + eL.Name + "\" from Available Pool").ToLog();
                ("Allocated \"" + name + "\" (" + key + ") for " + veh.Handle + " from Available Pool").ToLog();
            }
            else
            {
                if (EmergencyLighting.GetByName(name) == null)
                {
                    eL = veh.EmergencyLighting.Clone();
                    eL.Name = name;
                    ("Created \"" + name + "\" (" + key + ") for " + veh.Handle).ToLog();
                }
                else
                {
                    eL = EmergencyLighting.GetByName(name);
                    ("Allocated \"" + name + "\" (" + key + ") for " + veh.Handle + " from Game Memory").ToLog();
                }
            }

            foreach (Mode mode in modes)
            {
                SirenApply.ApplySirenSettingsToEmergencyLighting(mode.SirenSettings, eL);
                foreach (Extra extra in mode.Extra)
                    if (veh.HasExtra(extra.ID.ToInt32())) veh.SetExtra(extra.ID.ToInt32(), extra.Enabled.ToBoolean());
            }
                
            
            if (!Entrypoint.ELUsedPool.ContainsKey(key))
                Entrypoint.ELUsedPool.Add(key, eL);
            managedVehicle.CurrentELHash = key;
            return eL;
        }
    }
}
