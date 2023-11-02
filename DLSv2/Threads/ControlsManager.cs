﻿using DLSv2.Utils;
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

        public static void Process()
        {
            while(true)
            {
                GameFiber.Yield();

                if (Game.IsPaused) continue;

                foreach (Input input in ManagedInputs.Keys)
                {                    
                    if (input.Key != Keys.None && Game.IsKeyDown(input.Key))
                    {
                        switch (Settings.SET_MODIFIER)
                        {
                            case Keys.Shift:
                                if (Controls.KeysLocked && input.Name != "LOCKALL") continue;
                                PlayInputSound();
                                ManagedInputs[input](Game.IsShiftKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Control:
                                if (Controls.KeysLocked && input.Name != "LOCKALL") continue;
                                PlayInputSound();
                                ManagedInputs[input](Game.IsControlKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Alt:
                                if (Controls.KeysLocked && input.Name != "LOCKALL") continue;
                                PlayInputSound();
                                ManagedInputs[input](Game.IsAltKeyDownRightNow, EventArgs.Empty);
                                break;
                            default:
                                if (Controls.KeysLocked && input.Name != "LOCKALL") continue;
                                PlayInputSound();
                                ManagedInputs[input](false, EventArgs.Empty);
                                break;
                        }
                    }
                    else if (input.ControllerButton != ControllerButtons.None && Game.IsControllerButtonDown(input.ControllerButton))
                    {
                        if (Controls.KeysLocked && input.Name != "LOCKALL") continue;
                        PlayInputSound();
                        ManagedInputs[input](false, EventArgs.Empty);
                    }
                }
            }
        }

        public static void RegisterInput(string inputName, Input.InputEventHandler eventHandler)
        {
            Input input = new Input
            {
                Name = inputName,
                Key = Settings.INI.ReadEnum(inputName, "Key", Keys.None),
                ControllerButton = Settings.INI.ReadEnum(inputName, "ControllerButton", ControllerButtons.None)
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

        public static void PlayInputSound() => NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, Settings.SET_AUDIONAME, Settings.SET_AUDIOREF, true);
    }
}
