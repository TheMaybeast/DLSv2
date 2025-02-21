using Rage;
using System.Linq;

namespace DLSv2.Threads;
using Core;
using Utils;

internal static class VehicleManager
{
    internal static void Process()
    {
        while(true)
        {
            foreach (ManagedVehicle mv in Entrypoint.ManagedVehicles.Values.ToArray())
            {
                // Check if the vehicle is still valid
                if (mv.Vehicle)
                {
                    // Update triggers and conditions
                    foreach (BaseCondition condition in mv.Conditions) condition.Update(mv);

                    // Check if DLS light status matches vehicle siren status
                    if (mv.LightsOn != mv.Vehicle.IsSirenOn)
                    {
                        // If status does not match, force update lights now
                        mv.LightsOn = mv.Vehicle.IsSirenOn;
                        // Change this to set waiting for update = true
                        mv.UpdateLights();
                    } else if (mv.LightsOn)
                    {
                        // If DLS lights enabled, process sequences and updates
                        if (mv.lightsNeedUpdate) mv.UpdateLights();
                        else mv.ProcessExtendedSequences(false);
                    }
                } else
                {
                    // Vehicle is no longer valid. Do cleanup.
                    Entrypoint.ManagedVehicles.Remove(mv.Vehicle);

                    // Adds EL to available pool, if used
                    if (Entrypoint.ELUsedPool.ContainsKey(mv.VehicleHandle))
                    {
                        ("Moving " + mv.VehicleHandle + " to Available Pool").ToLog();
                        Entrypoint.ELAvailablePool.Add(Entrypoint.ELUsedPool[mv.VehicleHandle]);
                        Entrypoint.ELUsedPool.Remove(mv.VehicleHandle);
                    }

                    // Clears all sound IDs
                    foreach (var soundId in mv.SoundIds.ToArray())
                        mv.StopMode(soundId.Key);
                }
            }
            GameFiber.Yield();
        }
    }
}
