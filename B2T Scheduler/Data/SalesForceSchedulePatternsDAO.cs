using System.Threading.Tasks;
namespace B2T_Scheduler.Data
{
    public class SalesForceSchedulePatternsDAO
    {
        private ScheduleDataSet.SchedulePatternsDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceSchedulePatternsDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.SchedulePatterns;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync()
        {
            Log.MarkQueryStart(Table.TableName);
            var soql = Soql.Pack($@"
                SELECT  Id, Name, Description__c, IconKey__c, WorkWeeksInPeriod__c, WeeksInPeriod__c
                FROM    schInstructorSchedulePattern__c");
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(Table.TableName, response.Records.Count);

            Table.Clear();
            foreach (var rec in response.Records)
            {
                var row = Table.NewSchedulePatternsRow();
                row.PatternID = rec.Id;
                row.Name = rec.Name;
                row.Description = rec.Description__c;
                row.IconKey = rec.IconKey__c;
                row.WorkWeeksInPeriod = rec.WorkWeeksInPeriod__c;
                row.WeeksInPeriod = rec.WeeksInPeriod__c;
                Table.AddSchedulePatternsRow(row);
            }
            Log.MarkLoadComplete(Table.TableName, Table.Count);

            return Table.Count;
        }
    }
}
