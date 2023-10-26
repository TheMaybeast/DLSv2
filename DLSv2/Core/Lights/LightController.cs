using DLSv2.Core.Sound;
using DLSv2.Utils;
using Rage;
using Rage.Native;

namespace DLSv2.Core.Lights
{
    internal class LightController
    {
        // Should only be called for DLS vehicles
        public static void Update(ManagedVehicle managedVehicle)
        {
            if (managedVehicle == null | !managedVehicle.Vehicle) return;

            if (managedVehicle.LightStage > 0)
            {
                managedVehicle.Vehicle.IsSirenOn = true;
                managedVehicle.Vehicle.IsSirenSilent = true;

                DLSModel dlsModel = managedVehicle.Vehicle.GetDLSModel();
                LightStage lightStage = dlsModel.LightStages[managedVehicle.LightStage-1];
                managedVehicle.Vehicle.ShouldVehiclesYieldToThisVehicle = lightStage.Yield.ToBoolean();

                if (lightStage.ForceSiren != "0" && dlsModel.SoundSettings.SirenTones[lightStage.ForceSiren.ToInt32()] != null)
                {
                    managedVehicle.SirenOn = true;
                    managedVehicle.SirenStage = lightStage.ForceSiren.ToInt32();
                    SirenController.Update(managedVehicle);
                }
            }
            else
            {
                managedVehicle.Vehicle.IsSirenOn = false;
            }

            managedVehicle.Vehicle.EmergencyLightingOverride = GetEL(managedVehicle.Vehicle, managedVehicle);
        }

        public static void MoveUpStage(ManagedVehicle managedVehicle)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            int newStage = managedVehicle.LightStage + 1;
            if (newStage > managedVehicle.Vehicle.GetDLSModel().LightStages.Count)
                managedVehicle.LightStage = 0;
            else
                managedVehicle.LightStage = newStage;

            Update(managedVehicle);
        }

        public static void MoveDownStage(ManagedVehicle managedVehicle)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            int newStage = managedVehicle.LightStage - 1;
            if (newStage < 0)
                managedVehicle.LightStage = managedVehicle.Vehicle.GetDLSModel().LightStages.Count - 1;
            else
                managedVehicle.LightStage = newStage;

            Update(managedVehicle);
        }

        private static EmergencyLighting GetEL(Vehicle veh, ManagedVehicle managedVehicle = null)
        {
            DLSModel dlsModel = veh.GetDLSModel();
            if (managedVehicle == null)
                managedVehicle = veh.GetActiveVehicle();
            string name = veh.Model.Name + " | " + managedVehicle.LightStage.ToString();
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
                    Model model = new Model("police");
                    eL = model.EmergencyLighting.Clone();
                    eL.Name = name;
                    ("Created \"" + name + "\" (" + key + ") for " + veh.Handle).ToLog();
                }
                else
                {
                    eL = EmergencyLighting.GetByName(name);
                    ("Allocated \"" + name + "\" (" + key + ") for " + veh.Handle + " from Game Memory").ToLog();
                }
            }
            if (managedVehicle.LightStage > 0)
                SirenApply.ApplySirenSettingsToEmergencyLighting(dlsModel.LightStages[managedVehicle.LightStage - 1].SirenSettings, eL);
                
            else
            {
                SirenApply.ApplySirenSettingsToEmergencyLighting(dlsModel.LightStages[0].SirenSettings, eL);
                eL.LeftHeadLightSequence = "00000000000000000000000000000000";
                eL.LeftTailLightSequence = "00000000000000000000000000000000";
                eL.RightHeadLightSequence = "00000000000000000000000000000000";
                eL.RightTailLightSequence = "00000000000000000000000000000000";
                for (int i = 0; i < eL.Lights.Length; i++)
                {
                    EmergencyLight eLig = eL.Lights[i];
                    eLig.FlashinessSequence = "00000000000000000000000000000000";
                    eLig.RotationSequence = "00000000000000000000000000000000";
                }
            }
            if (!Entrypoint.ELUsedPool.ContainsKey(key))
                Entrypoint.ELUsedPool.Add(key, eL);
            managedVehicle.CurrentELHash = key;
            return eL;
        }
    }
}
