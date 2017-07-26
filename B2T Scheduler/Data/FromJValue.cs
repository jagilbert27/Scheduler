using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    class FromJValue
    {
        public static string CleanAddress(object value, string defaultValue = "")
        {
            string v = value.ToString();
            if (v == "<br>,  <br>")
                return string.Empty;
            return v;
        }

        public static string ToAppointmentCategoryID(object value, string defaultValue = "0")
        {
            try
            {
                var str = value.ToString().Replace(" ", "");
                if (String.IsNullOrEmpty(str)) return defaultValue;
                return str.Substring(0, Math.Min(40, str.Length)).ToUpper();
            }
            catch { return defaultValue; }
        }

        public static string ToAppointmentCategoryName(object value, string defaultValue = "0")
        {
            try
            {
                var str = value.ToString();
                if (String.IsNullOrEmpty(str)) return defaultValue;
                if (str.Length <= 50) return str;
                return str.Substring(0, 46) + "...";
            }
            catch { return defaultValue; }
        }

        public static string ToString(object value, string defaultValue = "")
        {
            try { return value.ToString(); }
            catch { return defaultValue; }
        }

        public static DateTime ToDate(object value)
        {
            DateTime d;
            if (value != null)
                if (DateTime.TryParse(value.ToString(), out d)) return d;
            return DateTime.MinValue;
        }

        public static DateTime ToTime(object value)
        {
            DateTime d;
            if (value != null)
                if (DateTime.TryParse(value.ToString(), out d)) return d;
            return DateTime.MinValue;
        }

        public static short ToShort(object value, short defaultValue = 0)
        {
            try { return (short)value; }
            catch
            {
                short v;
                if (short.TryParse(value.ToString(), out v)) return v;
                return defaultValue;
            }
        }

        public static decimal ToDecimal(object value, decimal defaultValue = 0)
        {
            try { return (decimal)value; }
            catch { return defaultValue; }
        }

        public static bool ToBool(dynamic value, bool defaultValue = false)
        {
            try { return (bool)value; }
            catch { return defaultValue; }
        }
    }
}
