using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salesforce.Force;
using System.Data;


namespace B2T_Scheduler
{
    public class SalesForceDataAccessor
    {
        public const int LoadDataStepCount = 13;
        public const int ReloadDataStepCount = 4;

        //ToDo: move somewhere else
        private static string _clientId = "3MVG9snqYUvtJB1OjjKeISDtvXiPVZpp2zmsjjbltCws7Pa5PV9jJapRREsN4Ez9Bg038chFJLHlom_AN_R2y";
        private static string _clientSecret = "458262255620564311";
        private static string _username = "jeff.gilbert@b2t.com.qa";
        private static string _securitytoken = "5lrBa4mDRdlT1Qmksl3sEhBGE";
        private static string _password = "DeerFoot27";
        private static string _tokenRequestEndpointUrl = "https://test.salesforce.com/services/oauth2/token";

        private ForceClient _forceClient = null;
        public ScheduleDataSet DataSet { get; set; }
        public MainForm ParentForm { get; set; }
        public QueryHistory q = new QueryHistory();
        public string Error { get; private set; }

        public SalesForceUser CurrentUserDetail = new SalesForceUser();

        public Action<QueryHistory.QueryRecord> ProgressCallback { get; set; } = null;

        private void _ProgressCallback(QueryHistory.QueryRecord queryRecord)
        {
            if (ProgressCallback != null)
                ProgressCallback(queryRecord);
        }

        async public Task<int> LoadDataAsync()
        {

            var tasks = new List<Task<DataTable>>
            {
                //GetFormatCategoriesAsync(),
                //GetAppointmentCategoriesAsync(),
                GetFormatsAsync()
            };

            DataSet.FormatCategories.BeginLoadData();
            DataSet.AppointmentCategories.BeginLoadData();
            DataSet.Formats.BeginLoadData();
            while (tasks.Count > 0)
            {
                Console.WriteLine($"{tasks.Count} tasks remaining");
                Task.WaitAny(tasks)
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                DataTable result = await completedTask;
                Console.WriteLine(result.TableName);
                DataSet.Tables[result.TableName].Merge(result);
            }
            DataSet.Formats.EndLoadData();
            DataSet.FormatCategories.EndLoadData();
            DataSet.AppointmentCategories.EndLoadData();

            return 1;
        }

        public void LoadData()
        {
            var lda = LoadDataAsync();
            lda.Wait();

            q = new QueryHistory()
            {
                IsTimingEnabled = true,
                IsVerbose = true,
                ProgressCallback = _ProgressCallback
            };

            q.MarkStart("Load Data");
            q.MarkStart("Connect");
            Connect();
            q.MarkStop("Connect");
            q.MarkStart("First Pass");

            var tasks = new List<Task<DataTable>>
            {
                GetFormatCategoriesAsync(),
                GetAppointmentCategoriesAsync(),
                GetFormatsAsync()
            };

            //var tasks = new List<Task<int>> {
            //    //GetEventsAsync(),

            //    GetFormatsAsync(),
            //    GetAppointmentCategoriesAsync(),
            //    GetClassLocationsAsync(),
            //    GetEmployeeListAsync(),
            //    GetSchedulePatternsAsync(),
            //    GetPreferenceTypesAsync(),
            //    GetAccountsAsync(),
            //    GetHolidaysAsync()
            //};
            //Task.WaitAll(tasks.ToArray());
            //q.MarkStop("First Pass");





            //q.MarkStart("Second Pass");
            //tasks = new List<Task<int>>{
            //    GetEmployeeScheduleFactorsAsync(),
            //    GetEmployeeCourseQualificationsAsync(),
            //    //GetCoursesAsync(),
            //    //GetClassesAsync(),
            //    //GetAppointmentsAsync(),
            //};
            //Task.WaitAll(tasks.ToArray());
            //q.MarkStop("Second Pass");



            ////test load appointments
            //var arow = DataSet.Appointments.NewAppointmentsRow();
            //arow.AppointmentID = "00U3B0000016WfhUAE";
            //arow.StartDate = DateTime.Today;
            //arow.EndDate = DateTime.Today.AddDays(1);
            //arow.EmployeeID = "0053B000000EnvsQAC";
            //DataSet.Appointments.AddAppointmentsRow(arow);



            q.MarkStop("Load Data");
            q.Write();
        }

        public int ReloadData()
        {
            q = new QueryHistory();
            q.MarkStart("ReloadData");

            const int maxTries = 3;
            for (var tries = 0; tries < maxTries; tries++)
            {
                try
                {
                    var count = ReloadDataInternal();
                    q.MarkStop("ReloadData");
                    return count;
                }

                catch (Exception ex)
                {
                    ParentForm.showProgress("Lost database connection in ReloadData() because\n" + ex.Message + ".\n Trying to reconnect...");
                    System.Threading.Thread.Sleep(3000);
                    try
                    {
                        Authenticate();
                    }
                    catch (Exception ex2)
                    {
                        ParentForm.showProgress("Authenticate threw an exception: " + ex2.Message);
                    }
                }
            }
            throw (new Exception("Lost database connection in ReloadData().  Unable to reestablish connection after " + maxTries + " attempts.\n"));
        }

        public int GetClassesModifiedSince(DateTime modifiedDate)
        {
            q.MarkStart("GetClassesModifiedSince");
            Task<int> task = GetClassesAsync(null, null, modifiedDate);
            task.Wait();
            int count = task.Result;
            q.MarkStop("GetClassesModifiedSince");
            return count;
        }

        public int GetClassesBetweenDates(DateTime startDate, DateTime endDate)
        {
            q.MarkStart("GetClassesBetweenDates");
            Task<int> task = GetClassesAsync(startDate, endDate, null);
            task.Wait();
            int count = task.Result;
            q.MarkStop("GetClassesBetweenDates");
            return count;
        }

        private int ReloadDataInternal()
        {
            int newRecords = 0;

            q.MarkStart("Refresh");
            q.MarkStart("Connect");
            Connect();
            q.MarkStop("Connect");

            q.MarkStart("First Pass");
            var tasks = new List<Task<int>> {
                GetClassLocationsAsync(),
                GetEmployeeListAsync(),
                GetAccountsAsync(),
                GetHolidaysAsync()
            };
            Task.WaitAll(tasks.ToArray());
            q.MarkStop("First Pass");
            
            q.MarkStart("Second Pass");
            tasks = new List<Task<int>>{
                GetEmployeeScheduleFactorsAsync(),
                GetEmployeeCourseQualificationsAsync()
                //GetCoursesAsync(),
                //GetClassesAsync(),
                //GetAppointmentsAsync(),
            };
            Task.WaitAll(tasks.ToArray());
            q.MarkStop("Second Pass");
            q.MarkStop("Refresh");
            q.Write();
            return newRecords;
        }

        public int SaveData()
        {
            int recordCount = 0;
            Connect();
            //ToDo:SaveAppointments();
            //ToDo:SaveInstructors();
            //ToDo:SaveSchedulePatterns();
            //ToDo:SaveEmployeeCourseQualifications();
            //ToDo:SaveAccountEmployeePreferences();

            //ToDo:ReloadData();
            ParentForm.showProgress("Ready");
            return recordCount;
        }

        #region GetAsync Methods Loaded Once on Startup

        async Task<DataTable> GetFormatCategoriesAsync()
        {
            var table = new ScheduleDataSet.FormatCategoriesDataTable();
            table.TableName = "FormatCategories";
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"SELECT Name, Key__c FROM schFormatCategory__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);
            foreach (var rec in response.Records)
            {
                var row = table.NewFormatCategoriesRow();
                row.Key = rec.Key__c;
                row.Name = rec.Name;
                table.AddFormatCategoriesRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);
            return table;
        }

        async Task<DataTable> GetAppointmentCategoriesAsync() //OK
        {
            var table = new ScheduleDataSet.AppointmentCategoriesDataTable();
            table.TableName = "AppointmentCategories";
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"
                SELECT  Id, Name, Abbreviation__c, Description__c, LongDescription__c, Type__c, 
                        DurationDays__c, MaxStudents__c, Image__c, SortOrder__c, IsExclusive__c,
                        IsWorking__c, IsDeleted__c, LastModifiedDate
                FROM    schAppointmentCategory__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);
            table.Clear();
            foreach (var rec in response.Records)
            {
                var row = table.NewAppointmentCategoriesRow();
                row.AppointmentCategoryID = rec.Id;
                row.CategoryName = rec.Name;
                row.Description = rec.Description__c;
                row.Type = rec.Type__c;
                row.DurationDays = rec.DurationDays__c;
                row.MaxStudents = rec.MaxStudents__c;
                row.LongDescription = rec.LongDescription__c;
                row.IsExclusive = rec.IsExclusive__c;
                row.IsWorking = rec.IsWorking__c;
                row.CategoryAbbreviation = rec.Abbreviation__c;
                row.Image = rec.Image__c;
                row.SortOrder = rec.SortOrder__c;
                row.Deleted = rec.IsDeleted__c;
                row.LastModifiedDate = rec.LastModifiedDate;
                table.AddAppointmentCategoriesRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);
            return table;
        }

        async Task<DataTable> GetFormatsAsync()//OK
        {
            var table = new ScheduleDataSet.FormatsDataTable();

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"
                SELECT  Id, Name, Key__c, Enum__c, DisplayOrder__c, Category__c,
                        Category__r.Key__c, 
                        Icon__c, ForegroundColor__c, BackgroundColor__c, BackgroundImage__c, 
                        LastModifiedDate
                FROM    schFormat__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            foreach (var rec in response.Records)
            {
                var row = table.NewFormatsRow();
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
                table.AddFormatsRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);

            return table;
        }

        async Task<int> GetSchedulePatternsAsync()//OK
        {
            var table = DataSet.SchedulePatterns;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"
                SELECT  Id, Name, Description__c, IconKey__c, WorkWeeksInPeriod__c, WeeksInPeriod__c
                FROM    schInstructorSchedulePattern__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            table.Clear();
            foreach (var rec in response.Records)
            {
                var row = table.NewSchedulePatternsRow();
                row.PatternID = rec.Id;
                row.Name = rec.Name;
                row.Description = rec.Description__c;
                row.IconKey = rec.IconKey__c;
                row.WorkWeeksInPeriod = rec.WorkWeeksInPeriod__c;
                row.WeeksInPeriod = rec.WeeksInPeriod__c;
                DataSet.SchedulePatterns.AddSchedulePatternsRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);

            return table.Count;
        }

        async Task<int> GetPreferenceTypesAsync()//OK
        {
            var table = DataSet.PreferenceTypes;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  Id, Name, Description__c, Enum__c
                FROM    schPreferenceType__c");
            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            table.Clear();
            foreach (var rec in response.Records)
            {
                var row = table.NewPreferenceTypesRow();
                row.PreferenceTypeID = rec.Enum__c;
                row.Name = rec.Name;
                row.Description = rec.Description;
                row.IconKey = rec.Enum__c;
                table.AddPreferenceTypesRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);

            return table.Count;
        }

        async Task<int> GetHolidaysAsync()//OK
        {
            var table = DataSet.Holidays;
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"SELECT Id, ActivityDate, Name, Description,  LastModifiedDate FROM Holiday");
            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            table.Clear();
            foreach (var rec in response.Records)
            {
                var row = table.NewHolidaysRow();
                row.ID = rec.Id;
                row.StartDate = rec.ActivityDate;
                row.EndDate = rec.ActivityDate;
                row.Name = rec.Name;
                row.Description = rec.Description;
                row.LastModifiedDate = rec.LastModifiedDate;
                table.AddHolidaysRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);

            return table.Count;
        }

        #endregion GetAsync Methods Loaded Once on Startup

        #region GetAsync Methods that are also polled:
        async Task<int> GetAccountsAsync(DateTime? since = null)//OK
        {
            var table = DataSet.Accounts;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"
                SELECT  Id, Name, LastModifiedDate
                FROM    Account
               {(since.HasValue ? $"WHERE LastModifiedDate > '{soqlDate(since.Value)}'" : "")}");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
                DataSet.Accounts.LoadDataRow(new[]{
                    rec.Id, //AccountId,
                    rec.Name, //Name
                    rec.Name, //Abbreviation
                    (rec.LastModifiedDate) ?? DateTime.MinValue  //LastModifiedDate
                }, LoadOption.Upsert);
            var count = DataSet.Accounts.Count - initialCount;
            q.MarkLoadComplete(table.TableName, count);

            return count;
        }

        async Task<int> GetClassLocationsAsync()//OK
        {
            var table = DataSet.ClassLocations;
            var since = q.GetLastQueryDate(table.TableName);
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  Id, Account__c, Name, Metro_Area__c, City__c, State__c, LastModifiedDate 
                FROM    Class_Location__c
                WHERE   IsDeleted=false
                {soqlWhere("AND   LastModifiedDate >", since)}");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
                table.LoadDataRow(new[]{
                    rec.Id, //ClassLocationID
                    rec.Account__c, //AccountID
                    rec.Name, //Name
                    rec.Metro_Area__c, //MetroArea
                    rec.City__c, //City
                    rec.State__c, //State
                    (rec.LastModifiedDate) ?? DateTime.MinValue //LastModifiedDate
                }, LoadOption.Upsert);
            var count = table.Count - initialCount;
            q.MarkLoadComplete(table.TableName, count);

            return count;
        }

        async Task<int> GetEmployeeListAsync(DateTime? since = null)//OK
        {
            var table = DataSet.EmployeeList;
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  Id, Name, FirstName, LastName, Username, Title, AboutMe,
                        Street,City,State,PostalCode,Country,
                        Phone, MobilePhone, IsActive, IsInstructor__c, 
                        LastModifiedById, LastModifiedDate
                FROM    User") + soqlWhere("WHERE   LastModifiedDate >", since);
            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
                table.LoadDataRow(new[]{
                    rec.Id, //EmployeeID
                    rec.Name,
                    rec.FirstName,
                    rec.LastName,
                    rec.Username,
                    rec.Title,
                    "", //Image
                    0, //DisplayOrder
                    rec.IsInstructor__c, //IsInstructor
                    (bool)rec.IsActive ? "Active":"Inactive", //EmployeeStatus
                    rec.Street, //AddressStreet
                    rec.City, //AddressCity
                    rec.State, //AddressState
                    rec.PostalCode, //AddressZip
                    rec.Phone,
                    rec.PhoneMobile,
                    rec.Email,
                    "", //Email2
                    rec.AboutMe, //Description
                    false, //IsDeleted,
                    rec.LastModifiedDate ?? DateTime.MinValue,
                    rec.LastModifiedBy
                }, LoadOption.Upsert);

            AddAllB2TUser();

            var count = DataSet.EmployeeList.Count - initialCount;
            q.MarkLoadComplete(table.TableName, count);
            return count;
        }

        private void AddAllB2TUser()
        {
            var row = DataSet.EmployeeList.FindByEmployeeID("allb2t");
            if (row != null) return;
            row = DataSet.EmployeeList.NewEmployeeListRow();
            row.EmployeeID = "allb2t";
            row.Username = "allb2t";
            row.FirstName = "All";
            row.LastName = "B2T";
            DataSet.EmployeeList.AddEmployeeListRow(row);
        }

        async Task<int> GetEmployeeScheduleFactorsAsync(DateTime? since = null)//OK (Requires EmployeeList)
        {



            var table = DataSet.EmployeeScheduleFactors;
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  Id, Name, User__c, InstructorSchedulePatternId__c, 
                        EffectiveDate__c, ExpirationDate__c, LastModifiedDate, IsDeleted__c 
                FROM    schInstructorSchedulePreferences__c
                WHERE   IsDeleted=false
                {soqlWhere("AND   LastModifiedDate >", since)}");
            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
                table.LoadDataRow(new[]{
                    rec.User__c,
                    rec.InstructorSchedulePatternId__c,
                    0,
                    0,
                    rec.EffectiveDate__c,
                    rec.IsDeleted__c,
                    rec.LastModifiedBy,
                    (rec.LastModifiedDate) ?? DateTime.MinValue
                }, LoadOption.Upsert);
            var count = table.Count - initialCount;
            q.MarkLoadComplete(table.TableName, count);

            return count;
        }

        async Task<int> GetEmployeeCourseQualificationsAsync(DateTime? since = null)//OK (Requires EmployeeList)
        {
            var table = DataSet.EmployeeCourseQualifications;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  User__c, SchCourseId__c, EffectiveDate__c, ExpirationDate__c, 
                        QualificationLevel__c, IsDeleted,LastModifiedById, LastModifiedDate
                FROM    schInstructorCourseQualification__c
               {(since.HasValue ? $"WHERE LastModifiedDate > '{soqlDate(since.Value)}'" : "")}");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
                table.LoadDataRow(new[]{
                    rec.User__c,//EmployeeID
                    rec.SchCourseId__c, //CourseID
                    (rec.EffectiveDate__c) ?? DateTime.MinValue, //StartDate
                    //DateTime.MaxValue, // (rec.ExpirationDate__c) ?? DateTime.MaxValue, //EndDate
                    (rec.ExpirationDate_c != null)? rec.ExpirationDate_c:DateTime.MaxValue,
                    1, //rec.QualificationLevel__c, //QualificationLevel
                    rec.IsDeleted, //Deleted
                    rec.LastModifiedById, //LastModifiedBy
                    (rec.LastModifiedDate) ?? DateTime.MinValue  //LastModifiedDate
                }, LoadOption.Upsert);
            var count = table.Count - initialCount;
            q.MarkLoadComplete(table.TableName, count);

            return count;
        }

        //events
        async Task<int> GetClassesAsync(DateTime? startDate, DateTime? endDate, DateTime? modifiedSince = null)
        {
            var table = DataSet.Appointments;

            q.MarkQueryStart(table.TableName + "(Classes)");

            var whereClause = "";
            if (modifiedSince.HasValue)
                whereClause += whereClause == "" ? " WHERE " : " AND " + "LastModifiedDate > " + soqlDate(modifiedSince.Value);
            if (startDate.HasValue)
                whereClause += whereClause == "" ? " WHERE " : " AND " + "Start_Date__c >= " + soqlDate(startDate.Value);
            if (endDate.HasValue)
                whereClause += whereClause == "" ? " WHERE " : " AND " + "End_Date__c < " + soqlDate(endDate.Value);

            var soql = soqlPack($@"  
                SELECT  Id, Name, Account_Name__c, B2T_Image_Logo__c, B2T_Sales_Contact__c, Billing_Contact__c, 
                        Billing_Notes__c, Class_Address__c, Class_Fee__c, Class_Location__c, Classroom_Name__c, 
                        plms_classtype__c, Class_Type__c, Course_Image_URL__c, Course_Label__c, Course_Summary__c, 
                        Customer_Contact__c, Customer_Contact_Email__c, Customer_Contact_Phone__c, Date_to_Invoice__c, 
                        plms_enrolled__c, Expected_of_Students__c, Expenses__c, plms_instructor__c, Moodle_Course_Id__c, 
                        plms_location__c, Manager_Contact__c, Manager_Contact_Email__c, Manager_Contact_Phone__c, 
                        Material_Version__c, Notes__c, PO__c, Shipping_Contact__c, Shipping_Contact_Address__c, 
                        Shipping_Contact_Email__c, Shipping_Contact_Phone__c, Start_Date__c, plms_startdate__c, 
                        Status__c, White_Paper_Sent__c, LastModifiedById, LastModifiedDate,
                FROM    pss_Course__c
                {whereClause}
                LIMIT 1000");

            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName + "(Classes)", response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
            {
                table.LoadDataRow(
                    new[]{
                        rec.Id,
                        rec.plms_instructor__c  , //EmployeeID
                        rec.Account_Name__c , //AccountID
                        rec.Class_Location__c   , //ClassLocationID
                        rec.Status__c   , //Status
                        "Subject"    , //Subject
                        "Description"    , //Description
                        rec.Start_Date__c   , //StartDate
                        DateTime.MinValue   , //EndDate
                        "AppointmentCategoryId", //AppointmentCategoryId
                        "AppointmentLayout", //AppointmentLayout
                        "RecurrencePattern", //RecurrencePattern
                        rec.plms_enrolled__c    , //NumStudents
                        rec.Expected_of_Students__c , //MaxStudents
                        rec.White_Paper_Sent__c , //WhitePaperSentDate
                        rec.Classroom_Name__c   , //Room
                        DateTime.MinValue   , //StartTime
                        DateTime.MinValue    , //EndTime
                        0  , //DisplayOrder
                        rec.plms_classtype__c   , //ClassType
                        DateTime.MinValue   , //MaterialShipDate
                        0   , //StudentPrice
                        rec.Class_Fee__c    , //ClassFee
                        rec.Expenses__c , //ExpenseMode
                        "InvoiceTerms"  , //InvoiceTerms
                        rec.Customer_Contact__c , //ClassContactName
                        rec.Customer_Contact_Phone__c   , //ClassContactPhone
                        rec.Customer_Contact_Email__c   , //ClassContactEmail
                        rec.Manager_Contact__c  , //BillingContactName
                        rec.Manager_Contact_Phone__c    , //BillingContactPhone
                        rec.Manager_Contact_Email__c    , //BillingContactEmail
                        "ShippingContactName"   , //ShippingContactName
                        rec.Shipping_Contact_Phone__c   , //ShippingContactPhone
                        rec.Shipping_Contact_Email__c   , //ShippingContactEmail
                        "ShipmentTrackingNumbers"   , //ShipmentTrackingNumbers
                        false   , //IsCustomClass
                        rec.Notes__c    , //ClassNotes
                        rec.Billing_Notes__c    , //BillingNotes
                        rec.Material_Version__c , //MaterialVersion
                        rec.Shipping_Contact_Address__c , //ShippingContactStreet
                        rec.Shipping_Contact_Address__c , //ShippingContactCity
                        rec.Shipping_Contact_Address__c , //ShippingContactState
                        rec.Shipping_Contact_Address__c , //ShippingContactZip
                        "-1" , //NumRegistered
                        false , //PendingDelete
                        false  , //Deleted
                        rec.LastModifiedDate ?? DateTime.MinValue,
                        rec.LastModifiedBy
                    },
                    LoadOption.Upsert
                );
            };
            var count = table.Count - initialCount;
            q.MarkLoadComplete(table.TableName + "(Classes)", count);
            return count;
        }

        async Task<int> GetEventsAsync(DateTime? startDate = null, DateTime? endDate = null, DateTime? modifiedSince = null)
        {
            var table = DataSet.Appointments;

            q.MarkQueryStart(table.TableName + "(Events)");
            var whereClause = "";
            if (modifiedSince.HasValue)
                whereClause += whereClause == "" ? " WHERE " : " AND " + "LastModifiedDate > " + soqlDate(modifiedSince.Value);
            if (startDate.HasValue)
                whereClause += whereClause == "" ? " WHERE " : " AND " + "Start_Date__c >= " + soqlDate(startDate.Value);
            if (endDate.HasValue)
                whereClause += whereClause == "" ? " WHERE " : " AND " + "End_Date__c < " + soqlDate(endDate.Value);

            var soql = soqlPack($@"  
                SELECT  Id, OwnerId, AccountId, Type, Subject, Description, ActivityDateTime, ActivityDate, StartDateTime,
                        EndDateTime, Location, EventSubtype,LastModifiedById, LastModifiedDate
                FROM    Event 
                {whereClause}
                LIMIT 1");


            //var response = await _forceClient.QueryAsync<dynamic>(soql); //never returns
            //var response = _forceClient.QueryAsync<dynamic>(soql).Wait(-1); //doesn't hang, but wheres the data?
            //var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(true);//never returns
            //var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);  //hangs at AAR
            //var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false); //Make this the first query: Still hangs at AAR
            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false); //Make this the only query: Still hangs at AAR




            //var response = _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            //q.MarkQueryComplete(table.TableName + "(Events)", response.Records.Count);

            //test load appointments
            var arow = DataSet.Appointments.NewAppointmentsRow();
            arow.AppointmentID = "00U3B0000016WfhUAE";
            arow.StartDate = DateTime.Today;
            arow.EndDate = DateTime.Today.AddDays(1);
            arow.EmployeeID = "0053B000000EnvsQAC";
            DataSet.Appointments.AddAppointmentsRow(arow);

            var initialCount = table.Count;

            //    var values = new[]{
            //            rec.Id,                 //AppointmentID
            //            rec.OwnerId,            //EmployeeID
            //            rec.AccountId,          //AccountID
            //            "a063B000001LMKKQA4	",  //ClassLocationID
            //            rec.Type,               //Status
            //            rec.Subject,            //Subject
            //            rec.Description,        //Description
            //            rec.ActivityDateTime ?? rec.ActivityDate ?? rec.StartDateTime, //StartDate
            //            rec.EndDateTime,        //EndDate
            //            "a0S3B0000004Y9NUAU",   //AppointmentCategoryId
            //            null,                   //AppointmentLayout
            //            null,                   //RecurrencePattern
            //            null,                   //NumStudents
            //            null,                   //MaxStudents
            //            null,                   //WhitePaperSentDate
            //            rec.Location,           //Room
            //            DateTime.MinValue,      //StartTime
            //            DateTime.MinValue,      //EndTime
            //            0,                      //DisplayOrder
            //            rec.EventSubtype,       //ClassType
            //            DateTime.MinValue,      //MaterialShipDate
            //            0,                      //StudentPrice
            //            0,                      //ClassFee
            //            null,                   //ExpenseMode
            //            null,                   //InvoiceTerms
            //            null,                   //ClassContactName
            //            null,                   //ClassContactPhone
            //            null,                   //ClassContactEmail
            //            null,                   //BillingContactName
            //            null,                   //BillingContactPhone
            //            null,                   //BillingContactEmail
            //            null,                   //ShippingContactName
            //            null,                   //ShippingContactPhone
            //            null,                   //ShippingContactEmail
            //            null,                   //ShipmentTrackingNumbers
            //            false,                  //IsCustomClass
            //            null,                   //ClassNotes
            //            null,                   //BillingNotes
            //            null,                   //MaterialVersion
            //            null,                   //ShippingContactStreet
            //            null,                   //ShippingContactCity
            //            null,                   //ShippingContactState
            //            null,                   //ShippingContactZip
            //            0,                      //NumRegistered
            //            false,                  //PendingDelete
            //            false,                  //Deleted
            //            rec.LastModifiedById,   //LastModifiedBy
            //            rec.LastModifiedDate ?? DateTime.MinValue //LastModifiedDate
            //        };
            //    table.LoadDataRow(values, LoadOption.Upsert);
            //}
            //try {
            //    table.EndLoadData();
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine("Boom");
            //}

            var count = table.Count - initialCount;
            q.MarkLoadComplete(table.TableName + "(Events)", count);
            return count;
        }

        async Task<int> GetAppointmentNotificationsAsync(DateTime? since = null)
        {
            Console.WriteLine("AppointmentNotifications Starting");
            var count = DataSet.EmployeeList.Count;

            //ToDo: How do I get the User Addresses?
            var soql = soqlPack($@"  
                SELECT  Id, Name, FirstName, LastName, Username, Title,  
                        IsInstructor__c, IsActive, Phone, MobilePhone, 
                        LastModifiedById, LastModifiedDate
                FROM    User") + soqlWhere("WHERE   LastModifiedDate >", since);

            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);

            foreach (var rec in response.Records)
            {
                var values = new object[]{
                    rec.Id, //EmployeeID
                    rec.Name,
                    rec.FirstName,
                    rec.LastName,
                    rec.Username,
                    rec.Title,
                    "", //Image
                    0, //DisplayOrder
                    rec.IsInstructor__c,
                    "Active", //EmployeeStatus
                    "", //AddressStreet
                    "", //AddressCity
                    "", //AddressState
                    "", //AddressZip
                    rec.Phone,
                    rec.PhoneMobile,
                    rec.Email,
                    "", //Email2
                    "", //Description
                    false, //IsDeleted,
                    rec.LastModifiedDate ?? DateTime.MinValue,
                    rec.LastModifiedBy
                };
                DataSet.EmployeeList.LoadDataRow(values, LoadOption.Upsert);
            }
            var loaded = DataSet.EmployeeList.Count - count;
            Console.WriteLine($"GetEmployeeListAsync() got {loaded} records");
            return loaded;
        }

        async Task<int> GetAppointmentCategoriesAsync(DateTime? since = null)
        {
            Console.WriteLine("GetAppointmentCategoriesAsync() Starting");
            DataSet.SchedulePatterns.Clear();

            var soql = soqlPack($@"
                SELECT  Id, Name, Abbreviation__c, Description__c, LongDescription__c, Type__c, 
                        DurationDays__c, MaxStudents__c, Image__c, SortOrder__c, IsExclusive__c,
                        IsWorking__c, IsDeleted__c, LastModifiedDate
                FROM    schAppointmentCategory__c");

            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);

            foreach (var rec in response.Records)
            {
                ScheduleDataSet.AppointmentCategoriesRow row = DataSet.AppointmentCategories.NewAppointmentCategoriesRow();
                row.AppointmentCategoryID = rec.Id;
                row.CategoryName = rec.Name;
                row.Description = rec.Description__c;
                row.Type = rec.Type__c;
                row.DurationDays = rec.DurationDays__c;
                row.MaxStudents = rec.MaxStudents__c;
                row.LongDescription = rec.LongDescription__c;
                row.IsExclusive = rec.IsExclusive__c;
                row.IsWorking = rec.IsWorking__c;
                row.CategoryAbbreviation = rec.Abbreviation__c;
                row.Image = rec.Image__c;
                row.SortOrder = rec.SortOrder__c;
                row.Deleted = rec.IsDeleted;
                row.LastModifiedDate = rec.LastModifiedDate;
            }
            Console.WriteLine($"GetAppointmentCategoriesAsync() Got {DataSet.AppointmentCategories.Count} records");
            return DataSet.AppointmentCategories.Count;
        }

        #endregion GetAsync Methods that are also polled:




        #region Save Methods

        private int SaveAccountEmployeePreferences()
        {
            foreach (ScheduleDataSet.AccountEmployeePreferencesRow row in DataSet.AccountEmployeePreferences.Rows)
            {
                switch (row.RowState)
                {
                    case DataRowState.Deleted:
                        break;
                    case DataRowState.Added:
                        break;
                    case DataRowState.Modified:
                        break;
                }
            }
            return -1;
        }

        #endregion Save Methods

        #region local classes
        public class SalesForceUser
        {
            public string id { get; set; } = "-1";
            public string first_name { get; set; } = "first_name";
            public string last_name { get; set; } = "last_name";
        }
        #endregion

        #region Connectivity

        public bool IsAuthenticated()
        {
            return Authenticate();
        }

        public bool Authenticate(string username = null, string password = null)
        {
            _username = username ?? _username;
            _password = password ?? _password;
            return (Connect() != null);
        }

        public ForceClient Connect()
        {
            if (_forceClient == null)
            {
                var auth = new Salesforce.Common.AuthenticationClient();
                auth.UsernamePasswordAsync(_clientId, _clientSecret, _username, _password + _securitytoken, _tokenRequestEndpointUrl).Wait();
                _forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
            }
            return _forceClient;
        }

        #endregion Connectivity

        #region helpers
        static int getNumberOrDefault(string value, int defaultValue)
        {
            int number;
            if (int.TryParse(value, out number))
                return number;
            return defaultValue;
        }

        static string soqlDate(DateTime d)
        {
            //2005-05-18T14:01:00-04:00
            return d.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        static string soqlWhere(string clause, DateTime? d)
        {
            return d.HasValue ? clause + " " + soqlDate(d.Value) : string.Empty;
        }

        static string soqlPack(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim(" \n".ToArray());
        }

        #endregion
    }
}
