using System;
using Rage;

namespace DLSv2.Utils
{
    using Threads;

    internal static class VehicleOwner
    {
        public static bool IsPlayerVehicle(this Vehicle veh)
        {
            return PlayerManager.cachedPlayerVehicles.Contains(veh);
        }
    }
}
