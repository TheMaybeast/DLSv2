using System;
using System.Runtime.InteropServices;

namespace DLSv2.Memory;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal unsafe struct SoundSet
{
    [FieldOffset(0)] public byte Class;
    [FieldOffset(1)] public uint Flags;
    [FieldOffset(5)] public uint SoundCount;
    [FieldOffset(9)] public fixed ulong Sounds[1000];
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct audSoundSet
{
    public SoundSet* Data;
    public uint NameHash;
    
    public void Init(uint soundSetNameHash)
    {
        fixed (audSoundSet* self = &this)
        {
            GameFunctions.InitAudSoundSet((IntPtr)self, soundSetNameHash);
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
internal struct sirenInstanceData
{
    [FieldOffset(0x000)] public uint sirenOnTime;
    [FieldOffset(0x004)] public float sirenTimeDelta;
    [FieldOffset(0x008)] public int lastSirenBeat;
};