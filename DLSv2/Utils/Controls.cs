using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DLSv2.Utils
{
    internal class Controls
    {
        private static List<int> DisabledControls = new List<int>();

        public static bool KeysLocked = false;

        public static bool IsDLSControlDown(DLSControls controls)
        {
            if (KeysLocked && controls != DLSControls.GEN_LOCKALL) return false;

            switch (controls)
            {
                // General
                case DLSControls.GEN_LOCKALL:
                    return Game.IsKeyDown(Settings.KB_LOCKALL);
                // Generic Lights
                case DLSControls.LIGHT_INTLT:
                    return Game.IsKeyDown(Settings.KB_INTLT);
                case DLSControls.LIGHT_INDL:
                    return Game.IsKeyDown(Settings.KB_INDL);
                case DLSControls.LIGHT_INDR:
                    return Game.IsKeyDown(Settings.KB_INDR);
                case DLSControls.LIGHT_HZRD:
                    return Game.IsKeyDown(Settings.KB_HZRD);
                // Sirens
                case DLSControls.SIREN_TOGGLE:
                    return Game.IsKeyDown(Settings.KB_TOGGLESIREN)
                        || Game.IsControllerButtonDown(Settings.CON_TOGGLESIREN);
                case DLSControls.SIREN_CYCLE:
                    return Game.IsKeyDown(Settings.KB_CYCLESIREN)
                        || Game.IsControllerButtonDown(Settings.CON_CYCLESIREN);
                case DLSControls.SIREN_MAN:
                    return Game.IsKeyDownRightNow(Settings.KB_MAN)
                        || Game.IsControllerButtonDownRightNow(Settings.CON_CYCLESIREN);
                case DLSControls.SIREN_AUX:
                    return Game.IsKeyDown(Settings.KB_TOGGLEAUX)
                        || Game.IsControllerButtonDown(Settings.CON_TOGGLEAUX);
                case DLSControls.SIREN_HORN:
                    return Game.IsKeyDownRightNow(Settings.KB_HORN)
                        || Game.IsControllerButtonDownRightNow(Settings.CON_HORN);
                default:
                    return false;
            }
        }

        public static bool IsDLSControlDownWithModifier(DLSControls controls)
        {
            if (KeysLocked) return false;

            switch (controls)
            {
                case DLSControls.SIREN_CYCLE:
                    switch (Settings.KB_MODIFIER)
                    {
                        case Keys.Shift:
                            return Game.IsShiftKeyDownRightNow
                                && Game.IsKeyDown(Settings.KB_CYCLESIREN);
                        case Keys.Control:
                            return Game.IsControlKeyDownRightNow
                                && Game.IsKeyDown(Settings.KB_CYCLESIREN);
                        case Keys.Alt:
                            return Game.IsAltKeyDownRightNow
                                && Game.IsKeyDown(Settings.KB_CYCLESIREN);
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        public static void DisableControls()
        {
            foreach (int i in DisabledControls)
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, i, true);
        }
    }

    internal enum DLSControls
    {
        // General
        GEN_LOCKALL,
        // Lights
        LIGHT_STAGE,
        LIGHT_INTLT,
        LIGHT_INDL,
        LIGHT_INDR,
        LIGHT_HZRD,
        // Sirens
        SIREN_TOGGLE,
        SIREN_CYCLE,
        SIREN_AUX,
        SIREN_MAN,
        SIREN_HORN
    }

    internal class Input
    {
        public string Name { get; set; }
        public Keys Key { get; set; } = Keys.None;
        public ControllerButtons ControllerButton { get; set; } = ControllerButtons.None;

        public delegate void InputEventHandler(bool withModifier, EventArgs args);
    }
}