using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceFormatCategoriesDAO
    {
        private ScheduleDataSet.FormatCategoriesDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceFormatCategoriesDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.FormatCategories;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync()
        {
            Log.MarkQueryStart(Table.TableName);
            var soql = Soql.Pack($@"SELECT Name, Key__c FROM schFormatCategory__c");
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(Table.TableName, response.Records.Count);
            Table.Clear();
            foreach (var rec in response.Records)
            {
                var row = Table.NewFormatCategoriesRow();
                row.Key = rec.Key__c;
                row.Name = rec.Name;
                Table.AddFormatCategoriesRow(row);
            }
            Log.MarkLoadComplete(Table.TableName, Table.Count);
            return Table.Count;
        }
    }
}
