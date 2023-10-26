using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DLSv2.Utils
{
    internal static class Sirens
    {
        private static readonly List<SirenStage> defaultSirenStages = new List<SirenStage> { SirenStage.One, SirenStage.Two, SirenStage.Warning };
        public static void Update(ManagedVehicle activeVeh)
        {
            switch (activeVeh.SirenStage)
            {
                case SirenStage.Off:
                    Sound.NewSoundID(activeVeh);
                    break;
                case SirenStage.One:
                    if (activeVeh.SoundSet == null)
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), "VEHICLES_HORNS_SIREN_1", activeVeh.Vehicle, 0, 0, 0);
                    else
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), activeVeh.SoundSet.SirenTones[0], activeVeh.Vehicle, 0, 0, 0);
                    break;
                case SirenStage.Two:
                    if (activeVeh.SoundSet == null)
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), "VEHICLES_HORNS_SIREN_2", activeVeh.Vehicle, 0, 0, 0);
                    else
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), activeVeh.SoundSet.SirenTones[1], activeVeh.Vehicle, 0, 0, 0);
                    break;
                case SirenStage.Warning:
                    if (activeVeh.SoundSet == null)
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), "VEHICLES_HORNS_POLICE_WARNING", activeVeh.Vehicle, 0, 0, 0);
                    else
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), activeVeh.SoundSet.SirenTones[2], activeVeh.Vehicle, 0, 0, 0);
                    break;
                case SirenStage.Warning2:
                    NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), activeVeh.SoundSet.SirenTones[3], activeVeh.Vehicle, 0, 0, 0);
                    break;
                case SirenStage.Horn:
                    if (activeVeh.SoundSet == null)
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), "SIRENS_AIRHORN", activeVeh.Vehicle, 0, 0, 0);
                    else
                        NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(Sound.NewSoundID(activeVeh), activeVeh.SoundSet.Horn, activeVeh.Vehicle, 0, 0, 0);
                    break;
                default:
                    break;
            }
        }
        public static void MoveUpStage(ManagedVehicle activeVeh)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
            if (activeVeh.SoundSet == null)
                activeVeh.SirenStage = defaultSirenStages.Next(activeVeh.SirenStage);
            else
                activeVeh.SirenStage = activeVeh.SoundSet.AvailableSirenStages.Next(activeVeh.SirenStage);
            Update(activeVeh);
        }
        public static void MoveDownStage(ManagedVehicle activeVeh)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
            if (activeVeh.SoundSet == null)
                activeVeh.SirenStage = defaultSirenStages.Previous(activeVeh.SirenStage);
            else
                activeVeh.SirenStage = activeVeh.SoundSet.AvailableSirenStages.Previous(activeVeh.SirenStage);
            Update(activeVeh);
        }
        public static void SetAirManuState(ManagedVehicle activeVeh, int? newState)
        {
            if(newState != activeVeh.AirManuState)
            {
                if(activeVeh.AirManuID != null)
                {
                    Sound.ClearTempSoundID((int)activeVeh.AirManuID);
                    activeVeh.AirManuID = null;
                }

                switch (newState)
                {
                    case 1:
                        activeVeh.AirManuID = Sound.TempSoundID();
                        if (activeVeh.SoundSet == null)
                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVeh.AirManuID, "SIRENS_AIRHORN", activeVeh.Vehicle, 0, 0, 0);
                        else
                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVeh.AirManuID, activeVeh.SoundSet.Horn, activeVeh.Vehicle, 0, 0, 0);
                        break;
                    case 2:
                        activeVeh.AirManuID = Sound.TempSoundID();
                        if (activeVeh.SoundSet == null)
                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVeh.AirManuID, "VEHICLES_HORNS_SIREN_1", activeVeh.Vehicle, 0, 0, 0);
                        else
                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVeh.AirManuID, activeVeh.SoundSet.SirenTones[0], activeVeh.Vehicle, 0, 0, 0);
                        break;
                    case 3:
                        activeVeh.AirManuID = Sound.TempSoundID();
                        if (activeVeh.SoundSet == null)
                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVeh.AirManuID, "VEHICLES_HORNS_SIREN_2", activeVeh.Vehicle, 0, 0, 0);
                        else
                            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(activeVeh.AirManuID, activeVeh.SoundSet.SirenTones[1], activeVeh.Vehicle, 0, 0, 0);
                        break;
                }

                activeVeh.AirManuState = newState;
            }
        }

        public static Dictionary<string, SoundSet> GetSirenSets()
        {
            string path = @"Plugins\DLSv2\";
            _ = new SirenSet();
            Dictionary<string, SoundSet> dictSirenSets = new Dictionary<string, SoundSet>();
            foreach (string file in Directory.EnumerateFiles(path, "*.xml"))
            {
                try
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(SirenSets));
                    StreamReader streamReader = new StreamReader(file);

                    SirenSets sirenSets = (SirenSets)mySerializer.Deserialize(streamReader);
                    streamReader.Close();

                    string name = Path.GetFileNameWithoutExtension(file);
                    ("Added SirenSets: " + name).ToLog();

                    foreach (SirenSet sirenSet in sirenSets.SirenSet)
                    {
                        List<string> vehicles = sirenSet.Vehicles.Replace(" ", "").Split(',').ToList();

                        List<string> SirenTones = sirenSet.SoundSet.Tones.Replace(" ", "").Split(',').ToList();
                        for (int i = 0; i < SirenTones.Count; i++)
                            sirenSet.SoundSet.SirenTones[i] = SirenTones[i];

                        if (sirenSet.SoundSet.SirenTones[2] != "")
                            sirenSet.SoundSet.AvailableSirenStages.Add(SirenStage.Warning);
                        if (sirenSet.SoundSet.SirenTones[3] != "")
                            sirenSet.SoundSet.AvailableSirenStages.Add(SirenStage.Warning2);

                        foreach (string vehicle in vehicles)
                        {
                            dictSirenSets.Add(vehicle.ToLower(), sirenSet.SoundSet);
                        }
                    }
                }
                catch (Exception e)
                {
                    ("SIRENSET IMPORT ERROR (" + Path.GetFileNameWithoutExtension(file) + "): " + e.Message).ToLog();
                    Game.LogTrivial("SIRENSET IMPORT ERROR (" + Path.GetFileNameWithoutExtension(file) + "): " + e.Message);
                }
            }

            return dictSirenSets;
        }
    }
}