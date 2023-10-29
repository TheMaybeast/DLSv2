using DLSv2.Core.Sound;
using Rage;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core.Lights
{
    internal class LightController
    {
        public static void Update(ManagedVehicle managedVehicle)
        {
            Vehicle vehicle = managedVehicle.Vehicle;

            // Start with no modes activated
            List<Mode> modes = new List<Mode>();

            // Go through all modes in order, and enable modes based on trigger criteria
            List<Mode> standaloneModes = ModeManager.GetStandaloneModes(managedVehicle);
            if (standaloneModes != null) modes.AddRange(standaloneModes);

            // Go through all control groups in order, and enable modes based on control inputs
            List<Mode> cgModes = ControlGroupManager.GetModes(managedVehicle);
            if (cgModes != null) modes.AddRange(cgModes);

            // If no active modes, clears EL and disables siren
            if (modes.Count == 0)
            {
                managedVehicle.LightsOn = false;
                ModeManager.ApplyModes(managedVehicle, new List<Mode>());
                managedVehicle.Vehicle.IsSirenOn = false;
                SirenController.KillSirens(managedVehicle);
                return;
            }
            
            // Turns on vehicle siren
            managedVehicle.LightsOn = true;
            managedVehicle.Vehicle.IsSirenOn = true;
            managedVehicle.Vehicle.IsSirenSilent = true;

            // Sets EL with appropriate modes
            ModeManager.ApplyModes(managedVehicle, modes);
        }
    }
}
