using DLSv2.Utils;
using Rage;
using Rage.Native;
using System.Collections.Generic;

namespace DLSv2.Core.Lights
{
    internal class ControlGroupManager
    {
        public static Dictionary<Model, Dictionary<string, ControlGroup>> ControlGroups = new Dictionary<Model, Dictionary<string, ControlGroup>>();

        public static void NextInControlGroup(ManagedVehicle managedVehicle, string controlGroupName)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            ControlGroup controlGroup = ControlGroups[vehicle.Model][controlGroupName];
            
            // Triggers if Control Group is Active
            if (managedVehicle.ControlGroupIndex.ContainsKey(controlGroupName))
            {
                int prevIndex = managedVehicle.ControlGroupIndex[controlGroupName];
                int newIndex = prevIndex + 1;
                if (newIndex >= controlGroup.Modes.Count)
                {
                    managedVehicle.ControlGroupIndex.Remove(controlGroupName);
                    foreach (ModeSelection modeSelection in controlGroup.Modes)
                        foreach (string mode in modeSelection.Modes)
                            managedVehicle.CurrentModes.Remove(mode);

                    ModeManager.Update(managedVehicle);
                    return;
                }
                else
                    managedVehicle.ControlGroupIndex[controlGroupName] = newIndex;

                foreach (string mode in controlGroup.Modes[prevIndex].Modes)
                    managedVehicle.CurrentModes.Remove(mode);
            }
            // Triggers if Control Group is Inactive
            else
                managedVehicle.ControlGroupIndex.Add(controlGroupName, 0);
                        
            foreach (string mode in controlGroup.Modes[managedVehicle.ControlGroupIndex[controlGroupName]].Modes)
                managedVehicle.CurrentModes.Add(mode);

            ModeManager.Update(managedVehicle);
        }

        public static void PreviousInControlGroup(ManagedVehicle managedVehicle, string controlGroupName)
        {
            // Safety checks
            if (managedVehicle == null) return;
            Vehicle vehicle = managedVehicle.Vehicle;
            if (!vehicle || !ControlGroups.ContainsKey(vehicle.Model)
                || !ControlGroups[vehicle.Model].ContainsKey(controlGroupName)) return;

            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            ControlGroup controlGroup = ControlGroups[vehicle.Model][controlGroupName];

            // Triggers if Control Group is Active
            if (managedVehicle.ControlGroupIndex.ContainsKey(controlGroupName))
            {
                int prevIndex = managedVehicle.ControlGroupIndex[controlGroupName];
                int newIndex = prevIndex - 1;
                if (newIndex < 0)
                {
                    managedVehicle.ControlGroupIndex.Remove(controlGroupName);
                    foreach (ModeSelection modeSelection in controlGroup.Modes)
                        foreach (string mode in modeSelection.Modes)
                            managedVehicle.CurrentModes.Remove(mode);

                    ModeManager.Update(managedVehicle);
                    return;
                }
                else
                    managedVehicle.ControlGroupIndex[controlGroupName] = newIndex;
            }
            // Triggers if Control Group is Inactive
            else
                managedVehicle.ControlGroupIndex.Add(controlGroupName, controlGroup.Modes.Count - 1);

            foreach (string mode in controlGroup.Modes[managedVehicle.ControlGroupIndex[controlGroupName]].Modes)
                managedVehicle.CurrentModes.Add(mode);

            ModeManager.Update(managedVehicle);
        }
    }
}
