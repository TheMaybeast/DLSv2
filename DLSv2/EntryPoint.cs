using DLSv2.Threads;
using DLSv2.Utils;
using Rage;
using Rage.Attributes;
using Rage.Native;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

[assembly: Plugin("Dynamic Lighting System v2", Description = "Better ELS but for default lighting", Author = "TheMaybeast", PrefersSingleInstance = true, ShouldTickInPauseMenu = true, SupportUrl = "https://discord.gg/HUcXxkq")]
namespace DLSv2
{
    internal class Entrypoint
    {
        //Vehicles currently being managed by DLS
        public static List<ManagedVehicle> activeVehicles = new List<ManagedVehicle>();
        //List of used Sound IDs
        public static List<int> UsedSoundIDs = new List<int>();
        // List of Siren Sets
        public static Dictionary<string, SoundSet> SirenSets = new Dictionary<string, SoundSet>();

        public static void Main()
        {
            //Initiates Log File
            Log Log = new Log();

            // Checks if .ini file is created.
            Settings.IniCheck();

            //Version check and logging.
            FileVersionInfo rphVer = FileVersionInfo.GetVersionInfo("ragepluginhook.exe");
            Game.LogTrivial("Detected RPH " + rphVer.FileVersion);
            if (rphVer.FileMinorPart < 78)
            {
                Game.LogTrivial("RPH 78+ is required to use this mod");
                "ERROR: RPH 78+ is required but not found".ToLog();
                Game.DisplayNotification($"~y~Unable to load DLSv2~w~\nRagePluginHook version ~b~78~w~ or later is required, you are on version ~b~{rphVer.FileMinorPart}");
                return;
            }
            AssemblyName pluginInfo = Assembly.GetExecutingAssembly().GetName();
            Game.LogTrivial($"LOADED DLS v{pluginInfo.Version}");

            //Loads MPDATA audio
            NativeFunction.Natives.SET_AUDIO_FLAG("LoadMPData", true);

            //Loads SirenSets
            SirenSets = Sirens.GetSirenSets();

            //Creates player controller
            "Loading: DLS - Player Controller".ToLog();
            GameFiber.StartNew(delegate { PlayerController.MainLoop(); }, "DLS - Player Controller");
            "Loaded: DLS - Player Controller".ToLog();
        }

        private static void OnUnload(bool isTerminating)
        {
            "Unloading DLS".ToLog();
            if (UsedSoundIDs.Count > 0)
            {
                "Unloading used SoundIDs".ToLog();
                foreach (int id in UsedSoundIDs)
                {
                    NativeFunction.Natives.STOP_SOUND(id);
                    NativeFunction.Natives.RELEASE_SOUND_ID(id);
                    ("Unloaded SoundID " + id).ToLog();
                }
                "Unloaded all used SoundIDs".ToLog();
            }
            if (activeVehicles.Count > 0)
            {
                "Refreshing vehicle's default EL".ToLog();
                foreach (ManagedVehicle aVeh in activeVehicles)
                {
                    if (aVeh.Vehicle)
                    {
                        aVeh.Vehicle.IsSirenOn = false;
                        aVeh.Vehicle.IsSirenSilent = false;
                        aVeh.Vehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Off;
                        ("Refreshed " + aVeh.Vehicle.Handle).ToLog();
                    }
                    else
                        ("Vehicle does not exist anymore!").ToLog();
                }
                "Refreshed vehicle's default EL".ToLog();
            }
        }
    }
}