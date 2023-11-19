using Rage;

namespace DLSv2.Utils
{
    using Core;

    internal static class DLSExtensions
    {
        internal static ManagedVehicle GetManagedVehicle(this Vehicle veh)
        {
            if (!veh) return null;
            foreach (var mV in Entrypoint.ManagedVehicles)
            {
                if (mV.Vehicle == veh)
                    return mV;
            }

            var managedVehicle = new ManagedVehicle(veh);
            Entrypoint.ManagedVehicles.Add(managedVehicle);
            return managedVehicle;
        }

        internal static bool IsDLS(this Vehicle veh) => veh && Entrypoint.DLSModels.Contains(veh.Model);
    }
}