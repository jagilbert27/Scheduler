using System;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class SalesForceEmployeesDAO
    {
        private ScheduleDataSet.EmployeeListDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceEmployeesDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            Table = ds.EmployeeList;
            Log = log;
            Force = force;
        }

        public async Task<int> GetAsync(DateTime? since = null)
        {
            var logName = Table.TableName + " (Instructors)";
            var count = Table.Count;

            Log.MarkQueryStart(logName);
            var soql = Soql.Pack(@"  
                SELECT  Id, FirstName, LastName, Name, Title, MailingStreet, MailingCity, 
                        MailingStateCode, MailingPostalCode, Phone, MobilePhone, InstructorDisplayOrder__c,
                        Email, LastModifiedDate, LastModifiedById
                FROM    Contact" +
                new WhereBuilder()
                    .Where("Instructor__c = true")
                    .WhereDateTime("LastModifiedDate > ", since)
            );
            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(logName, response.Records.Count);

            lock (Table)
            {
                foreach (var rec in response.Records)
                {
                    var row = Table.NewEmployeeListRow();
                    row.Type = "Contact";
                    row.EmployeeID = rec.Id;
                    row.DisplayOrder = FromJValue.ToShort(rec.InstructorDisplayOrder__c);
                    row.Username = rec.Id;
                    row.FullName = rec.Name;
                    row.FirstName = rec.FirstName;
                    row.LastName = rec.LastName;
                    row.Title = rec.Title;
                    row.IsInstructor = true;
                    row.AddressStreet = rec.MailingStreet;
                    row.AddressCity = rec.MailingCity;
                    row.AddressState = rec.MailingStateCode;
                    row.AddressZip = rec.MailingPostalCode;
                    row.EmployeeStatus = "Active";
                    row.PhoneHome = rec.Phone;
                    row.PhoneMobile = rec.MobilePhone;
                    row.Email1 = rec.Email;
                    row.Image = "Person";
                    row.LastModifiedDate = rec.LastModifiedDate ?? DateTime.MinValue;
                    row.LastModifiedBy = rec.LastModifiedById;
                    Table.AddEmployeeListRow(row);
                }
                //AddAllB2TUser();
            }
            Log.MarkLoadComplete(logName, Table.Count - count);

            logName = Table.TableName + " (Users)";
            count = Table.Count;
            Log.MarkQueryStart(logName);
            soql = Soql.Pack($@"  
                SELECT  Id, EmployeeNumber, Name, FirstName, LastName, Username, Title,  
                        IsActive, Phone, MobilePhone, 
                        LastModifiedById, LastModifiedDate
                FROM    User" +
                new WhereBuilder()
                    .Where("EmployeeNumber > ","0")
                    .WhereDateTime("LastModifiedDate > ", since));

            response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(logName, response.Records.Count);

            lock (Table)
            {
                foreach (var rec in response.Records)
                {
                    var row = Table.NewEmployeeListRow();
                    row.EmployeeID = rec.Id;
                    row.DisplayOrder = FromJValue.ToShort(rec.EmployeeNumber);
                    row.Type = "User";
                    row.FullName = rec.Name;
                    row.FirstName = rec.FirstName;
                    row.LastName = rec.LastName;
                    row.Username = rec.Username;
                    row.Title = rec.Title;
                    row.IsInstructor = false;
                    row.Image = "Person";
                    row.EmployeeStatus = "Active";
                    row.PhoneMobile = rec.PhoneMobile;
                    row.Email1 = rec.Email;
                    row.LastModifiedBy = rec.LastModifiedById;
                    row.LastModifiedDate = rec.LastModifiedDate ?? DateTime.MinValue;
                    Table.AddEmployeeListRow(row);
                }
            }
            Log.MarkLoadComplete(logName, Table.Rows.Count - count);
            return Table.Rows.Count - count;
        }

        //private void AddAllB2TUser()
        //{
        //    var row = Table.FindByEmployeeID("allb2t");
        //    if (row != null) return;
        //    row = Table.NewEmployeeListRow();
        //    row.DisplayOrder = 2000;
        //    row.Type = "System";
        //    row.EmployeeID = "allb2t";
        //    row.Username = "allb2t";
        //    row.FirstName = "All";
        //    row.LastName = "B2T";
        //    row.FullName = "All B2T";
        //    row.Image = "People";
        //    Table.AddEmployeeListRow(row);
        //}
    }
}
