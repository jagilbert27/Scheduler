using System.Threading.Tasks;
namespace B2T_Scheduler.Data
{
    public class SalesForceHolidaysDAO
    {
        private ScheduleDataSet.HolidaysDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceHolidaysDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.Holidays;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync()
        {
            Log.MarkQueryStart(Table.TableName);
            var soql = Soql.Pack($@"
                SELECT  Id, ActivityDate, Name, Description, IsAllDay,StartTimeInMinutes,EndTimeInMinutes, LastModifiedDate 
                FROM    Holiday");
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(Table.TableName, response.Records.Count);

            lock (Table)
            {
                Table.Clear();
                foreach (var rec in response.Records)
                {
                    var row = Table.NewHolidaysRow();
                    row.ID = rec.Id;
                    row.Name = rec.Name;
                    row.Description = rec.Description;
                    if (FromJValue.ToBool(rec.IsAllDay))
                    {
                        row.StartDate = rec.ActivityDate;
                        row.EndDate = row.StartDate.AddDays(1);
                    }
                    else
                    {
                        row.StartDate = FromJValue.ToDate(rec.ActivityDate).AddMinutes(FromJValue.ToShort(rec.StartTimeInMinutes));
                        row.EndDate = FromJValue.ToDate(rec.ActivityDate).AddMinutes(FromJValue.ToShort(rec.EndTimeInMinutes));
                    }
                    row.LastModifiedDate = rec.LastModifiedDate;
                    Table.AddHolidaysRow(row);
                }
                Log.MarkLoadComplete(Table.TableName, Table.Count);
            }
            return Table.Count;
        }
    }
}
