using DLSv2.Core;
using DLSv2.Core.Triggers;
using Rage;
using System;
using System.Collections.Generic;

namespace DLSv2.Threads
{
    internal class TriggersManager
    {
        public static Dictionary<string, GlobalCondition> GlobalConditions = new Dictionary<string, GlobalCondition>()
        {
            //{ "SpeedAbove", new GlobalCondition(new Func<bool>(() => Game.LocalPlayer.Character.CurrentVehicle.Speed > 10 ))},
        };

        public static void Process()
        {
            uint lastProcessTime = Game.GameTime;
            int timeBetweenChecks = 100;

            while (true)
            {
                foreach (GlobalCondition condition in GlobalConditions.Values)
                    condition.ConditionResult(condition.Evaluate());

                foreach (ManagedVehicle mV in Entrypoint.ManagedVehicles)
                    foreach (VehicleCondition condition in mV.VehicleConditions)
                        condition.ConditionResult(condition.Evaluate(mV));

                GameFiber.Sleep((int)Math.Max(timeBetweenChecks, Game.GameTime - lastProcessTime));
                lastProcessTime = Game.GameTime;
            }
        }
    }   
}