using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Rage;
using Rage.Attributes;
using Rage.ConsoleCommands.AutoCompleters;

namespace DLSv2.Utils
{
    internal static class VehicleOwner
    {
        private static HashSet<Vehicle> cachedPlayerVehicles = new HashSet<Vehicle>();
        
        /*
        private delegate IntPtr GetLastDriverAddressDelegate(IntPtr vehAddress);
        private static GetLastDriverAddressDelegate getDriverFunc = null;

        static unsafe VehicleOwner()
        {
            IntPtr getDriverLoc = Game.FindPattern("1C 57 48 81 C1 ?? ?? ?? ??");
            if (getDriverLoc == IntPtr.Zero) return;

            getDriverFunc = Marshal.GetDelegateForFunctionPointer<GetLastDriverAddressDelegate>(getDriverLoc);
        }
        */

        public static bool IsPlayerVehicle(this Vehicle veh)
        {
            if (!veh) return false;

            bool isPlayer = false;

            if (veh.Driver)
            {
                isPlayer = veh.Driver.IsLocalPlayer;
            }
            else if (Game.LocalPlayer.Character.LastVehicle == veh)
            {
                isPlayer = true;
            } 
            else if (cachedPlayerVehicles.Contains(veh))
            {
                return true;
            }
            /*
            else
            {
                isPlayer = getDriverFunc(veh.MemoryAddress) == Game.LocalPlayer.Character.MemoryAddress;
            }
            */

            if (isPlayer) cachedPlayerVehicles.Add(veh);
            else cachedPlayerVehicles.Remove(veh);

            return isPlayer;
        }

#if DEBUG
        [ConsoleCommand]
        private static void DebugIsPlayerVehicle([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterVehicle))] Vehicle vehicle)
        {
            if (vehicle)
            {
                Game.LogTrivial($"{vehicle.Model} {vehicle.Handle.Value.ToString("X")} player vehicle = {vehicle.IsPlayerVehicle()}");
            }
        }
#endif
    }
}
