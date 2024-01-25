using System;

namespace DLSv2.Utils;

internal class Memory
{
    public Memory()
    {
        // Initializes memory utils
        SirenInstance.MemoryInit();
        SirenSounds.MemoryInit();
        Wheels.MemoryInit();
        ExtraRepairPatch.MemoryInit();
        Indicators.MemoryInit();
    }
    
    public static bool AssertAddress(IntPtr address, string name)
    {
        if (address != IntPtr.Zero)
        {
            $"  {name} @ {(ulong)address:X}".ToLog(LogLevel.DEBUG);
            return true;
        }

        $"Incompatible game version, couldn't find {name} instance.".ToLog(LogLevel.ERROR);
        return false;
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
            ("MEMORY GET ERROR: " + e.Message).ToLog(LogLevel.ERROR);
            return default;
        }
    }
}

