using Rage;
using System.Windows.Forms;

namespace DLSv2.Utils
{
    class Settings
    {
        internal static InitializationFile INI = new InitializationFile(@"Plugins\DLS.ini");

        /// KEYS
        // General
        public static Keys CON_MODIFIER { get; } = INI.ReadEnum("Keyboards.Default", "Modifier", Keys.Shift);
        public static Keys CON_LOCKALL { get; } = INI.ReadEnum("Keyboards.Default", "LockAll", Keys.Scroll);

        // Lights
        public static Keys CON_LIGHTSTAGE { get; } = INI.ReadEnum("Keyboards.Default", "LightStage", Keys.J);
        public static Keys CON_INTLT { get; } = INI.ReadEnum("Keyboards.Default", "InteriorLight", Keys.OemCloseBrackets);
        public static Keys CON_INDLEFT { get; } = INI.ReadEnum("Keyboards.Default", "IndL", Keys.OemMinus);
        public static Keys CON_INDRIGHT { get; } = INI.ReadEnum("Keyboards.Default", "IndL", Keys.Oemplus);
        public static Keys CON_HZRD { get; } = INI.ReadEnum("Keyboards.Default", "Hazard", Keys.Back);

        // Sirens
        public static Keys CON_TOGGLESIREN { get; } = INI.ReadEnum("Keyboards.Default", "SirenToggle", Keys.G);
        public static Keys CON_CYCLESIREN { get; } = INI.ReadEnum("Keyboards.Default", "SirenCycle", Keys.R);
        public static Keys CON_TOGGLEAUX { get; } = INI.ReadEnum("Keyboards.Default", "AuxToggle", Keys.D6);
        public static Keys CON_MANUAL { get; } = INI.ReadEnum("Keyboards.Default", "CON_AUXSIREN", Keys.T);
        public static Keys CON_HORN { get; } = INI.ReadEnum("Keyboards.Default", "CON_HORN", Keys.Y);

        /// SETTINGS
        // General
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