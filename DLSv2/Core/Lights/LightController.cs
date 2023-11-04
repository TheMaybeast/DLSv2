using DLSv2.Core.Sound;
using DLSv2.Utils;
using Rage;
using System.Collections.Generic;
using System.Linq;

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
                if (cG.Exclusive.ToBoolean())
                {
                    List<string> cGExclusiveModes = new List<string>();
                    foreach (ModeSelection modeSelection in cG.Modes)
                        cGExclusiveModes.AddRange(modeSelection.Modes);

                    modes.RemoveAll(x => cGExclusiveModes.Contains(x.Name));
                }
                
                modes.AddRange(cgModes[cGName]);
            }

            // If no active modes, clears EL and disables siren
            if (modes.Count == 0)
            {
                managedVehicle.LightsOn = false;
                ModeManager.ApplyModes(managedVehicle, new List<Mode>());
                managedVehicle.Vehicle.IsSirenOn = false;
                //AudioController.KillSirens(managedVehicle);
                return;
            }

            modes = modes.OrderBy(d => ModeManager.Modes[vehicle.Model].Values.ToList().IndexOf(d)).ToList();

            // Turns on vehicle siren
            managedVehicle.LightsOn = true;
            managedVehicle.Vehicle.IsSirenOn = true;
            // TODO: Add siren sound override and do not change IsSirenSilent
            // Automatically turn siren audio on/off when IsSirenSilent is changed by another script
            managedVehicle.Vehicle.IsSirenSilent = true;

            // Sets EL with appropriate modes
            ModeManager.ApplyModes(managedVehicle, modes);
        }
    }
}
