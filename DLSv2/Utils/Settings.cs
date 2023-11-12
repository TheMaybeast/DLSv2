using Rage;

namespace DLSv2.Utils
{
    class Settings
    {
        internal static InitializationFile INI = new InitializationFile(@"Plugins\DLS.ini");

        /// SETTINGS
        // General
        public static string AUDIONAME { get; } = INI.ReadString("Settings", "AudioName", "TOGGLE_ON");
        public static string AUDIOREF { get; } = INI.ReadString("Settings", "AudioRef", "HUD_FRONTEND_DEFAULT_SOUNDSET");
        // Disabled Keys
        public static string DISABLEDCONTROLS { get; } = INI.ReadString("Settings", "DisabledControls", "80");
        // Extra Patch
        public static bool EXTRAPATCH { get; } = INI.ReadBoolean("Settings", "ExtraPatch", true);
        // Developer Mode
        public static bool DEVMODE { get; } = INI.ReadBoolean("Settings", "DevMode", false);
        // Brake Lights
        public static bool BRAKELIGHTS { get; } = INI.ReadBoolean("Settings", "BrakeLights", true);

        internal static void IniCheck()
        {
            if (INI.Exists())
            {
                "Loaded: DLS.ini".ToLog();
                return;
            }
            "ERROR: DLS.ini was not found!".ToLog();
        }
    }
}