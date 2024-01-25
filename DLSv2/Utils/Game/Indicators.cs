using System.Runtime.InteropServices;
using Rage;

namespace DLSv2.Utils;

public static class Indicators
{
    public static int CVehicle_IndicatorsOffset;
    
    public static void MemoryInit()
    {
        var address = Game.FindPattern("FD 02 DB 08 98 ?? ?? ?? ?? 48 8B 5C 24 30");
        if (Memory.AssertAddress(address, nameof(CVehicle_IndicatorsOffset)))
        {
            CVehicle_IndicatorsOffset = Marshal.ReadInt32(address - 4);
            $"  CVehicle_IndicatorsOffset = {CVehicle_IndicatorsOffset}".ToLog(LogLevel.DEBUG);
        }
    }
    
    public static VehicleIndicatorLightsStatus GetIndicatorStatus(this Vehicle vehicle)
    {
        // Gets vehicle memory address
        var address = vehicle.MemoryAddress;

        if (CVehicle_IndicatorsOffset == 0) return 0;
        var status = Marshal.ReadInt32(address + CVehicle_IndicatorsOffset) & 3;

        // swap first and second bit because RPH defines the enum backwards (1 = left, 2 = right)
        status = ((status & 1) << 1) | ((status & 2) >> 1);

        return (VehicleIndicatorLightsStatus)status;
    }
}