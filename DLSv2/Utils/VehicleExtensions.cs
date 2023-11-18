using System;
using System.Linq;
using System.Runtime.InteropServices;
using Rage;
using Rage.Native;

namespace DLSv2.Utils
{
    internal static class VehicleExtensions
    {
        public static float GetForwardSpeed(this Vehicle vehicle) => NativeFunction.Natives.GET_ENTITY_SPEED_VECTOR<Vector3>(vehicle, true).Y;

        public static int GetLivery(this Vehicle vehicle) => NativeFunction.Natives.GET_VEHICLE_LIVERY<int>(vehicle);

        public static void ClearSiren(this Vehicle vehicle)
        {
            bool delv = false;
            Vehicle v = Game.LocalPlayer.Character.GetNearbyVehicles(16).FirstOrDefault(veh => veh && !veh.HasSiren);
            if (!v)
                v = World.EnumerateVehicles().FirstOrDefault(veh => veh && !veh.HasSiren);
            if (!v)
            {
                delv = true;
                v = new Vehicle("asea", Vector3.Zero);
            }
            if (!v)
            {
                ("ClearSiren: Unable to find/generate vehicle without a siren").ToLog();
                return;
            }
            if (!vehicle)
            {
                ("ClearSiren: Target vehicle does not exist").ToLog();
                return;
            }

            vehicle.ApplySirenSettingsFrom(v);

            if (delv && v) v.Delete();
        }

        private static int? indicatorOffset = null;
        public static unsafe VehicleIndicatorLightsStatus GetIndicatorStatus(this Vehicle vehicle)
        {
            // https://github.com/citizenfx/fivem/blob/bb270ff5cb320831d77a2ca4541159a1dc582362/code/components/extra-natives-five/src/VehicleExtraNatives.cpp#L528
            // https://github.com/citizenfx/fivem/blob/bb270ff5cb320831d77a2ca4541159a1dc582362/code/components/extra-natives-five/src/VehicleExtraNatives.cpp#L1139

            if (!indicatorOffset.HasValue)
            {
                IntPtr location = Game.FindPattern("FD 02 DB 08 98 ?? ?? ?? ?? 48 8B 5C 24 30");
                if (location == IntPtr.Zero) indicatorOffset = -1;
                else indicatorOffset = Marshal.ReadInt32(location, -4);
            }

            if (indicatorOffset == -1) return VehicleIndicatorLightsStatus.Off;

            int status = Marshal.ReadInt32(vehicle.MemoryAddress, indicatorOffset.Value) & 3;

            // swap first and second bit because RPH defines the enum backwards (1 = left, 2 = right)
            status = ((status & 1) << 1) | ((status & 2) >> 1);

            return (VehicleIndicatorLightsStatus)status;
        }
    }
}
