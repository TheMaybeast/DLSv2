using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DLSv2.Memory;
using Rage;

namespace DLSv2.Utils;

internal static class SirenSounds
{
    private static readonly Dictionary<Vehicle, IntPtr> DefaultSoundSets = new();
    private static readonly IntPtr EmptySoundSet;

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
            Marshal.ReadIntPtr(vehicleAddress + GameOffsets.CVehicle_AudVehicleAudioEntityOffset);
        var sirenSoundSet = audVehicleAudioEntityPtr + GameOffsets.audVehicleAudioEntity_SirenSoundsOffset;

        return (audSoundSet*)sirenSoundSet;
    }
    
    public static unsafe void DisableSirenSounds(this Vehicle vehicle)
    {
        if (DefaultSoundSets.ContainsKey(vehicle)) return;
            
        DefaultSoundSets[vehicle] = (IntPtr)vehicle.GetAudioSoundSetPtr()->Data;
#if DEBUG
        $"Setting {vehicle.MemoryAddress}'s SirenSounds to Empty".ToLog();            
#endif
        vehicle.GetAudioSoundSetPtr()->Data = (SoundSet*)EmptySoundSet;
    }
    
    public static unsafe void EnableSirenSounds(this Vehicle vehicle)
    {
        if (!DefaultSoundSets.ContainsKey(vehicle)) return;
#if DEBUG
        $"Setting {vehicle.MemoryAddress}'s SirenSounds to {DefaultSoundSets[vehicle]}".ToLog();            
#endif
        vehicle.GetAudioSoundSetPtr()->Data = (SoundSet*)DefaultSoundSets[vehicle];
        DefaultSoundSets.Remove(vehicle);
    }
}