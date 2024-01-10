using System;
using System.Collections.Generic;
using System.Linq;
using Rage;

namespace DLSv2.Utils
{
    internal static class VehicleOwner
    {
        public delegate void IsPlayerVehicleChangedEvent(Vehicle vehicle, bool isPlayerVehicle);
        public static event IsPlayerVehicleChangedEvent OnIsPlayerVehicleChanged;

        private static HashSet<Vehicle> cachedPlayerVehicles = new();

        static VehicleOwner()
        {
            Ped p = Game.LocalPlayer.Character;
            Vehicle lv = p.LastVehicle;
            if (lv && (!lv.HasDriver || lv.Driver.IsPlayer)) cachedPlayerVehicles.Add(lv);
        }

        public static bool IsPlayerVehicle(this Vehicle veh)
        {
            return cachedPlayerVehicles.Contains(veh);
        }

        public static void Process()
        {
            foreach (var v in cachedPlayerVehicles.ToArray())
            {
                if (!v || (v.HasDriver && !v.Driver.IsPlayer))
                {
                    cachedPlayerVehicles.Remove(v);
                    if (v) OnIsPlayerVehicleChanged?.Invoke(v, false);
                }
            }
        }

        public static void AddPlayerVehicle(Vehicle veh)
        {
            if (!cachedPlayerVehicles.Contains(veh))
            {
                cachedPlayerVehicles.Add(veh);
                OnIsPlayerVehicleChanged?.Invoke(veh, true);
            }
        }
    }
}
