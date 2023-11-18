using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Rage;
using Rage.Native;

namespace DLSv2.Utils
{
    public static class VehicleExtensions
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


        // Thanks to VincentGM for this method.
        public static unsafe bool GetLightEmissiveStatus(this Vehicle vehicle, LightID lightId)
        {
            var v = vehicle.MemoryAddress;
            var drawHandler = *(IntPtr*)((IntPtr)v + 0x48);
            if (drawHandler == IntPtr.Zero)
                return false;

            var customShaderEffect = *(IntPtr*)(drawHandler + 0x20);
            if (customShaderEffect == IntPtr.Zero)
                return false;

            var lightEmissives = (float*)(customShaderEffect + 0x20);
            if (lightEmissives == null) return false;
            return lightEmissives[(int)lightId] > 1f;
        }

        public enum LightID
        {
            [XmlEnum("defaultlight")]
            defaultlight = 0,
            [XmlEnum("headlight_l")]
            headlight_l = 1,
            [XmlEnum("headlight_r")]
            headlight_r = 2,
            [XmlEnum("taillight_l")]
            taillight_l = 3,
            [XmlEnum("taillight_r")]
            taillight_r = 4,
            [XmlEnum("indicator_lf")]
            indicator_lf = 5,
            [XmlEnum("indicator_rf")]
            indicator_rf = 6,
            [XmlEnum("indicator_lr")]
            indicator_lr = 7,
            [XmlEnum("indicator_rr")]
            indicator_rr = 8,
            [XmlEnum("brakelight_l")]
            brakelight_l = 9,
            [XmlEnum("brakelight_r")]
            brakelight_r = 10,
            [XmlEnum("brakelight_m")]
            brakelight_m = 11,
            [XmlEnum("reversinglight_l")]
            reversinglight_l = 12,
            [XmlEnum("reversinglight_r")]
            reversinglight_r = 13,
            [XmlEnum("extralight_1")]
            extralight_1 = 14,
            [XmlEnum("extralight_2")]
            extralight_2 = 15,
            [XmlEnum("extralight_3")]
            extralight_3 = 16,
            [XmlEnum("extralight_4")]
            extralight_4 = 17
        }
    }
}
