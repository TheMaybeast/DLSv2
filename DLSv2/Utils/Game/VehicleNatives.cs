using Rage;
using Rage.Native;

namespace DLSv2.Utils;

public static class VehicleNatives
{
    public static float GetForwardSpeed(this Vehicle vehicle) => NativeFunction.Natives.GET_ENTITY_SPEED_VECTOR<Vector3>(vehicle, true).Y;

    public static int GetLivery(this Vehicle vehicle) => NativeFunction.Natives.GET_VEHICLE_LIVERY<int>(vehicle);

    public static float GetPetrolTankHealth(this Vehicle vehicle) => NativeFunction.Natives.GET_VEHICLE_PETROL_TANK_HEALTH<float>(vehicle);

    public static bool IsHeadlightDamaged(this Vehicle vehicle, bool left)
    {
        string nativeName = left ? "GET_IS_LEFT_VEHICLE_HEADLIGHT_DAMAGED" : "GET_IS_RIGHT_VEHICLE_HEADLIGHT_DAMAGED";
        return NativeFunction.CallByName<bool>(nativeName, vehicle);
    } 

    public static bool AreBothHeadlightsDamaged(this Vehicle vehicle) => NativeFunction.Natives.GET_BOTH_VEHICLE_HEADLIGHTS_DAMAGED<bool>(vehicle);

    public static bool IsBumperBroken(this Vehicle vehicle, bool front, bool detached)
    {
        string nativeName = detached ? "IS_VEHICLE_BUMPER_BROKEN_OFF" : "IS_VEHICLE_BUMPER_BOUNCING";
        return NativeFunction.CallByName<bool>(nativeName, vehicle, front);
    }
        
    public static bool HasExtra(this Vehicle vehicle, int extra) =>
        NativeFunction.Natives.DoesExtraExist<bool>(vehicle, extra);

    public static bool IsExtraEnabled(this Vehicle vehicle, int extra) =>
        NativeFunction.Natives.IsVehicleExtraTurnedOn<bool>(vehicle, extra);

    public static void SetExtra(this Vehicle vehicle, int extra, bool enabled) =>
        NativeFunction.Natives.SetVehicleExtra(vehicle, extra, !enabled);
}