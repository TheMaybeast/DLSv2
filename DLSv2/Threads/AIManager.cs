using Rage;
using System;
using System.Collections.Generic;
using DLSv2.Utils;

namespace DLSv2.Threads
{
    internal class AiManager
    {
        private static uint _lastProcessTime = Game.GameTime;
        private const int TimeBetweenChecks = 1000;
        private const int YieldAfterChecks = 10;

        private static HashSet<Vehicle> _checkedVehicles = new HashSet<Vehicle>();

        public static void Process()
        {
            while (true)
            {
                var checksDone = 0;

                var allVeh = new HashSet<Vehicle>(World.GetAllVehicles());
                allVeh.ExceptWith(_checkedVehicles);

                foreach (var vehicle in allVeh)
                {
                    if (vehicle.GetDLS() != null)
                        _ = vehicle.GetManagedVehicle();

                    _checkedVehicles.Add(vehicle);

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