using DLSv2.Core;
using DLSv2.Utils;
using Rage;
using System;
using System.Collections.Generic;
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
                List<uint> UsedHashes = new List<uint>();

                foreach (ManagedVehicle activeVeh in Entrypoint.ManagedVehicles.ToList())
                {
                    if (!activeVeh.Vehicle)
                        Entrypoint.ManagedVehicles.Remove(activeVeh);
                    else
                        UsedHashes.Add(activeVeh.CurrentELHash);

                    checksDone++;
                    if (checksDone % yieldAfterChecks == 0)
                        GameFiber.Yield();
                }

                foreach (uint hash in Entrypoint.ELUsedPool.Keys.ToList())
                {
                    if (!UsedHashes.Contains(hash))
                    {
                        ("Moving " + hash + " to Available Pool").ToLog();
                        Entrypoint.ELAvailablePool.Add(Entrypoint.ELUsedPool[hash]);
                        Entrypoint.ELUsedPool.Remove(hash);
                    }
                }
                GameFiber.Sleep((int)Math.Max(timeBetweenChecks, Game.GameTime - lastProcessTime));
                lastProcessTime = Game.GameTime;
            }
        }
    }
}
