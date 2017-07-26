using System.Threading.Tasks;
namespace B2T_Scheduler.Data
{
    public class SalesForcePreferenceTypesDAO
    {
        private ScheduleDataSet.PreferenceTypesDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForcePreferenceTypesDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.PreferenceTypes;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync()
        {
            Log.MarkQueryStart(Table.TableName);
            var soql = Soql.Pack($@"  
                SELECT  Key__c, Name, Description__c
                FROM    schPreferenceType__c");
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(Table.TableName, response.Records.Count);

            Table.Clear();
            foreach (var rec in response.Records)
            {
                var row = Table.NewPreferenceTypesRow();
                row.PreferenceTypeID = FromJValue.ToShort(rec.Key__c);
                row.Name = rec.Name;
                row.Description = rec.Description;
                row.IconKey = rec.Key__c;
                Table.AddPreferenceTypesRow(row);
            }
            Log.MarkLoadComplete(Table.TableName, Table.Count);

            return Table.Count;
        }
    }
}
