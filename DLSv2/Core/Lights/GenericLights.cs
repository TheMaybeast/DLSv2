using Rage;
using Rage.Native;

namespace DLSv2.Core.Lights
{
    internal class GenericLights
    {
        public static void SetIndicator(Vehicle vehicle, IndStatus indStatus)
        {
            if (!vehicle) return;

            switch (indStatus)
            {
                case IndStatus.Off:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 0, false);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 1, false);
                    break;
                case IndStatus.Left:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 0, false);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 1, true);
                    break;
                case IndStatus.Right:
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 0, true);
                    NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(vehicle, 1, false);
                    break;
                case IndStatus.Hazard:
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
