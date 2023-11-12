using Rage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core.Sound
{
    internal class AudioControlGroupManager
    {
        public static Dictionary<Model, Dictionary<string, AudioControlGroup>> ControlGroups = new Dictionary<Model, Dictionary<string, AudioControlGroup>>();

        public static Dictionary<string, List<AudioMode>> GetModes(ManagedVehicle managedVehicle)
        {
            // Safety checks
            if (managedVehicle == null) return null;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)) return null;

            Dictionary<string, List<AudioMode>> modes = new Dictionary<string, List<AudioMode>>();

            foreach (string cgName in managedVehicle.AudioControlGroups.Where(pair => pair.Value.Item1).Select(pair => pair.Key))
            {
                // Skips if CG does not exist
                if (!ControlGroups[vehicle.Model].ContainsKey(cgName)) continue;
                modes.Add(cgName, new List<AudioMode>());

                var cgModes = ControlGroups[vehicle.Model][cgName].Modes[managedVehicle.AudioControlGroups[cgName].Item2].Modes;

                foreach (string AudioMode in cgModes)
                    modes[cgName].Add(AudioModeManager.Modes[vehicle.Model][AudioMode]);
            }

            return modes;
        }

        public static void NextInControlGroup(ManagedVehicle managedVehicle, string controlGroupName, bool fromToggle = false, bool cycleOnly = false)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            AudioControlGroup AudioControlGroup = ControlGroups[vehicle.Model][controlGroupName];
            bool previousStatus = managedVehicle.AudioControlGroups[controlGroupName].Item1;

            if (previousStatus == false && !fromToggle)
            {
                int currentIndex = managedVehicle.AudioControlGroups[controlGroupName].Item2;
                if (currentIndex == 0 || currentIndex == AudioControlGroup.Modes.Count - 1)
                {
                    managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(true, 0);
                    return;
                }
            }

            int prevIndex = managedVehicle.AudioControlGroups[controlGroupName].Item2;
            int newIndex = prevIndex + 1;
            if (newIndex >= AudioControlGroup.Modes.Count)
                managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(fromToggle ? previousStatus : !cycleOnly, 0);
            else
                managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(!fromToggle || previousStatus, newIndex);
        }

        public static void PreviousInControlGroup(ManagedVehicle managedVehicle, string controlGroupName, bool cycleOnly = false)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            AudioControlGroup AudioControlGroup = ControlGroups[vehicle.Model][controlGroupName];

            if (managedVehicle.AudioControlGroups[controlGroupName].Item1 == false)
            {
                int currentIndex = managedVehicle.AudioControlGroups[controlGroupName].Item2;
                if (currentIndex == 0)
                {
                    managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(true, AudioControlGroup.Modes.Count - 1);
                    return;
                }
            }

            int prevIndex = managedVehicle.AudioControlGroups[controlGroupName].Item2;
            int newIndex = prevIndex - 1;
            if (newIndex < 0)
                managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(!cycleOnly, cycleOnly ? 0 : AudioControlGroup.Modes.Count - 1);
            else
                managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(true, newIndex);
        }

        public static void ToggleControlGroup(ManagedVehicle managedVehicle, string controlGroupName, bool toggleOnly = false)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            bool newStatus = !managedVehicle.AudioControlGroups[controlGroupName].Item1;
            int previousIndex = managedVehicle.AudioControlGroups[controlGroupName].Item2;

            managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(newStatus, previousIndex);

            if (toggleOnly && !newStatus)
                NextInControlGroup(managedVehicle, controlGroupName, true);
        }

        public static void SetControlGroupIndex(ManagedVehicle managedVehicle, string controlGroupName, int newIndex)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            if (newIndex < 0 && newIndex > (ControlGroups[vehicle.Model].Count - 1)) return;

            managedVehicle.AudioControlGroups[controlGroupName] = new Tuple<bool, int>(true, newIndex);
        }
    }
}
