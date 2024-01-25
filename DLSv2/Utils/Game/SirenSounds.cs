using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rage;

namespace DLSv2.Utils;

internal static class SirenSounds
{
    public delegate void InitAudSoundSetDelegate(IntPtr soundSet, uint soundSetNameHash);
    public static InitAudSoundSetDelegate InitAudSoundSet { get; private set; }

    public static int CVehicle_AudVehicleAudioEntityOffset;
    public static int audVehicleAudioEntity_SirenSoundsOffset;
    
    
    private static readonly Dictionary<Vehicle, IntPtr> DefaultSoundSets = new();
    private static readonly IntPtr EmptySoundSet;

    public static void MemoryInit()
    {
        var address = Game.FindPattern("40 53 48 83 EC 20 48 8B D9 89 51 08");
        if (Memory.AssertAddress(address, nameof(InitAudSoundSet)))
        {
            InitAudSoundSet = Marshal.GetDelegateForFunctionPointer<InitAudSoundSetDelegate>(address);
        }
        
        address = Game.FindPattern("48 8B 8B ?? ?? 00 00 E8 ?? ?? ?? ?? 48 8B 8B 70 13 00 00");
        if (Memory.AssertAddress(address, nameof(CVehicle_AudVehicleAudioEntityOffset)))
        {
            CVehicle_AudVehicleAudioEntityOffset = Marshal.ReadInt32(address + 3);
            $"  CVehicle_AudVehicleAudioEntity = {CVehicle_AudVehicleAudioEntityOffset}".ToLog(LogLevel.DEBUG);
        }
        
        address = Game.FindPattern("75 6D 48 8D B1 ?? ?? 00 00");
        if (Memory.AssertAddress(address, nameof(audVehicleAudioEntity_SirenSoundsOffset)))
        {
            audVehicleAudioEntity_SirenSoundsOffset = Marshal.ReadInt32(address + 5);
            $"  audVehicleAudioEntity_SirenSoundsOffset = {audVehicleAudioEntity_SirenSoundsOffset}".ToLog(LogLevel.DEBUG);
        }
    }

    static SirenSounds()
    {
        var soundSet = new SoundSet();
        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(soundSet));
        Marshal.StructureToPtr(soundSet, ptr, false);

        EmptySoundSet = ptr;
    }
    
    private static unsafe audSoundSet* GetAudioSoundSetPtr(this Vehicle vehicle)
    {
        var vehicleAddress = vehicle.MemoryAddress;
        var audVehicleAudioEntityPtr =
            Marshal.ReadIntPtr(vehicleAddress + CVehicle_AudVehicleAudioEntityOffset);
        var sirenSoundSet = audVehicleAudioEntityPtr + audVehicleAudioEntity_SirenSoundsOffset;

        return (audSoundSet*)sirenSoundSet;
    }
    
    public static unsafe void DisableSirenSounds(this Vehicle vehicle)
    {
        if (DefaultSoundSets.ContainsKey(vehicle)) return;
            
        DefaultSoundSets[vehicle] = (IntPtr)vehicle.GetAudioSoundSetPtr()->Data;
        $"Setting {vehicle.MemoryAddress}'s SirenSounds to Empty".ToLog(LogLevel.DEBUG);            
        vehicle.GetAudioSoundSetPtr()->Data = (SoundSet*)EmptySoundSet;
    }
    
    public static unsafe void EnableSirenSounds(this Vehicle vehicle)
    {
        if (!DefaultSoundSets.ContainsKey(vehicle)) return;
        $"Setting {vehicle.MemoryAddress}'s SirenSounds to {DefaultSoundSets[vehicle]}".ToLog(LogLevel.DEBUG);            
        vehicle.GetAudioSoundSetPtr()->Data = (SoundSet*)DefaultSoundSets[vehicle];
        DefaultSoundSets.Remove(vehicle);
    }
    
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
                InitAudSoundSet((IntPtr)self, soundSetNameHash);
            }
        }
    }
}