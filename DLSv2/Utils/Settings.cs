using Rage;

namespace DLSv2.Utils
{
    class Settings
    {
        internal static InitializationFile INI = new InitializationFile(@"Plugins\DLS.ini");

        // Controls
        public static int CON_TOGGLELIGHTS { get; } = INI.ReadInt32("Controls", "CON_TOGGLELIGHTS", 85);
        public static int CON_INDLEFT { get; } = INI.ReadInt32("Controls", "CON_INDLEFT", 84);
        public static int CON_INDRIGHT { get; } = INI.ReadInt32("Controls", "CON_INDRIGHT", 83);
        public static int CON_HZRD { get; } = INI.ReadInt32("Controls", "CON_HZRD", 202);
        public static int CON_INTLT { get; } = INI.ReadInt32("Controls", "CON_INTLT", 42);
        public static int CON_TOGGLESIREN { get; } = INI.ReadInt32("Controls", "CON_TOGGLESIREN", 19);
        public static int CON_NEXTSIREN { get; } = INI.ReadInt32("Controls", "CON_NEXTSIREN", 80);
        public static int CON_PREVSIREN { get; } = INI.ReadInt32("Controls", "CON_PREVSIREN", 73);
        public static int CON_AUXSIREN { get; } = INI.ReadInt32("Controls", "CON_AUXSIREN", 27);
        public static int CON_HORN { get; } = INI.ReadInt32("Controls", "CON_HORN", 86);

        // Settings
        public static string SET_AUDIONAME { get; } = INI.ReadString("Settings", "AudioName", "TOGGLE_ON");
        public static string SET_AUDIOREF { get; } = INI.ReadString("Settings", "AudioRef", "HUD_FRONTEND_DEFAULT_SOUNDSET");

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