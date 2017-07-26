using System;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceCoursesDAO
    {
        private ScheduleDataSet.AppointmentCategoriesDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceCoursesDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.AppointmentCategories;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync(DateTime? startDate = null, DateTime? endDate = null, DateTime? modifiedSince = null)
        {
            var logName = "Courses (Distinct Classes)";
            Log.MarkQueryStart(logName);
            var soql = Soql.Pack(@"SELECT Count(Id), Name FROM pss_Course__c group by Name");
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(logName, response.Records.Count);

            lock (Table)
            {
                foreach (var rec in response.Records)
                {
                    var row = Table.NewAppointmentCategoriesRow();
                    row.AppointmentCategoryID = FromJValue.ToAppointmentCategoryID(rec.Name);
                    row.CategoryName = FromJValue.ToAppointmentCategoryName(rec.Name);
                    //row.CategoryAbbreviation = rec.Abbreviation__c;
                    row.IsExclusive = true;
                    row.IsWorking = true;
                    row.Type = "Classes";
                    if (!Table.Rows.Contains(row.AppointmentCategoryID))
                        Table.AddAppointmentCategoriesRow(row);
                }
            }
            Log.MarkLoadComplete(logName, Table.Rows.Count);
            return Table.Rows.Count;
        }
    }
}
