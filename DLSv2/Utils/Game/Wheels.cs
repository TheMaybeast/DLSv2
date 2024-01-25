using System.Runtime.InteropServices;
using Rage;

namespace DLSv2.Utils;

public static class Wheels
{
    public static int CVehicle_WheelsOffset;
    public static int CWheels_WheelCountOffset;
    public static int CWheel_BrakePressureOffset;
    
    public static void MemoryInit()
    {
        // https://github.com/ikt32/GTAVGlowingBrakes/blob/211c352c459ab89e940134f45b4448a8d783fb7a/GlowingBrakes/VehicleExtensions.cs
        var address = Game.FindPattern("3B B7 ?? ?? ?? ?? 7D 0D");
        if (Memory.AssertAddress(address, nameof(CVehicle_WheelsOffset)))
        {
            CVehicle_WheelsOffset = Marshal.ReadInt32(address + 2) - 8;
            $"  CVehicle_WheelsOffset = {CVehicle_WheelsOffset}".ToLog(LogLevel.DEBUG);
            
            CWheels_WheelCountOffset = Marshal.ReadInt32(address + 2);
            $"  CWheels_WheelCountOffset = {CWheels_WheelCountOffset}".ToLog(LogLevel.DEBUG);
        }
        
        address = Game.FindPattern("74 ?? F3 0F 58 89 ?? ?? 00 00 F3 0F");
        if (Memory.AssertAddress(address, nameof(CWheel_BrakePressureOffset)))
        {
            CWheel_BrakePressureOffset = Marshal.ReadInt32(address + 6);
            $"  CWheel_BrakePressureOffset = {CWheel_BrakePressureOffset}".ToLog(LogLevel.DEBUG);
        }
    }
    
    public static float GetBrakePressure(this Vehicle vehicle, int wheelIndex)
    {
        // Gets vehicle memory address
        var address = vehicle.MemoryAddress;

        // Gets the wheels pointer for current vehicle
        if (CVehicle_WheelsOffset == 0) return -1f;
        var wheelsPtr = Marshal.ReadIntPtr(address + CVehicle_WheelsOffset);

        // Gets the number of wheels
        var wheelsCount = vehicle.GetNumWheels();
        if (wheelsCount == -1) return -1f;

        // Gets pointer to the specific wheel
        var wheelPtr = Marshal.ReadIntPtr(wheelsPtr + (8 * wheelIndex));

        // Gets the brake pressure offset
        if (CWheel_BrakePressureOffset == 0) return -1f;
        var brakePressure = (float)Marshal.ReadInt32(wheelPtr + CWheel_BrakePressureOffset);

        return brakePressure;
    }

    public static int GetNumWheels(this Vehicle vehicle)
    {
        // Gets vehicle memory address
        var address = vehicle.MemoryAddress;

        // Gets vehicle wheels count
        if (CWheels_WheelCountOffset == 0) return -1;
        var wheelsCount = Marshal.ReadInt32(address + CWheels_WheelCountOffset);

        return wheelsCount;
    }
}