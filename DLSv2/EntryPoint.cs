using Rage;
using Rage.Attributes;
using Rage.Native;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

[assembly: Plugin("Dynamic Lighting System v2", Description = "Better ELS but for default lighting", Author = "TheMaybeast", PrefersSingleInstance = true, ShouldTickInPauseMenu = true, SupportUrl = "https://discord.gg/HUcXxkq")]
namespace DLSv2;

using Core;
using Threads;
using Utils;

internal class Entrypoint
{
    // Vehicles currently being managed by DLS
    public static Dictionary<Vehicle, ManagedVehicle> ManagedVehicles = new();
    // List of used DLS Models
    public static Dictionary<Model, DLSModel> DLSModels = new();
    // Pool of Available ELs
    public static List<EmergencyLighting> ELAvailablePool = new();
    // Pool of Used ELs
    public static Dictionary<uint, EmergencyLighting> ELUsedPool = new();

    public static void Main()
    {
        // RPH Version check.
        var rphVer = FileVersionInfo.GetVersionInfo("ragepluginhook.exe");
        $"Detected RPH {rphVer.FileVersion}".ToLog();
        if (rphVer.FileMinorPart < 78)
        {
            "RPH 78+ is required but not found".ToLog(LogLevel.FATAL);
            Game.DisplayNotification($"~y~Unable to load DLSv2~w~\nRagePluginHook version ~b~78~w~ or later is required, you are on version ~b~{rphVer.FileMinorPart}");
            return;
        }
        var pluginInfo = Assembly.GetExecutingAssembly().GetName();
            
        // Parses memory patterns and offsets
        _ = new Memory();

        // Checks if .ini file is created.
        _ = new Settings();

        // Creates GameTime cache thread - must be the first fiber started!
        "Loading: DLS - GameTime Cache Thread".ToLog();
        GameFiber.StartNew(CachedGameTime.Process, "DLS - GameTime Cache");
        "Loaded: DLS - GameTime Cache Thread".ToLog();

        // Creates Triggers manager
        "Loading: DLS - Triggers Manager".ToLog();
        GameFiber.StartNew(TriggersManager.Process, "DLS - Triggers Manager");
        "Loaded: DLS - Triggers Manager".ToLog();

        // Loads DLS Models
        DLSModels = Loaders.ParseVCFs();

        // Creates control manager
        "Loading: DLS - Control Manager".ToLog();
        GameFiber.StartNew(ControlsManager.Process, "DLS - Control Manager");
        "Loaded: DLS - Control Manager".ToLog();

        // Creates player controller
        "Loading: DLS - Player Controller".ToLog();
        GameFiber.StartNew(PlayerManager.MainLoop, "DLS - Player Controller");
        "Loaded: DLS - Player Controller".ToLog();

        // Creates cleanup manager
        "Loading: DLS - Cleanup Manager".ToLog();
        GameFiber.StartNew(Threads.CleanupManager.Process, "DLS - Cleanup Manager");
        "Loaded: DLS - Cleanup Manager".ToLog();

        // Creates AI manager
        "Loading: DLS - AI Manager".ToLog();
        GameFiber.StartNew(AiManager.ScanProcess, "DLS - AI Manager Scan");
        GameFiber.StartNew(AiManager.MonitorProcess, "DLS - AI Manager Monitor");
        "Loaded: DLS - AI Manager".ToLog();

        //If extra patch is enabled
        if (Settings.EXTRAPATCH)
        {
            var patched = ExtraRepairPatch.Patch();
            if (patched) "Patched extra repair".ToLog();
            else "Failed to patch extra repair".ToLog(LogLevel.ERROR);
        }
            
        Game.LogTrivial($"LOADED DLS v{pluginInfo.Version}");
    }

    private static void OnUnload(bool isTerminating)
    {
        "Unloading DLS".ToLog();
        if (ManagedVehicles.Count > 0)
        {
            "Refreshing managed vehicles".ToLog();
            foreach (var managedVehicle in ManagedVehicles.Values)
            {
                if (managedVehicle.Vehicle)
                {
                    foreach(var extra in managedVehicle.ManagedExtras.OrderByDescending(e => e.Value))
                    {
                        managedVehicle.Vehicle.SetExtra(extra.Key, extra.Value);
                    }
                    foreach (var paint in managedVehicle.ManagedPaint)
                    {
                        managedVehicle.Vehicle.SetPaint(paint.Key, paint.Value);
                    }
                    managedVehicle.Vehicle.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Off;
                    managedVehicle.Vehicle.EmergencyLightingOverride = managedVehicle.Vehicle.DefaultEmergencyLighting;
                    managedVehicle.Vehicle.IsSirenSilent = false;
                    managedVehicle.Vehicle.EnableSirenSounds();
                    
                    if (managedVehicle.SoundIds.Values.Count > 0)
                    {
                        foreach (var id in managedVehicle.SoundIds.Values)
                            Audio.StopSound(id);
                    }
                    
                    ("Refreshed " + managedVehicle.Vehicle.Handle).ToLog();
                }
                else
                    ("Vehicle does not exist anymore!").ToLog();
            }
            "Refreshed managed vehicles".ToLog();
        }

        var removedExtraPatch = ExtraRepairPatch.Remove();
        if (removedExtraPatch) "Removed patch extra repair".ToLog();
        else "Failed to remove patch extra repair".ToLog(LogLevel.ERROR);
            
        Log.Terminate();
    }
}