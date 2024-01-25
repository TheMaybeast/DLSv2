using System;
using Rage;

namespace DLSv2.Utils;

using Threads;

class Settings
{
    internal static InitializationFile INI = new(@"Plugins\DLS.ini");

    /// SETTINGS
    // General
    public static string AUDIONAME { get; } = INI.ReadString("Settings", "AudioName", "TOGGLE_ON");
    public static string AUDIOREF { get; } = INI.ReadString("Settings", "AudioRef", "HUD_FRONTEND_DEFAULT_SOUNDSET");
    // Disabled Keys
    public static GameControl[] DISABLEDCONTROLS { get; } = INI.ReadArray("Settings", "DisabledControls", new GameControl[] { GameControl.VehicleCinCam }, Enum.TryParse);
    // Extra Patch
    public static bool EXTRAPATCH { get; } = INI.ReadBoolean("Settings", "ExtraPatch", true);
    // Developer Mode
    public static bool DEVMODE { get; } = INI.ReadBoolean("Settings", "DevMode");
    // Brake Lights
    public static bool BRAKELIGHTS { get; } = INI.ReadBoolean("Settings", "BrakeLights", true);

    public Settings()
    {
        if (INI.Exists())
            "Loaded: DLS.ini".ToLog();
        else
        {
            "DLS.ini was not found!".ToLog(LogLevel.ERROR);
            return;
        }

        foreach (var sectionName in INI.GetSectionNames())
        {
            if (sectionName == "Settings") continue;
            ControlsManager.RegisterInput(sectionName);
        }
    }
}