using Rage;
using System;
using DLSv2.Utils;

namespace DLSv2.Threads
{
    internal class AiManager
    {
        private static uint _lastProcessTime = Game.GameTime;
        private const int TimeBetweenChecks = 10000;
        private const int YieldAfterChecks = 10;

        public static void Process()
        {
            while (true)
            {
                var checksDone = 0;

                foreach (var vehicle in World.GetAllVehicles())
                {
                    if (vehicle.IsDLS())
                        _ = vehicle.GetManagedVehicle();

                    checksDone++;
                    if (checksDone % YieldAfterChecks == 0)
                        GameFiber.Yield();
                }
                GameFiber.Sleep((int)Math.Max(TimeBetweenChecks, Game.GameTime - _lastProcessTime));
                _lastProcessTime = Game.GameTime;
            }
        }
    }
}