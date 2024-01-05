using Rage.Native;
using Rage;
using System;
using System.Runtime.InteropServices;

namespace DLSv2.Memory.Patches;

internal static class ExtraRepair
{
    private static IntPtr _location = IntPtr.Zero;

    public static bool Patched;

    private static int _extraRepairBytes;

    private const string Pattern = "48 8B CB ?? ?? ?? ?? 00 00 48 8B ?? ?? 8B 81 ?? ?? 00 00";
    private const int Offset = 3;

    public static bool Patch()
    {
        if (Patched) return true;

        try
        {
            if (_location == IntPtr.Zero)
            {
                var addr = Game.FindPattern(Pattern);
                if (addr == IntPtr.Zero) return false;
                _location = addr;
            }

            if (_extraRepairBytes == 0) _extraRepairBytes = Marshal.ReadInt32(_location, Offset);

            Marshal.WriteInt32(_location, Offset, 0);
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
        if (!Patched || _extraRepairBytes == 0) return false;

        try
        {
            Marshal.WriteInt32(_location, Offset, _extraRepairBytes);
            Patched = false;
            return true;
        }
        catch
        {
            return false;
        }
    }
}