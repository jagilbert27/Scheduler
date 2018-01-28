using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Dynamic;
using Salesforce.Common.Models;
using System.Linq;

namespace B2T_Scheduler.Data
{
    public class SalesForceAppointmentsDAO
    {
        private ScheduleDataSet DataSet { get; set; }
        private ScheduleDataSet.AppointmentsDataTable Table { get; set; }
        private QueryHistory Log { get; set; }
        private ForceConnection Force = null;

        public SalesForceAppointmentsDAO(ScheduleDataSet ds, QueryHistory log, ForceConnection force)
        {
            DataSet = ds;
            Table = ds.Appointments;
            Log = log;
            Force = force;
        }

        public async Task<int> GetClassesAsync(DateTime? startDate = null, DateTime? endDate = null, DateTime? modifiedSince = null)
        {
            var logName = Table.TableName + " (Classes)";
            var initialCount = Table.Count;
            var count = 0;

            Log.MarkQueryStart(logName);
            var soql = Soql.Pack(@"  
                SELECT  Id, Name, Account_Name__c, B2T_Sales_Contact__c, 
                        Billing_Notes__c, Class_Address__c, Class_Fee__c, Class_Location__c, Classroom_Name__c, 
                        Class_Type__c, Course_Label__c, 
                        Customer_Contact__r.Name, Customer_Contact__r.Email, Customer_Contact__r.Phone,
                        Billing_Contact__r.Name,  Billing_Contact__r.Email,  Billing_Contact__r.Phone, 
                        Shipping_Contact__r.Name, Shipping_Contact__r.Email, Shipping_Contact__r.Phone, Shipping_Contact__r.MailingAddress,
                        Manager_Contact__r.Name,  Manager_Contact__r.Email,  Manager_Contact__r.Phone, 
                        Start_Date__c, End_Date__c, Start_Time__c, End_Time__c, 
                        plms_enrolled__c, Expected_of_Students__c, Expenses__c, 
                        plms_instructor__c,  plms_instructor__r.InstructorDisplayOrder__c,
                        Material_Version__c, Notes__c,  
                        Status__c, White_Paper_Sent__c, LastModifiedById, LastModifiedDate
                FROM    pss_Course__c" +
                 new WhereBuilder()
                     .WhereDateTime("LastModifiedDate > ", modifiedSince)
                     .WhereDate("Start_Date__c >= ", startDate)
                     .WhereDate("End_Date__c < ", endDate)
                     .ToString() +
                "ORDER BY Start_Date__c");

            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);

            Log.MarkQueryComplete(logName, response.Records.Count);

            AddClassRecords(response.Records);

            for (var i = 0; !string.IsNullOrEmpty(response.NextRecordsUrl); i++)
            {
                Log.MarkQueryStart($"{logName}({i})");
                response = await Force.Connect().QueryContinuationAsync<dynamic>(response.NextRecordsUrl).ConfigureAwait(false); //new
                Log.MarkQueryComplete($"{logName}({i})", response.Records.Count);
                AddClassRecords(response.Records);
                count = Table.Rows.Count - initialCount;
                Log.MarkLoadComplete($"{logName}({i})", count);
            }
            Table.AcceptChanges();
            Log.MarkLoadComplete(logName, Table.Rows.Count);
            return count;
        }

        public int GetClassesModifiedSince(DateTime modifiedDate)
        {
            //q.MarkStart("GetClassesModifiedSince");
            //Task<int> task = GetClassesAsync(null, null, modifiedDate);
            //task.Wait();
            //int count = task.Result;
            //q.MarkStop("GetClassesModifiedSince");
            //return count;
            return -1;

        }

        public int GetClassesBetweenDates(DateTime startDate, DateTime endDate)
        {
            //q.MarkStart("GetClassesBetweenDates");
            //Task<int> task = GetClassesAsync(startDate, endDate, null);
            //task.Wait();
            //int count = task.Result;
            //q.MarkStop("GetClassesBetweenDates");
            //return count;
            return -1;
        }

        public async Task<int> GetEventsAsync(DateTime? startDate = null, DateTime? endDate = null, string eventId = null, DateTime? modifiedSince = null)
        {
            var logName = Table.TableName + " (Events)";
            Log.MarkQueryStart(logName);

            var soql = Soql.Pack($@"  
                SELECT  Id, OwnerId, AccountId, Type, schEventType__c, EventSubtype, Subject, Description, Location,
                        WhoId, Who.Name, lastModifiedBy.EmployeeNumber,
                        (SELECT RelationId, Status, IsInvitee, Relation.Name, Relation.Type FROM EventRelations),
                        StartDateTime, EndDateTime, LastModifiedById, LastModifiedDate
                FROM    Event" +
                new WhereBuilder()
                    .Where("Id = ", eventId)
                    .WhereDateTime("LastModifiedDate > ", modifiedSince)
                    .WhereDateTime("StartDateTime >= ", startDate)
                    .WhereDateTime("EndDateTime < ", endDate));

            var response = await Force.Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            Log.MarkQueryComplete(logName, response.Records.Count);

            lock (Table)
            {
                foreach (var rec in response.Records)
                {
                    int subId = 0;
                    AddEventRow(Table, rec, subId, rec.OwnerId.ToString());
                    if (rec.EventRelations != null)
                        foreach (var invitee in rec.EventRelations.records)
                            if (invitee.Relation.Type == "Contact" || invitee.Relation.Type == "User")
                                if (FromJValue.ToBool(invitee.IsInvitee))
                                    AddEventRow(Table, rec, ++subId, invitee.RelationId.ToString());
                }
            }
            Table.AcceptChanges();
            Log.MarkLoadComplete(logName, Table.Rows.Count);
            return response.Records.Count;
        }

        public async Task<int> SaveEventsAsync(UserInfo CurrentUser)
        {
            int recordCount = 0;
            recordCount += await SaveDeletedEventsAsync(CurrentUser).ConfigureAwait(false);
            recordCount += await SaveAddedEventsAsync(CurrentUser).ConfigureAwait(false);
            recordCount += await SaveModifiedEventsAsync(CurrentUser).ConfigureAwait(false);
            return recordCount;
        }

        private async Task<int> SaveDeletedEventsAsync(UserInfo CurrentUser)
        {
            int recordCount = 0;
            while (true)
            {
                var row =
                    (from r in Table.AsEnumerable()
                     where r.RowState == DataRowState.Modified
                     && r.PendingDelete
                     && r.AppointmentID != null
                     select r).FirstOrDefault();
                if (row == null) break;

                var rootAppointmentId = row.AppointmentID.Split('.')[0];
                var logName = $"Deleting event {rootAppointmentId}: {row.Subject}";
                Log.MarkStart(logName);
                bool response = await Force.Connect().DeleteAsync("Event", rootAppointmentId).ConfigureAwait(false);
                if (!response)
                    if (MessageBox.Show(logName + " Failed", "Delete Failed", MessageBoxButtons.OKCancel) == DialogResult.OK) continue;
                    else break;
                row.Delete();
                row.AcceptChanges();
                Log.MarkStop(logName);
                recordCount++;
            }
            return recordCount;
        }

        private async Task<int> SaveAddedEventsAsync(UserInfo CurrentUser)
        {
            int recordCount = 0;
            foreach (var row in
                from row in Table.AsEnumerable()
                where row.RowState == DataRowState.Added
                select row)
            {
                var logName = $"Adding event {row.AppointmentID}: {row.Subject}";
                Log.MarkStart(logName);
                var cat = DataSet.AppointmentCategories.FindByAppointmentCategoryID(row.AppointmentCategoryID);
                dynamic o = new ExpandoObject();
                o.Data_Source__c = "Scheduler";
                o.EventSubtype = "Event";
                o.schEventType__c = row.AppointmentCategoryID;
                o.StartDateTime = row.StartDate.ToUniversalTime();
                o.EndDateTime = row.EndDate.ToUniversalTime();
                o.ShowAs = cat.IsExclusive ? "Busy" : "Free";
                o.Subject = row.Subject;
                o.Description = row.Description;
                SuccessResponse response = await Force.Connect().CreateAsync("Event", o).ConfigureAwait(false);
                if (!response.Success)
                    if (MessageBox.Show(logName + " Failed", "Save Failed", MessageBoxButtons.OKCancel) == DialogResult.OK) continue;
                    else break;
                row.AppointmentID = response.Id;
                row.AcceptChanges();

                //Add the employee to the event
                if (CurrentUser.Id != row.EmployeeID)
                {
                    dynamic r = new ExpandoObject();
                    r.EventId = row.AppointmentID;
                    r.RelationId = row.EmployeeID;
                    response = await Force.Connect().CreateAsync("EventRelation", r).ConfigureAwait(false);
                    if (!response.Success)
                        if (MessageBox.Show(logName + " Failed", "Save Failed", MessageBoxButtons.OKCancel) == DialogResult.OK) continue;
                        else break;
                }
                recordCount++;
                Log.MarkStop(logName);
            }
            return recordCount;
        }

        private async Task<int> SaveModifiedEventsAsync(UserInfo CurrentUser)
        {
            int recordCount = 0;
            foreach (var row in
                from row in Table.AsEnumerable()
                where row.RowState == DataRowState.Modified
                select row)
            {
                var rootAppointmentId = row.AppointmentID.Split('.')[0];
                var logName = $"Updating event {rootAppointmentId}: {row.Subject}";
                Log.MarkStart(logName);
                dynamic o = new ExpandoObject();
                o.Data_Source__c = "Scheduler";

                if (Util.ColumnHasChanged(row, "StartDate"))
                    o.StartDateTime = DateTime.SpecifyKind(row.StartDate, DateTimeKind.Local).ToUniversalTime();

                if (Util.ColumnHasChanged(row, "EndDate"))
                    o.EndDateTime = DateTime.SpecifyKind(row.EndDate, DateTimeKind.Local).ToUniversalTime();

                //if (ColumnHasChanged(row, "Type")) o.Type = row.Type;
                if (Util.ColumnHasChanged(row, "Subject")) o.Subject = row.Subject;
                if (Util.ColumnHasChanged(row, "Description")) o.Description = row.Description;
                SuccessResponse response = await Force.Connect().UpdateAsync("Event", rootAppointmentId, o).ConfigureAwait(false);
                if (!response.Success)
                    if (MessageBox.Show(logName + " Failed", "Save Failed", MessageBoxButtons.OKCancel) == DialogResult.OK) continue;
                    else break;
                row.AcceptChanges();
                recordCount++;
                Log.MarkStop(logName);
            }
            return recordCount;
        }

        private void AddClassRecords(List<dynamic> records)
        {
            lock (Table)
            {
                foreach (var rec in records)
                {
                    var row = Table.NewAppointmentsRow();
                    row.AppointmentID = rec.Id;
                    row.AccountID = rec.Account_Name__c;
                    try
                    {
                        if (rec.plms_instructor__c != null)
                        {
                            row.EmployeeID = rec.plms_instructor__c;
                            row.DisplayOrder = FromJValue.ToShort(rec.plms_instructor__r.InstructorDisplayOrder__c);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("doh");
                    }
                    row.Status = rec.Status__c;
                    row.PendingDelete = false;
                    row.Subject = rec.Name;
                    row.Description = rec.Notes__c;
                    row.EndDate = FromJValue.ToDate(rec.End_Date__c).AddDays(1);
                    row.StartDate = FromJValue.ToDate(rec.Start_Date__c);
                    row.StartTime = FromJValue.ToTime(rec.Start_Time__c);
                    row.EndTime = FromJValue.ToTime(rec.End_Time__c);
                    row.AppointmentCategoryID = FromJValue.ToAppointmentCategoryID(rec.Name);
                    rec.Status__c.ToString().ToUpper();
                    row.AppointmentLayout = "AppointmentLayout";
                    row.RecurrencePattern = "RecurrencePattern";
                    row.NumStudents = FromJValue.ToShort(rec.plms_enrolled__c);
                    row.MaxStudents = FromJValue.ToShort(rec.Expected_of_Students__c);
                    row.WhitePaperSentDate = FromJValue.ToDate(rec.White_Paper_Sent__c);
                    row.ClassLocationID = rec.Class_Location__c;
                    row.Room = rec.Classroom_Name__c;
                    row.ClassType = rec.Class_Type__c;
                    row.ClassFee = FromJValue.ToDecimal(rec.Class_Fee__c);
                    row.ExpenseMode = rec.Expenses__c;
                    if (rec.Customer_Contact__r != null)
                    {
                        row.ClassContactName = rec.Customer_Contact__r.Name;
                        row.ClassContactPhone = rec.Customer_Contact__r.Phone;
                        row.ClassContactEmail = rec.Customer_Contact__r.Email;
                    }
                    if (rec.Billing_Contact__r != null)
                    {
                        row.BillingContactName = rec.Billing_Contact__r.Name;
                        row.BillingContactPhone = rec.Billing_Contact__r.Phone;
                        row.BillingContactEmail = rec.Billing_Contact__r.Email;
                    }
                    if (rec.Shipping_Contact__r != null)
                    {
                        row.ShippingContactName = rec.Shipping_Contact__r.Name;
                        row.ShippingContactPhone = rec.Shipping_Contact__r.Phone;
                        row.ShippingContactEmail = rec.Shipping_Contact__r.Email;
                        if (rec.Shipping_Contact__r.MailingAddress != null)
                        {
                            row.ShippingContactStreet = rec.Shipping_Contact__r.MailingAddress.street;
                            row.ShippingContactCity = rec.Shipping_Contact__r.MailingAddress.city;
                            row.ShippingContactState = rec.Shipping_Contact__r.MailingAddress.stateCode;
                            row.ShippingContactZip = rec.Shipping_Contact__r.MailingAddress.postalCode;
                        }
                    }
                    row.ClassNotes = rec.Notes__c;
                    row.BillingNotes = rec.Billing_Notes__c;
                    row.MaterialVersion = rec.Material_Version__c;
                    row.Deleted = false;
                    row.LastModifiedDate = FromJValue.ToDate(rec.LastModifiedDate);
                    row.LastModifiedBy = rec.LastModifiedBy;
                    Table.AddAppointmentsRow(row);
                }
            }
        }

        private void AddEventRow(ScheduleDataSet.AppointmentsDataTable table, dynamic rec, int subId, string employeeId)
        {
            var row = table.NewAppointmentsRow();
            row.AppointmentID = rec.Id + (subId > 0 ? "." + subId.ToString() : "");
            row.ClassType = "Event";
            row.AccountID = rec.AccountId;
            row.EmployeeID = employeeId;
            row.AppointmentCategoryID = FromJValue.ToAppointmentCategoryID(rec.schEventType__c, "NOTE");
            row.StartDate = rec.StartDateTime;
            row.EndDate = rec.EndDateTime;
            row.Subject = rec.Subject;
            row.Description = rec.Description;
            //if (rec.Location.ToString().Length > 0) row.Description += "\n@" + rec.Location;
            //if (rec.Who.Name.ToString().Length > 0) row.Description += "\n@" + rec.Who?.Name;
            row.Deleted = false;
            row.LastModifiedBy = rec.LastModifiedById;
            row.LastModifiedDate = rec.LastModifiedDate;
            row.PendingDelete = false;
            table.AddAppointmentsRow(row);
        }
    }
}
