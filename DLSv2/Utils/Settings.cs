using Rage;
using System.Windows.Forms;

namespace DLSv2.Utils
{
    class Settings
    {
        internal static InitializationFile INI = new InitializationFile(@"Plugins\DLS.ini");

        /// SETTINGS
        // General
        public static Keys SET_MODIFIER { get; } = INI.ReadEnum("Settings", "Modifier", Keys.Shift);
        public static string SET_AUDIONAME { get; } = INI.ReadString("Settings", "AudioName", "TOGGLE_ON");
        public static string SET_AUDIOREF { get; } = INI.ReadString("Settings", "AudioRef", "HUD_FRONTEND_DEFAULT_SOUNDSET");
        // Disabled Keys
        public static string SET_DISABLEDCONTROLS { get; } = INI.ReadString("Settings", "DisabledControls", "80");
        // Extra Patch
        public static bool SET_EXTRAPATCH { get; } = INI.ReadBoolean("Settings", "ExtraPatch", true);
        // Developer Mode
        public static bool SET_DEVMODE { get; } = INI.ReadBoolean("Settings", "DevMode", false);
        // Brake Lights
        public static bool SET_BRAKELIGHTS { get; } = INI.ReadBoolean("Settings", "BrakeLights", true);

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