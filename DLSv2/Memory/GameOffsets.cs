using System;
using System.Runtime.InteropServices;
using Rage;
using DLSv2.Utils;

namespace DLSv2.Memory;

internal static class GameOffsets
{
    public static int CVehicle_AudVehicleAudioEntityOffset { get; private set; }
    public static int audVehicleAudioEntity_SirenSoundsOffset { get; private set; }
    public static int CVehicle_WheelsOffset { get; private set; }
    public static int CWheels_WheelCountOffset { get; private set; }
    public static int CWheel_BrakePressureOffset { get; private set; }
    public static int CVehicle_IndicatorsOffset { get; private set; }
    public static int CVehicle_SirensDataOffset { get; private set; }
    
    public static bool Init()
    {
        "Memory Offsets:".ToLog(LogLevel.DEBUG);

        var address = Game.FindPattern("48 8B 8B ?? ?? 00 00 E8 ?? ?? ?? ?? 48 8B 8B 70 13 00 00");
        if (AssertAddress(address, nameof(CVehicle_AudVehicleAudioEntityOffset)))
        {
            CVehicle_AudVehicleAudioEntityOffset = Marshal.ReadInt32(address + 3);
            $"  CVehicle_AudVehicleAudioEntity = {CVehicle_AudVehicleAudioEntityOffset}".ToLog(LogLevel.DEBUG);
        }
        address = Game.FindPattern("75 6D 48 8D B1 ?? ?? 00 00");
        if (AssertAddress(address, nameof(audVehicleAudioEntity_SirenSoundsOffset)))
        {
            audVehicleAudioEntity_SirenSoundsOffset = Marshal.ReadInt32(address + 5);
            $"  audVehicleAudioEntity_SirenSoundsOffset = {audVehicleAudioEntity_SirenSoundsOffset}".ToLog(LogLevel.DEBUG);
        }
        
        // https://github.com/ikt32/GTAVGlowingBrakes/blob/211c352c459ab89e940134f45b4448a8d783fb7a/GlowingBrakes/VehicleExtensions.cs
        address = Game.FindPattern("3B B7 ?? ?? ?? ?? 7D 0D");
        if (AssertAddress(address, nameof(CVehicle_WheelsOffset)))
        {
            CVehicle_WheelsOffset = Marshal.ReadInt32(address + 2) - 8;
            $"  CVehicle_WheelsOffset = {CVehicle_WheelsOffset}".ToLog(LogLevel.DEBUG);
            
            CWheels_WheelCountOffset = Marshal.ReadInt32(address + 2);
            $"  CWheels_WheelCountOffset = {CWheels_WheelCountOffset}".ToLog(LogLevel.DEBUG);
        }
        
        address = Game.FindPattern("74 ?? F3 0F 58 89 ?? ?? 00 00 F3 0F");
        if (AssertAddress(address, nameof(CWheel_BrakePressureOffset)))
        {
            CWheel_BrakePressureOffset = Marshal.ReadInt32(address + 6);
            $"  CWheel_BrakePressureOffset = {CWheel_BrakePressureOffset}".ToLog(LogLevel.DEBUG);
        }
        
        address = Game.FindPattern("FD 02 DB 08 98 ?? ?? ?? ?? 48 8B 5C 24 30");
        if (AssertAddress(address, nameof(CVehicle_IndicatorsOffset)))
        {
            CVehicle_IndicatorsOffset = Marshal.ReadInt32(address - 4);
            $"  CVehicle_IndicatorsOffset = {CVehicle_IndicatorsOffset}".ToLog(LogLevel.DEBUG);
        }

        address = Game.FindPattern("48 89 B7 ?? ?? ?? ?? 48 8B 0B");
        if (AssertAddress(address, nameof(CVehicle_SirensDataOffset)))
        {
            CVehicle_SirensDataOffset = Marshal.ReadInt16(address + 3);
            $"  CVehicle_SirensDataOffset = {CVehicle_SirensDataOffset}".ToLog(LogLevel.DEBUG);
        }
        
        return !anyAssertFailed;
    }

    private static bool anyAssertFailed;
    private static bool AssertAddress(IntPtr address, string name)
    {
        if (address != IntPtr.Zero) return true;

        $"Incompatible game version, couldn't find {name} instance.".ToLog(LogLevel.ERROR);
        anyAssertFailed = true;
        return false;
    }
}