using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Rage;
using Rage.Native;

namespace DLSv2.Utils
{
    using Core;

    public static class VehicleExtensions
    {
        public static float GetForwardSpeed(this Vehicle vehicle) => NativeFunction.Natives.GET_ENTITY_SPEED_VECTOR<Vector3>(vehicle, true).Y;

        public static int GetLivery(this Vehicle vehicle) => NativeFunction.Natives.GET_VEHICLE_LIVERY<int>(vehicle);

        public static VehicleIndicatorLightsStatus GetIndicatorStatus(this Vehicle vehicle)
        {
            // Gets vehicle memory address
            var address = vehicle.MemoryAddress;

            if (Memory.GameOffsets.CVehicle_IndicatorsOffset == 0) return 0;
            var status = Marshal.ReadInt32(address + Memory.GameOffsets.CVehicle_IndicatorsOffset) & 3;

            // swap first and second bit because RPH defines the enum backwards (1 = left, 2 = right)
            status = ((status & 1) << 1) | ((status & 2) >> 1);

            return (VehicleIndicatorLightsStatus)status;
        }

        public static float GetBrakePressure(this Vehicle vehicle, int wheelIndex)
        {
            // Gets vehicle memory address
            var address = vehicle.MemoryAddress;

            // Gets the wheels pointer for current vehicle
            if (Memory.GameOffsets.CVehicle_WheelsOffset == 0) return -1f;
            var wheelsPtr = Marshal.ReadIntPtr(address + Memory.GameOffsets.CVehicle_WheelsOffset);

            // Gets the number of wheels
            var wheelsCount = vehicle.GetNumWheels();
            if (wheelsCount == -1) return -1f;

            // Gets pointer to the specific wheel
            var wheelPtr = Marshal.ReadIntPtr(wheelsPtr + (8 * wheelIndex));

            // Gets the brake pressure offset
            if (Memory.GameOffsets.CWheel_BrakePressureOffset == 0) return -1f;
            var brakePressure = (float)Marshal.ReadInt32(wheelPtr + Memory.GameOffsets.CWheel_BrakePressureOffset);

            return brakePressure;
        }

        public static int GetNumWheels(this Vehicle vehicle)
        {
            // Gets vehicle memory address
            var address = vehicle.MemoryAddress;

            // Gets vehicle wheels count
            if (Memory.GameOffsets.CWheels_WheelCountOffset == 0) return -1;
            var wheelsCount = Marshal.ReadInt32(address + Memory.GameOffsets.CWheels_WheelCountOffset);

            return wheelsCount;
        }

        // Thanks to VincentGM for this method.
        public static bool GetLightEmissiveStatus(this Vehicle vehicle, LightID lightId)
        {
            var v = vehicle.MemoryAddress;
            
            var drawHandler = Marshal.ReadIntPtr(v + 72);
            if (drawHandler == IntPtr.Zero) return false;

            var customShaderEffect = Marshal.ReadIntPtr(drawHandler + 32);
            if (customShaderEffect == IntPtr.Zero) return false;

            var lightEmissives = Memory.Unsafe.GetArray<float>((ulong)customShaderEffect, 32, 18);
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
        
        public static bool HasExtra(this Vehicle vehicle, int extra) =>
            NativeFunction.Natives.DoesExtraExist<bool>(vehicle, extra);

        public static bool IsExtraEnabled(this Vehicle vehicle, int extra) =>
            NativeFunction.Natives.IsVehicleExtraTurnedOn<bool>(vehicle, extra);

        public static void SetExtra(this Vehicle vehicle, int extra, bool enabled) =>
            NativeFunction.Natives.SetVehicleExtra(vehicle, extra, !enabled);

        public static void SetAnimSpeed(this Vehicle vehicle, string animDict, string animName, float speed) =>
            NativeFunction.Natives.SET_ENTITY_ANIM_SPEED(vehicle, animDict, animName, speed);

        public static bool PlayAnim(this Vehicle vehicle, string animDict, string animName, float blend, bool loop, bool keepLastFrame, float startPos = 0f, int flags = 0) =>
            NativeFunction.Natives.PLAY_ENTITY_ANIM<bool>(vehicle, animName, animDict, blend, loop, keepLastFrame, false, startPos, flags);

        public static bool LoadAndPlayAnim(this Vehicle vehicle, AnimationDictionary animDict, string animName, float blend, bool loop, bool keepLastFrame, float startPos = 0f, int flags = 0)
        {
            if (!animDict.IsLoaded)
            {
                if (!NativeFunction.Natives.DOES_ANIM_DICT_EXIST<bool>(animDict.Name)) return false;

                animDict.LoadAndWait();
            }
            
            return PlayAnim(vehicle, animDict.Name, animName, blend, loop, keepLastFrame, startPos, flags);
        }

        public static bool LoadAndPlayAnim(this Vehicle vehicle, Animation anim)
        {
            bool result = LoadAndPlayAnim(vehicle, anim.AnimDict, anim.AnimName, anim.BlendDelta, anim.Loop, anim.StayInLastFrame, anim.StartPhase, anim.Flags);
            if (anim.Speed.HasValue) GameFiber.StartNew(() => SetAnimSpeed(vehicle, anim.AnimDict, anim.AnimName, anim.Speed.Value));
            return result;
        }

        public static bool StopAnim(this Vehicle vehicle, string animDict, string animName, float blend = 4f) =>
            NativeFunction.Natives.STOP_ENTITY_ANIM<bool>(vehicle, animName, animDict, blend);

        public static bool StopAnim(this Vehicle vehicle, Animation anim) =>
            StopAnim(vehicle, anim.AnimDict, anim.AnimName, anim.BlendDelta);

        public static bool IsAnimEventActive(this Vehicle vehicle, uint eventHash) =>
            NativeFunction.Natives.HAS_ANIM_EVENT_FIRED<bool>(vehicle, eventHash);

        public static bool IsAnimEventActive(this Vehicle vehicle, string eventName) =>
            IsAnimEventActive(vehicle, Game.GetHashKey(eventName));

        public static (int primary, int secondary) GetColors(this Vehicle vehicle)
        {
            NativeFunction.Natives.GET_VEHICLE_COLOURS(vehicle, out int primary, out int secondary);
            return (primary, secondary);
        }

        public static void SetColors(this Vehicle vehicle, int primaryColorCode, int secondaryColorCode)
        {
            NativeFunction.Natives.SET_VEHICLE_COLOURS(vehicle, primaryColorCode, secondaryColorCode);
        }

        public static void SetPrimaryColor(this Vehicle vehicle, int primaryColorCode)
        {
            SetColors(vehicle, primaryColorCode, GetColors(vehicle).secondary);
        }

        public static void SetSecondaryColor(this Vehicle vehicle, int secondaryColorCode)
        {
            SetColors(vehicle, GetColors(vehicle).primary, secondaryColorCode);
        }

        public static (int pearl, int wheel) GetExtraColors(this Vehicle vehicle)
        {
            NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOURS(vehicle, out int pearl, out int wheel);
            return (pearl, wheel);
        }

        public static void SetExtraColors(this Vehicle vehicle, int pearlescentColorCode, int wheelColorCode)
        {
            NativeFunction.Natives.SET_VEHICLE_EXTRA_COLOURS(vehicle, pearlescentColorCode, wheelColorCode);
        }

        public static void SetPearlescentColor(this Vehicle vehicle, int pearlescentColorCode)
        {
            SetExtraColors(vehicle, pearlescentColorCode, GetExtraColors(vehicle).wheel);
        }

        public static void SetWheelColor(this Vehicle vehicle, int wheelColorCode)
        {
            SetExtraColors(vehicle, GetExtraColors(vehicle).pearl, wheelColorCode);
        }

        public static int GetColor5(this Vehicle vehicle)
        {
            NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOUR_5<bool>(vehicle, out int color);
            return color;
        }

        public static void SetColor5(this Vehicle vehicle, int colorCode)
        {
            NativeFunction.Natives.SET_VEHICLE_EXTRA_COLOUR_5(vehicle, colorCode);
        }

        public static int GetColor6(this Vehicle vehicle)
        {
            NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOUR_6<bool>(vehicle, out int color);
            return color;
        }

        public static void SetColor6(this Vehicle vehicle, int colorCode)
        {
            NativeFunction.Natives.SET_VEHICLE_EXTRA_COLOUR_6(vehicle, colorCode);
        }

        public static void SetPaint(this Vehicle vehicle, int paintSlot, int colorCode)
        {
            switch (paintSlot)
            {
                case 1:
                    vehicle.SetPrimaryColor(colorCode);
                    return;
                case 2:
                    vehicle.SetSecondaryColor(colorCode);
                    return;
                case 3:
                    vehicle.SetPearlescentColor(colorCode);
                    return;
                case 4:
                    vehicle.SetWheelColor(colorCode);
                    return;
                case 6:
                    vehicle.SetColor5(colorCode);
                    return;
                case 7:
                    vehicle.SetColor6(colorCode);
                    return;
                default:
                    $"Paint slot {paintSlot} is invalid. Must be one of 1, 2, 3, 4, 6, 7.".ToLog(LogLevel.ERROR);
                    return;
            }
        }

        public static int GetPaint(this Vehicle vehicle, int paintSlot)
        {
            switch (paintSlot)
            {
                case 1:
                    return vehicle.GetColors().primary;
                case 2:
                    return vehicle.GetColors().secondary;
                case 3:
                    return vehicle.GetExtraColors().pearl;
                case 4:
                    return vehicle.GetExtraColors().wheel;
                case 6:
                    return vehicle.GetColor5();
                case 7:
                    return vehicle.GetColor6();
                default:
                    $"Paint slot {paintSlot} is invalid. Must be one of 1, 2, 3, 4, 6, 7.".ToLog(LogLevel.ERROR);
                    return -1;
            }
        }
    }
}
