using System;
using System.Runtime.InteropServices;
using Rage;

namespace DLSv2.Utils;

public static class Emissives
{
    public static bool GetLightEmissiveStatus(this Vehicle vehicle, LightID lightId)
    {
        var v = vehicle.MemoryAddress;
            
        var drawHandler = Marshal.ReadIntPtr(v + 72);
        if (drawHandler == IntPtr.Zero) return false;

        var customShaderEffect = Marshal.ReadIntPtr(drawHandler + 32);
        if (customShaderEffect == IntPtr.Zero) return false;

        var lightEmissives = Memory.GetArray<float>((ulong)customShaderEffect, 32, 18);
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
}