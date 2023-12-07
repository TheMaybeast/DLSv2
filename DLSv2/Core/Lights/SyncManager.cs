using System;
using System.Collections.Generic;
using System.Linq;
using Rage;

namespace DLSv2.Core.Lights
{
    using Utils;

    internal static class SyncManager
    {
        internal static Dictionary<Model, uint> SyncGroups = new Dictionary<Model, uint>();

        internal static void AddGroup(Model model, string group)
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
}
