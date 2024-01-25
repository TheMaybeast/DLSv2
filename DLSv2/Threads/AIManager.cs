using Rage;
using System;
using System.Collections.Generic;

namespace DLSv2.Threads;

using Utils;

internal class AiManager
{
    private static uint lastScanTime = CachedGameTime.GameTime;
    private const int timeBetweenScans = 1000;
    private const int yieldAfterScan = 10;

    private const int timeBetweenMonitor = 100;

    private static HashSet<Vehicle> scannedVehicles = new();

    public static void ScanProcess()
    {
        while (true)
        {
            var checksDone = 0;

            var allVeh = new HashSet<Vehicle>(World.GetAllVehicles());
            allVeh.ExceptWith(scannedVehicles);

            foreach (var vehicle in allVeh)
            {
                if (vehicle.GetDLS() != null)
                    _ = vehicle.GetManagedVehicle();

                scannedVehicles.Add(vehicle);

                checksDone++;
                if (checksDone % yieldAfterScan == 0)
                    GameFiber.Yield();
            }
            GameFiber.Sleep((int)Math.Max(timeBetweenScans, CachedGameTime.GameTime - lastScanTime));
            lastScanTime = CachedGameTime.GameTime;
        }
    }

    public static void MonitorProcess()
    {
        while (true)
        {
            foreach (var mv in Entrypoint.ManagedVehicles)
            {
                if (!mv.Key) continue;
                if (mv.Value.LightsOn == mv.Key.IsSirenOn) continue;

                mv.Value.LightsOn = mv.Key.IsSirenOn;
                mv.Value.UpdateLights();
            }

            GameFiber.Sleep(timeBetweenMonitor);
        }
    }
}