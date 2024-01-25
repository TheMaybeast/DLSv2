using Rage;
using Rage.Native;

namespace DLSv2.Utils
{
    internal static class Modkits
    {
        public static bool HasModKits(this Vehicle vehicle) => NativeFunction.Natives.GET_NUM_MOD_KITS<int>(vehicle) > 0;

        public static bool HasModkitMod(this Vehicle vehicle, ModKitType modType) => GetModkitModCount(vehicle, modType) > 0;

        public static int GetModkitModCount(this Vehicle vehicle, ModKitType modType)
        {
            vehicle.Mods.InstallModKit();
            return NativeFunction.Natives.GET_NUM_VEHICLE_MODS<int>(vehicle, (int)modType);
        }

        public static int GetModkitModIndex(this Vehicle vehicle, ModKitType modType)
        {
            vehicle.Mods.InstallModKit();
            return NativeFunction.Natives.GET_VEHICLE_MOD<int>(vehicle, (int)modType);
        }

        public static void SetModkitModIndex(this Vehicle vehicle, ModKitType modType, int value)
        {
            vehicle.Mods.InstallModKit();
            NativeFunction.Natives.SET_VEHICLE_MOD(vehicle, (int)modType, value, false);
        }
    }

    public enum ModKitType
    {
        NONE = -1,
        VMT_SPOILER = 0,
        VMT_BUMPER_F = 1,
        VMT_BUMPER_R = 2,
        VMT_SKIRT = 3,
        VMT_EXHAUST = 4,
        VMT_CHASSIS = 5,
        VMT_GRILL = 6,
        VMT_BONNET = 7,
        VMT_WING_L = 8,
        VMT_WING_R = 9,
        VMT_ROOF = 10,
        VMT_ENGINE = 11,
        VMT_BRAKES = 12,
        VMT_GEARBOX = 13,
        VMT_HORN = 14,
        VMT_SUSPENSION = 15,
        VMT_ARMOUR = 16,
        VMT_NITROUS = 17,
        VMT_TURBO = 18,
        VMT_SUBWOOFER = 19,
        VMT_TYRE_SMOKE = 20,
        VMT_HYDRAULICS = 21,
        VMT_XENON_LIGHTS = 22,
        VMT_WHEELS = 23,
        VMT_WHEELS_REAR_OR_HYDRAULICS = 24,
        VMT_PLTHOLDER = 25,
        VMT_PLTVANITY = 26,
        VMT_INTERIOR1 = 27,
        VMT_INTERIOR2 = 28,
        VMT_INTERIOR3 = 29,
        VMT_INTERIOR4 = 30,
        VMT_INTERIOR5 = 31,
        VMT_SEATS = 32,
        VMT_STEERING = 33,
        VMT_KNOB = 34,
        VMT_PLAQUE = 35,
        VMT_ICE = 36,
        VMT_TRUNK = 37,
        VMT_HYDRO = 38,
        VMT_ENGINEBAY1 = 39,
        VMT_ENGINEBAY2 = 40,
        VMT_ENGINEBAY3 = 41,
        VMT_CHASSIS2 = 42,
        VMT_CHASSIS3 = 43,
        VMT_CHASSIS4 = 44,
        VMT_CHASSIS5 = 45,
        VMT_DOOR_L = 46,
        VMT_DOOR_R = 47,
        VMT_LIVERY_MOD = 48,
        VMT_LIGHTBAR = 49,
    }
}
