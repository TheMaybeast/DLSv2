using DLSv2.Core;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DLSv2.Utils
{
    internal static class Extensions
    {
        internal static ManagedVehicle GetActiveVehicle(this Vehicle veh)
        {
            if (!veh) return null;
            for (int i = 0; i < Entrypoint.ManagedVehicles.Count; i++)
            {
                if (Entrypoint.ManagedVehicles[i].Vehicle == veh)
                    return Entrypoint.ManagedVehicles[i];
            }
            ManagedVehicle aVeh;
            if (veh.IsSirenOn)
                aVeh = new ManagedVehicle(veh, true);
            else
                aVeh = new ManagedVehicle(veh);
            if(aVeh != null) Entrypoint.ManagedVehicles.Add(aVeh);
            return aVeh;
        }

        internal static DLSModel GetDLSModel(this Vehicle vehicle)
        {
            if (!vehicle) return null;
            DLSModel dlsModel;
            return Entrypoint.DLSModelsDict.TryGetValue(vehicle.Model, out dlsModel) ? dlsModel : null;
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

        internal static float ToFloat(this string text, [CallerMemberName] string callingMethod = null)
        {
            float i = 0f;
            try
            {
                i = float.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                string message = "ERROR: " + e.Message + " ( " + text + " ) [" + callingMethod + "() -> " + MethodBase.GetCurrentMethod().Name + "()]";
                (message).ToLog();
            }
            return i;
        }

        internal static Color HexToColor(this string text, [CallerMemberName] string callingMethod = null)
        {
            Color i = new Color();
            try
            {
                i = ColorTranslator.FromHtml("#" + text);
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

        internal static void ToLog(this string log)
        {
            string path = @"Plugins/DLS.log";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine("[" + DateTime.Now.ToString() + "] " + log);
                writer.Close();
            }
        }
        internal static T Next<T>(this List<T> list, T currentItem, int by = 1)
        {
            int index = list.IndexOf(currentItem);
            index = (index + by) % list.Count;
            return list[index];
        }
        internal static T Previous<T>(this List<T> list, T currentItem, int by = 1)
        {
            int index = list.IndexOf(currentItem);
            index = (list.Count + index - by) % list.Count;
            return list[index];
        }
    }
}