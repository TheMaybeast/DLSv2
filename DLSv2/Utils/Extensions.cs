using DLSv2.Core;
using Rage;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DLSv2.Utils
{
    internal static class Extensions
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

        internal static int ToInt32(this string text, [CallerMemberName] string callingMethod = null)
        {
            int i = 0;
            try
            {
                i = Convert.ToInt32(text);
            }
            catch (Exception e)
            {
                string message = "ERROR: " + e.Message + " ( " + text + " ) [" + callingMethod + "() -> " + MethodBase.GetCurrentMethod().Name + "()]";
                (message).ToLog();
            }
            return i;
        }

        internal static bool ToBoolean(this string text, [CallerMemberName] string callingMethod = null)
        {
            bool i = false;
            try
            {
                i = Convert.ToBoolean(text);
            }
            catch (Exception e)
            {
                string message = "ERROR: " + e.Message + " ( " + text + " ) [" + callingMethod + "() -> " + MethodBase.GetCurrentMethod().Name + "()]";
                (message).ToLog();
            }
            return i;
        }

        public static void ClearSiren(this Vehicle vehicle)
        {
            bool delv = false;
            Vehicle v = Game.LocalPlayer.Character.GetNearbyVehicles(16).FirstOrDefault(veh => veh && !veh.HasSiren);
            if (!v)
                v = World.EnumerateVehicles().FirstOrDefault(veh => veh && !veh.HasSiren);
            if (!v)
            {
                delv = true;
                v = new Vehicle("asea", Vector3.Zero);
            }
            if (!v)
            {
                ("ClearSiren: Unable to find/generate vehicle without a siren").ToLog();
                return;
            }            
            if (!vehicle)
            {
                ("ClearSiren: Target vehicle does not exist").ToLog();
                return;
            }

            vehicle.ApplySirenSettingsFrom(v);

            if (delv && v) v.Delete();
        }
    }
}