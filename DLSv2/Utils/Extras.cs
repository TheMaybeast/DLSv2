using Rage.Native;
using Rage;
using System;
using System.Runtime.InteropServices;

namespace DLSv2.Utils
{
    internal static class ExtrasExtensions
    {
        public static bool HasExtra(this Vehicle vehicle, int extra) => NativeFunction.Natives.DoesExtraExist<bool>(vehicle, extra);
        public static bool IsExtraEnabled(this Vehicle vehicle, int extra) => NativeFunction.Natives.IsVehicleExtraTurnedOn<bool>(vehicle, extra);
        public static void SetExtra(this Vehicle vehicle, int extra, bool enabled) => NativeFunction.Natives.SetVehicleExtra(vehicle, extra, !enabled);
    }

    internal static class ExtraRepairPatch
    {
        private static int _extraRepairBytes;
        private const string Pattern = "48 8B CB ?? ?? ?? ?? 00 00 48 8B ?? ?? 8B 81 ?? ?? 00 00";

        public static bool Patch()
        {
            try
            {
                var addr = Game.FindPattern(Pattern);
                if (addr == IntPtr.Zero) return false;

                if (_extraRepairBytes == 0) _extraRepairBytes = Marshal.ReadInt32(addr, 3);

                Marshal.WriteInt32(addr, 3, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Remove()
        {
            if (_extraRepairBytes == 0) return false;

            try
            {
                var addr = Game.FindPattern(Pattern);
                if (addr == IntPtr.Zero) return false;

                Marshal.WriteInt32(addr, 3, _extraRepairBytes);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
