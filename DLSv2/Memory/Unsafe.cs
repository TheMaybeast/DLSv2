using System;
using DLSv2.Utils;

namespace DLSv2.Memory
{
    internal class Unsafe
    {
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
}
