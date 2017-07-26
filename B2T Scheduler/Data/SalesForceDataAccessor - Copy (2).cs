using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Salesforce.Force;
using System.Data;


namespace B2T_Scheduler.Data
{
    public class SalesForceDataAccessor
    {
        public const int LoadDataStepCount = 13;
        public const int ReloadDataStepCount = 4;

        //ToDo: move somewhere else
        public Dictionary<string, SalesForceCredential> SalesForceCredentials = new Dictionary<string, SalesForceCredential>();
        public string ActiveEnvironment = "PROD";


        //private static string _clientId = "3MVG9snqYUvtJB1OjjKeISDtvXiPVZpp2zmsjjbltCws7Pa5PV9jJapRREsN4Ez9Bg038chFJLHlom_AN_R2y";
        //private static string _clientSecret = "458262255620564311";
        //private static string _username = "jeff.gilbert@b2t.com.dev";
        //private static string _securitytoken = "5lrBa4mDRdlT1Qmksl3sEhBGE";
        //private static string _password = "DeerFoot27";
        //private static string _tokenRequestEndpointUrl = "https://test.salesforce.com/services/oauth2/token";

        private ForceClient _forceClient = null;
        public ScheduleDataSet DataSet { get; set; }
        public MainForm ParentForm { get; set; }
        public QueryHistory q = new QueryHistory();
        public string Error { get; private set; }

        public SalesForceUser CurrentUserDetail = new SalesForceUser();

        public Action<QueryHistory.QueryRecord> ProgressCallback { get; set; } = null;

        public SalesForceDataAccessor()
        {
            SalesForceCredentials.Add("QA", new SalesForceCredential
            {
                ClientId = "3MVG9snqYUvtJB1OjjKeISDtvXiPVZpp2zmsjjbltCws7Pa5PV9jJapRREsN4Ez9Bg038chFJLHlom_AN_R2y",
                ClientSecret = "458262255620564311",
                Securitytoken = "5lrBa4mDRdlT1Qmksl3sEhBGE",
                Username = "jeff.gilbert@b2t.com.qa",
                Password = "DeerFoot27",
                TokenRequestEndpointUrl = "https://test.salesforce.com/services/oauth2/token"
            });
            SalesForceCredentials.Add("DEV", new SalesForceCredential
            {
                ClientId = "3MVG9hq7jmfCuKfe8hUZHJ9MyeQ.kw55IUHqkWm5Ej98y2ngLsUYRzMM3xtP5zdcPfGbNWr1MkgwAni_3qiVs",
                ClientSecret = "3499427454799364352",
                Securitytoken = "7WjZzUOSFivrkmnGhKHIFJY7e",
                Username = "jeff@b2ttraining.com.dev",
                Password = "DeerFoot27",
                TokenRequestEndpointUrl = "https://test.salesforce.com/services/oauth2/token"
            });
            SalesForceCredentials.Add("PROD", new SalesForceCredential
            {
                ClientId = "3MVG9KI2HHAq33RzEfF5JLxL4LBJp7d0tKQ7dSTOQzB7rvUWlDXKe6iDqluna_6UI1VTcMb9bBPZadEie9k7u",
                ClientSecret = "8129887986087408605",
                Securitytoken = "hhWZiiiBx3SLunZhoDK5oGiP4",
                Username = "projects@b2ttraining.com",
                Password = "ACC360admin",
                TokenRequestEndpointUrl = "https://login.salesforce.com/services/oauth2/token"
            });
        }

        private void _ProgressCallback(QueryHistory.QueryRecord queryRecord)
        {
            if (ProgressCallback != null)
                ProgressCallback(queryRecord);
        }


        public void BeginLoadDataSet()
        {
            foreach (DataTable table in DataSet.Tables)
                table.BeginLoadData();
        }

        public void EndLoadDataSet()
        {
            foreach (DataTable table in DataSet.Tables)
                table.EndLoadData();
        }


        /*
        LoadData() should block until its done
          Launch all quries at once
          when they are ALL complete, merge the resulting data tables into the dataset
          return 
        */
        public void LoadData(DateTime startDate, DateTime endDate)
        {
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

            DataSet.Appointments.BeginLoadData;





            var tasks = new List<Task<int>> {
                GetFormatCategoriesAsync(),
                GetFormatsAsync(),
                GetEventTypesAsync(),
                GetClassLocationsAsync(),
                GetEmployeeListAsync(),
                GetUsersAsync(),
                GetSchedulePatternsAsync(),
                GetPreferenceTypesAsync(),
                GetAccountsAsync(),
                GetHolidaysAsync(),
                GetCoursesAsync()
            };
            q.MarkStart("Task.WaitAll");
            Task.WaitAll(tasks.ToArray());
            q.MarkStop("Task.WaitAll");
            q.MarkStop("First Pass");

            q.MarkStart("Second Pass");
            tasks = new List<Task<int>>{
                GetEmployeeScheduleFactorsAsync(),
                GetEmployeeCourseQualificationsAsync(),
            };
            Task.WaitAll(tasks.ToArray());

            DataSet.Appointments.BeginLoadData();

            //ToDo: re-enable events
            //q.MarkQueryStart("GetEventsAsync");
            //var table = GetEventsAsync().Result;
            //q.MarkQueryComplete("GetEventsAsync", table.Rows.Count);
            //DataSet.Appointments.Merge(table);
            //q.MarkLoadComplete("GetEventsAsync", table.Rows.Count);

            //table = GetClassesAsync(new DateTime(2016, 01, 01)).Result;
            var recordCount = GetClassesAsync(startDate, endDate).Result;
            //DataSet.Appointments.Merge(table);

            DataSet.Appointments.EndLoadData();

            //Temporary Kludge: Assign the classes with zombie instructor to me:
            var fakeEmployeeCount = 0;
            foreach (var appt in DataSet.Appointments)
            {
                if (appt.IsEmployeeIDNull())
                {
                    appt.EmployeeID = "0";
                    appt.DisplayOrder = 1000;
                }
                else {
                    if (!DataSet.EmployeeList.Rows.Contains(appt.EmployeeID))
                    {
                        fakeEmployeeCount++;
                        var row = DataSet.EmployeeList.NewEmployeeListRow();
                        row.EmployeeID = appt.EmployeeID;
                        row.DisplayOrder = (short)(100 + fakeEmployeeCount);
                        row.FirstName = ((char)(64 + fakeEmployeeCount)).ToString();
                        row.LastName = row.DisplayOrder.ToString();
                        row.IsInstructor = true;
                        row.EmployeeStatus = "Active";
                        row.Image = "Person";
                        DataSet.EmployeeList.AddEmployeeListRow(row);
                    }
                    appt.DisplayOrder = appt.EmployeeListRow.DisplayOrder;
                }
            }


            q.MarkStop("Second Pass");
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
                GetEmployeeCourseQualificationsAsync(),
                //GetCoursesAsync()
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

        async Task<int> GetFormatCategoriesAsync() //Imported,Visible, QueryComplete
        {
            var table = DataSet.FormatCategories;
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"SELECT Name, Key__c FROM schFormatCategory__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);
            table.Clear();
            foreach (var rec in response.Records)
            {
                var row = table.NewFormatCategoriesRow();
                row.Key = rec.Key__c;
                row.Name = rec.Name;
                table.AddFormatCategoriesRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);
            return table.Count;
        }

        async Task<int> GetAppointmentCategoriesAsync() //OK I'm getting nothing from here cuz schAppointmentCategory__c is mt
        {
            var table = DataSet.AppointmentCategories;
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"
                SELECT  Id, Name, Abbreviation__c, Description__c, LongDescription__c, Type__c, 
                        DurationDays__c, MaxStudents__c, Image__c, SortOrder__c, IsExclusive__c,
                        IsWorking__c, IsDeleted__c, LastModifiedDate
                FROM    schAppointmentCategory__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);
            //            table.Clear();
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
            return table.Count;
        }

        async Task<int> GetFormatsAsync()//TabVisible, DataImported, QueryComplete
        {
            var table = DataSet.Formats;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"
                SELECT  Id, Name, Key__c, Enum__c, DisplayOrder__c, Category__c,
                        Category__r.Key__c, 
                        Icon__c, ForegroundColor__c, BackgroundColor__c, BackgroundImage__c, 
                        LastModifiedDate
                FROM    schFormat__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            table.Clear();
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

            return table.Count;
        }

        async Task<int> GetSchedulePatternsAsync()//TabVisible, DataImported, QueryComplete
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

        async Task<int> GetPreferenceTypesAsync()//TabVisible, DataImported, QueryComplete
        {
            var table = DataSet.PreferenceTypes;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  Key__c, Name, Description__c
                FROM    schPreferenceType__c");
            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            table.Clear();
            foreach (var rec in response.Records)
            {
                var row = table.NewPreferenceTypesRow();
                row.PreferenceTypeID = FromJValue.ToShort(rec.Key__c);
                row.Name = rec.Name;
                row.Description = rec.Description;
                row.IconKey = rec.Key__c;
                table.AddPreferenceTypesRow(row);
            }
            q.MarkLoadComplete(table.TableName, table.Count);

            return table.Count;
        }

        async Task<int> GetHolidaysAsync()//QueryComplete
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

        //async Task<int> GetEventTypesAsync()//OK
        //{
        //    var table = DataSet.AppointmentCategories;

        //    q.MarkQueryStart(table.TableName);
        //    var soql = soqlPack($@"  
        //        SELECT  Id, Name, Description__c, Enum__c
        //        FROM    schPreferenceType__c");
        //    var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
        //    q.MarkQueryComplete(table.TableName, response.Records.Count);

        //    table.Clear();
        //    foreach (var rec in response.Records)
        //    {
        //        var row = table.NewPreferenceTypesRow();
        //        row.PreferenceTypeID = rec.Enum__c;
        //        row.Name = rec.Name;
        //        row.Description = rec.Description;
        //        row.IconKey = rec.Enum__c;
        //        table.AddPreferenceTypesRow(row);
        //    }
        //    q.MarkLoadComplete(table.TableName, table.Count);

        //    return table.Count;
        //}


        #endregion GetAsync Methods Loaded Once on Startup

        #region GetAsync Methods that are also polled:
        async Task<int> GetAccountsAsync(DateTime? since = null)//QueryComplete
        {
            var table = DataSet.Accounts;
            var initialCount = table.Count;
            var i = 0;
            var count = 0;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"
                SELECT  Id, Name, LastModifiedDate
                FROM    Account
               {(since.HasValue ? $"WHERE LastModifiedDate > '{soqlDate(since.Value)}'" : "")}");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            AddAccountRecords(response.Records);
            while (!string.IsNullOrEmpty(response.NextRecordsUrl))
            {
                i++;
                q.MarkQueryStart(table.TableName + "(" + i + ")");
                response = await Connect().QueryContinuationAsync<dynamic>(response.NextRecordsUrl);
                q.MarkQueryComplete(table.TableName + "(" + i + ")", response.Records.Count);
                AddAccountRecords(response.Records);
                count = DataSet.Accounts.Count - initialCount;
                q.MarkLoadComplete(table.TableName + "(" + i + ")", count);
            }
            return count;
        }

        private void AddAccountRecords(List<dynamic> records)
        {
            foreach (var rec in records)
            {
                var row = DataSet.Accounts.NewAccountsRow();
                row.AccountID = rec.Id;
                row.Name = rec.Name;
                row.LastModifiedDate = (rec.LastModifiedDate) ?? DateTime.MinValue;
                DataSet.Accounts.AddAccountsRow(row);
            }
        }

        async Task<int> GetClassLocationsAsync()//QueryComplete
        {
            var table = DataSet.ClassLocations;
            var since = q.GetLastQueryDate(table.TableName);
            var initialCount = table.Count;
            var i = 0;
            var count = 0;

            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  Id, Account__c, Name, Metro_Area__c, City__c, State__c, LastModifiedDate 
                FROM    Class_Location__c
                WHERE   IsDeleted=false
                {soqlWhere("AND   LastModifiedDate >", since)}");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);
            AddClassLocationRecords(response.Records);


            while (!string.IsNullOrEmpty(response.NextRecordsUrl))
            {
                i++;
                q.MarkQueryStart(table.TableName + "(" + i + ")");
                response = await Connect().QueryContinuationAsync<dynamic>(response.NextRecordsUrl);
                q.MarkQueryComplete(table.TableName + "(" + i + ")", response.Records.Count);
                AddAccountRecords(response.Records);
                count = DataSet.Accounts.Count - initialCount;
                q.MarkLoadComplete(table.TableName + "(" + i + ")", count);
            }
            return count;
        }

        private void AddClassLocationRecords(List<dynamic> records)
        {
            foreach (var rec in records)
            {
                var row = DataSet.ClassLocations.NewClassLocationsRow();
                row.ClassLocationID = rec.Id;
                row.AccountID = rec.Account__c;
                row.Name = rec.Name;
                row.MetroArea = rec.Metro_Area__c;
                row.City = rec.City__c;
                row.State = rec.State__c;
                row.LastModifiedDate = (rec.LastModifiedDate) ?? DateTime.MinValue;
                DataSet.ClassLocations.AddClassLocationsRow(row);
            }
        }

        async Task<int> GetEmployeeListAsync(DateTime? since = null)//TabVisible, DataImported, QueryComplete
        {
            var table = DataSet.EmployeeList;
            q.MarkQueryStart(table.TableName);

            var soql = soqlPack(@"  
                SELECT  Id, FirstName, LastName, Name, Title, MailingStreet, MailingCity, 
                        MailingStateCode, MailingPostalCode, Phone, MobilePhone, InstructorDisplayOrder__c,
                        Email, LastModifiedDate, LastModifiedById
                FROM    Contact" +
                new WhereBuilder()
                    .Add("Instructor__c = true")
                    .Add("LastModifiedDate > ", since)
            );

            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;

            foreach (var rec in response.Records)
            {
                var row = table.NewEmployeeListRow();
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
                //row.IsDeleted = false.ToString();
                row.PhoneHome = rec.Phone;
                row.PhoneMobile = rec.MobilePhone;
                row.Email1 = rec.Email;
                row.Image = "Person";
                row.LastModifiedDate = rec.LastModifiedDate ?? DateTime.MinValue;
                row.LastModifiedBy = rec.LastModifiedById;
                table.AddEmployeeListRow(row);
            }

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

        async Task<int> GetEmployeeScheduleFactorsAsync(DateTime? since = null)//TabVisible, DataImported, QueryComplete
        {
            var table = DataSet.EmployeeScheduleFactors;
            q.MarkQueryStart(table.TableName);
            var soql = soqlPack($@"  
                SELECT  Id, Name, Instructor__c, InstructorSchedulePatternId__c, 
                        EffectiveDate__c, ExpirationDate__c, LastModifiedDate, IsDeleted__c 
                FROM    schInstructorSchedulePreferences__c
                WHERE   IsDeleted=false
                {soqlWhere("AND   LastModifiedDate >", since)}");
            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
                table.LoadDataRow(new[]{
                    rec.Instructor__c,
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
                SELECT  Instructor__c, SchCourseId__c, EffectiveDate__c, ExpirationDate__c, 
                        QualificationLevel__c, IsDeleted,LastModifiedById, LastModifiedDate
                FROM    schInstructorCourseQualification__c
               {(since.HasValue ? $"WHERE LastModifiedDate > '{soqlDate(since.Value)}'" : "")}");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName, response.Records.Count);

            var initialCount = table.Count;
            foreach (var rec in response.Records)
            {
                var row = table.NewEmployeeCourseQualificationsRow();
                row.EmployeeID = rec.Instructor__c;
                row.CourseID = rec.SchCourseId__c;
                row.StartDate = rec.EffectiveDate__c;
                row.EndDate = rec.ExpirationDate__c ?? DateTime.MaxValue;
                row.QualificationLevel = 1;
                row.Deleted = rec.IsDeleted;
                row.LastModifiedBy = rec.LastModifiedById;
                row.LastModifiedDate = rec.LastModifiedDate ?? DateTime.MinValue;
                table.AddEmployeeCourseQualificationsRow(row);
            }
            var count = table.Count - initialCount;
            q.MarkLoadComplete(table.TableName, count);
            return count;
        }

        async Task<int> GetCoursesAsync(DateTime? startDate = null, DateTime? endDate = null, DateTime? modifiedSince = null) //QueryComplete
        {
            var table = DataSet.AppointmentCategories;

            q.MarkQueryStart(table.TableName + " (Distinct Classes)");

            var soql = soqlPack(@"SELECT Count(Id), Name FROM pss_Course__c group by Name");

            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName + " (Distinct Classes)", response.Records.Count);

            foreach (var rec in response.Records)
            {
                var row = table.NewAppointmentCategoriesRow();
                row.AppointmentCategoryID = FromJValue.ToAppointmentCategoryID(rec.Name);
                row.CategoryName = FromJValue.ToAppointmentCategoryName(rec.Name);
                //row.CategoryAbbreviation = rec.Abbreviation__c;
                row.IsExclusive = true;
                row.IsWorking = true;
                if (!table.Rows.Contains(row.AppointmentCategoryID))
                    table.AddAppointmentCategoriesRow(row);
            }
            q.MarkLoadComplete(table.TableName + " (Distinct Classes)", table.Rows.Count);
            return table.Rows.Count;
        }

        async Task<int> GetClassesAsync(DateTime? startDate = null, DateTime? endDate = null, DateTime? modifiedSince = null)
        {
            var table = DataSet.Appointments;
            var logName = table.TableName + " (Classes)";
            var initialCount = table.Count;
            var count = 0;

            q.MarkQueryStart(logName);
            var soql = soqlPack(@"  
                SELECT  Id, Name, Account_Name__c, B2T_Sales_Contact__c, 
                        Billing_Notes__c, Class_Address__c, Class_Fee__c, Class_Location__c, Classroom_Name__c, 
                        plms_classtype__c, Class_Type__c, Course_Image_URL__c, Course_Label__c, Course_Summary__c, 
                        Customer_Contact__r.Name, Customer_Contact__r.Email, Customer_Contact__r.Phone,
                        Billing_Contact__r.Name,  Billing_Contact__r.Email,  Billing_Contact__r.Phone, 
                        Shipping_Contact__r.Name, Shipping_Contact__r.Email, Shipping_Contact__r.Phone,
                        Manager_Contact__r.Name,  Manager_Contact__r.Email,  Manager_Contact__r.Phone, 
                        Start_Date__c, End_Date__c, plms_startdate__c, Date_to_Invoice__c,  plms_location__c,
                        plms_enrolled__c, Expected_of_Students__c, Expenses__c, plms_instructor__c, Moodle_Course_Id__c, 
                        Material_Version__c, Notes__c, PO__c, Shipping_Contact__c, Shipping_Contact_Address__c, 
                        Status__c, White_Paper_Sent__c, LastModifiedById, LastModifiedDate
                FROM    pss_Course__c" +
                 new WhereBuilder()
                     .Add("LastModifiedDate > ", modifiedSince)
                     .Add("Start_Date__c >= ", startDate)
                     .Add("End_Date__c < ", endDate)
                     .ToString()) +
                "ORDER BY Start_Date__c";

            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(logName, response.Records.Count);
            AddClassRecords(response.Records);

            for (var i = 0; !string.IsNullOrEmpty(response.NextRecordsUrl); i++)
            {
                q.MarkQueryStart(logName + "(" + i + ")");
                response = await Connect().QueryContinuationAsync<dynamic>(response.NextRecordsUrl);
                q.MarkQueryComplete(logName + "(" + i + ")", response.Records.Count);
                AddAccountRecords(response.Records);
                count = DataSet.Accounts.Count - initialCount;
                q.MarkLoadComplete(logName + "(" + i + ")", count);
            }
            q.MarkLoadComplete(logName, table.Rows.Count);
            return count;
        }


        private void AddClassRecords(List<dynamic> records)
        {
            foreach (var rec in records)
            {
                var row = DataSet.Appointments.NewAppointmentsRow();
                row.AppointmentID = rec.Id;
                row.AccountID = rec.Account_Name__c;
                row.EmployeeID = rec.plms_instructor__c;
                row.Status = rec.Status__c;
                row.PendingDelete = false;
                row.Subject = rec.Name;
                row.Description = "Description";
                row.EndDate = FromJValue.ToDate(rec.End_Date__c).AddDays(1);
                row.StartDate = FromJValue.ToDate(rec.Start_Date__c);
                row.AppointmentCategoryID = FromJValue.ToAppointmentCategoryID(rec.Name);
                // rec.Status__c.ToString().ToUpper();
                //row.AppointmentLayout = "AppointmentLayout";
                //row.RecurrencePattern = "RecurrencePattern";
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
                }
                row.ClassNotes = rec.Notes__c;
                row.BillingNotes = rec.Billing_Notes__c;
                row.MaterialVersion = rec.Material_Version__c;
                row.ShippingContactStreet = FromJValue.CleanAddress(rec.Shipping_Contact_Address__c);
                row.Deleted = false;
                row.LastModifiedDate = FromJValue.ToDate(rec.LastModifiedDate);
                row.LastModifiedBy = rec.LastModifiedBy;
                DataSet.Appointments.AddAppointmentsRow(row);
            }
        }


        async Task<DataTable> GetEventsAsync(DateTime? startDate = null, DateTime? endDate = null, DateTime? modifiedSince = null)
        {
            var table = new ScheduleDataSet.AppointmentsDataTable();

            q.MarkQueryStart(table.TableName + "(Events)");

            var soql = soqlPack($@"  
                SELECT  Id, OwnerId, AccountId, Type, Data_Source__c, Subject, Description, WhoId,
                        StartDateTime,EndDateTime, Location, EventSubtype,LastModifiedById, LastModifiedDate
                FROM    Event" +
                new WhereBuilder()
                    .Add("OwnerId = '0052C000000YUQwQAO'")
                    .Add("LastModifiedDate > ", modifiedSince)
                    .Add("StartDateTime >= ", startDate)
                    .Add("EndDateTime < ", endDate));

            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete((table.TableName + "(Events)"), response.Records.Count);

            foreach (var rec in response.Records)
            {
                var row = table.NewAppointmentsRow();
                row.AppointmentID = rec.Id;
                row.AppointmentCategoryID = FromJValue.ToAppointmentCategoryID(rec.Type);
                row.StartDate = rec.StartDateTime;
                row.EndDate = rec.EndDateTime;
                row.Subject = rec.Subject;
                row.Description = rec.Description;
                row.EmployeeID = rec.WhoId;
                row.Deleted = false;
                row.LastModifiedBy = rec.LastModifiedById;
                row.LastModifiedDate = rec.LastModifiedDate;
                table.AddAppointmentsRow(row);
            }
            q.MarkLoadComplete(table.TableName + "(Events)", table.Rows.Count);
            return table;
        }

        async Task<int> GetUsersAsync(DateTime? since = null)//QueryComplete
        {
            var table = DataSet.EmployeeList;
            q.MarkQueryStart(table.TableName + "(Users)");
            var count = table.Count;

            //ToDo: How do I get the User Addresses?
            var soql = soqlPack($@"  
                SELECT  Id, Name, FirstName, LastName, Username, Title,  
                        IsActive, Phone, MobilePhone, 
                        LastModifiedById, LastModifiedDate
                FROM    User") + soqlWhere("WHERE   LastModifiedDate >", since);

            var response = await _forceClient.QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete((table.TableName + "(Users)"), response.Records.Count);

            foreach (var rec in response.Records)
            {
                var row = table.NewEmployeeListRow();
                row.EmployeeID = rec.Id;
                row.FullName = rec.Name;
                row.FirstName = rec.FirstName;
                row.LastName = rec.LastName;
                row.Username = rec.Username;
                row.Title = rec.Title;
                row.IsInstructor = false;
                row.EmployeeStatus = "Active";
                row.PhoneMobile = rec.PhoneMobile;
                row.Email1 = rec.Email;
                row.LastModifiedBy = rec.LastModifiedById;
                row.LastModifiedDate = rec.LastModifiedDate ?? DateTime.MinValue;
                table.AddEmployeeListRow(row);
            }
            q.MarkLoadComplete(table.TableName + "(Users)", table.Rows.Count - count);
            return table.Rows.Count - count;
        }

        //async Task<int> GetAppointmentCategoriesAsync(DateTime? since = null)
        //{
        //    Console.WriteLine("GetAppointmentCategoriesAsync() Starting");
        //    DataSet.SchedulePatterns.Clear();

        //    var soql = soqlPack($@"
        //        SELECT  Id, Name, Abbreviation__c, Description__c, LongDescription__c, Type__c, 
        //                DurationDays__c, MaxStudents__c, Image__c, SortOrder__c, IsExclusive__c,
        //                IsWorking__c, IsDeleted__c, LastModifiedDate
        //        FROM    schEventType__c");

        //    var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);

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

        async Task<int> GetEventTypesAsync(DateTime? since = null)//TabVisible, DataImported, QueryComplete
        {
            var table = DataSet.AppointmentCategories;
            q.MarkQueryStart(table.TableName + " (Event Types)");
            var soql = soqlPack($@"
                SELECT  Id, Name, Key__c, Description__c,DisplayOrder__c, IsExclusive__c,
                        EventCategory__r.Key__c,IsWorking__c, IsDeleted, LastModifiedDate
                FROM    schEventType__c");
            var response = await Connect().QueryAsync<dynamic>(soql).ConfigureAwait(false);
            q.MarkQueryComplete(table.TableName + " (Event Types)", response.Records.Count);
            // table.Clear();
            foreach (var rec in response.Records)
            {
                var row = table.NewAppointmentCategoriesRow();
                row.Type = rec.EventCategory__r.Key__c;
                row.AppointmentCategoryID = rec.Key__c;
                row.CategoryName = rec.Name;
                row.Description = rec.Description__c;
                row.IsExclusive = rec.IsExclusive__c;
                row.IsWorking = rec.IsWorking__c;
                row.CategoryAbbreviation = rec.Key__c;
                row.SortOrder = rec.DisplayOrder__c;
                row.Deleted = rec.IsDeleted;
                row.LastModifiedDate = rec.LastModifiedDate;
                table.AddAppointmentCategoriesRow(row);
            }
            q.MarkLoadComplete(table.TableName + " (Event Types)", table.Count);
            return table.Count;
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

        public bool Authenticate(string username = null, string password = null, string environment = null)
        {
            ActiveEnvironment = environment ?? ActiveEnvironment;
            var cred = SalesForceCredentials[ActiveEnvironment];
            cred.Username = username ?? cred.Username;
            cred.Password = password ?? cred.Password;
            return (Connect() != null);
        }

        public ForceClient Connect()
        {
            if (_forceClient == null)
            {
                var cred = SalesForceCredentials[ActiveEnvironment];
                var auth = new Salesforce.Common.AuthenticationClient();
                auth.UsernamePasswordAsync(cred.ClientId, cred.ClientSecret, cred.Username, cred.Password + cred.Securitytoken, cred.TokenRequestEndpointUrl).Wait();
                _forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
                GetUserInfo(auth, _forceClient);
            }
            return _forceClient;
        }

        private async void GetUserInfo(Salesforce.Common.AuthenticationClient auth, ForceClient client)
        {
            var userInfo = await client.UserInfo<Salesforce.Common.Models.UserInfo>(auth.Id);

            CurrentUserDetail.first_name = userInfo.FirstName;
            CurrentUserDetail.last_name = userInfo.LastName;
            CurrentUserDetail.id = userInfo.UserId;
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
            if (d.Hour == 0 && d.Minute == 0 && d.Second == 0)
                return d.ToString("yyyy-MM-dd");
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

        private class WhereBuilder
        {
            private List<string> items = new List<string>();
            public WhereBuilder() { }

            public WhereBuilder Add(string clause)
            {
                items.Add(clause);
                return this;
            }

            public WhereBuilder Add(string fieldAndOperator, string value)
            {
                items.Add(fieldAndOperator + " '" + value + "'");
                return this;
            }

            public WhereBuilder Add(string fieldAndOperator, int? value)
            {
                if (value.HasValue)
                    items.Add(fieldAndOperator + " " + value);
                return this;
            }

            public WhereBuilder Add(string fieldAndOperator, DateTime? date)
            {
                if (date.HasValue)
                    items.Add(fieldAndOperator + soqlDate(date.Value));
                return this;
            }

            public override string ToString()
            {
                if (items.Count == 0) return string.Empty;
                var result = "";
                foreach (var item in items)
                    result += (result == "" ? " WHERE " : " AND ") + item;
                return result;
            }
        }

        private static class FromJValue
        {
            public static string CleanAddress(object value, string defaultValue = "")
            {
                string v = value.ToString();
                if (v == "<br>,  <br>")
                    return string.Empty;
                return v;
            }

            public static string ToAppointmentCategoryID(object value, string defaultValue = "0")
            {
                try
                {
                    var str = value.ToString().Replace(" ", "");
                    if (String.IsNullOrEmpty(str)) return defaultValue;
                    return str.Substring(0, Math.Min(40, str.Length)).ToUpper();
                }
                catch { return defaultValue; }
            }

            public static string ToAppointmentCategoryName(object value, string defaultValue = "0")
            {
                try
                {
                    var str = value.ToString();
                    if (String.IsNullOrEmpty(str)) return defaultValue;
                    if (str.Length <= 50) return str;
                    return str.Substring(0, 46) + "...";
                }
                catch { return defaultValue; }
            }

            public static string ToString(object value, string defaultValue = "")
            {
                try { return value.ToString(); }
                catch { return defaultValue; }
            }

            public static DateTime ToDate(object value)
            {
                DateTime d;
                if (value != null)
                    if (DateTime.TryParse(value.ToString(), out d)) return d;
                return DateTime.MinValue;
            }

            public static short ToShort(object value, short defaultValue = 0)
            {
                try { return (short)value; }
                catch
                {
                    short v;
                    if (short.TryParse(value.ToString(), out v)) return v;
                    return defaultValue;
                }
            }

            public static decimal ToDecimal(object value, decimal defaultValue = 0)
            {
                try { return (decimal)value; }
                catch { return defaultValue; }
            }
        }
        #endregion
    }
}
