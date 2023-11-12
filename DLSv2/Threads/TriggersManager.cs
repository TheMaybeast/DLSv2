﻿using DLSv2.Core;
using DLSv2.Core.Triggers;
using Rage;
using System;
using System.Collections.Generic;

namespace DLSv2.Threads
{
    internal class TriggersManager
    {
        public static void Process()
        {
            while (true)
            {
                foreach (ManagedVehicle mv in Entrypoint.ManagedVehicles)
                {
                    if (mv.Vehicle)
                    {
                        foreach (ConditionInstance condition in mv.Conditions)
                        {
                            condition.Update(mv);
                        }
                    }
                }
                GameFiber.Yield();
            }
        }
    }   
}