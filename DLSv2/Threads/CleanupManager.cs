using DLSv2.Core;
using DLSv2.Utils;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace DLSv2.Threads
{
    internal class CleanupManager
    {
        static uint lastProcessTime = Game.GameTime;
        static int timeBetweenChecks = 10000;
        static int yieldAfterChecks = 10;
        public static void Process()
        {
            while (true)
            {
                int checksDone = 0;

                foreach (ManagedVehicle activeVeh in Entrypoint.ManagedVehicles.ToList())
                {
                    if (!activeVeh.Vehicle)
                    {
                        // Removes from Managed Vehicles
                        Entrypoint.ManagedVehicles.Remove(activeVeh);

                        // Adds EL to available pool, if used
                        if (Entrypoint.ELUsedPool.ContainsKey(activeVeh.VehicleHandle))
                        {
                            ("Moving " + activeVeh.VehicleHandle + " to Available Pool").ToLog();
                            Entrypoint.ELAvailablePool.Add(Entrypoint.ELUsedPool[activeVeh.VehicleHandle]);
                            Entrypoint.ELUsedPool.Remove(activeVeh.VehicleHandle);
                        }

                        // Clears all sound IDs
                        foreach (var soundId in activeVeh.SoundIds)
                            Audio.StopMode(activeVeh, soundId.Key);
                    }

                    checksDone++;
                    if (checksDone % yieldAfterChecks == 0)
                        GameFiber.Yield();
                }
                GameFiber.Sleep((int)Math.Max(timeBetweenChecks, Game.GameTime - lastProcessTime));
                lastProcessTime = Game.GameTime;
            }
        }
    }
}
