using Rage;
using Rage.Native;

namespace DLSv2.Utils;

public static class Paint
{
    public static (int primary, int secondary) GetColors(this Vehicle vehicle)
    {
        NativeFunction.Natives.GET_VEHICLE_COLOURS(vehicle, out int primary, out int secondary);
        return (primary, secondary);
    }

    public static void SetColors(this Vehicle vehicle, int primaryColorCode, int secondaryColorCode)
    {
        NativeFunction.Natives.SET_VEHICLE_COLOURS(vehicle, primaryColorCode, secondaryColorCode);
    }

    public static void SetPrimaryColor(this Vehicle vehicle, int primaryColorCode)
    {
        SetColors(vehicle, primaryColorCode, GetColors(vehicle).secondary);
    }

    public static void SetSecondaryColor(this Vehicle vehicle, int secondaryColorCode)
    {
        SetColors(vehicle, GetColors(vehicle).primary, secondaryColorCode);
    }

    public static (int pearl, int wheel) GetExtraColors(this Vehicle vehicle)
    {
        NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOURS(vehicle, out int pearl, out int wheel);
        return (pearl, wheel);
    }

    public static void SetExtraColors(this Vehicle vehicle, int pearlescentColorCode, int wheelColorCode)
    {
        NativeFunction.Natives.SET_VEHICLE_EXTRA_COLOURS(vehicle, pearlescentColorCode, wheelColorCode);
    }

    public static void SetPearlescentColor(this Vehicle vehicle, int pearlescentColorCode)
    {
        SetExtraColors(vehicle, pearlescentColorCode, GetExtraColors(vehicle).wheel);
    }

    public static void SetWheelColor(this Vehicle vehicle, int wheelColorCode)
    {
        SetExtraColors(vehicle, GetExtraColors(vehicle).pearl, wheelColorCode);
    }

    public static int GetColor5(this Vehicle vehicle)
    {
        NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOUR_5<bool>(vehicle, out int color);
        return color;
    }

    public static void SetColor5(this Vehicle vehicle, int colorCode)
    {
        NativeFunction.Natives.SET_VEHICLE_EXTRA_COLOUR_5(vehicle, colorCode);
    }

    public static int GetColor6(this Vehicle vehicle)
    {
        NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOUR_6<bool>(vehicle, out int color);
        return color;
    }

    public static void SetColor6(this Vehicle vehicle, int colorCode)
    {
        NativeFunction.Natives.SET_VEHICLE_EXTRA_COLOUR_6(vehicle, colorCode);
    }

    public static void SetPaint(this Vehicle vehicle, int paintSlot, int colorCode)
    {
        switch (paintSlot)
        {
            case 1:
                vehicle.SetPrimaryColor(colorCode);
                return;
            case 2:
                vehicle.SetSecondaryColor(colorCode);
                return;
            case 3:
                vehicle.SetPearlescentColor(colorCode);
                return;
            case 4:
                vehicle.SetWheelColor(colorCode);
                return;
            case 6:
                vehicle.SetColor5(colorCode);
                return;
            case 7:
                vehicle.SetColor6(colorCode);
                return;
            default:
                $"Paint slot {paintSlot} is invalid. Must be one of 1, 2, 3, 4, 6, 7.".ToLog(LogLevel.ERROR);
                return;
        }
    }

    public static int GetPaint(this Vehicle vehicle, int paintSlot)
    {
        switch (paintSlot)
        {
            case 1:
                return vehicle.GetColors().primary;
            case 2:
                return vehicle.GetColors().secondary;
            case 3:
                return vehicle.GetExtraColors().pearl;
            case 4:
                return vehicle.GetExtraColors().wheel;
            case 6:
                return vehicle.GetColor5();
            case 7:
                return vehicle.GetColor6();
            default:
                $"Paint slot {paintSlot} is invalid. Must be one of 1, 2, 3, 4, 6, 7.".ToLog(LogLevel.ERROR);
                return -1;
        }
    }
}