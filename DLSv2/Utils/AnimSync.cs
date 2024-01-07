using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rage;
using Rage.Native;
using Rage.Attributes;


namespace DLSv2.Utils
{
    internal static class AnimSync
    {
        private static short UVAnimOffset;

        static unsafe AnimSync()
        {
            var addr = Game.FindPattern("F3 0F 11 85 B4 02 00 00 E8 ?? ?? ?? ?? 8A 05");
            if (addr == IntPtr.Zero) throw new Exception("Can't find UV anim offset");
            addr = addr + 9;
            var functionAddr = addr + 4 + *(int*)addr;
            Game.LogTrivial($"Function address: {functionAddr:X8}");
            UVAnimOffset = *(short*)(functionAddr + 17);
            Game.LogTrivial($"UVAnimOffset: {UVAnimOffset:X8}");
        }

        public static unsafe bool GetVehicleUVAnim(this Vehicle vehicle, int uvChannel, out NativeVector2* uvAnim)
        {
            NativeVector2 d = default;
            uvAnim = &d;

            if (UVAnimOffset == default) return false;

            var v = vehicle.MemoryAddress;
            var drawHandler = *(IntPtr*)((IntPtr)v + 0x48);
            if (drawHandler == IntPtr.Zero)
                return false;

            var customShaderEffect = *(IntPtr*)(drawHandler + 0x20);
            if (customShaderEffect == IntPtr.Zero)
                return false;

            int uvOffset = uvChannel == 1 ? UVAnimOffset : UVAnimOffset + 8;
            uvAnim = (NativeVector2*)(customShaderEffect + uvOffset);
            if (uvAnim == null) return false;
            return true;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x8)]
        public struct NativeVector2
        {
            public float x;
            public float y;
        };

        [ConsoleCommand]
        private static unsafe void TestUVStatus()
        {
            Vehicle v = Game.LocalPlayer.Character.LastVehicle;

            while (v)
            {
                bool b1 = v.GetVehicleUVAnim(1, out var uvAnim1);
                NativeVector2 uv1 = *((NativeVector2*)uvAnim1);
                bool b2 = v.GetVehicleUVAnim(2, out var uvAnim2);
                NativeVector2 uv2 = *((NativeVector2*)uvAnim2);

                string info = b1 ? $"~g~UV 1~w~: {uv1.x:0.##}, {uv1.y:0.##}\n" : "~r~UV 1~w~\n";
                info += b2 ? $"~g~UV 2~w~: {uv2.x:0.##}, {uv2.y:0.##}\n" : "~r~UV 2~w~\n";

                Game.DisplaySubtitle(info, 10);
                GameFiber.Yield();
            }
        }
    }
}
