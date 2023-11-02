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

        static Controls()
        {
            foreach (string control in Settings.SET_DISABLEDCONTROLS.Split(',').Select(s => s.Trim()).ToList())
                DisabledControls.Add(control.ToInt32());
        }

        public static void DisableControls()
        {
            foreach (int i in DisabledControls)
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, i, true);
        }
    }

    internal class Input
    {
        public string Name { get; set; }
        public Keys Key { get; set; } = Keys.None;
        public ControllerButtons ControllerButton { get; set; } = ControllerButtons.None;

        public delegate void InputEventHandler(bool withModifier, EventArgs args);
    }
}