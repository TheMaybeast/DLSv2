using DLSv2.Utils;
using Rage.Native;
using System.Collections.Generic;

namespace DLSv2.Core.Sound
{
    internal class SirenController
    {
        public static readonly List<string> defaultSirenTones = new List<string> { "sirens_slow_dir", "fast_9mvv0vf" };

        public static void Update(ManagedVehicle managedVehicle)
        {
            if (managedVehicle == null | !managedVehicle.Vehicle) return;

            DLSModel dlsModel = managedVehicle.Vehicle.GetDLSModel();
            List<string> sirenTones = dlsModel != null ? dlsModel.SoundSettings.SirenTones : defaultSirenTones;

            if (!managedVehicle.SirenOn) { SoundManager.NewSoundID(managedVehicle); return; }

            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(SoundManager.NewSoundID(managedVehicle), sirenTones[managedVehicle.SirenStage], managedVehicle.Vehicle, 0, 0, 0);
        }

        public static void MoveUpStage(ManagedVehicle managedVehicle)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            DLSModel dlsModel = managedVehicle.Vehicle.GetDLSModel();
            List<string> sirenTones = dlsModel != null ? dlsModel.SoundSettings.SirenTones : defaultSirenTones;

            int newStage = managedVehicle.SirenStage + 1;
            if (newStage >= sirenTones.Count)
                managedVehicle.SirenStage = 0;
            else
                managedVehicle.SirenStage = newStage;

            Update(managedVehicle);
        }

        public static void MoveDownStage(ManagedVehicle managedVehicle)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            DLSModel dlsModel = managedVehicle.Vehicle.GetDLSModel();
            List<string> sirenTones = dlsModel != null ? dlsModel.SoundSettings.SirenTones : defaultSirenTones;

            int newStage = managedVehicle.SirenStage - 1;
            if (newStage < 0)
                managedVehicle.SirenStage = sirenTones.Count - 1;
            else
                managedVehicle.SirenStage = newStage;

            Update(managedVehicle);
        }

        public static void KillSirens(ManagedVehicle managedVehicle)
        {
            managedVehicle.SirenOn = false;
            managedVehicle.SirenStage = 0;
            Update(managedVehicle);
        }

        public static void SetAirManuState(ManagedVehicle managedVehicle, int? newState)
        {
            if (newState != managedVehicle.AirManuState)
            {
                if (managedVehicle.AirManuID != null)
                {
                    SoundManager.ClearTempSoundID((int)managedVehicle.AirManuID);
                    managedVehicle.AirManuID = null;
                }

                DLSModel dlsModel = managedVehicle.Vehicle.GetDLSModel();
                List<string> sirenTones = dlsModel != null ? dlsModel.SoundSettings.SirenTones : defaultSirenTones;

                switch (newState)
                {
                    case 1:
                        managedVehicle.AirManuID = SoundManager.TempSoundID();
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(managedVehicle.AirManuID, "sirens_airhorn", managedVehicle.Vehicle, 0, 0, 0);
                        break;
                    case 2:
                        managedVehicle.AirManuID = SoundManager.TempSoundID();
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(managedVehicle.AirManuID, sirenTones[0], managedVehicle.Vehicle, 0, 0, 0);
                        break;
                    case 3:
                        managedVehicle.AirManuID = SoundManager.TempSoundID();
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(managedVehicle.AirManuID, sirenTones[1], managedVehicle.Vehicle, 0, 0, 0);
                        break;
                }

                managedVehicle.AirManuState = newState;
            }
        }
    }
}
