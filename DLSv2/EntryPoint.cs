using DLSv2.Core;
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
        public static List<ManagedVehicle> ManagedVehicles = new List<ManagedVehicle>();
        //List of used Sound IDs
        public static List<int> UsedSoundIDs = new List<int>();
        //List of used DLS Models
        public static List<Model> DLSModels = new List<Model>();
        //Pool of Available ELs
        public static List<EmergencyLighting> ELAvailablePool = new List<EmergencyLighting>();
        //Pool of Used ELs
        public static Dictionary<uint, EmergencyLighting> ELUsedPool = new Dictionary<uint, EmergencyLighting>();

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

            //Loads DLS Models
            DLSModels = Loaders.ParseVCFs();

            //Creates control manager
            "Loading: DLS - Control Manager".ToLog();
            GameFiber.StartNew(delegate { ControlsManager.Process(); }, "DLS - Control Manager");
            "Loaded: DLS - Control Manager".ToLog();

            //Creates player controller
            "Loading: DLS - Player Controller".ToLog();
            GameFiber.StartNew(delegate { PlayerController.MainLoop(); }, "DLS - Player Controller");
            "Loaded: DLS - Player Controller".ToLog();

            //Creates cleanup manager
            "Loading: DLS - Cleanup Manager".ToLog();
            GameFiber.StartNew(delegate { Threads.CleanupManager.Process(); }, "DLS - Cleanup Manager");
            "Loaded: DLS - Cleanup Manager".ToLog();

            //If extra patch is enabled
            if (Settings.SET_EXTRAPATCH)
            {
                bool patched = ExtraRepairPatch.DisableExtraRepair();
                if (patched) "Patched extra repair".ToLog();
                else "ERROR: Failed to patch extra repair".ToLog();
            }
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
            if (ManagedVehicles.Count > 0)
            {
                "Refreshing managed vehicles".ToLog();
                foreach (ManagedVehicle aVeh in ManagedVehicles)
                {
                    if (aVeh.Vehicle)
                    {
                        aVeh.Vehicle.IsSirenOn = false;
                        aVeh.Vehicle.IsSirenSilent = false;
                        aVeh.Vehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Off;
                        aVeh.Vehicle.EmergencyLightingOverride = aVeh.Vehicle.DefaultEmergencyLighting;
                        ("Refreshed " + aVeh.Vehicle.Handle).ToLog();
                    }
                    else
                        ("Vehicle does not exist anymore!").ToLog();
                }
                "Refreshed managed vehicles".ToLog();
            }
        }

        [ConsoleCommand]
        private static void Command_RefreshDLSVCFs() => DLSModels = Loaders.ParseVCFs();
    }
}