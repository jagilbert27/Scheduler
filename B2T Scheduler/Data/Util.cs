using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public static class Util
    {
        public static bool ColumnHasChanged(DataRow row, string columnName) =>
            row[columnName, DataRowVersion.Original] != row[columnName, DataRowVersion.Current];
    }
}
