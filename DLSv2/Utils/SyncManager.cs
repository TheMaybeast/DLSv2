using System;
using System.Collections.Generic;
using Rage;

namespace DLSv2.Utils;

internal static class SyncManager
{
    internal static Dictionary<Model, uint> SyncGroups = new();
    internal static Dictionary<Model, float> DriftRanges = new();
    internal static Dictionary<Vehicle, float> DriftMultipliers = new();

    internal static void AddDriftRange(Model model, float range)
    {
        if (!DriftRanges.ContainsKey(model) && range != 0)
        {
            DriftRanges.Add(model, range);
        }
    }

    internal static float? GetAdjustedMultiplier(Vehicle vehicle, float initial)
    {
        if (!DriftRanges.ContainsKey(vehicle.Model)) return null;

        if (!DriftMultipliers.TryGetValue(vehicle, out float drift))
        {
            float range = Math.Abs(DriftRanges[vehicle.Model]);
            drift = MathHelper.GetRandomSingle(-range, range);
            DriftMultipliers.Add(vehicle, drift);
        }

        return initial * (1 + drift);
    }

    internal static void AddSyncGroup(Model model, string group)
    {
        if (string.IsNullOrWhiteSpace(group)) return;
        group = group.Trim().ToUpper();
        if (!SyncGroups.ContainsKey(group))
        {
            SyncGroups.Add(model, Game.GetHashKey(group) % 60000);
        }
    }

    internal static void SyncSirens(Vehicle vehicle)
    {
        if (SyncGroups.TryGetValue(vehicle.Model, out uint offset))
        {
            new SirenInstance(vehicle).SetSirenOnTime(offset);
        }
    }
}