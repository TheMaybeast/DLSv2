using Rage.Native;
using Rage;
using System;
using System.Runtime.InteropServices;

namespace DLSv2.Utils;

internal static class ExtraRepairPatch
{
    private static IntPtr location = IntPtr.Zero;
    private static bool Patched;
    private static int extraRepairBytes;

    public static void MemoryInit()
    {
        var address = Game.FindPattern("48 8B CB ?? ?? ?? ?? 00 00 48 8B ?? ?? 8B 81 ?? ?? 00 00");
        if (Memory.AssertAddress(address, "ExtraRepairPatch"))
            location = address + 3;
    }

    public static bool Patch()
    {
        if (Patched) return true;

        try
        {
            if (location == IntPtr.Zero) return false;
            if (extraRepairBytes == 0) extraRepairBytes = Marshal.ReadInt32(location);

            Marshal.WriteInt32(location, 0);
            Patched = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Remove()
    {
        if (!Patched || extraRepairBytes == 0) return false;

        try
        {
            Marshal.WriteInt32(location, extraRepairBytes);
            Patched = false;
            return true;
        }
        catch
        {
            return false;
        }
    }
}