using System;
using System.Runtime.InteropServices;
using DLSv2.Utils;
using Rage;

namespace DLSv2.Memory;

internal static class GameFunctions
{
    public delegate void InitAudSoundSetDelegate(IntPtr soundSet, uint soundSetNameHash);
    public static InitAudSoundSetDelegate InitAudSoundSet { get; private set; }

    public static bool Init()
    {
        "Memory Functions:".ToLog(LogLevel.DEBUG);

        var address = Game.FindPattern("40 53 48 83 EC 20 48 8B D9 89 51 08");
        if (AssertAddress(address, nameof(InitAudSoundSet)))
        {
            InitAudSoundSet = Marshal.GetDelegateForFunctionPointer<InitAudSoundSetDelegate>(address);
        }

        return !_anyAssertFailed;
    }

    private static bool _anyAssertFailed;
    private static bool AssertAddress(IntPtr address, string name)
    {
        if (address != IntPtr.Zero)
        {
            $"  {name} @ {(ulong)address:X}".ToLog(LogLevel.DEBUG);
            return true;
        }

        $"Incompatible game version, couldn't find {name} instance.".ToLog(LogLevel.ERROR);
        _anyAssertFailed = true;
        return false;
    }
}