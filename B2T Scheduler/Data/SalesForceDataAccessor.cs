using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Salesforce.Force;
using System.Data;
using System.Dynamic;
using Salesforce.Common.Models;
using System.Windows.Forms;

//ToDo: Refresh
//ToDo: unhardcode creds

//ToDo: Flag events as ooo or busy
//ToDid: Add all event types to right
//ToDid: Mark new event w/ star
//ToDid: Enable event editor
//ToDid: Enable saving event
//ToDid: Drag Drop Events
//ToDid: Add Time to event editor
//ToDid: Remove * from Notes
//ToDid: put note subject in text

namespace B2T_Scheduler.Data
{
    public class SalesForceDataAccessor
    {
        public const int LoadDataStepCount = 13;
        public const int ReloadDataStepCount = 4;

        //ToDo: move somewhere else
        public ScheduleDataSet DataSet { get; set; }
        public MainForm ParentForm { get; set; }
        public QueryHistory Log = new QueryHistory();
        public string Error { get; private set; }
        public Action<QueryHistory.QueryRecord> ProgressCallback { get; set; } = null;
        private ForceConnection Force = new ForceConnection();

        public SalesForceAppointmentsDAO AppointmentsDAO = null;
        public SalesForceAccountsDAO AccountsDAO = null;
        public SalesForceFormatsDAO FormatsDAO = null;
        public SalesForceFormatCategoriesDAO FormatCategoriesDAO = null;
        public SalesForceSchedulePatternsDAO SchedulePatternsDAO = null;
        public SalesForcePreferenceTypesDAO PreferenceTypesDAO = null;
        public SalesForceHolidaysDAO HolidaysDAO = null;
        public SalesForceClassLocationsDAO ClassLocationsDAO = null;
        public SalesForceEmployeesDAO EmployeesDAO = null;
        public SalesForceAppointmentCategoriesDAO AppointmentCategoriesDAO = null;
        public SalesForceEmployeeScheduleFactorsDAO EmployeeScheduleFactorsDAO = null;
        public SalesForceEmployeeCourseQualificationsDAO EmployeeCourseQualificationsDAO = null;
        public SalesForceCoursesDAO CoursesDAO = null;

        public SalesForceDataAccessor(Form parentForm, DataSet dataSet)
        {
            ParentForm = parentForm as MainForm;
            DataSet = dataSet as ScheduleDataSet;
            AppointmentsDAO = new SalesForceAppointmentsDAO(DataSet, Log, Force);
            AccountsDAO = new SalesForceAccountsDAO(DataSet, Log, Force);
            FormatsDAO = new SalesForceFormatsDAO(DataSet, Log, Force);
            FormatCategoriesDAO = new SalesForceFormatCategoriesDAO(DataSet, Log, Force);
            SchedulePatternsDAO = new SalesForceSchedulePatternsDAO(DataSet, Log, Force);
            PreferenceTypesDAO = new SalesForcePreferenceTypesDAO(DataSet, Log, Force);
            HolidaysDAO = new SalesForceHolidaysDAO(DataSet, Log, Force);
            ClassLocationsDAO = new SalesForceClassLocationsDAO(DataSet, Log, Force);
            EmployeesDAO = new SalesForceEmployeesDAO(DataSet, Log, Force);
            AppointmentCategoriesDAO = new SalesForceAppointmentCategoriesDAO(DataSet, Log, Force);
            EmployeeScheduleFactorsDAO = new SalesForceEmployeeScheduleFactorsDAO(DataSet, Log, Force);
            EmployeeCourseQualificationsDAO = new SalesForceEmployeeCourseQualificationsDAO(DataSet, Log, Force);
            CoursesDAO = new SalesForceCoursesDAO(DataSet, Log, Force);
        }

        public bool IsAuthenticated() => Force.IsAuthenticated();

        public bool Authenticate(string username = null, string password = null, string environment = null) =>
            Force.Authenticate(username, password, environment);

        public UserInfo CurrentUser { get; private set; }

        private void _ProgressCallback(QueryHistory.QueryRecord queryRecord)
        {
            if (ProgressCallback != null)
                ProgressCallback(queryRecord);
        }

        public void LoadData(DateTime startDate, DateTime endDate)
        {
            Log = new QueryHistory()
            {
                IsTimingEnabled = true,
                IsVerbose = true,
                ProgressCallback = _ProgressCallback
            };

            Log.MarkStart("Load Data");

            foreach (DataTable table in DataSet.Tables)
                table.BeginLoadData();

            Log.MarkStart("Connect");
            Force.Connect();
            Log.MarkStop("Connect");


            Log.MarkStart("Send Queries");
            var tasks = new List<Task<int>> {
                //GetUserInfoAsync(),
                FormatCategoriesDAO.GetAsync(),
                FormatsDAO.GetAsync(),
                AppointmentCategoriesDAO.GetAsync(),
                ClassLocationsDAO.GetAsync(),
                EmployeesDAO.GetAsync(),
                SchedulePatternsDAO.GetAsync(),
                PreferenceTypesDAO.GetAsync(),
                AccountsDAO.GetAsync(),
                HolidaysDAO.GetAsync(),
                CoursesDAO.GetAsync(),
                EmployeeScheduleFactorsDAO.GetAsync(),
                EmployeeCourseQualificationsDAO.GetAsync(),
                AppointmentsDAO.GetClassesAsync(startDate, endDate),
                AppointmentsDAO.GetEventsAsync(startDate, endDate),
            };
            Log.MarkStop("Send Queries");

            Log.MarkStart("Waiting for Data");
            Task.WaitAll(tasks.ToArray());
            Log.MarkStop("Waiting for Data");

            Log.MarkStart("collating");
            {
                //Fill in DisplayOrder in appointments
                foreach (ScheduleDataSet.AppointmentsRow row in DataSet.Appointments)
                    if (row.EmployeeListRow != null)
                        row.DisplayOrder = row.EmployeeListRow.DisplayOrder;

                //Find the "Current User"
                GetUserInfo();

                foreach (DataTable table in DataSet.Tables)
                    table.EndLoadData();

                DataSet.AcceptChanges();
            }
            Log.MarkStop("collating");

            Log.Write();
        }

        public int ReloadData()
        {
            Log = new QueryHistory();
            Log.MarkStart("ReloadData");

            const int maxTries = 3;
            for (var tries = 0; tries < maxTries; tries++)
            {
                try
                {
                    var count = ReloadDataInternal();
                    Log.MarkStop("ReloadData");
                    return count;
                }

                catch (Exception ex)
                {
                    ParentForm.showProgress("Lost database connection in ReloadData() because\n" + ex.Message + ".\n Trying to reconnect...");
                    System.Threading.Thread.Sleep(3000);
                    try
                    {
                        Force.Authenticate();
                    }
                    catch (Exception ex2)
                    {
                        ParentForm.showProgress("Authenticate threw an exception: " + ex2.Message);
                    }
                }
            }
            throw (new Exception("Lost database connection in ReloadData().  Unable to reestablish connection after " + maxTries + " attempts.\n"));
        }


        //ToDo: Replace this with GetUserInfoAsync()
        private void GetUserInfo()
        {
            var Emp = DataSet.EmployeeList.FindByEmployeeID("00561000001XaXIAA0");
            CurrentUser = new UserInfo
            {
                DisplayName = "Administrator",
                Id = Emp.EmployeeID,
            };
        }

        private async Task<int> GetUserInfoAsync()
        {
            Log.MarkQueryStart("GetUserInfo");

            //Hangs:
            //CurrentUser = await Force.Connect().UserInfo<UserInfo>("https://test.salesforce.com/services/oauth2/userinfo").ConfigureAwait(false);

            //Hangs:
            //Task.Factory.StartNew(() => Force.Connect().UserInfo<UserInfo>(Force.AuthClient.Id)).

            //Hangs:
            //Task<UserInfo> userInfoTask = Force.Connect().UserInfo<UserInfo>("https://test.salesforce.com/services/oauth2/userinfo");
            //await userInfoTask.ConfigureAwait(false);
            //userInfoTask.Wait();

            //I GIVE UP!
            Log.MarkQueryComplete("GetUserInfo");
            return CurrentUser != null ? 1 : 0;
        }

        private int ReloadDataInternal()
        {
            int newRecords = 0;
            return newRecords;

            //Log.MarkStart("Refresh");
            //Log.MarkStart("Connect");
            //Force.Connect();
            //Log.MarkStop("Connect");



            //Log.MarkStart("Send Queries");
            //var tasks = new List<Task<int>> {
            //    //GetUserInfoAsync(),
            //    FormatCategoriesDAO.GetAsync(),
            //    FormatsDAO.GetAsync(),
            //    AppointmentCategoriesDAO.GetAsync(),
            //    ClassLocationsDAO.GetAsync(),
            //    EmployeesDAO.GetAsync(),
            //    SchedulePatternsDAO.GetAsync(),
            //    PreferenceTypesDAO.GetAsync(),
            //    AccountsDAO.GetAsync(),
            //    HolidaysDAO.GetAsync(),
            //    CoursesDAO.GetAsync(),
            //    EmployeeScheduleFactorsDAO.GetAsync(),
            //    EmployeeCourseQualificationsDAO.GetAsync(),
            //    AppointmentsDAO.GetClassesAsync(startDate, endDate),
            //    AppointmentsDAO.GetEventsAsync(startDate, endDate),
            //};
            //Log.MarkStop("Send Queries");

            //Log.MarkStart("First Pass");
            //tasks = new List<Task<int>> {
            //    ClassLocationsDAO.GetAsync(),
            //    EmployeesDAO.GetAsync(),
            //    AccountsDAO.GetAsync(),
            //    HolidaysDAO.GetAsync()
            //};
            //Task.WaitAll(tasks.ToArray());
            //Log.MarkStop("First Pass");
            //Log.MarkStop("Refresh");
            //Log.Write();
            //return newRecords;
        }

        public async Task<int> SaveData()
        {
            int recordCount = 0;
            try
            {
                recordCount += await AppointmentsDAO.SaveEventsAsync(CurrentUser).ConfigureAwait(false);

                //ToDo:SaveInstructors();
                //ToDo:SaveSchedulePatterns();
                //ToDo:SaveEmployeeCourseQualifications();
                //ToDo:SaveAccountEmployeePreferences();

                //ToDo:ReloadData();
                //ParentForm.showProgress("Ready");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Failed: " + ex.Message);
            }
            return recordCount;
        }







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


    }
}
