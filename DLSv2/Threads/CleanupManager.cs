using DLSv2.Core;
using DLSv2.Utils;
using Rage;
using System;
using System.Linq;

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

                foreach (ManagedVehicle managedVehicle in Entrypoint.ManagedVehicles.ToList())
                {
                    if (!managedVehicle.Vehicle)
                    {
                        // Removes from Managed Vehicles
                        Entrypoint.ManagedVehicles.Remove(managedVehicle);

                        // Adds EL to available pool, if used
                        if (Entrypoint.ELUsedPool.ContainsKey(managedVehicle.VehicleHandle))
                        {
                            ("Moving " + managedVehicle.VehicleHandle + " to Available Pool").ToLog();
                            Entrypoint.ELAvailablePool.Add(Entrypoint.ELUsedPool[managedVehicle.VehicleHandle]);
                            Entrypoint.ELUsedPool.Remove(managedVehicle.VehicleHandle);
                        }

                        // Clears all sound IDs
                        foreach (var soundId in managedVehicle.SoundIds.ToList())
                            Audio.StopMode(managedVehicle, soundId.Key);
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
