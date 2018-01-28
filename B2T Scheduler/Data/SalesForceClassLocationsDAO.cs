using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceClassLocationsDAO
    {
        private ScheduleDataSet.ClassLocationsDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceClassLocationsDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.ClassLocations;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync()
        {
            var since = Log.GetLastQueryDate(Table.TableName);
            var initialCount = Table.Count;
            var count = 0;

            Log.MarkQueryStart(Table.TableName);
            var soql = Soql.Pack($@"  
                SELECT  Id, Account__c, Name, Metro_Area__c, City__c, State__c, LastModifiedDate 
                FROM    Class_Location__c" +
                new WhereBuilder()
                    .Where("IsDeleted=false")
                    .WhereDateTime("LastModifiedDate >", since));
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(Table.TableName, response.Records.Count);

            AddClassLocationRecords(response.Records);

            for (var i = 0; !string.IsNullOrEmpty(response.NextRecordsUrl); i++)
            {
                Log.MarkQueryStart($"{Table.TableName}({i})");
                response = await Force.Connect().QueryContinuationAsync<dynamic>(response.NextRecordsUrl).ConfigureAwait(false); //new
                Log.MarkQueryComplete($"{Table.TableName}({i})", response.Records.Count);
                AddClassLocationRecords(response.Records);
                count = Table.Count - initialCount;
                Log.MarkLoadComplete($"{Table.TableName}({i})", count);
            }
            return count;
        }

        private void AddClassLocationRecords(List<dynamic> records)
        {
            lock (Table)
            {
                foreach (var rec in records)
                {
                    var row = Table.NewClassLocationsRow();
                    row.ClassLocationID = rec.Id;
                    row.AccountID = rec.Account__c;
                    row.Name = rec.Name;
                    row.MetroArea = rec.Metro_Area__c;
                    row.City = rec.City__c;
                    row.State = rec.State__c;
                    row.LastModifiedDate = (rec.LastModifiedDate) ?? DateTime.MinValue;
                    Table.AddClassLocationsRow(row);
                }
            }
        }
    }
}
