using DLSv2.Core;
using Rage;
using System;
using System.Collections.Generic;

namespace DLSv2.Threads
{
    internal class TriggersManager
    {
        public static Dictionary<string, GlobalCondition> GlobalConditions = new Dictionary<string, GlobalCondition>();

        public static void Process()
        {
            uint lastProcessTime = Game.GameTime;
            int timeBetweenChecks = 100;

            while (true)
            {
                foreach (GlobalCondition condition in GlobalConditions.Values)
                    condition.ConditionResult(condition.Evaluate());

                foreach (ManagedVehicle mV in Entrypoint.ManagedVehicles)
                {
                    if (mV.Vehicle)
                        foreach (VehicleCondition condition in mV.VehicleConditions)
                            condition.ConditionResult(condition.Evaluate());
                }

                GameFiber.Sleep((int)Math.Max(timeBetweenChecks, Game.GameTime - lastProcessTime));
                lastProcessTime = Game.GameTime;
            }
        }
    }   
}