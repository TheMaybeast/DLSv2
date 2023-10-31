using Rage;
using Rage.Native;

namespace DLSv2.Core.Lights
{
    internal class GenericLights
    {
        public static void SetIndicator(Vehicle vehicle, VehicleIndicatorLightsStatus indStatus)
        {
            if (!vehicle) return;

            switch (indStatus)
            {
                case VehicleIndicatorLightsStatus.Off:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 0, false);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 1, false);
                    break;
                case VehicleIndicatorLightsStatus.LeftOnly:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 0, false);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 1, true);
                    break;
                case VehicleIndicatorLightsStatus.RightOnly:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 0, true);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 1, false);
                    break;
                case VehicleIndicatorLightsStatus.Both:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 0, true);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 1, true);
                    break;
            }
        }

        public static void SetInteriorLight(Vehicle vehicle, bool isOn)
        {
            if (!vehicle) return;
            vehicle.IsInteriorLightOn = isOn;
        }
    }
}
