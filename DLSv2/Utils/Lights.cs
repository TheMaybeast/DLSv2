using Rage.Native;

namespace DLSv2.Utils
{
    class Lights
    {
        public static void UpdateIndicator(ManagedVehicle activeVeh)
        {
            switch (activeVeh.IndStatus)
            {
                case IndStatus.Off:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 0, false);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 1, false);
                    break;
                case IndStatus.Left:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 0, false);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 1, true);
                    break;
                case IndStatus.Right:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 0, true);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 1, false);
                    break;
                case IndStatus.Both:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 0, true);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(activeVeh.Vehicle, 1, true);
                    break;
            }
        }
    }
}
