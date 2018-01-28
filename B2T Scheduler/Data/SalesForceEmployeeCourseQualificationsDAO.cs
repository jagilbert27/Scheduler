using System;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceEmployeeCourseQualificationsDAO
    {
        private ScheduleDataSet.EmployeeCourseQualificationsDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceEmployeeCourseQualificationsDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.EmployeeCourseQualifications;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync(DateTime? since = null)
        {
            var logName = Table.TableName;
            Log.MarkQueryStart(logName);
            var soql = Soql.Pack($@"  
                SELECT  Instructor__c, SchCourseId__c, EffectiveDate__c, ExpirationDate__c, 
                        QualificationLevel__c, IsDeleted,LastModifiedById, LastModifiedDate
                FROM    schInstructorCourseQualification__c" + 
                new WhereBuilder().WhereDateTime("LastModifiedDate > ",since));
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(logName, response.Records.Count);

            lock (Table)
            {
                foreach (var rec in response.Records)
                {
                    var row = Table.NewEmployeeCourseQualificationsRow();
                    row.EmployeeID = rec.Instructor__c;
                    row.CourseID = rec.SchCourseId__c;
                    row.StartDate = rec.EffectiveDate__c;
                    row.EndDate = rec.ExpirationDate__c ?? DateTime.MaxValue;
                    row.QualificationLevel = 1;
                    row.Deleted = rec.IsDeleted;
                    row.LastModifiedBy = rec.LastModifiedById;
                    row.LastModifiedDate = rec.LastModifiedDate ?? DateTime.MinValue;
                    Table.AddEmployeeCourseQualificationsRow(row);
                }
            }
            Log.MarkLoadComplete(logName, Table.Count);
            return Table.Count;
        }
    }
}
