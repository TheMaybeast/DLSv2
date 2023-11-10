using DLSv2.Utils;
using Rage;
using System.Collections.Generic;

namespace DLSv2.Core.Sound
{
    internal class AudioModeManager
    {
        public static Dictionary<Model, Dictionary<string, AudioMode>> Modes = new Dictionary<Model, Dictionary<string, AudioMode>>();

        public static void ApplyModes(ManagedVehicle managedVehicle, List<AudioMode> modes)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !Modes.ContainsKey(vehicle.Model)) return;

            bool shouldYield = false;

            foreach (AudioMode audioMode in modes)
                Audio.PlayMode(managedVehicle, audioMode);

            vehicle.ShouldVehiclesYieldToThisVehicle = shouldYield;
        }
    }
}
