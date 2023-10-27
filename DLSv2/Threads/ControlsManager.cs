using DLSv2.Core;
using DLSv2.Utils;
using Rage;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DLSv2.Threads
{
    internal class ControlsManager
    {
        private static Dictionary<Keys, ControlGroup.ControlGroupKeybindingEventHandler> ManagedKeys = new Dictionary<Keys, ControlGroup.ControlGroupKeybindingEventHandler>();

        public static void Process()
        {
            while(true)
            {
                foreach (Keys key in ManagedKeys.Keys)
                {
                    if (Game.IsKeyDown(key))
                    {
                        switch (Settings.KB_MODIFIER)
                        {
                            case Keys.Shift:
                                ManagedKeys[key](Game.IsShiftKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Control:
                                ManagedKeys[key](Game.IsControlKeyDownRightNow, EventArgs.Empty);
                                break;
                            case Keys.Alt:
                                ManagedKeys[key](Game.IsAltKeyDownRightNow, EventArgs.Empty);
                                break;
                            default:
                                ManagedKeys[key](false, EventArgs.Empty);
                                break;
                        }
                    }
                        
                }

                GameFiber.Yield();
            }
        }

        public static void RegisterKey(Keys key, ControlGroup.ControlGroupKeybindingEventHandler eventHandler)
        {
            if (ManagedKeys.ContainsKey(key)) ManagedKeys.Remove(key);
            ManagedKeys.Add(key, eventHandler);
        }

        public static void ClearKeys() => ManagedKeys.Clear();
    }
}
