using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rage;

namespace DLSv2.Utils
{
    using Core;

    internal static class DLSExtensions
    {
        internal static ManagedVehicle GetActiveVehicle(this Vehicle veh)
        {
            if (!veh) return null;
            foreach (var mV in Entrypoint.ManagedVehicles)
            {
                if (mV.Vehicle == veh)
                    return mV;
            }

            var aVeh = new ManagedVehicle(veh);
            Entrypoint.ManagedVehicles.Add(aVeh);
            return aVeh;
        }

        internal static bool IsDLS(this Vehicle veh)
        {
            if (!veh || !Entrypoint.DLSModels.Contains(veh.Model)) return false;
            else return true;
        }
    }
}