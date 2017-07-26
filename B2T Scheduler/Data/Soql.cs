using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace B2T_Scheduler.Data
{
    public static class Soql
    {
        public static string Pack(string s) => Regex.Replace(s, @"\s+", " ").Trim(" \n".ToArray());

        public static string Date(DateTime d) => d.ToString("yyyy-MM-dd");

        public static string DateTime(DateTime d) => d.ToString("yyyy-MM-ddTHH:mm:sszzz");


    }
}
