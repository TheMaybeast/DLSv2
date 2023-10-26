using DLSv2.Utils;
using Rage;
using Rage.Native;
using System.Collections.Generic;

namespace DLSv2.Core.Sound
{
    internal class SirenController
    {
        public static Dictionary<Model, List<Tone>> SirenTones = new Dictionary<Model, List<Tone>>();
        public static List<Tone> DefaultSirenTones = new List<Tone>
        {
            new Tone
            {
                Name = "",
                ToneHash = ""
            },
            new Tone
            {
                Name = "",
                ToneHash = ""
            },
            new Tone
            {
                Name = "",
                ToneHash = ""
            },
        };

        public static Dictionary<Model, string> Horns = new Dictionary<Model, string>();

        public static void Update(ManagedVehicle managedVehicle)
        {
            if (managedVehicle == null | !managedVehicle.Vehicle) return;

            List<Tone> sirenTones = SirenTones.ContainsKey(managedVehicle.Vehicle.Model) ? SirenTones[managedVehicle.Vehicle.Model] : DefaultSirenTones;

            if (!managedVehicle.SirenOn) { SoundManager.NewSoundID(managedVehicle); return; }

            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(SoundManager.NewSoundID(managedVehicle), sirenTones[managedVehicle.SirenToneIndex].ToneHash, managedVehicle.Vehicle, 0, 0, 0);
        }

        public static void MoveUpStage(ManagedVehicle managedVehicle)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            List<Tone> sirenTones = SirenTones.ContainsKey(managedVehicle.Vehicle.Model) ? SirenTones[managedVehicle.Vehicle.Model] : DefaultSirenTones;

            int newStage = managedVehicle.SirenToneIndex + 1;
            if (newStage >= sirenTones.Count)
                managedVehicle.SirenToneIndex = 0;
            else
                managedVehicle.SirenToneIndex = newStage;

            Update(managedVehicle);
        }

        public static void MoveDownStage(ManagedVehicle managedVehicle)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);

            List<Tone> sirenTones = SirenTones.ContainsKey(managedVehicle.Vehicle.Model) ? SirenTones[managedVehicle.Vehicle.Model] : DefaultSirenTones;

            int newStage = managedVehicle.SirenToneIndex - 1;
            if (newStage < 0)
                managedVehicle.SirenToneIndex = sirenTones.Count - 1;
            else
                managedVehicle.SirenToneIndex = newStage;

            Update(managedVehicle);
        }

        public static void KillSirens(ManagedVehicle managedVehicle)
        {
            managedVehicle.SirenOn = false;
            managedVehicle.SirenToneIndex = 0;
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

                List<Tone> sirenTones = SirenTones.ContainsKey(managedVehicle.Vehicle.Model) ? SirenTones[managedVehicle.Vehicle.Model] : DefaultSirenTones;

                switch (newState)
                {
                    case 1:
                        managedVehicle.AirManuID = SoundManager.TempSoundID();
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(managedVehicle.AirManuID, "sirens_airhorn", managedVehicle.Vehicle, 0, 0, 0);
                        break;
                    case 2:
                        managedVehicle.AirManuID = SoundManager.TempSoundID();
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(managedVehicle.AirManuID, sirenTones[0].ToneHash, managedVehicle.Vehicle, 0, 0, 0);
                        break;
                    case 3:
                        managedVehicle.AirManuID = SoundManager.TempSoundID();
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(managedVehicle.AirManuID, sirenTones[1].ToneHash, managedVehicle.Vehicle, 0, 0, 0);
                        break;
                }

                managedVehicle.AirManuState = newState;
            }
        }
    }
}
