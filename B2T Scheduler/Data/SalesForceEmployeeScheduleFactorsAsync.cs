using System;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceEmployeeScheduleFactorsDAO
    {
        private ScheduleDataSet.EmployeeScheduleFactorsDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceEmployeeScheduleFactorsDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.EmployeeScheduleFactors;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync(DateTime? since = null)
        {
            var logName = Table.TableName ;
            Log.MarkQueryStart(logName);
            var soql = Soql.Pack($@"  
                SELECT  Id, Name, Instructor__c, InstructorSchedulePatternId__c, 
                        EffectiveDate__c, ExpirationDate__c, LastModifiedDate, IsDeleted__c 
                FROM    schInstructorSchedulePreferences__c" +
                new WhereBuilder()
                    .Where("IsDeleted=false")
                    .WhereDateTime(" LastModifiedDate > ", since));
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(logName, response.Records.Count);

            foreach (var rec in response.Records)
            {
                var row = Table.NewEmployeeScheduleFactorsRow();
                row.EmployeeID = rec.Instructor__c;
                row.PatternID = rec.InstructorSchedulePatternId__c;
                row.EffectiveDate = rec.EffectiveDate__c;
                row.LastModifiedBy = rec.LastModifiedBy;
                row.LastModifiedDate = (rec.LastModifiedDate) ?? DateTime.MinValue;
                Table.AddEmployeeScheduleFactorsRow(row);
            }
            Log.MarkLoadComplete(logName, Table.Count);
            return Table.Count;
        }
    }
}
