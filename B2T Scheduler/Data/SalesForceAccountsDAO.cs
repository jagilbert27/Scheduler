using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceAccountsDAO
    {
        private ScheduleDataSet.AccountsDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceAccountsDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.Accounts;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync(DateTime? modifiedSince = null)
        {
            var initialCount = Table.Count;
            var count = 0;

            Log.MarkQueryStart(Table.TableName);
            var soql = Soql.Pack($@"
                SELECT  Id, Name, LastModifiedDate
                FROM    Account" +
                new WhereBuilder()
                    .WhereDate("WHERE LastModifiedDate >", modifiedSince));

            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(Table.TableName, response.Records.Count);

            AddAccountRecords(response.Records);

            for (var i = 0; !string.IsNullOrEmpty(response.NextRecordsUrl); i++)
            {
                Log.MarkQueryStart($"{Table.TableName}({i})");
                response = await Force.Connect().QueryContinuationAsync<dynamic>(response.NextRecordsUrl).ConfigureAwait(false); //new
                Log.MarkQueryComplete($"{Table.TableName}({i})", response.Records.Count);
                AddAccountRecords(response.Records);
                count = Table.Count - initialCount;
                Log.MarkLoadComplete($"{Table.TableName}({i})", count);
            }
            Log.MarkLoadComplete(Table.TableName, Table.Rows.Count);

            return count;
        }

        public void AddAccountRecords(List<dynamic> records)
        {
            lock (Table)
            {
                foreach (var rec in records)
                {
                    var row = Table.NewAccountsRow();
                    row.AccountID = rec.Id;
                    row.Name = rec.Name;
                    row.LastModifiedDate = (rec.LastModifiedDate) ?? DateTime.MinValue;
                    Table.AddAccountsRow(row);
                }
            }
        }
    }
}
