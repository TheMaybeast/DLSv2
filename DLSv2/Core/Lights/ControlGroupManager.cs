using Rage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core.Lights
{
    internal class ControlGroupManager
    {
        public static Dictionary<Model, Dictionary<string, ControlGroup>> ControlGroups = new Dictionary<Model, Dictionary<string, ControlGroup>>();

        public static Dictionary<string, List<Mode>> GetModes(ManagedVehicle managedVehicle)
        {
            // Safety checks
            if (managedVehicle == null) return null;
            Vehicle vehicle = managedVehicle.Vehicle;
            if(!vehicle || !ControlGroups.ContainsKey(vehicle.Model)) return null;

            Dictionary<string, List<Mode>> modes = new Dictionary<string, List<Mode>>();

            foreach (string cgName in managedVehicle.LightControlGroups.Where(pair => pair.Value.Item1).Select(pair => pair.Key))
            {
                // Skips if CG does not exist
                if (!ControlGroups[vehicle.Model].ContainsKey(cgName)) continue;
                modes.Add(cgName, new List<Mode>());

                var cgModes = ControlGroups[vehicle.Model][cgName].Modes[managedVehicle.LightControlGroups[cgName].Item2].Modes;

                foreach (string mode in cgModes)
                    modes[cgName].Add(ModeManager.Modes[vehicle.Model][mode]);
            }

            return modes;
        }

        public static void NextInControlGroup(ManagedVehicle managedVehicle, string controlGroupName, bool fromToggle = false)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            ControlGroup controlGroup = ControlGroups[vehicle.Model][controlGroupName];
            bool previousStatus = managedVehicle.LightControlGroups[controlGroupName].Item1;

            if (previousStatus == false && !fromToggle)
            {
                int currentIndex = managedVehicle.LightControlGroups[controlGroupName].Item2;
                if (currentIndex == 0 || currentIndex == controlGroup.Modes.Count - 1)
                {
                    managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(true, 0);
                    return;
                }
            }

            int prevIndex = managedVehicle.LightControlGroups[controlGroupName].Item2;
            int newIndex = prevIndex + 1;
            if (newIndex >= controlGroup.Modes.Count)
                managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(fromToggle && previousStatus, 0);
            else
                managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(!fromToggle || previousStatus, newIndex);
        }

        public static void PreviousInControlGroup(ManagedVehicle managedVehicle, string controlGroupName)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            ControlGroup controlGroup = ControlGroups[vehicle.Model][controlGroupName];

            if (managedVehicle.LightControlGroups[controlGroupName].Item1 == false)
            {
                int currentIndex = managedVehicle.LightControlGroups[controlGroupName].Item2;
                if (currentIndex == 0)
                {
                    managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(true, controlGroup.Modes.Count - 1);
                    return;
                }
            }

            int prevIndex = managedVehicle.LightControlGroups[controlGroupName].Item2;
            int newIndex = prevIndex - 1;
            if (newIndex < 0)
                managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(false, 0);
            else
                managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(true, newIndex);
        }
    
        public static void ToggleControlGroup(ManagedVehicle managedVehicle, string controlGroupName, bool toggleOnly = false)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            bool newStatus = !managedVehicle.LightControlGroups[controlGroupName].Item1;
            int previousIndex = managedVehicle.LightControlGroups[controlGroupName].Item2;

            managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(newStatus, previousIndex);

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

            managedVehicle.LightControlGroups[controlGroupName] = new Tuple<bool, int>(true, newIndex);
        }
    }
}
