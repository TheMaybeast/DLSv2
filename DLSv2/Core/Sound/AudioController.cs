using DLSv2.Utils;
using Rage;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core.Sound
{
    internal class AudioController
    {
        public static void Update(ManagedVehicle managedVehicle)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle) return;

            // Start with no modes activated
            List<AudioMode> modes = new List<AudioMode>();

            // Go through all control groups in order, and enable modes based on control inputs
            Dictionary<string, List<AudioMode>> cgModes = AudioControlGroupManager.GetModes(managedVehicle);
            foreach (string cGName in cgModes.Keys)
            {
                AudioControlGroup cG = AudioControlGroupManager.ControlGroups[vehicle.Model][cGName];
                // If group is exclusive, disables every AudioMode if enabled previously
                if (cG.Exclusive.ToBoolean())
                {
                    List<string> cGExclusiveModes = new List<string>();
                    foreach (AudioModeSelection modeSelection in cG.Modes)
                        cGExclusiveModes.AddRange(modeSelection.Modes);

                    modes.RemoveAll(x => cGExclusiveModes.Contains(x.Name));
                }

                modes.AddRange(cgModes[cGName]);
            }

            List<string> audioModes = modes.Select(x => x.Name).ToList();
            foreach (var item in managedVehicle.AudioModes.Keys)
                if (!audioModes.Contains(item))
                    Audio.StopMode(managedVehicle, item);

            // If no active modes
            if (modes.Count == 0)
            {
                managedVehicle.SirenOn = false;
                return;
            }

            modes = modes.OrderBy(d => AudioModeManager.Modes[vehicle.Model].Values.ToList().IndexOf(d)).ToList();

            // Sets EL with appropriate modes
            AudioModeManager.ApplyModes(managedVehicle, modes);
        }
    }
}
