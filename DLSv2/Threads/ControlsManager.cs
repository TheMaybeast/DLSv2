using DLSv2.Utils;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DLSv2.Threads
{
    internal class ControlsManager
    {
        private static Dictionary<Input, Input.InputEventHandler> ManagedInputs = new Dictionary<Input, Input.InputEventHandler>();

        public static void Process()
        {
            while(true)
            {
                GameFiber.Yield();

                if (Game.IsPaused) continue;

                foreach (Input input in ManagedInputs.Keys)
                {                    
                    if (input.Key != Keys.None)
                    {
                        switch (Settings.MODIFIER)
                        {
                            case Keys.Shift:
                                if (KeysLocked && input.Name != "LOCKALL") continue;
                                ManagedInputs[input](input.Hold ? Game.IsKeyDownRightNow(input.Key) : Game.IsKeyDown(input.Key), Game.IsShiftKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Control:
                                if (KeysLocked && input.Name != "LOCKALL") continue;
                                ManagedInputs[input](input.Hold ? Game.IsKeyDownRightNow(input.Key) : Game.IsKeyDown(input.Key), Game.IsControlKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Alt:
                                if (KeysLocked && input.Name != "LOCKALL") continue;
                                ManagedInputs[input](input.Hold ? Game.IsKeyDownRightNow(input.Key) : Game.IsKeyDown(input.Key), Game.IsAltKeyDownRightNow, EventArgs.Empty);
                                break;
                            default:
                                if (KeysLocked && input.Name != "LOCKALL") continue;
                                ManagedInputs[input](input.Hold ? Game.IsKeyDownRightNow(input.Key) : Game.IsKeyDown(input.Key), false, EventArgs.Empty);
                                break;
                        }
                    }
                    else if (input.ControllerButton != ControllerButtons.None)
                    {
                        if (KeysLocked && input.Name != "LOCKALL") continue;
                        ManagedInputs[input](Game.IsControllerButtonDown(input.ControllerButton), false, EventArgs.Empty);
                    }
                }
            }
        }

        public static void RegisterInput(string inputName, Input.InputEventHandler eventHandler, bool hold = false)
        {
            Input input = new Input
            {
                Name = inputName,
                Key = Settings.INI.ReadEnum(inputName, "Key", Keys.None),
                ControllerButton = Settings.INI.ReadEnum(inputName, "ControllerButton", ControllerButtons.None),
                Hold = hold
            };

            ("Mapped input [" + input.Name + "] to Key [" + input.Key + "]" +
                (input.ControllerButton != ControllerButtons.None ? " and ControllerButton [" + input.ControllerButton + "]" : "")).ToLog();

            if (ManagedInputs.ContainsKey(input)) ManagedInputs.Remove(input);
            ManagedInputs.Add(input, eventHandler);
        }

        public static void ClearKeys()
        {
            ManagedInputs.Clear();
            PlayerManager.registeredKeys = false;
        }

        public static void PlayInputSound() => NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.AUDIONAME, Settings.AUDIOREF, true);

        // Disabled inputs
        private static List<int> DisabledControls = new List<int>();
        public static bool KeysLocked = false;

        static ControlsManager()
        {
            foreach (string control in Settings.DISABLEDCONTROLS.Split(',').Select(s => s.Trim()).ToList())
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
        public bool Hold { get; set; } = false;

        public delegate void InputEventHandler(bool pressed, bool withModifier, EventArgs args);
    }
}
