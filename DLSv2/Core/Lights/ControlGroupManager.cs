using Rage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLSv2.Core.Lights
{
    internal class ControlGroupManager
    {
        public static Dictionary<Model, Dictionary<string, ControlGroup>> ControlGroups = new Dictionary<Model, Dictionary<string, ControlGroup>>();

        public static List<Mode> GetModes(ManagedVehicle managedVehicle)
        {
            // Safety checks
            if (managedVehicle == null) return null;
            Vehicle vehicle = managedVehicle.Vehicle;
            if(!vehicle || !ControlGroups.ContainsKey(vehicle.Model)) return null;

            List<Mode> modes = new List<Mode>();

            foreach (string cgName in managedVehicle.ControlGroups.Where(pair => pair.Value.Item1 == true).Select(pair => pair.Key))
            {
                // Skips if CG does not exist
                if (!ControlGroups[vehicle.Model].ContainsKey(cgName)) continue;

                var cgModes = ControlGroups[vehicle.Model][cgName].Modes[managedVehicle.ControlGroups[cgName].Item2].Modes;

                foreach (string mode in cgModes)
                    modes.Add(ModeManager.Modes[vehicle.Model][mode]);
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
            bool previousStatus = managedVehicle.ControlGroups[controlGroupName].Item1;

            if (previousStatus == false && !fromToggle)
            {
                int currentIndex = managedVehicle.ControlGroups[controlGroupName].Item2;
                if (currentIndex == 0 || currentIndex == controlGroup.Modes.Count - 1)
                {
                    managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(true, 0);
                    return;
                }
            }

            int prevIndex = managedVehicle.ControlGroups[controlGroupName].Item2;
            int newIndex = prevIndex + 1;
            if (newIndex >= controlGroup.Modes.Count)
            {
                managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(fromToggle ? previousStatus : false, 0);
                return;
            }
            else
                managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(fromToggle ? previousStatus : true, newIndex);
        }

        public static void PreviousInControlGroup(ManagedVehicle managedVehicle, string controlGroupName)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            ControlGroup controlGroup = ControlGroups[vehicle.Model][controlGroupName];

            if (managedVehicle.ControlGroups[controlGroupName].Item1 == false)
            {
                int currentIndex = managedVehicle.ControlGroups[controlGroupName].Item2;
                if (currentIndex == 0)
                {
                    managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(true, controlGroup.Modes.Count - 1);
                    return;
                }
            }

            int prevIndex = managedVehicle.ControlGroups[controlGroupName].Item2;
            int newIndex = prevIndex - 1;
            if (newIndex < 0)
            {
                managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(false, 0);
                return;
            }
            else
                managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(true, newIndex);
        }
    
        public static void ToggleControlGroup(ManagedVehicle managedVehicle, string controlGroupName, bool toggleOnly = false)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            bool newStatus = !managedVehicle.ControlGroups[controlGroupName].Item1;
            int previousIndex = managedVehicle.ControlGroups[controlGroupName].Item2;

            managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(newStatus, previousIndex);

            if (toggleOnly && !newStatus)
            {
                NextInControlGroup(managedVehicle, controlGroupName, true);
                return;
            }
        }

        public static void SetControlGroupIndex(ManagedVehicle managedVehicle, string controlGroupName, int newIndex)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            if (newIndex < 0 && newIndex > (ControlGroups[vehicle.Model].Count - 1)) return;

            managedVehicle.ControlGroups[controlGroupName] = new Tuple<bool, int>(true, newIndex);
        }
    }
}
