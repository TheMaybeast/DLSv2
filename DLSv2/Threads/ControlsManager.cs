using DLSv2.Utils;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DLSv2.Threads
{
    internal class ControlsManager
    {
        private static Dictionary<Input, Input.InputEventHandler> ManagedInputs = new Dictionary<Input, Input.InputEventHandler>();

        public static bool IsLocked = false;

        public static void Process()
        {
            while(true)
            {
                foreach (Input input in ManagedInputs.Keys)
                {                    
                    if (input.Key != Keys.None && Game.IsKeyDown(input.Key))
                    {
                        PlayInputSound();
                        switch (Settings.KB_MODIFIER)
                        {
                            case Keys.Shift:
                                ManagedInputs[input](Game.IsShiftKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Control:
                                ManagedInputs[input](Game.IsControlKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Alt:
                                ManagedInputs[input](Game.IsAltKeyDownRightNow, EventArgs.Empty);
                                break;
                            default:
                                ManagedInputs[input](false, EventArgs.Empty);
                                break;
                        }
                    }
                    else if (input.ControllerButton != ControllerButtons.None && Game.IsControllerButtonDown(input.ControllerButton))
                    {
                        PlayInputSound();
                        ManagedInputs[input](false, EventArgs.Empty);
                    }
                }

                GameFiber.Yield();
            }
        }

        public static void RegisterInput(Input input, Input.InputEventHandler eventHandler)
        {
            ("Mapped input [" + input.Name + "] to Key [" + input.Key + "]" +
                (input.ControllerButton != ControllerButtons.None ? " and ControllerButton [" + input.ControllerButton + "]" : "")).ToLog();

            if (ManagedInputs.ContainsKey(input)) ManagedInputs.Remove(input);
            ManagedInputs.Add(input, eventHandler);
        }

        public static void ClearKeys() => ManagedInputs.Clear();

        public static void PlayInputSound() => NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
    }
}
