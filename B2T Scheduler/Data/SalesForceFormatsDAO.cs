using System.Threading.Tasks;
namespace B2T_Scheduler.Data
{
    public class SalesForceFormatsDAO
    {
        private ScheduleDataSet.FormatsDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceFormatsDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.Formats;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync()
        {
            Log.MarkQueryStart(Table.TableName);
            var soql = Soql.Pack($@"
                SELECT  Id, Name, Key__c, Enum__c, DisplayOrder__c, Category__c,
                        Category__r.Key__c, 
                        Icon__c, ForegroundColor__c, BackgroundColor__c, BackgroundImage__c, 
                        LastModifiedDate
                FROM    schFormat__c");
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(Table.TableName, response.Records.Count);

            Table.Clear();
            foreach (var rec in response.Records)
            {
                var row = Table.NewFormatsRow();
                row.FormatID = rec.Enum__c;
                row.FormatName = rec.Name;
                row.ParentID = rec.Key__c;
                row.ParentType = rec.Category__r.Key__c;
                row.ForecolorName = rec.ForegroundColor__c;
                row.BackcolorName = rec.BackgroundColor__c;
                row.BackgroundImageKey = rec.BackgroundImage__c;
                row.IconImageKey = rec.Icon__c;
                row.SortOrder = (short)((rec.DisplayOrder__c == null) ? -1 : rec.DisplayOrder__c);
                row.LastModifiedDate = rec.LastModifiedDate;
                Table.AddFormatsRow(row);
            }
            Log.MarkLoadComplete(Table.TableName, Table.Count);

            return Table.Count;
        }
    }
}
