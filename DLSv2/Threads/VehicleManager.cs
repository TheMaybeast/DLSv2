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
            GameFiber.Yield();

            if (Game.IsPaused || Game.Console.IsOpen) continue;

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
                        // TODO: Update this to smartly toggle lights for player-controlled vehicles
                        mv.LightsOn = mv.Vehicle.IsSirenOn;
                        mv.UpdateLights();
                    }
                    else if (mv.lightsNeedUpdate)
                    {
                        // Process updates if required
                        // UpdateLights calls ProcessExtendedSequences already
                        mv.UpdateLights();
                    }
                    else if (mv.LightsOn) 
                    {
                        // Process extended sequences if lights are enabled and no updates required
                        mv.ProcessExtendedSequences(false);
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
        }
    }
}
