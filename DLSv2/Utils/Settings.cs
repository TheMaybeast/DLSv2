using Rage;
using System.Windows.Forms;

namespace DLSv2.Utils
{
    class Settings
    {
        internal static InitializationFile INI = new InitializationFile(@"Plugins\DLS.ini");

        /// KEYBOARD
        // General
        public static Keys KB_MODIFIER { get; } = INI.ReadEnum("Keyboards.Default", "Modifier", Keys.Shift);
        public static Keys KB_LOCKALL { get; } = INI.ReadEnum("Keyboards.Default", "LockAll", Keys.Scroll);

        // Lights
        public static Keys KB_LIGHTSTAGE { get; } = INI.ReadEnum("Keyboards.Default", "LightStage", Keys.J);
        public static Keys KB_INTLT { get; } = INI.ReadEnum("Keyboards.Default", "InteriorLight", Keys.OemCloseBrackets);
        public static Keys KB_INDL { get; } = INI.ReadEnum("Keyboards.Default", "IndL", Keys.OemMinus);
        public static Keys KB_INDR { get; } = INI.ReadEnum("Keyboards.Default", "IndL", Keys.Oemplus);
        public static Keys KB_HZRD { get; } = INI.ReadEnum("Keyboards.Default", "Hazard", Keys.Back);

        // Sirens
        public static Keys KB_TOGGLESIREN { get; } = INI.ReadEnum("Keyboards.Default", "SirenToggle", Keys.G);
        public static Keys KB_CYCLESIREN { get; } = INI.ReadEnum("Keyboards.Default", "SirenCycle", Keys.R);
        public static Keys KB_MAN { get; } = INI.ReadEnum("Keyboards.Default", "SirenManual", Keys.T);
        public static Keys KB_TOGGLEAUX { get; } = INI.ReadEnum("Keyboards.Default", "AuxToggle", Keys.D6);
        public static Keys KB_HORN { get; } = INI.ReadEnum("Keyboards.Default", "CON_HORN", Keys.Y);

        /// CONTROLLER
        // Lights
        public static ControllerButtons CON_LIGHTSTAGE { get; } = INI.ReadEnum("Controller.Default", "LightStage", ControllerButtons.DPadLeft);

        // Sirens
        public static ControllerButtons CON_TOGGLESIREN { get; } = INI.ReadEnum("Controller.Default", "SirenToggle", ControllerButtons.DPadDown);
        public static ControllerButtons CON_CYCLESIREN { get; } = INI.ReadEnum("Controller.Default", "SirenCycle", ControllerButtons.B);
        public static ControllerButtons CON_TOGGLEAUX { get; } = INI.ReadEnum("Controller.Default", "AuxToggle", ControllerButtons.DPadUp);
        public static ControllerButtons CON_HORN { get; } = INI.ReadEnum("Controller.Default", "CON_HORN", ControllerButtons.LeftThumb);

        /// SETTINGS
        // General
        public static string SET_AUDIONAME { get; } = INI.ReadString("Settings", "AudioName", "TOGGLE_ON");
        public static string SET_AUDIOREF { get; } = INI.ReadString("Settings", "AudioRef", "HUD_FRONTEND_DEFAULT_SOUNDSET");
        // Disabled Keys
        public static string SET_DISABLEDCONTROLS { get; } = INI.ReadString("Settings", "DisabledControls", "80");
        // Extra Patch
        public static bool SET_EXTRAPATCH { get; } = INI.ReadBoolean("Settings", "ExtraPatch", false);

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