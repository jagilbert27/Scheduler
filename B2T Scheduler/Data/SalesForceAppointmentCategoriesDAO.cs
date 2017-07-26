using System;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceAppointmentCategoriesDAO
    {
        private ScheduleDataSet.AppointmentCategoriesDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceAppointmentCategoriesDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.AppointmentCategories;
            Log = log;
            Force = force;
        }
        
        public async Task<int> GetAsync(DateTime? since = null)
        {
            var logName = Table.TableName + " (Event Types)";

            Log.MarkQueryStart(logName);
            var soql = Soql.Pack($@"
                SELECT  Id, Name, Key__c, Description__c,DisplayOrder__c, IsExclusive__c,
                        EventCategory__r.Key__c,IsWorking__c, IsDeleted, LastModifiedDate
                FROM    schEventType__c");
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(logName, response.Records.Count);

            foreach (var rec in response.Records)
            {
                var row = Table.NewAppointmentCategoriesRow();
                row.Type = rec.EventCategory__r.Key__c;
                row.AppointmentCategoryID = rec.Key__c;
                row.DurationDays = 1;
                row.MaxStudents = 0;
                row.CategoryName = rec.Name;
                row.Description = rec.Description__c;
                row.IsExclusive = rec.IsExclusive__c;
                row.IsWorking = rec.IsWorking__c;
                row.CategoryAbbreviation = rec.Key__c;
                row.SortOrder = rec.DisplayOrder__c;
                row.Deleted = rec.IsDeleted;
                row.LastModifiedDate = rec.LastModifiedDate;
                Table.AddAppointmentCategoriesRow(row);
            }
            Log.MarkLoadComplete(logName, Table.Count);
            return Table.Count;
        }

        //async Task<int> GetAppointmentCategoriesAsync(DateTime? since = null)
        //{
        //    Console.WriteLine("GetAppointmentCategoriesAsync() Starting");
        //    DataSet.SchedulePatterns.Clear();

        //    var soql = Soql.Pack($@"
        //        SELECT  Id, Name, Abbreviation__c, Description__c, LongDescription__c, Type__c, 
        //                DurationDays__c, MaxStudents__c, Image__c, SortOrder__c, IsExclusive__c,
        //                IsWorking__c, IsDeleted__c, LastModifiedDate
        //        FROM    schEventType__c");

        //    var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);

        //    foreach (var rec in response.Records)
        //    {
        //        ScheduleDataSet.AppointmentCategoriesRow row = DataSet.AppointmentCategories.NewAppointmentCategoriesRow();
        //        row.AppointmentCategoryID = rec.Id;
        //        row.CategoryName = rec.Name;
        //        row.Description = rec.Description__c;
        //        row.Type = rec.Type__c;
        //        row.DurationDays = rec.DurationDays__c;
        //        row.MaxStudents = rec.MaxStudents__c;
        //        row.LongDescription = rec.LongDescription__c;
        //        row.IsExclusive = rec.IsExclusive__c;
        //        row.IsWorking = rec.IsWorking__c;
        //        row.CategoryAbbreviation = rec.Abbreviation__c;
        //        row.Image = rec.Image__c;
        //        row.SortOrder = rec.SortOrder__c;
        //        row.Deleted = rec.IsDeleted;
        //        row.LastModifiedDate = rec.LastModifiedDate;
        //    }
        //    Console.WriteLine($"GetAppointmentCategoriesAsync() Got {DataSet.AppointmentCategories.Count} records");
        //    return DataSet.AppointmentCategories.Count;
        //}

        //async Task<int> GetAppointmentCategoriesAsync() //OK I'm getting nothing from here cuz schAppointmentCategory__c is mt
        //{
        //    Log.MarkQueryStart(Table.TableName);
        //    var soql = Soql.Pack($@"
        //        SELECT  Id, Name, Abbreviation__c, Description__c, LongDescription__c, Type__c, 
        //                DurationDays__c, MaxStudents__c, Image__c, SortOrder__c, IsExclusive__c,
        //                IsWorking__c, IsDeleted__c, LastModifiedDate
        //        FROM    schAppointmentCategory__c");
        //    var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
        //    Log.MarkQueryComplete(Table.TableName, response.Records.Count);

        //    foreach (var rec in response.Records)
        //    {
        //        var row = Table.NewAppointmentCategoriesRow();
        //        row.AppointmentCategoryID = rec.Id;
        //        row.CategoryName = rec.Name;
        //        row.Description = rec.Description__c;
        //        row.Type = rec.Type__c;
        //        row.DurationDays = rec.DurationDays__c;
        //        row.MaxStudents = rec.MaxStudents__c;
        //        row.LongDescription = rec.LongDescription__c;
        //        row.IsExclusive = rec.IsExclusive__c;
        //        row.IsWorking = rec.IsWorking__c;
        //        row.CategoryAbbreviation = rec.Abbreviation__c;
        //        row.Image = rec.Image__c;
        //        row.SortOrder = rec.SortOrder__c;
        //        row.Deleted = rec.IsDeleted__c;
        //        row.LastModifiedDate = rec.LastModifiedDate;
        //        Table.AddAppointmentCategoriesRow(row);
        //    }
        //    Log.MarkLoadComplete(Table.TableName, Table.Count);
        //    return Table.Count;
        //}

    }
}
