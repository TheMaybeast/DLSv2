using Rage;
using System.Collections.Generic;
using System.Linq;
using DLSv2.Utils;

namespace DLSv2.Core.Lights
{
    internal class LightController
    {
        public static void Update(ManagedVehicle managedVehicle)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle) return;

            // Start with no modes activated
            List<Mode> modes = new List<Mode>();

            // Go through all modes in order, and enable modes based on trigger criteria
            List<Mode> standaloneModes = ModeManager.GetStandaloneModes(managedVehicle);
            if (standaloneModes != null) modes.AddRange(standaloneModes);

            // Go through all control groups in order, and enable modes based on control inputs
            Dictionary<string, List<Mode>> cgModes = ControlGroupManager.GetModes(managedVehicle);
            foreach (string cGName in cgModes.Keys)
            {
                ControlGroup cG = ControlGroupManager.ControlGroups[vehicle.Model][cGName];
                // If group is exclusive, disables every mode if enabled previously
                if (cG.Exclusive)
                {
                    List<string> cGExclusiveModes = new List<string>();
                    foreach (ModeSelection modeSelection in cG.Modes)
                        cGExclusiveModes.AddRange(modeSelection.Modes);

                    modes.RemoveAll(x => cGExclusiveModes.Contains(x.Name));
                }
                
                modes.AddRange(cgModes[cGName]);
            }

            // Remove modes that are disabled
            foreach (Mode mode in modes.ToArray())
            {
                if (!mode.Requirements.Update(managedVehicle)) modes.RemoveAll(m => m == mode);
            }

            // Sort modes by the order they initially appear in the config file
            modes = modes.OrderBy(d => ModeManager.Modes[vehicle.Model].Values.ToList().IndexOf(d)).ToList();

            // Update active light modes (must be done before modes == 0 count to clear inactive modes)
            managedVehicle.ActiveLightModes = modes.Select(x => x.Name).ToList();

            // If no active modes, clears EL and disables siren
            if (modes.Count == 0)
            {
                if (vehicle.IsPlayerVehicle() || !vehicle.IsSirenOn) managedVehicle.LightsOn = false;
                ModeManager.ApplyModes(managedVehicle, new List<Mode>());
                //AudioController.KillSirens(managedVehicle);
                return;
            }

            // Turns on vehicle siren
            if (vehicle.IsPlayerVehicle() || vehicle.IsSirenOn) managedVehicle.LightsOn = true;

            // Sets EL with appropriate modes
            ModeManager.ApplyModes(managedVehicle, modes);
        }
    }
}
