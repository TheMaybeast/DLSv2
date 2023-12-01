using Rage;

namespace DLSv2.Utils
{
    using Core;

    internal static class DLSExtensions
    {
        internal static ManagedVehicle GetManagedVehicle(this Vehicle veh)
        {
            if (!veh) return null;
            if (Entrypoint.ManagedVehicles.TryGetValue(veh, out var managedVehicle))
                return managedVehicle;

            managedVehicle = new ManagedVehicle(veh);
            Entrypoint.ManagedVehicles.Add(veh, managedVehicle);
            return managedVehicle;
        }

        internal static bool IsDLS(this Vehicle veh) => veh && Entrypoint.DLSModels.Contains(veh.Model);
    }
}