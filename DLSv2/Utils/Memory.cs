using Rage;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLSv2.Utils
{
    internal class Memory
    {
        // Default: 9999 (error)
        internal static Dictionary<string, uint> Offsets = new Dictionary<string, uint>();
        // Default: IntPtr.Zero
        internal static Dictionary<string, IntPtr> Patterns = new Dictionary<string, IntPtr>();

        internal Memory()
        {
            LoadPatterns();
            LoadOffsets();
        }

        public static unsafe T Get<T>(ulong pointer, ulong offset = 0)
        {
            try
            {
                return *(T*)(pointer + offset);
            }
            catch (Exception e)
            {
                ("MEMORY GET ERROR: " + e.Message).ToLog(true);
                return default;
            }
        }

        public static unsafe T[] GetArray<T>(ulong pointer, ulong offset = 0, int length = 1)
        {
            try
            {
                var array = new T[length];
                for (var i = 0; i < length; i++)
                    array[i] = ((T*)(pointer + offset))[i];
                return array;
            }
            catch (Exception e)
            {
                ("MEMORY GET ERROR: " + e.Message).ToLog(true);
                return default;
            }
        }

        private static void LoadPatterns()
        {
            // https://github.com/ikt32/GTAVGlowingBrakes/blob/211c352c459ab89e940134f45b4448a8d783fb7a/GlowingBrakes/VehicleExtensions.cs
            // Gets Wheels Pattern 
            Patterns["WHEELS"] = Game.FindPattern("3B B7 ?? ?? ?? ?? 7D 0D");
            // Gets Wheel Brake Pattern
            Patterns["WHEEL_BRAKE"] = Game.FindPattern("0F 2F ?? ?? ?? 00 00 0F 97 C0 EB ?? D1 ??");

            // https://github.com/citizenfx/fivem/blob/bb270ff5cb320831d77a2ca4541159a1dc582362/code/components/extra-natives-five/src/VehicleExtraNatives.cpp
            // Gets Indicators Pattern
            Patterns["INDICATORS"] = Game.FindPattern("FD 02 DB 08 98 ?? ?? ?? ?? 48 8B 5C 24 30");

            foreach (var pattern in Patterns)
                if (pattern.Value == IntPtr.Zero) ("Failed to obtain \"" + pattern.Key + "\" pattern from Memory").ToLog();
        }

        private static unsafe void LoadOffsets()
        {
            // https://github.com/ikt32/GTAVGlowingBrakes/blob/211c352c459ab89e940134f45b4448a8d783fb7a/GlowingBrakes/VehicleExtensions.cs
            // Gets Wheels Offset
            Offsets["WHEELS"] = Patterns["WHEELS"] != IntPtr.Zero ? *(uint*)((long)Patterns["WHEELS"] + 2) - 8 : 9999;
            // Gets Wheel Brake Offset
            Offsets["WHEEL_BRAKE"] = Patterns["WHEEL_BRAKE"] != IntPtr.Zero ? *(uint*)((long)Patterns["WHEEL_BRAKE"] + 3) + 0x4 : 9999;

            // https://github.com/citizenfx/fivem/blob/bb270ff5cb320831d77a2ca4541159a1dc582362/code/components/extra-natives-five/src/VehicleExtraNatives.cpp
            // Gets Indicators Offset
            Offsets["INDICATORS"] = (uint)(Patterns["INDICATORS"] != IntPtr.Zero ? Marshal.ReadInt32(Patterns["INDICATORS"], -4) : 9999);
        }
    }
}
