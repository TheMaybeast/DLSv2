using System;
using System.Linq;
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

        public static VehicleIndicatorLightsStatus GetIndicatorStatus(this Vehicle vehicle)
        {
            // Gets vehicle memory address
            var address = (ulong)vehicle.MemoryAddress;

            var status = Memory.Get<int>(address, Memory.Offsets["INDICATORS"]) & 3;

            // swap first and second bit because RPH defines the enum backwards (1 = left, 2 = right)
            status = ((status & 1) << 1) | ((status & 2) >> 1);

            return (VehicleIndicatorLightsStatus)status;
        }

        public static float GetBrakePressure(this Vehicle vehicle, int wheelIndex)
        {
            // Gets vehicle memory address
            var address = (ulong)vehicle.MemoryAddress;

            // Gets the wheels pointer for current vehicle
            if (Memory.Offsets["WHEELS"] == 9999) return -1f;
            var wheelsPtr = Memory.Get<ulong>(address, Memory.Offsets["WHEELS"]);

            // Gets the number of wheels
            var wheelsCount = vehicle.GetNumWheels();
            if (wheelsCount == -1) return -1f;

            // Gets pointer to the specific wheel
            var wheelPtr = Memory.Get<ulong>(wheelsPtr, 0x008 * (ulong)wheelIndex);

            // Gets the brake pressure offset
            var brakePressure = Memory.Get<float>(wheelPtr, Memory.Offsets["WHEEL_BRAKE"]);

            return brakePressure;
        }

        public static int GetNumWheels(this Vehicle vehicle)
        {
            // Gets vehicle memory address
            var address = (ulong)vehicle.MemoryAddress;

            var wheelsCountOffset = Memory.Get<uint>((ulong)Memory.Patterns["WHEELS"], 2);

            // Gets vehicle wheels count
            var wheelsCount = Memory.Patterns["WHEELS"] != IntPtr.Zero
                ? Memory.Get<int>(address, wheelsCountOffset)
                : -1;

            return wheelsCount;
        }

        // Thanks to VincentGM for this method.
        public static bool GetLightEmissiveStatus(this Vehicle vehicle, LightID lightId)
        {
            var v = (ulong)vehicle.MemoryAddress;
            var drawHandler = Memory.Get<ulong>(v, 0x48);
            if (drawHandler == 0) return false;

            var customShaderEffect = Memory.Get<ulong>(drawHandler, 0x20);
            if (customShaderEffect == 0) return false;

            var lightEmissives = Memory.GetArray<float>(customShaderEffect, 0x20, 18);
            if (lightEmissives == null) return false;

            return lightEmissives[(int)lightId] > 1f;
        }

        public enum LightID
        {
            defaultlight = 0, headlight_l = 1, headlight_r = 2,
            taillight_l = 3, taillight_r = 4, indicator_lf = 5,
            indicator_rf = 6, indicator_lr = 7, indicator_rr = 8,
            brakelight_l = 9, brakelight_r = 10, brakelight_m = 11,
            reversinglight_l = 12, reversinglight_r = 13, extralight_1 = 14,
            extralight_2 = 15, extralight_3 = 16, extralight_4 = 17
        }

        public static float GetPetrolTankHealth(this Vehicle vehicle) => NativeFunction.Natives.GET_VEHICLE_PETROL_TANK_HEALTH<float>(vehicle);

        public static bool IsHeadlightDamaged(this Vehicle vehicle, bool left)
        {
            string nativeName = left ? "GET_IS_LEFT_VEHICLE_HEADLIGHT_DAMAGED" : "GET_IS_RIGHT_VEHICLE_HEADLIGHT_DAMAGED";
            return NativeFunction.CallByName<bool>(nativeName, vehicle);
        } 

        public static bool AreBothHeadlightsDamaged(this Vehicle vehicle) => NativeFunction.Natives.GET_BOTH_VEHICLE_HEADLIGHTS_DAMAGED<bool>(vehicle);

        public static bool IsBumperBroken(this Vehicle vehicle, bool front, bool detached)
        {
            string nativeName = detached ? "IS_VEHICLE_BUMPER_BROKEN_OFF" : "IS_VEHICLE_BUMPER_BOUNCING";
            return NativeFunction.CallByName<bool>(nativeName, vehicle, front);
        }
    }
}
