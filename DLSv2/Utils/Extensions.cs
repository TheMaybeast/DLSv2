using DLSv2.Core;
using DLSv2.Core.Triggers;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            for (int i = 0; i < Entrypoint.ManagedVehicles.Count; i++)
            {
                if (Entrypoint.ManagedVehicles[i].Vehicle == veh)
                    return Entrypoint.ManagedVehicles[i];
            }
            ManagedVehicle aVeh;
            aVeh = new ManagedVehicle(veh);
            if (aVeh != null) Entrypoint.ManagedVehicles.Add(aVeh);
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

        internal static void ToLog(this string log, bool toConsole = false)
        {
            if (toConsole) Game.Console.Print(log);
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

        /*
        internal static BaseCondition GetCondition(this TriggerRaw rawTrigger)
        {
            if (BaseCondition.TriggerTypes.TryGetValue(rawTrigger.Name, out Type triggerType))
            {
                return (BaseCondition)Activator.CreateInstance(triggerType);
            } 
            return null;
        }
        */

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
            vehicle.IsSirenSilent = true;

            if (delv && v) v.Delete();
        }
    }
}