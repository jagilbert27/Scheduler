using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Janus.Windows.TimeLine;
using System.Collections;
using System.Runtime.InteropServices;
using B2T_Scheduler.Data;
using System.Drawing.Drawing2D;
using B2T_Scheduler.UI;
using System.Linq;


namespace B2T_Scheduler
{
    public partial class MainForm : Form
    {

        #region Global Variables
        private Filter Filter;

        public Salesforce.Common.Models.UserInfo CurrentUser;
        protected TreeNode DraggingCourse = null;
        protected bool IsDraggingTime = false;
        public bool IsFilterCheckEnabled = true;
        protected bool IsReadOnly = true;
        protected bool IsProduction = true;
        public bool IsLoading = true;
        protected TimeLineItem ContextMenuItem;
        protected TimeLineGroupRow SelectedGroupRow = null;
        protected Point LastClickPoint;
        public bool IsLongOperationCanceled = false;
        protected bool PreviousShowErrorsChecked = true;
        private int NavPaintCalls = 0;
        private long NavPaintTicks = 0;
        private Random randomGUIDSeed = new System.Random();
        int MonthHeadHeight = 14;
        protected ScheduleDataSet.EmployeeListRow AllB2TEmployee = null;
        public enum IsWorkingFilter { DontCare = -1, NotWorking = 0, Working = 1 }
        public enum IsExclusiveFilter { DontCare = -1, NotExclusive = 0, Exclusive = 1 }
        public string BgLoadProgressMessage = "";
        private SalesForceDataAccessor SalesForceDA;
        Queue DirtyAppointments = new Queue();
        private ScheduleDataSet.EmployeeListRow SelectedEmployee;

        #endregion



        #region Manange Global and Main Form items

        public MainForm()
        {
            InitializeComponent();
            Filter = new Filter(this);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            InitData();
        }

        //Set focus to first empty login control
        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (txt_Username.Text.Length == 0) txt_Username.Focus();
            else if (txt_Password.Text.Length == 0) txt_Password.Focus();
            else btn_Login.Focus();
        }


        /// <summary>
        /// Saves the user preferenes when the application closes
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (ExitPlease() == false)
            {
                e.Cancel = true;
                return;
            }
            base.OnClosing(e);
        }

        /// <summary>
        /// Sets the window title to indicate the database that is being used and if the application is in read-only mode.
        /// </summary>
        private void RefreshTitle()
        {
            System.Reflection.AssemblyTitleAttribute title = (System.Reflection.AssemblyTitleAttribute)
                System.Reflection.AssemblyTitleAttribute.GetCustomAttribute(
                System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Reflection.AssemblyTitleAttribute));
        }

        /// <summary>
        /// Loads data after an initial pause
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeferLoadTimer_Tick(object sender, EventArgs e)
        {
            DeferLoadTimer.Enabled = false;
            switch (DeferLoadTimer.Tag.ToString())
            {
                case "ApplyFilter":
                    ApplyFilter();
                    break;
                case "ApplyDisplayOptions":
                    ApplyDisplayOptions();
                    break;
            }
        }


        /// <summary>
        /// Manages the geometry of the inner panels when the main form is resized
        /// </summary>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            return;
            if (uiPanel_Middle.Height > 280)
                uiPanel_MiddleTop.MinimumSize = new Size(-1, uiPanel_Middle.Height - 280);
            if (uiPanel_MiddleTop.Height < uiPanel_MiddleTop.MinimumSize.Height)
                uiPanel_MiddleTop.Height = uiPanel_MiddleTop.MinimumSize.Height;
        }
        #endregion



        #region Manage the data layer

        /// <summary>
        /// Performs one-time initialization of necessary display options and data structures
        /// </summary>
        /// <seealso cref="LoadData"/>
        /// <see cref="RefreshData"/>
        public void InitData()
        {
            ShowFreePanel();

            //SetReadOnlyMode(true);

            //Apply reasonable defaults to the timeline
            ShowEditor();
            //            uiPanel_MiddleBottom.SelectedPanel = uiPanel_MiddleBottom.Panels[2];
            dtp_StartDateFilter.Value = DateTime.Today.AddMonths(-3);
            dtp_EndDateFilter.Value = DateTime.Today.AddYears(1);
            ApplyDateFilter();
            timeLine1.FirstDate = DateTime.Today.AddDays(-14);

            LocalStorage.LoadDateState(this);
            ApplyDateFilter();

            LocalStorage.LoadLoginState(this);
            // LoadDisplayOptionState();

            SetMenuBarState();

            //Display the application version on the splash/login screen
            try
            {
                lbl_ProductVersion.Text = "Version: " +
                    System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Major.ToString() + "." +
                    System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Minor.ToString() + "." +
                    System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Build.ToString() + "." +
                    System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Revision.ToString();
            }
            catch
            {
                lbl_ProductVersion.Text = "Version: [NOT DEPLOYED]";
            }

            //Add the fields to the timeline fields structure that are required to implement filtering on these values
            timeLine1.Fields.Add(new TimeLineField("DisplayOrder"));
            timeLine1.Fields.Add(new TimeLineField("AccountID"));
            timeLine1.Fields.Add(new TimeLineField("ClassType"));
            timeLine1.Fields.Add(new TimeLineField("Status"));
            timeLine1.Fields.Add(new TimeLineField("Deleted"));
            timeLine1.Fields.Add(new TimeLineField("WhitePaperSentDate"));

            timeLine1.Fields["EmployeeID"].ImageKey = "Person";
            timeLine1.Fields["EmployeeID"].HasValueList = true;
            timeLine1.Fields["DisplayOrder"].ImageKey = "Person";
            timeLine1.Fields["DisplayOrder"].HasValueList = true;
            timeLine1.Fields["AppointmentCategoryID"].ImageKey = "Course";
            timeLine1.Fields["AppointmentCategoryID"].HasValueList = true;
            timeLine1.Fields["AppointmentCategoryID"].ImageKey = "Course";
            timeLine1.Fields["AppointmentCategoryID"].HasValueList = true;
            timeLine1.Fields["AccountID"].HasValueList = true;

            scheduleDataSet1.AppointmentCategories.DefaultView.Sort = "SortOrder asc";
            scheduleDataSet1.AppointmentCategories.DefaultView.ApplyDefaultSort = true;

            SetReadOnlyMode(tlb_ReadOnly.Pressed);
        }

        private int ReloadData()
        {
            RefreshTitle();

            int recordCount = 0;
            IsLoading = true;
            timeLine1.Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            var startDate = dtp_StartDateFilter.Value;
            var endDate = dtp_EndDateFilter.Value;

            recordCount = SalesForceDA.ReloadData(startDate, endDate);


            if (recordCount > 0)
            {
                showProgress("Validating Schedule...");
                ValidateSchedule();
                RefreshNavMap(true);
            }

            timeLine1.Cursor = Cursors.Default;
            IsLoading = false;

            RefreshTitle();

            ShowEditor();

            return recordCount;
        }

        public delegate void ShowProgressDelegate(string msg);
        private void DAProgress(QueryHistory.QueryRecord queryRecord)
        {
            var msg = queryRecord.GetProgressMessage();
            BeginInvoke(new ShowProgressDelegate(showProgress), new object[] { msg });
            Application.DoEvents();
        }

        private void LoadData()
        {
            SalesForceDA.DataSet = scheduleDataSet1;
            SalesForceDA.ProgressCallback = DAProgress;

            RefreshTitle();

            if (LocalStorage.ShowTipOnStartup(this))
                new TipOfTheDay().Show();

            showProgress("Loading...");


            var startDate = dtp_StartDateFilter.Value;
            var endDate = dtp_EndDateFilter.Value;
            if (SalesForceDA.IsAuthenticated())
            {
                LoadDisplayOptionState();
                SalesForceDA.LoadData(startDate, endDate);
                CurrentUser = SalesForceDA.CurrentUser;
            }
            else
            {
                MessageBox.Show("Invalid Username & Password.");
                return;
            }

            scheduleDataSet1.ClassLocations.AddClassLocationsRow(
                "0", "0", "Unassigned Location", "", "", "", DateTime.MinValue);

            scheduleDataSet1.Accounts.AddAccountsRow(
                "0", "Public", "Pub", DateTime.MinValue);


            SetTimelineFormat();
            InitLegend();

            AllB2TEmployee = scheduleDataSet1.EmployeeList.Where(e => e.DisplayOrder == 1001).FirstOrDefault();

            showProgress("Adding Holidays...");
            AddHolidaysToTimeLine();

            //queue up all appointments for being painted
            foreach (ScheduleDataSet.AppointmentsRow appt in scheduleDataSet1.Appointments)
                if (!DirtyAppointments.Contains(appt))
                    DirtyAppointments.Enqueue(appt);

            timeLine1.Fields["EmployeeID"].ValueList.PopulateValueList(
                SalesForceDA.DataSet.EmployeeList, "EmployeeID", GetEmployeeNameFieldPreference());

            timeLine1.Fields["DisplayOrder"].ValueList.PopulateValueList(
                scheduleDataSet1.EmployeeList, "DisplayOrder", GetEmployeeNameFieldPreference());

            timeLine1.Fields["AppointmentCategoryID"].ValueList.PopulateValueList(
                scheduleDataSet1.AppointmentCategories, "AppointmentCategoryID", "CategoryName");

            timeLine1.Fields["AccountID"].ValueList.PopulateValueList(
                scheduleDataSet1.Accounts, "AccountID", "AccountName");

            showProgress("Grouping...");
            GroupByInstructor();

            InitEditForm();

            //Initialize the Instructor Editor user control
            instructorEditor1.DataSet = scheduleDataSet1;
            instructorEditor1.CurrentUserDetail = this.CurrentUser;
            instructorEditor1.NameField = GetEmployeeNameFieldPreference();

            showProgress("Initializing Filters...");
            InitCourses();
            Filter.InitAllFilters();

            showProgress("Loading saved filters...");
            showProgress("Validating Schedule...");
            LoadFilterState();
            ApplyFilter();

            RefreshNavMap(true);

            showProgress("Ready");
            toolStripProgressBar1.Visible = false;

            timeLine1.Cursor = Cursors.Default;
            IsLoading = false;
            ValidateSchedule();

            RefreshTitle();

            bgWorker_AnimateNavBar.RunWorkerAsync();
        }


        /// <summary>
        /// Adds Holidays that are in the data adapter into the TimeLine.  Ok to call multiple times
        /// </summary>
        private void AddHolidaysToTimeLine()
        {
            timeLine1.WorkingHourSchema.Exceptions.Clear();

            //if (tre_Filters.Nodes.Find("HOLIDAYWORK", true).Length == 0) return;

            foreach (ScheduleDataSet.HolidaysRow holiday in scheduleDataSet1.Holidays.Rows)
            {
                var whe = new WorkingHourException(new DateRange(holiday.StartDate, holiday.EndDate));
                whe.HourRange.FormatStyle.BackColor = GetFormat("EventType", "HOLIDAY").GetBackColor();
                timeLine1.WorkingHourSchema.Exceptions.Add(whe);
            }

            //Interate over all the appts for "allb2t"
            foreach (ScheduleDataSet.AppointmentsRow appt in AllB2TEmployee.GetAppointmentsRows())
            {
                if (appt.PendingDelete) continue;
                if ((appt.AppointmentCategoriesRow.AppointmentCategoryID == "HOLIDAYWORK"
                    && tre_Filters.Nodes.Find("HOLIDAYWORK", true)[0].Checked)
                    ||
                    (appt.AppointmentCategoriesRow.AppointmentCategoryID == "HOLIDAY"
                    && tre_Filters.Nodes.Find("HOLIDAY", true)[0].Checked)
                    )
                {
                    WorkingHourException whe = new WorkingHourException(new DateRange(appt.StartDate.Date, appt.EndDate.Date));
                    whe.HourRange.FormatStyle.BackColor = appt.AppointmentCategoriesRow.FormatsRow.GetBackColor();
                    if (!appt.AppointmentCategoriesRow.FormatsRow.IsBackgroundImageKeyNull())
                        whe.HourRange.FormatStyle.BackgroundImage = imageList1.Images[appt.AppointmentCategoriesRow.FormatsRow.BackgroundImageKey];

                    whe.HourRange.FormatStyle.BackgroundImageDrawMode = BackgroundImageDrawMode.Tile;
                    timeLine1.WorkingHourSchema.Exceptions.Add(whe);

                }
            }
        }
        #endregion



        #region Login

        private void txt_Password_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 13)
                btn_Login_Click(sender, e);
        }

        private void cmb_SelectDatabase_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 13)
                txt_Username.Focus();
        }

        private void txt_Username_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 13)
                txt_Password.Focus();
        }

        /// <summary>
        /// User clicked cancel on the login screen
        /// </summary>
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            pnl_Login.Visible = false;
        }


        /// <summary>
        /// When the user clicks the login button.  Tries to authenticate 
        /// to the selected database using the specified username and 
        /// password and loads the data from the database.
        /// </summary>
        private void btn_Login_Click(object sender, EventArgs e)
        {
            if (txt_Username.Text.Length == 0 ||
                txt_Password.Text.Length == 0)
            {
                MessageBox.Show(
                    "Please enter your username and password",
                    "Login Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);
                return;
            }

            // Show the progress bar
            IsLoading = true;
            timeLine1.Cursor = Cursors.WaitCursor;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Visible = true;
            Application.DoEvents();

            SalesForceDA = new SalesForceDataAccessor(this, scheduleDataSet1);
            //SalesForceDA.ParentForm = this;
            //SalesForceDA.DataSet = scheduleDataSet1;
            //SalesForceDA.LoadData();

            toolStripProgressBar1.Maximum = SalesForceDataAccessor.LoadDataStepCount + 11;
            showProgress("Logging in to SalesForce");

            // Try to authenticate via soap
            if (SalesForceDA.Authenticate(txt_Username.Text, txt_Password.Text) == false)
            {
                //Couldn't connect to the database
                MessageBox.Show(
                    SalesForceDA.Error,
                    "Login Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);

                IsLoading = false;
                timeLine1.Cursor = Cursors.Default;
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Visible = false;
                showProgress("Ready");
                Application.DoEvents();
                return;
            }

            try
            {
                SalesForceDA.Authenticate();
            }
            catch (Exception ex)
            {
                //Couldn't connect to the database
                MessageBox.Show(
                    "Unable to connect to the Salesforce database\n" + ex.Message,
                    "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);

                IsLoading = false;
                timeLine1.Cursor = Cursors.Default;
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Visible = false;
                showProgress("Ready");
                Application.DoEvents();
                return;
            }

            HideLoginPanel();
            this.Enabled = false;
            LocalStorage.LoadGeometryState(this);
            uiPanel_LeftMenu.Closed = false;
            uiPanel_MiddleBottom.Closed = false;
            LoadData();
            CurrentUser = SalesForceDA.CurrentUser;
            tsl_CurrentUserName.Text = "Welcome " + CurrentUser.DisplayName;
            this.Enabled = true;
            SetMenuBarState();
        }

        private void HideLoginPanel()
        {
            Panel pnl = new Panel();
            pnl.Size = pnl_Login.Size;

            PictureBox pic = new PictureBox();
            pic.Size = pnl_Login.Size;
            pic.Image = (Image)GetImageFromControl(pnl_Login);

            pnl_Login.Parent.Controls.Add(pic);
            pnl_Login.Parent.Controls.Add(pnl);

            pnl_Login.Visible = false;
            double r1 = -115 * Math.PI / 180;
            double r2 = 45 * Math.PI / 180;
            int steps = 100;
            double y1 = Math.Sin(r1);
            double y2 = Math.Sin(r2);
            double scale = Math.Abs(pic.Height / (y2 - y1));
            for (double r = r1; r <= r2; r += (r2 - r1) / steps)
            {
                double y = scale * (y1 - Math.Sin(r));
                pic.Top = (int)y;
                Application.DoEvents();
                System.Threading.Thread.Sleep(2);
            }

            pnl.Visible = false;
            pic.Visible = false;
            pnl = null;
            pic = null;
        }


        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            int dwRop);


        private Bitmap GetImageFromControl(Control control)
        {
            Bitmap controlBmp;
            using (Graphics g1 = control.CreateGraphics())
            {
                controlBmp = new Bitmap(control.Width, control.Height, g1);
                using (Graphics g2 = Graphics.FromImage(controlBmp))
                {
                    IntPtr dc1 = g1.GetHdc();
                    IntPtr dc2 = g2.GetHdc();
                    BitBlt(dc2, 0, 0, control.Width, control.Height, dc1, 0, 0, 13369376);
                    g1.ReleaseHdc(dc1);
                    g2.ReleaseHdc(dc2);
                }
            }
            return controlBmp;
        }


        /// <summary>
        /// Enables the login button once there are values in the username and password
        /// </summary>
        private void LoginValuesChanged(object sender, EventArgs e)
        {
            btn_Login.Enabled = (
                txt_Username.Text.Length > 0 &&
                txt_Password.Text.Length > 0);
        }
        #endregion



        #region Manage the timeline control

        //Initially group by instructor
        private void GroupByInstructor()
        {
            timeLine1.Groups.Clear();
            TimeLineGroup group = new TimeLineGroup(
                timeLine1.Fields["DisplayOrder"],
                Janus.Windows.TimeLine.SortOrder.Ascending);
            this.timeLine1.Groups.Add(group);

            //Make sure the status indicators are displayed on the instructor row headings
            foreach (ScheduleDataSet.EmployeeListRow employeeListRow in scheduleDataSet1.EmployeeList)
                RenderEmployee(employeeListRow);
        }

        //Define timeline formats
        private void SetTimelineFormat()
        {
            //Format the weekends
            TimeLineHourRange weekend = new TimeLineHourRange();
            weekend.DayOfWeek =
                Janus.Windows.TimeLine.DayOfWeek.Saturday |
                Janus.Windows.TimeLine.DayOfWeek.Sunday;

            weekend.FormatStyle.BackColor = GetFormat("DAY", "WEEKEND").GetBackColor();
            timeLine1.WorkingHourSchema.WorkingHoursRange.Add(weekend);

            //Put a vertical line at the beginning of the month
            TimeLineHourRange firstDayOfMonth = new TimeLineHourRange();
            if (!GetFormat("Timeline", "MonthStart").IsBackgroundImageKeyNull())
                firstDayOfMonth.Key = GetFormat("Timeline", "MonthStart").BackgroundImageKey;
            firstDayOfMonth.StartTime = new TimeSpan(0, 0, 0);
            firstDayOfMonth.EndTime = new TimeSpan(1, 0, 0);
            firstDayOfMonth.RecurrencePattern = new WorkingHourRecurrencePattern();
            firstDayOfMonth.RecurrencePattern.Duration = new TimeSpan(1, 0, 0);
            firstDayOfMonth.RecurrencePattern.RecurrenceType = RecurrenceType.Monthly;
            firstDayOfMonth.RecurrencePattern.DayOfMonth = 1;
            firstDayOfMonth.RecurrencePattern.RecurrenceEndMode = RecurrenceEndMode.NoEndDate;
            firstDayOfMonth.FormatStyle.BackColor = GetFormat("Timeline", "MonthStart").GetBackColor();
            firstDayOfMonth.FormatStyle.BackColorGradient = timeLine1.BackColor;
            firstDayOfMonth.FormatStyle.BackgroundGradientMode = BackgroundGradientMode.Horizontal;
            timeLine1.WorkingHourSchema.WorkingHoursRange.Add(firstDayOfMonth);
        }

        //ToDo: Make format conditions work
        private void SetFormatConditions()
        {
            TimeLineFormatCondition barCond = new TimeLineFormatCondition(this.timeLine1.Fields["NumStudents"], ConditionOperator.GreaterThanOrEqualTo, timeLine1.Fields["NumMaxStudents"]);
            barCond.ItemsBarFormatStyle.ForeColor = Color.Green;
            barCond.Key = "Full Class";
        }

        private void RenderEmployeeTimeLineItems(string employeeID)
        {
            RenderEmployeeTimeLineItems(scheduleDataSet1.EmployeeList.FindByEmployeeID(employeeID));
        }

        private void RenderEmployeeTimeLineItems(ScheduleDataSet.EmployeeListRow ee)
        {
            if (ee == null) return;
            foreach (ScheduleDataSet.AppointmentsRow appt in ee.GetAppointmentsRows())
            {
                RenderAppointment(appt);
            }
        }

        // Apply the current data and display options to all TimeLineItems
        private void RenderAllTimeLineItems()
        {
            foreach (TimeLineItem item in timeLine1.Items)
                RenderTimeLineItem(item);
        }

        private void RenderTimeLineItem(TimeLineItem tmlItem)
        {
            RenderAppointment(GetAppointmentFromTimeLineItem(tmlItem));
        }

        // Applies colors, labels etc. to the specified time line item
        private void RenderAppointment(ScheduleDataSet.AppointmentsRow appt)
        {
            //Bail out if we get a bogus item
            if (appt == null || appt.AppointmentCategoriesRow == null) return;

            //Get the data for this item
            TimeLineItem tmlItem = GetTimeLineItemFromAppointment(appt);
            if (tmlItem == null) return;

            String apptType = appt.AppointmentCategoriesRow.Type;
            String apptId = appt.AppointmentCategoriesRow.AppointmentCategoryID;
            ScheduleDataSet.FormatsRow fmt = appt.AppointmentCategoriesRow.FormatsRow;

            tmlItem.FormatStyle.BackColorAlphaMode = AlphaMode.UseAlpha;
            tmlItem.FormatStyle.Alpha = 192;
            tmlItem.FormatStyle.BackgroundImageDrawMode = BackgroundImageDrawMode.Tile;

            //Set the label and tool tip text
            if (appt.AppointmentCategoriesRow.AppointmentCategoryID.StartsWith("HOLIDAY"))
            {
                tmlItem.Text = appt.Status;
                tmlItem.ToolTipText = TimeLineLabel(tre_DisplayOptions.Nodes["TimeLine"].Nodes["Events"].Nodes["ToolTip"], tmlItem).Replace(", ", "\n");
            }
            else if (appt.IsProposed)
            {
                tmlItem.Text = "";
                tmlItem.ToolTipText = TimeLineLabel(tre_DisplayOptions.Nodes["TimeLine"].Nodes["Events"].Nodes["ToolTip"], tmlItem).Replace(", ", "\n");
            }
            else if (appt.ClassType == "Event")
            {
                tmlItem.Text = TimeLineLabel(tre_DisplayOptions.Nodes["TimeLine"].Nodes["Events"].Nodes["Label"], tmlItem);
                tmlItem.ToolTipText = TimeLineLabel(tre_DisplayOptions.Nodes["TimeLine"].Nodes["Events"].Nodes["ToolTip"], tmlItem).Replace(", ", "\n");
            }
            else
            {
                tmlItem.Text = TimeLineLabel(tre_DisplayOptions.Nodes["TimeLine"].Nodes["Classes"].Nodes["Label"], tmlItem);
                tmlItem.ToolTipText = TimeLineLabel(tre_DisplayOptions.Nodes["TimeLine"].Nodes["Classes"].Nodes["ToolTip"], tmlItem).Replace(", ", "\n");
            }

            if (fmt != null)
            {
                tmlItem.FormatStyle.BackColor = fmt.GetBackColor(Color.Transparent);
                tmlItem.FormatStyle.ForeColor = fmt.GetForeColor(Color.Black);
                if (!fmt.IsBackgroundImageKeyNull())
                {
                    tmlItem.FormatStyle.BackgroundImage = fmt.GetBackgroundImage(imageList1);
                    tmlItem.FormatStyle.BackgroundImage.Tag = fmt.BackgroundImageKey;
                }
            }
            //Override the background color to indicate class status
            switch (appt.Status)
            {
                case "Hold":
                    tmlItem.FormatStyle.BackColor = GetFormat("ClassStatus", "Hold").GetBackColor();
                    break;
                case "Tentative":
                    tmlItem.FormatStyle.BackColor = GetFormat("ClassStatus", "Tentative").GetBackColor();
                    break;
                case "Confirmed":
                    tmlItem.FormatStyle.BackColor = IsDateEmpty(appt.WhitePaperSentDate)
                        ? GetFormat("ClassStatus", "ConfirmedNotSent").GetBackColor()
                        : GetFormat("ClassStatus", "ConfirmedSent").GetBackColor();
                    break;
            }

            //Display error/warning indicator if necessary
            if (tre_DisplayOptions.Nodes["TimeLine"].Nodes["Classes"].Nodes["Label"].Nodes["Icon"].Checked && apptId != "PROPOSED")
            {
                String iconName = "";

                if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Violation)
                    iconName += "Warning.";

                if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Error)
                    iconName += "Error.";

                if (appt.RowState == DataRowState.Modified || appt.RowState == DataRowState.Added)
                    iconName += "Modified";
                else
                    iconName += "Unmodified";

                if (appt.ErrorLevel != ScheduleDataSet.ErrorLevels.None)
                    if (tmlItem.EndTime < DateTime.Now)
                        iconName += ".Past";
                    else
                        iconName += ".Future";

                tmlItem.Image = timeLine1.ImageList.Images[iconName];
            }
            else
            {
                tmlItem.Image = null;
            }

            // appt.AppointmentCategoriesRow.AppointmentCategoryID.StartsWith("PROPOSED")

            //Render the class size bar in a darker version of the background color
            if (appt.IsEvent)
            {
                tmlItem.ItemBarFomatStyle.BackColor = Color.Transparent;
                tmlItem.ItemEstimatedBarFormatStyle.BackColor = Color.Transparent;
            }
            else
            {
                tmlItem.ItemBarFomatStyle.BackColor =
                    tmlItem.ItemEstimatedBarFormatStyle.BackColor =
                    AdjustColor(tmlItem.FormatStyle.BackColor, 1, 10);
            }


            //Class Enrollment Bar
            if (!appt.IsMaxStudentsNull())
            {
                int numStudents = appt.NumStudents > appt.NumRegistered ? appt.NumStudents : appt.NumRegistered;

                //If Class is empty
                if (numStudents == 0)
                {
                    tmlItem.EstimatedStartTime = appt.StartDate;
                    tmlItem.EstimatedEndTime = appt.StartDate;
                }
                //If class is partially full
                else if (numStudents < appt.MaxStudents)
                {
                    float percentFull = (float)(numStudents) / appt.MaxStudents;
                    tmlItem.EstimatedStartTime = appt.StartDate;
                    tmlItem.EstimatedEndTime = appt.StartDate.Add(new TimeSpan((long)(tmlItem.Duration.Ticks * percentFull)));
                }
                else
                {
                    tmlItem.EstimatedStartTime = appt.StartDate;
                    tmlItem.EstimatedEndTime = appt.EndDate;
                }
            }


            //Pending Deletion
            if (!appt.IsPendingDeleteNull() && appt.PendingDelete)
            {
                tmlItem.Image = timeLine1.ImageList.Images["DeleteGray"];
                tmlItem.FormatStyle.BackColor = Color.Black;
                tmlItem.FormatStyle.Alpha = 24;
                tmlItem.FormatStyle.BackColorAlphaMode = AlphaMode.UseAlpha;
                tmlItem.ItemBarFomatStyle.BackColor = Color.Black;
                tmlItem.ItemBarFomatStyle.Alpha = 24;
                tmlItem.ItemBarFomatStyle.BackColorAlphaMode = AlphaMode.UseAlpha;
            }
            // RefreshNavMap(true);
            // Application.DoEvents();
        }

        private string GetDescriptionOfTimeLineLocation(MouseEventArgs evt)
        {
            return GetDescriptionOfTimeLineLocation(evt.Location);
        }

        public static bool IsDateEmpty(DateTime d)
        {
            return d == DateTime.MinValue;
        }

        private string GetDescriptionOfTimeLineLocation(Point pt)
        {
            var clickPointDesc = "";
            var clickGroup = timeLine1.GetGroupRowAt(pt);
            var clickDate = timeLine1.GetDateTimeAt(pt).Date;
            clickPointDesc += clickDate.ToLongDateString();
            var holiday = GetHoliday(clickDate);
            if (holiday != null) clickPointDesc += " (" + holiday.Name + ")";
            return (clickPointDesc);
        }

        private String GetDescriptionOfTimeLineItem(TimeLineItem item)
        {
            return GetDescriptionOfTimeLineItem(GetAppointmentFromTimeLineItem(item));
        }

        private string GetDescriptionOfTimeLineItem(ScheduleDataSet.AppointmentsRow appt)
        {
            var clickPointDesc =
                 appt.EmployeeListRow[GetEmployeeNameFieldPreference()] + ", " +
                 appt.StartDate.Date.ToLongDateString();

            var holiday = GetHoliday(appt.StartDate.Date, appt.EndDate.Date);
            if (holiday != null) clickPointDesc += " (" + holiday.Name + ")";

            return (clickPointDesc);
        }

        //Displayes the selected row as "highlighted"
        private void HighlightRow(TimeLineGroupRow gr)
        {
            if (SelectedGroupRow != null) SelectedGroupRow.FormatStyle.BackColor = Color.Empty;
            SelectedGroupRow = gr;
            SelectedGroupRow.FormatStyle.BackColor = GetFormat("Timeline", "RowHighlight").GetBackColor();
        }

        //Validate the whole schedule
        private void ValidateSchedule()
        {
            if (IsLoading) return;
            ArrayList employeeIDs = new ArrayList();
            foreach (TimeLineGroupRow row in GetVisibleGroups().Values)
            {
                int displayOrder;
                if (int.TryParse(row.Value?.ToString(), out displayOrder))
                    employeeIDs.Add(GetEmployeeIdFromDisplayOrder(displayOrder));
            }

            if (employeeIDs.Count > 0)
                ValidateSchedule(employeeIDs);
        }

        private void ValidateSchedule(ArrayList employeeIDs)
        {
            if (IsLoading) return;
            if (employeeIDs == null) return;

            this.toolStripProgressBar1.Maximum = employeeIDs.Count;
            this.toolStripProgressBar1.Value = 0;
            this.toolStripProgressBar1.Visible = true;
            foreach (string employeeID in employeeIDs)
            {
                ScheduleDataSet.EmployeeListRow ee = scheduleDataSet1.EmployeeList.FindByEmployeeID(employeeID);
                showProgress("Validating " + ee[GetEmployeeNameFieldPreference()]);
                ValidateEmployeeSchedule(ee);
                RenderEmployeeTimeLineItems(ee);
                RenderEmployee(ee);
                RefreshNavMap(true);
            }
            showProgress("Ready");
            this.toolStripProgressBar1.Visible = false;
            Log("Validation complete");
        }

        private void ValidateEmployeeSchedule(string employeeID)
        {
            ValidateEmployeeSchedule(scheduleDataSet1.EmployeeList.FindByEmployeeID(employeeID));
        }

        private void ValidateEmployeeSchedule(ScheduleDataSet.EmployeeListRow employeeListRow)
        {
            if (employeeListRow == null) return;
            string employeeID = employeeListRow.EmployeeID;
            DateTime firstApptDate = DateTime.MaxValue;
            DateTime lastApptDate = DateTime.MinValue;
            DateTime dateFirstQualified = DateTime.MaxValue;

            SetMenuBarState();

            // Clear all violation indicators for this employee:
            foreach (ScheduleDataSet.AppointmentsRow appt in employeeListRow.GetAppointmentsRows())
            {
                if (appt.StartDate < firstApptDate) firstApptDate = appt.StartDate;
                if (appt.StartDate > lastApptDate) lastApptDate = appt.StartDate;
                //Remove the violation thingies
                if (appt.IsProposed)
                {
                    appt.Delete();
                }
                else if (appt.GetAppointmentNotificationsRows().Length > 0)
                {
                    //appt.ClearErrors();
                    foreach (ScheduleDataSet.AppointmentNotificationsRow note in appt.GetAppointmentNotificationsRows())
                    {
                        note.Delete();
                        note.AcceptChanges();
                    }
                    if (!DirtyAppointments.Contains(appt))
                        DirtyAppointments.Enqueue(appt);
                }
            }

            //If this is the "Unassigned" employee, or if we didn't find any apointments 
            // for this employee then just render the row and bail out

            if (employeeListRow.DisplayOrder <= 1000 && firstApptDate != DateTime.MaxValue && lastApptDate != DateTime.MinValue)
            {

                //Find the first date this instructor was qualified for anything:
                ScheduleDataSet.EmployeeCourseQualificationsRow[] rows =
                    ((ScheduleDataSet.EmployeeCourseQualificationsRow[])
                    (scheduleDataSet1.EmployeeCourseQualifications.Select("EmployeeID='" + employeeID + "'", "StartDate")));

                if (rows != null && rows.Length > 0)
                    dateFirstQualified = rows[0].StartDate;

                //Course Qualifications
                foreach (ScheduleDataSet.AppointmentsRow appt in employeeListRow.GetAppointmentsRows())
                {
                    if (appt.IsEvent) break;
                    int qualificationLevel = getEmployeeCourseQualificationLevel(employeeListRow, appt.AppointmentCategoriesRow, appt.StartDate);
                    if (qualificationLevel <= 0)
                        showViolation(
                            employeeListRow.EmployeeID,
                            appt.StartDate,
                            appt.EndDate,
                            "UNQ",
                            ScheduleDataSet.ErrorLevels.Error,
                            "Not Qualified",
                            "This instructor is not qualified to teach this course on this date");
                    //showViolation(
                    //      employeeListRow.EmployeeID,
                    //      appt.StartDate,
                    //      "UNQ",
                    //      ScheduleDataSet.ErrorLevels.Error,
                    //      "Not Qualified",
                    //      "This instructor is not qualified to teach this course on this date");
                }

                //String SequenceLengthWarningMessage =
                //    "Scheduling a work assignment here would result in {0} work assignments in a row.  " +
                //    "The maximum is {1} work assignments in a row.";
                String SequenceLengthErrorMessage =
                    "There are {0} work assignments scheduled in a row.  " +
                    "The maximum is {1} work assignment(s) in a row.";
                String UtilizationWarningMessage =
                    "Scheduling a work assignment here would result in {0} work assignments in a {1} week period.  " +
                    "This person has specified a maxium of {2} work assignment in a {1} week period.";
                String UtilizationErrorMessage =
                    "There are {0} work assignments in a {1} week period.  " +
                    "This person has specified a maxium of {2} work assignment in a {1} week period.";

                //Load the general and employee specific scheduling factors:
                int maxSequentialWorkWeeks = 2;
                int MaxWorkWeeksInPeriod = 0;
                int WeeksInPeriod = 0;
                int sequentialWorkWeeks = 1;
                DateTime minDate = timeLine1.MinDate.Date;
                DateTime maxDate = timeLine1.MaxDate.Date;

                //These two lines are to validate the entire schedule instead of just the visible part
                //minDate = firstApptDate;
                //maxDate = lastApptDate;

                DateTime firstDate = minDate
                    .AddDays(0 - ((int)(minDate.DayOfWeek)))
                    .AddDays(-7 * maxSequentialWorkWeeks);
                DateTime lastDate = maxDate
                    .AddDays(8 - ((int)(maxDate.DayOfWeek)))
                    .AddDays(7 * maxSequentialWorkWeeks);
                int numWeeks = ((int)((TimeSpan)(lastDate.Subtract(firstDate))).TotalDays / 7) + 1;

                //Fill an array where each element corresponds to a week of this employee's schedule
                //containing a true for any week they are scheduled to work:
                //BitArray isWorkingArray = new BitArray(numWeeks);
                String[] workMode = new String[numWeeks];

                //Fill in exclusive holidays
                foreach (ScheduleDataSet.AppointmentsRow appt in AllB2TEmployee.GetAppointmentsRows())
                {
                    if (appt.AppointmentCategoryID == "HOLIDAY" || appt.AppointmentCategoryID == "NOTAVAILABLE")
                    {
                        //mark each week contained in the appt as working. Because a holiday might span more than one week
                        for (DateTime d = appt.StartDate.Date; d <= appt.EndDate.Date; d = d.AddDays(7))
                        {
                            int idx = ((int)((TimeSpan)(d.Subtract(firstDate))).TotalDays / 7) + 0;
                            if (idx >= 0 && idx < numWeeks)
                            {
                                workMode[idx] = "h";
                            }
                        }
                    }
                }


                foreach (ScheduleDataSet.AppointmentsRow appt in employeeListRow.GetAppointmentsRows())
                {
                    if (!appt.IsDeletedNull() && appt.Deleted) continue;
                    if (!appt.IsPendingDeleteNull() && appt.PendingDelete) continue;
                    if (appt.StartDate < firstDate) continue;
                    if (appt.StartDate > lastDate) continue;
                    if (appt.AppointmentCategoriesRow == null) continue;

                    //mark each week contained in the appt as working. Because an appt might span more than one week
                    for (DateTime d = appt.StartDate.Date; d <= appt.EndDate.Date; d = d.AddDays(7 - (int)(d.DayOfWeek)))
                    {
                        int idx = (int)((TimeSpan)(d.Subtract(firstDate))).TotalDays / 7;
                        if (idx > 0 && idx < numWeeks)
                        {
                            if (appt.AppointmentCategoriesRow.IsWorking == false && appt.AppointmentCategoriesRow.IsExclusive == true)
                                workMode[idx] = "o";
                            if (appt.AppointmentCategoriesRow.IsWorking == true)
                                workMode[idx] = "w";
                        }
                    }
                }



                //Iterate over each week of the schedule determining what to put in each week
                for (int weekIdx = 0; weekIdx < numWeeks; weekIdx++)
                {
                    // string msg = "";

                    //Evaluate BR1: Max Actual classes in a row
                    sequentialWorkWeeks = 1;
                    for (int weekOffset = weekIdx - 1; weekOffset >= 0 && workMode[weekOffset] == "w"; weekOffset--)
                        sequentialWorkWeeks++;
                    for (int weekOffset = weekIdx + 1; weekOffset < numWeeks && workMode[weekOffset] == "w"; weekOffset++)
                        sequentialWorkWeeks++;

                    //Display the indication of a sequence length error if necessary
                    if (sequentialWorkWeeks > maxSequentialWorkWeeks)
                    {
                        String summary = "";
                        ScheduleDataSet.ErrorLevels severity = ScheduleDataSet.ErrorLevels.None;

                        if (workMode[weekIdx] == "w" && firstDate.AddDays(weekIdx * 7) >= DateTime.Today)
                        {
                            summary = "Instructor Has Too Many Assignments in a Row";
                            severity = ScheduleDataSet.ErrorLevels.Violation;
                        }
                        else if (workMode[weekIdx] == "w")
                        {
                            summary = "Instructor Had Too Many Assignments in a Row";
                            severity = ScheduleDataSet.ErrorLevels.Violation;
                        }
                        else if (workMode[weekIdx] == "" && firstDate.AddDays(weekIdx * 7) >= DateTime.Today)
                        {
                            summary = "Instructor Would Have Too Many Assignments in a Row";
                            severity = ScheduleDataSet.ErrorLevels.PotentialViolation;
                        }
                        else if (workMode[weekIdx] == "")
                        {
                            summary = "Instructor Would Have Had Too Many Assignments in a Row";
                            severity = ScheduleDataSet.ErrorLevels.PotentialViolation;
                        }

                        if (severity > ScheduleDataSet.ErrorLevels.None)
                        {
                            showViolation(
                                employeeID,
                                firstDate.AddDays(weekIdx * 7),
                                "SLE",
                                severity,
                                summary,
                                String.Format(
                                    SequenceLengthErrorMessage,
                                    sequentialWorkWeeks,
                                    maxSequentialWorkWeeks));
                        }
                    }

                    //Fetch this employee's schedule factors in effect on weekIdx 
                    ScheduleDataSet.EmployeeScheduleFactorsRow esfr = getEmployeeSchedulePattern(employeeListRow, firstDate.AddDays(weekIdx * 7));
                    if (esfr != null)
                    {
                        MaxWorkWeeksInPeriod = esfr.SchedulePatternsRow.WorkWeeksInPeriod;
                        WeeksInPeriod = esfr.SchedulePatternsRow.WeeksInPeriod;

                        int workWeeksInPeriod = 0;
                        for (int weekOffset = 1 - WeeksInPeriod; weekOffset <= 0; weekOffset++)
                        {
                            int workWeeksThisFrame = 0;
                            for (int k = 0; k < WeeksInPeriod; k++)
                                if (weekIdx + weekOffset + k >= 0 && weekIdx + weekOffset + k < numWeeks)
                                    if (workMode[weekIdx + weekOffset + k] == "w")
                                        workWeeksThisFrame++;
                            workWeeksInPeriod = Math.Max(workWeeksInPeriod, workWeeksThisFrame);
                        }

                        if (workMode[weekIdx] == "w" && workWeeksInPeriod > MaxWorkWeeksInPeriod && firstDate.AddDays(weekIdx * 7) > DateTime.Today)
                        {
                            showViolation(
                                employeeID,
                                firstDate.AddDays(weekIdx * 7),
                                "OUE",
                                ScheduleDataSet.ErrorLevels.Violation,
                                "Instructor Scheduled to be Over-Utilized",
                                String.Format(
                                    UtilizationErrorMessage,
                                    workWeeksInPeriod,
                                    WeeksInPeriod,
                                    MaxWorkWeeksInPeriod));
                            continue;
                        }

                        if (workMode[weekIdx] == "w" && workWeeksInPeriod > MaxWorkWeeksInPeriod)
                        {
                            showViolation(
                                employeeID,
                                firstDate.AddDays(weekIdx * 7),
                                "OUE",
                                ScheduleDataSet.ErrorLevels.Violation,
                                "Instructor Was Over-Utilized",
                                String.Format(
                                    UtilizationErrorMessage,
                                    workWeeksInPeriod,
                                    WeeksInPeriod,
                                    MaxWorkWeeksInPeriod));
                            continue;
                        }

                        if (workMode[weekIdx] != "w" && workWeeksInPeriod >= MaxWorkWeeksInPeriod && firstDate.AddDays(weekIdx * 7) > DateTime.Today)
                        {
                            showViolation(
                                employeeID,
                                firstDate.AddDays(weekIdx * 7),
                                "OUE",
                                ScheduleDataSet.ErrorLevels.PotentialViolation,
                                "Instructor Would Be Over-Utilized",
                                String.Format(
                                    UtilizationWarningMessage,
                                    workWeeksInPeriod + 1,
                                    WeeksInPeriod,
                                    MaxWorkWeeksInPeriod));
                            continue;
                        }

                        if (workMode[weekIdx] != "w" && workWeeksInPeriod >= MaxWorkWeeksInPeriod)
                        {
                            showViolation(
                                employeeID,
                                firstDate.AddDays(weekIdx * 7),
                                "OUE",
                                ScheduleDataSet.ErrorLevels.PotentialViolation,
                                "Instructor Would Have Been Over-Utilized",
                                String.Format(
                                    UtilizationWarningMessage,
                                    workWeeksInPeriod + 1,
                                    WeeksInPeriod,
                                    MaxWorkWeeksInPeriod));
                            continue;
                        }
                    }


                    //Do the same as above looking for a's instead of w/s
                    if (workMode[weekIdx] == null)
                    {

                        //Count sequential weeks:
                        sequentialWorkWeeks = 1;
                        for (int weekOffset = weekIdx - 1; weekOffset >= 0 && (workMode[weekOffset] == "a" || workMode[weekOffset] == "w"); weekOffset--)
                            sequentialWorkWeeks++;
                        for (int weekOffset = weekIdx + 1; weekOffset < numWeeks && (workMode[weekOffset] == "a" || workMode[weekOffset] == "w"); weekOffset++)
                            sequentialWorkWeeks++;

                        //Display the indication of a sequence length error if necessary
                        if (sequentialWorkWeeks > maxSequentialWorkWeeks)
                        {
                            showViolation(
                                employeeID,
                                firstDate.AddDays(weekIdx * 7),
                                "PSW",
                                ScheduleDataSet.ErrorLevels.PotentialRecalc,
                                "Proposed Off to Prevent Too Many Assignments in a Row",
                                "A work activity may be scheduled here without violating any rules, " +
                                "but doing so will recalculate the \"Proposed Available\" dates to " +
                                "avoid scheduling too many work activities in a row.");
                            workMode[weekIdx] = "v";
                        }

                        //Count the number of working weeks in the preferred period
                        if (MaxWorkWeeksInPeriod > 0)
                        {
                            int availableWeeksInPeriod = 0;
                            for (int weekOffset = 1 - WeeksInPeriod; weekOffset <= 0; weekOffset++)
                            {
                                int availableWeeksThisFrame = 0;
                                for (int k = 0; k < WeeksInPeriod; k++)
                                    if (weekIdx + weekOffset + k >= 0 && weekIdx + weekOffset + k < numWeeks)
                                        if (workMode[weekIdx + weekOffset + k] == "a")
                                            availableWeeksThisFrame++;
                                availableWeeksInPeriod = Math.Max(availableWeeksInPeriod, availableWeeksThisFrame);
                            }

                            if (availableWeeksInPeriod >= MaxWorkWeeksInPeriod)
                            {
                                showViolation(
                                    employeeID,
                                    firstDate.AddDays(weekIdx * 7),
                                    "PUW",
                                    ScheduleDataSet.ErrorLevels.PotentialRecalc,
                                    "Proposed Off to prevent Over-Utilization",
                                    "A work activity may be scheduled here without violating any rules, " +
                                    "but doing so will recalculate the \"Proposed Available\" dates to " +
                                    "avoid exceeding the instructor's utilization parameters.");
                                workMode[weekIdx] = "v";
                            }
                        }

                        //If we dont have any errors or warnings, then this week must be available
                        if (workMode[weekIdx] == null && firstDate.AddDays(weekIdx * 7) >= dateFirstQualified)
                        {
                            showAvailable(employeeID, firstDate.AddDays(weekIdx * 7), "");
                            workMode[weekIdx] = "a";
                        }
                    }
                }



                // Look for overlapping appointments
                foreach (ScheduleDataSet.AppointmentsRow appt in employeeListRow.GetAppointmentsRows())
                {
                    if (!appt.IsDeletedNull() && appt.Deleted) continue;
                    if (!appt.IsPendingDeleteNull() && appt.PendingDelete) continue;
                    if (appt.StartDate < timeLine1.MinDate) continue;
                    if (appt.StartDate > timeLine1.MaxDate) continue;
                    //Overlapping error occurs when exclusive appointments overlap.
                    if (appt.AppointmentCategoriesRow != null && appt.AppointmentCategoriesRow.IsExclusive && appt.AppointmentCategoriesRow.CategoryName != "Off")
                    {
                        //The only exception is "Off" and "NotAvailable" are not red-alerts
                        Boolean apptIsOffOrNotAvailable = (appt.AppointmentCategoriesRow.CategoryName == "Not Available" || appt.AppointmentCategoriesRow.CategoryName == "Off");
                        foreach (ScheduleDataSet.AppointmentsRow overlappingAppt in FindAppointments(employeeID, appt.StartDate, appt.EndDate, IsWorkingFilter.DontCare, IsExclusiveFilter.Exclusive))
                        {
                            if (appt.Equals(overlappingAppt))
                                continue;

                            if (overlappingAppt.AppointmentCategoriesRow == null || overlappingAppt.AppointmentCategoriesRow.CategoryName == "Off")
                                continue;

                            if (appt.AppointmentCategoriesRow.AppointmentCategoryID == "FORCEDUNAVAILABLE")
                                continue;

                            if (overlappingAppt.AppointmentCategoriesRow.AppointmentCategoryID == "FORCEDUNAVAILABLE")
                                addNotificationToAppointment(
                                    appt,
                                    "AUE",
                                    ScheduleDataSet.ErrorLevels.Violation,
                                    "Assigned while Unavailable",
                                    "Work assignment conflicts with a previously established unavailable date.");
                            else
                                addNotificationToAppointment(
                                    appt,
                                    "OLE",
                                    ScheduleDataSet.ErrorLevels.Error,
                                    "Overlapping Assignments",
                                    "Personnel cannot be assigned to multiple work assignments at the same time.");
                            break;
                        }

                        //Warning if class overlaps a weekend:
                        if (appt.AppointmentCategoriesRow != null &&
                            appt.AppointmentCategoriesRow.IsWorking &&
                            (appt.StartDate.DayOfWeek == System.DayOfWeek.Saturday ||
                            appt.StartDate.DayOfWeek == System.DayOfWeek.Sunday ||
                            appt.EndDate.AddMilliseconds(-1).DayOfWeek == System.DayOfWeek.Saturday ||
                            appt.EndDate.AddMilliseconds(-1).DayOfWeek == System.DayOfWeek.Sunday ||
                            ((int)(appt.StartDate.DayOfWeek) + appt.StartDate.Subtract(appt.EndDate).TotalDays > 7)
                            ))
                        {
                            addNotificationToAppointment(
                                appt,
                                "WEE",
                                ScheduleDataSet.ErrorLevels.Violation,
                                "Working Weekend",
                                "This work assignment is scheduled to include a weekend.  " +
                                "Work assignments should only be scheduled on week days");
                        }

                        //Warning if class overlaps a holiday:
                        if (appt.AppointmentCategoriesRow != null && appt.AppointmentCategoriesRow.IsWorking)
                        {
                            var holiday = GetHoliday(appt.StartDate, appt.EndDate);
                            if (holiday != null)
                                addNotificationToAppointment(
                                    appt,
                                    "WEE",
                                    ScheduleDataSet.ErrorLevels.Violation,
                                    "Working Holiday",
                                    "This work assignment is scheduled on " + (holiday.Name == "" ? "A Holiday" : holiday.Name));
                        }
                    }
                }
            }


            while (DirtyAppointments.Count > 0)
            {
                ScheduleDataSet.AppointmentsRow dirtyAppt = ((ScheduleDataSet.AppointmentsRow)DirtyAppointments.Dequeue());
                RenderAppointment(dirtyAppt);
            }
            //foreach (ScheduleDataSet.AppointmentsRow appt in employeeListRow.GetAppointmentsRows())
            //    if (appt.NeedsPaint)
            //        RenderAppointment(appt);
            RenderEmployee(employeeListRow);
        }

        public ScheduleDataSet.HolidaysRow GetHoliday(DateTime startDate, DateTime endDate)
        {
            foreach (ScheduleDataSet.HolidaysRow holiday in scheduleDataSet1.Holidays.Rows)
            {
                if (startDate >= holiday.StartDate && startDate < holiday.EndDate)
                    return holiday;
                if (endDate > holiday.StartDate && endDate <= holiday.EndDate)
                    return holiday;
                if (startDate <= holiday.StartDate && endDate >= holiday.EndDate)
                    return holiday;
            }
            return null;
        }

        public ScheduleDataSet.HolidaysRow GetHoliday(DateTime date)
        {
            if (date == DateTime.MinValue) return null;
            foreach (ScheduleDataSet.HolidaysRow holiday in scheduleDataSet1.Holidays.Rows)
                if (date >= holiday.StartDate && date < holiday.EndDate)
                    return holiday;
            return null;
        }

        private int getEmployeeCourseQualificationLevel(ScheduleDataSet.EmployeeListRow employeeListRow, ScheduleDataSet.AppointmentCategoriesRow appointmentCategoriesRow, DateTime effectiveDate)
        {
            int maxQualificationLevel = 0;
            foreach (ScheduleDataSet.EmployeeCourseQualificationsRow row in employeeListRow.GetEmployeeCourseQualificationsRows())
                if (row.AppointmentCategoriesRow.AppointmentCategoryID == appointmentCategoriesRow.AppointmentCategoryID)
                    if (row.IsStartDateNull() || effectiveDate >= row.StartDate)
                        if (row.IsEndDateNull() || effectiveDate <= row.EndDate)
                            if (row.QualificationLevel > maxQualificationLevel)
                                maxQualificationLevel = row.QualificationLevel;
            return maxQualificationLevel;
        }

        private ScheduleDataSet.EmployeeScheduleFactorsRow getEmployeeSchedulePattern(ScheduleDataSet.EmployeeListRow employeeListRow, DateTime effectiveDate)
        {
            //Fetch this employee's schedule factors
            if (employeeListRow.GetEmployeeScheduleFactorsRowsByFK_EmployeeList_EmployeeScheduleFactors().Length == 0)
                return null;
            //Find the one with the most recent effective date that is less than or equal to the specified date
            DateTime selectedEffectiveDate = DateTime.MinValue;
            ScheduleDataSet.EmployeeScheduleFactorsRow selectedRow = null;
            foreach (ScheduleDataSet.EmployeeScheduleFactorsRow row in employeeListRow.GetEmployeeScheduleFactorsRowsByFK_EmployeeList_EmployeeScheduleFactors())
            {
                if (row.EffectiveDate > selectedEffectiveDate && row.EffectiveDate <= effectiveDate)
                {
                    selectedEffectiveDate = row.EffectiveDate;
                    selectedRow = row;
                }
            }
            return selectedRow;
        }

        private void showAvailable(string employeeID, DateTime violationDate, string reason)
        {
            DateTime startDate = violationDate.AddDays(1 - (int)(violationDate.DayOfWeek));
            DateTime endDate = startDate.AddDays(5);
            showAvailable(employeeID, startDate, endDate, reason);
        }

        private void showAvailable(string employeeID, DateTime startDate, int numDays, string reason)
        {
            showAvailable(employeeID, startDate, startDate.AddDays(numDays), reason);
        }

        private void showAvailable(string employeeID, DateTime startDate, DateTime endDate, string reason)
        {
            String appointmentID = CreateGUID();
            ScheduleDataSet.EmployeeListRow employeeListRow =
                scheduleDataSet1.EmployeeList.FindByEmployeeID(employeeID);
            ScheduleDataSet.AccountsRow accountsRow =
                scheduleDataSet1.Accounts.FindByAccountID("0");
            ScheduleDataSet.ClassLocationsRow classLocationsRow =
                scheduleDataSet1.ClassLocations.FindByClassLocationID("0");
            ScheduleDataSet.AppointmentCategoriesRow appointmentCategoriesRow =
                scheduleDataSet1.AppointmentCategories.FindByAppointmentCategoryID("PROPOSEDAVAILABLE");
            Console.WriteLine(appointmentID);
            ScheduleDataSet.AppointmentsRow appt = scheduleDataSet1.Appointments.AddAppointmentsRow(
                appointmentID,
                employeeListRow,
                accountsRow,
                classLocationsRow,
                "Available",
                "Available",
                reason,
                startDate,
                endDate,
                appointmentCategoriesRow,
                null,
                null,
                0,
                0,
                new DateTime(),
                "",
                new DateTime(0),
                new DateTime(0).AddDays(1),
                employeeListRow.DisplayOrder,
                "Violation", new DateTime(0), 0, 0, "", "",
                "", "", "", "", "", "", "", "", "", "", false,
                "", "", "", "", "", "", "", 0, false, false,
                scheduleDataSet1.EmployeeList.FindByEmployeeID(CurrentUser.Id), DateTime.MinValue);

            appt.AcceptChanges();
            if (!DirtyAppointments.Contains(appt))
                DirtyAppointments.Enqueue(appt);
            // appt.NeedsPaint = true;

            addNotificationToAppointment(appt, "AVL", ScheduleDataSet.ErrorLevels.Info, "Proposed Available", "This is an optimum date to schedule a work assignment");

        }

        private void addNotificationToAppointment(ScheduleDataSet.AppointmentsRow appt, string ruleID, ScheduleDataSet.ErrorLevels severity, string summary, string detail)
        {
            ScheduleDataSet.AppointmentNotificationsRow note =
                scheduleDataSet1.AppointmentNotifications.AddAppointmentNotificationsRow(
                    appt,
                    ruleID,
                    (short)severity,
                    summary,
                    detail);
            note.AcceptChanges();
            if (!DirtyAppointments.Contains(appt))
                DirtyAppointments.Enqueue(appt);
            //appt.NeedsPaint = true;
        }

        private void showViolation(string employeeID, DateTime violationDate, string ruleID, ScheduleDataSet.ErrorLevels severity, string summary, string detail)
        {
            //Find the Monday through Friday (5 days) of the week of the violation date
            DateTime startDate = violationDate.AddDays(1 - (int)(violationDate.DayOfWeek));
            DateTime endDate = startDate.AddDays(5);

            // Sunday through Saturday (7 days)
            // violationDate.AddDays(0 - (int)(violationDate.DayOfWeek)),
            // violationDate.AddDays(7 - (int)(violationDate.DayOfWeek)),

            showViolation(employeeID, startDate, endDate, ruleID, severity, summary, detail);

        }

        private void showViolation(string employeeID, DateTime startDate, int numDays, string ruleID, ScheduleDataSet.ErrorLevels severity, string summary, string detail)
        {
            showViolation(employeeID, startDate, startDate.AddDays(numDays), ruleID, severity, summary, detail);
        }

        private void showViolation(string employeeID, DateTime startDate, DateTime endDate, string ruleID, ScheduleDataSet.ErrorLevels severity, string summary, string detail)
        {
            //If an appointment(s) already exist on this day, mark 'em othwise create a violation appointment
            ArrayList appts = FindAppointments(
                employeeID,
                startDate,
                endDate,
                IsWorkingFilter.DontCare,
                IsExclusiveFilter.DontCare);
            if (appts.Count > 0)
            {
                foreach (ScheduleDataSet.AppointmentsRow violatingAppt in appts)
                {
                    addNotificationToAppointment(violatingAppt, ruleID, severity, summary, detail);
                    if (!DirtyAppointments.Contains(violatingAppt))
                        DirtyAppointments.Enqueue(violatingAppt);
                    //violatingAppt.NeedsPaint = true;
                }
                return;
            }

            var appointmentID = CreateGUID();
            var employeeListRow = scheduleDataSet1.EmployeeList.FindByEmployeeID(employeeID);
            var accountsRow = scheduleDataSet1.Accounts.FindByAccountID("0");
            var classLocationsRow = scheduleDataSet1.ClassLocations.FindByClassLocationID("0");
            ScheduleDataSet.AppointmentCategoriesRow appointmentCategoriesRow;

            if (severity <= ScheduleDataSet.ErrorLevels.PotentialRecalc)
                appointmentCategoriesRow = scheduleDataSet1.AppointmentCategories.FindByAppointmentCategoryID("PROPOSEDOFF");
            else
                appointmentCategoriesRow = scheduleDataSet1.AppointmentCategories.FindByAppointmentCategoryID("PROPOSEDOFFVIOLATION");

            ScheduleDataSet.AppointmentsRow appt = scheduleDataSet1.Appointments.AddAppointmentsRow(
                appointmentID,
                employeeListRow,
                accountsRow,
                classLocationsRow,
                "Off",
                "Off",
                detail,
                startDate,
                endDate,
                appointmentCategoriesRow,
                null,
                null,
                0,
                0,
                new DateTime(),
                "",
                new DateTime(0),
                new DateTime(0).AddDays(1),
                employeeListRow.DisplayOrder,
                "Violation", new DateTime(0), 0, 0, "", "",
                "", "", "", "", "", "", "", "", "", "", false,
                "", "", "", "", "", "", "", 0, false, false,
                scheduleDataSet1.EmployeeList.FindByEmployeeID(CurrentUser.Id),
                DateTime.MinValue);

            appt.AcceptChanges();
            if (!DirtyAppointments.Contains(appt))
                DirtyAppointments.Enqueue(appt);
            // appt.NeedsPaint = true;
            addNotificationToAppointment(appt, ruleID, severity, summary, detail);
        }

        private ArrayList FindAppointments(string employeeID, DateTime startDate, DateTime endDate, IsWorkingFilter isWorking, IsExclusiveFilter isExclusive)
        {
            bool w = isWorking == IsWorkingFilter.Working ? true : false;
            bool e = isExclusive == IsExclusiveFilter.Exclusive ? true : false;
            ArrayList appts = new ArrayList();
            foreach (ScheduleDataSet.AppointmentsRow appt in scheduleDataSet1.EmployeeList.FindByEmployeeID(employeeID).GetAppointmentsRows())
            {
                if (!appt.IsDeletedNull() && appt.Deleted) continue;
                if (!appt.IsPendingDeleteNull() && appt.PendingDelete) continue;
                if (appt.EndDate <= startDate) continue;
                if (appt.StartDate >= endDate) continue;
                if (appt.AppointmentCategoriesRow == null) continue;
                if (isWorking == IsWorkingFilter.DontCare || appt.AppointmentCategoriesRow.IsWorking == w)
                    if (isExclusive == IsExclusiveFilter.DontCare || appt.AppointmentCategoriesRow.IsExclusive == e)
                        appts.Add(appt);
            }
            return appts;
        }

        private TimeLineGroupRow FindEmployee(string employeeID)
        {
            foreach (TimeLineGroupRow g in timeLine1.GetGroupRows())
                if (g.Value.ToString() == employeeID)
                    return g;
            return null;
        }

        private void RenderEmployee(ScheduleDataSet.EmployeeListRow ee)
        {
            TimeLineGroupRow eeRowHead = FindEmployee(ee.DisplayOrder.ToString());
            if (eeRowHead == null)
                return;
            else if (!tre_DisplayOptions.Nodes["TimeLine"].Nodes["Classes"].Nodes["Label"].Nodes["Icon"].Checked)
                eeRowHead.Image = timeLine1.ImageList.Images[ee.Image];
            else if (ee.DisplayOrder == 1001)
                eeRowHead.Image = timeLine1.ImageList.Images["People"];
            else if (ee.DisplayOrder == 1002)
                eeRowHead.Image = timeLine1.ImageList.Images["PersonUnknown"];
            else if (ee.ErrorLevel == ScheduleDataSet.ErrorLevels.Violation)
                eeRowHead.Image = timeLine1.ImageList.Images["PersonWarning"];
            else if (ee.ErrorLevel == ScheduleDataSet.ErrorLevels.Error)
                eeRowHead.Image = timeLine1.ImageList.Images["PersonError"];
            else
                eeRowHead.Image = timeLine1.ImageList.Images[ee.Image];
        }

        #region timeline callbacks

        //When an appointment was dragged and dropped within the timeline
        private void timeLine1_ItemDropped(object sender, ItemEventArgs e)
        {

            ScheduleDataSet.AppointmentsRow appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(e.Item.DataRow)).Row);
            if (appt == null) return;
            if (appt.IsEmployeeIDNull()) return;

            //If Dragging an automatic off calc, then change it to "Unavailable"
            if (appt.AppointmentCategoriesRow.Type == "Violation" && appt.AppointmentCategoriesRow.CategoryName == "Off")
            {
                appt.AppointmentCategoriesRow = scheduleDataSet1.AppointmentCategories.FindByAppointmentCategoryID("NOTAVAILABLE");
            }

            string oldEmployeeID = appt.EmployeeID.ToString();
            string newEmployeeID = GetEmployeeIdFromDisplayOrder(e.Item.ParentRow.Value.ToString());
            RenderTimeLineItem(e.Item);
            ValidateEmployeeSchedule(oldEmployeeID);
            if (oldEmployeeID != newEmployeeID)
            {
                appt.EmployeeID = newEmployeeID;
                ValidateEmployeeSchedule(newEmployeeID);
            }

            RefreshNavMap(true);

            //SetDirty(true);
        }


        //When the user clicks anywhere on the timeline
        private void timeLine1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            LastClickPoint = me.Location;

            DateTime clickDate = timeLine1.GetDateTimeAt(me.Location).Date;

            TimeLineGroupRow tlgr = timeLine1.GetGroupRowAt();
            if (tlgr != null)
            {
                //Highlight the timeline row
                HighlightRow(tlgr);

                //Figure out which employee is on the row that was clicked
                SelectedEmployee = scheduleDataSet1.EmployeeList.FindByEmployeeID(GetEmployeeIdFromDisplayOrder(tlgr.Value.ToString()));

                //Tell the instructor editor to load itself
                if (SelectedEmployee != null)
                {
                    uiPanel_EmployeeEditor3.Text = SelectedEmployee[GetEmployeeNameFieldPreference()].ToString();
                    instructorEditor1.Show(SelectedEmployee.EmployeeID, clickDate, clickDate.AddDays(1));
                    instructorEditor1.Employee = SelectedEmployee;
                }
            }

            //Set the text of the editor tab
            //            uiPanel_AppointmentEditor.Text = GetDescriptionOfTimeLineLocation((MouseEventArgs)e);

            switch (timeLine1.HitTest(me.Location))
            {
                case TimeLineArea.TimeLineItem:
                    uiPanel_AppointmentEditor.Visible = true;
                    uiPanel_AppointmentEditor.Text = GetDescriptionOfTimeLineLocation(me);
                    ShowEditor();
                    break;
                case TimeLineArea.RowHeader:
                    uiPanel_AppointmentEditor.Text = "";
                    uiPanel_AppointmentEditor.Visible = false;
                    timeLine1.SelectedItems.Clear();
                    ShowEditor();
                    instructorEditor1.Show(SelectedEmployee.EmployeeID);
                    break;
                case TimeLineArea.Background:
                    timeLine1.SelectedItems.Clear();
                    uiPanel_AppointmentEditor.Text = GetDescriptionOfTimeLineLocation(me);
                    //ShowFreePanel();
                    ShowEditor();
                    break;
            }
        }

        private void timeLine1_DraggingItem(object sender, DraggingItemEventArgs e)
        {

            ScheduleDataSet.AppointmentsRow appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(e.Item.DataRow)).Row);
            if (appt == null || appt.AppointmentCategoriesRow == null)
            {
                e.Cancel = true;
                return;
            }

            //for now, I'm only allowing dragging of events, not classes or anything else
            String apptType = appt.AppointmentCategoriesRow.Type;
            if (apptType != "Event")
            {
                e.Cancel = true;
                return;
            }

            //Who is being dragged over
            TimeLineGroupRow targetRow = e.GroupRow;
            String employeeID = GetEmployeeIdFromDisplayOrder(targetRow.Value.ToString());

            //Change the editor title bar
            String locationDescription = GetDescriptionOfTimeLineItem(appt);

            //Display the changing values in the appropriate editor
            if (apptType == "Event" || apptType == "Violation")
            {
                uiPanel_MiddleBottom.SelectedPanel = uiPanel_EditEvent;
                uiPanel_EditEvent.Text = locationDescription;

                if (employeeID != null)
                    cmb_Event_Instructor.SelectedValue = employeeID;
                dtp_Event_StartDate.Value = e.StartTime.Date;
                dtp_Event_EndDate.Value = e.EndTime.Date.AddDays(-1);

                btn_event_undo.Enabled = (appt.RowState == DataRowState.Modified && appt.IsEditable);
            }
            else
            {
                uiPanel_MiddleBottom.SelectedPanel = uiPanel_MiddleBottom.Panels[0];
                dtp_StartDate.MinDate = timeLine1.FirstDate;
                dtp_StartDate.MaxDate = timeLine1.LastDate;
                dtp_EndDate.MinDate = timeLine1.FirstDate;
                dtp_EndDate.MaxDate = timeLine1.LastDate;
                dtp_StartDate.Value = e.StartTime.Date;
                dtp_EndDate.Value = e.EndTime.Date;
            }

            RenderTimeLineItem(e.Item);
        }

        private void timeLine1_ItemChanged(object sender, ItemChangeEventArgs e)
        {
            //  RenderTimeLineItem(e.Item);
        }

        //Display drag effects when dragging a course over the timeline
        private void timeLine1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        //Create an appointment when a course is dropped on it
        private void timeLine1_DragDrop(object sender, DragEventArgs e)
        {
            //This is only set when dragging a course from the menu, not dragging something within the timeline
            if (DraggingCourse == null)
            {
                foreach (TimeLineItem tli in timeLine1.SelectedItems)
                    RenderTimeLineItem(tli);
                return;
            }

            //figure out what was dragged
            String courseID = DraggingCourse.Name;
            DataRow courseDataRow = scheduleDataSet1.Tables["AppointmentCategories"].Rows.Find(courseID);
            String courseName = courseDataRow["CategoryName"].ToString();
            Double durationDays = double.Parse(courseDataRow["DurationDays"].ToString());
            String employeeID = "0"; //unknown employee
            String accountID = "0";  //public account;
            String classLocationID = "0"; //unknown location;

            //Is it a class or an exception?
            var classType = DraggingCourse.Parent.Text == "Events" ? "Event" : "Public";


            //Where was it dropped
            DateTime startDate = timeLine1.GetDateTimeAt().Date;
            DateTime startTime = startDate.Add(new TimeSpan(8, 30, 0));
            DateTime endDate = startDate.AddDays(durationDays);
            DateTime endTime = endDate.Add(new TimeSpan(17, 30, 0));

            //ToDo: Figure out what kind of row was dropped on.
            TimeLineGroupRow targetRow = timeLine1.GetGroupRowAt();

            if (targetRow == null)
            {
                MessageBox.Show(
                    "I'm sorry, I can't figure out which employee \n" +
                    "this was dropped on.  Please try again.",
                    "Try Again", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            short displayOrder = (short)(targetRow.Value);
            if (targetRow == null) return;
            while (targetRow != null)
            {
                switch (targetRow.Group.Field.Key)
                {
                    case "DisplayOrder":
                        employeeID = GetEmployeeIdFromDisplayOrder(targetRow.Value.ToString());
                        break;
                    case "EmployeeID":
                        employeeID = targetRow.Value.ToString();
                        break;
                    case "AccountID":
                        accountID = targetRow.Value.ToString();
                        break;
                    case "ClassLocationID":
                        classLocationID = targetRow.Value.ToString();
                        break;
                }
                targetRow = targetRow.ParentRow;
            }


            //Lets add the data to the dataset, then try to bind it:
            String appointmentID = CreateGUID();
            ScheduleDataSet.EmployeeListRow employeeListRow =
                scheduleDataSet1.EmployeeList.FindByEmployeeID(employeeID);

            ScheduleDataSet.AccountsRow accountsRow =
                scheduleDataSet1.Accounts.FindByAccountID(accountID);

            ScheduleDataSet.ClassLocationsRow classLocationsRow =
                scheduleDataSet1.ClassLocations.FindByClassLocationID(classLocationID);

            ScheduleDataSet.AppointmentCategoriesRow appointmentCategoriesRow =
                scheduleDataSet1.AppointmentCategories.FindByAppointmentCategoryID(courseID);

            string status = "";
            string subject = "";
            string description = ""; ;
            DateTime whitePaperSentDate = DateTime.MinValue;
            string room = "";
            short numStudents = 0;
            short maxStudents = appointmentCategoriesRow.MaxStudents;
            //DateTime startTime = DateTime.MinValue;
            //DateTime endTime = DateTime.MinValue;

            ScheduleDataSet.AppointmentsRow newAppt =
            scheduleDataSet1.Appointments.AddAppointmentsRow(
                appointmentID,
                employeeListRow,
                accountsRow,
                classLocationsRow,
                status,
                subject,
                description,
                startDate,
                endDate,
                appointmentCategoriesRow,
                null,
                null,
                numStudents,
                maxStudents,
                whitePaperSentDate,
                room,
                startTime,
                endTime,
                displayOrder,
                classType, new DateTime(0), 0, 0, "", "",
                "", "", "", "", "", "", "", "", "", "", false, "", "", "", "", "", "", "", 0, false, false,
                scheduleDataSet1.EmployeeList.FindByEmployeeID(CurrentUser.Id), DateTime.Now);

            //var newAppt = scheduleDataSet1.Appointments.NewAppointmentsRow();
            //newAppt.AppointmentID = appointmentID;
            //newAppt.EmployeeListRow = employeeListRow;
            //newAppt.AccountsRow = accountsRow;
            //newAppt.ClassLocationsRow = classLocationsRow;
            //newAppt.AppointmentCategoriesRow = appointmentCategoriesRow;
            //newAppt.Status = status;
            //newAppt.Description = description;
            //newAppt.StartDate = startDate;
            //newAppt.StartTime = startTime;
            //newAppt.EndDate = endDate;
            //newAppt.EndTime = endTime;
            //newAppt.DisplayOrder = displayOrder;
            //newAppt.ClassType = classType;
            //newAppt.EmployeeListRowByEmployeeList_AppointmentsLastModifiedBy = scheduleDataSet1.EmployeeList.FindByEmployeeID(CurrentUserDetail.id);
            //newAppt.LastModifiedDate = DateTime.Now;

            if (newAppt.IsEmployeeIDNull())
            {
                MessageBox.Show(
                    "I'm sorry, I didn't get the EmployeeID \n" +
                    "this was dropped on.  Please try again.",
                    "Try Again", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //If it is a company wide event
            if (employeeListRow.Username == "allb2t")
            {
                AddHolidaysToTimeLine();
                if (newAppt.AppointmentCategoriesRow.IsExclusive)
                    ValidateSchedule();
                else
                    ValidateEmployeeSchedule(employeeID);
            }
            else
            {
                ValidateEmployeeSchedule(employeeID);
            }

            timeLine1.SelectedItems.Clear();
            timeLine1.SelectedItems.Add(GetTimeLineItemFromAppointment(newAppt));

            RenderAppointment(newAppt);

            ShowEditor();

            DraggingCourse = null;

            RefreshNavMap(true);
        }

        /// <summary>
        /// Called when the user right clicks over the timeline control.
        /// Determines which menu items are enabled based on the state of the 
        /// appointment that was right clicked.
        /// </summary>
        private void mnu_TimeLineItem_Opening(object sender, CancelEventArgs e)
        {
            if (timeLine1.HitTest() == TimeLineArea.RowHeader)
            {
                TimeLineGroupRow g = timeLine1.GetGroupRowAt();
            }
            else if (timeLine1.HitTest() == TimeLineArea.TimeLineItem)
            {

                ContextMenuItem = timeLine1.GetItemAt();
                TimeLineItem tmlItem = timeLine1.GetItemAt();

                if (tmlItem == null)
                {
                    e.Cancel = true;
                    return;
                }

                //Deselect all and select the one right-clicked on
                if (timeLine1.SelectedItems.Count > 0)
                    timeLine1.SelectedItems[0].Selected = false;
                tmlItem.Selected = true;


                //foreach (TimeLineItem tml in timeLine1.SelectedItems)
                //    tml.Selected = false;


                //while(timeLine1.SelectedItems.Count > 0)
                //    timeLine1.SelectedItems[0].Selected = false;

                if (GetAppointmentFromTimeLineItem(tmlItem).IsEvent == false)
                {
                    e.Cancel = true;
                    return;
                }

                switch (((System.Data.DataRowView)(tmlItem.DataRow)).Row.RowState)
                {
                    case DataRowState.Added:
                        mnu_TimeLineItem.Items["Undo"].Enabled = true;
                        break;
                    case DataRowState.Deleted:
                        break;
                    case DataRowState.Detached:
                        break;
                    case DataRowState.Modified:
                        mnu_TimeLineItem.Items["Undo"].Enabled = true;
                        break;
                    case DataRowState.Unchanged:
                        mnu_TimeLineItem.Items["Undo"].Enabled = false;
                        break;
                }
            }
            else
            {
                e.Cancel = true;
                return;
            }
        }

        /// <summary>
        /// Performs the functions in the timeline context (right-click) menu
        /// </summary>
        private void mnu_TimeLineItem_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //ToDo: Hide the popup menu before displaying the msgbox.  Application.DoEvents doesn't seem to do it.
            Application.DoEvents();

            ScheduleDataSet.AppointmentsRow appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(ContextMenuItem.DataRow)).Row);


            if (appt.IsEmployeeIDNull())
            {
                MessageBox.Show(
                    "Null EmployeeID detected",
                    "Try Again", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            String EmployeeID = appt.EmployeeID;

            switch (e.ClickedItem.Name)
            {
                case "Edit":
                    ShowEditor();
                    break;
                case "Undo":
                    undoTimeLineItem(ContextMenuItem);
                    ShowEditor();
                    break;
                case "Delete":
                    deleteTimeLineItem(ContextMenuItem);
                    ShowEditor();
                    break;
            }
        }

        /// <summary>
        /// Changes the icon associated with the "All B2t and "UnAssigned" time Line Row Headings
        /// </summary>
        private void timeLine1_FormattingGroupRow(object sender, FormattingGroupRowEventArgs e)
        {
            switch (e.GroupRow.GroupCaption.Trim())
            {
                case "All B2T":
                    e.GroupRow.ImageKey = "People";
                    break;
                case "Unassigned Instructor":
                    e.GroupRow.ImageKey = "PersonUnknown";
                    break;
            }
        }

        int prevVerticalPosition = -1;
        private void timeLine1_Paint(object sender, PaintEventArgs e)
        {
            if (!LastClickPoint.IsEmpty)
            {
                // horizontal bar
                int row = (LastClickPoint.Y - timeLine1.TimeScalesHeight) / timeLine1.RowHeight;
                int x = 0; // timeLine1.RowHeaderWidth;
                int y = timeLine1.TimeScalesHeight + 2 + ((timeLine1.RowHeight - 1) * row);
                int w = timeLine1.ClientRectangle.Width - x;
                int h = timeLine1.RowHeight;

                e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(128, Color.Black)), 3),
                        new Rectangle(x, y, w, h));

            }
            if (timeLine1.VerticalScrollbarPosition != prevVerticalPosition)
            {
                NavMap.Refresh();
                prevVerticalPosition = timeLine1.VerticalScrollbarPosition;
            }

            button_ApplyStartDateFilter.Visible = (timeLine1.MinDate != dtp_StartDateFilter.Value);
            button_ApplyEndDateFilter.Visible = (timeLine1.MaxDate != dtp_EndDateFilter.Value);
            //if (timeLine1.MinDate == dtp_StartDateFilter.Value)
            //    button_ApplyStartDateFilter.ImageIndex = 44;
            //else
            //    button_ApplyStartDateFilter.ImageIndex = 43;

            //if (timeLine1.MaxDate == dtp_EndDateFilter.Value)
            //    button_ApplyEndDateFilter.ImageIndex = 44;
            //else
            //    button_ApplyEndDateFilter.ImageIndex = 43;
        }

        #endregion timeline1 callbacks

        #endregion Manage the timeline control



        #region Navigation map (under the timeline)
        private Boolean NavMapDirty = true;
        private Bitmap NavMapImage;
        private Bitmap NavMapImage1;
        private Boolean NavMapLocked = false;

        /// <summary>
        /// Redraws the NavMap
        /// </summary>
        /// <param name="recalculate">
        /// if true: rebuilds the image by querying the data.  
        /// if false: just rescales the image</param>
        private void RefreshNavMap(Boolean recalculate)
        {
            NavMapDirty = recalculate;
            try
            {
                NavMap.Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        // When the timeline is scrolled, redraw the navigation map
        private void timeLine1_FirstDateChanged(object sender, EventArgs e)
        {
            RefreshNavMap(false);
        }

        // Draw the navigiation window
        int NavPaintDirtyCalls = 0;
        long NavPaintDirtyTicks = 0;
        int NumVisibleGroups = -1;
        private void NavMap_Paint(object sender, PaintEventArgs e)
        {
            NavMapLocked = true;
            SortedList visibleGroupList = new SortedList();

            //Bail out if the user has disabled the nav map
            if (!tre_DisplayOptions.Nodes["NavMap"].Checked) return;

            //Bail out if there is no data to show
            if (timeLine1.GetGroupRows().GetLength(0) == 0) return;

            //If this is the first time this is called, then count the visible groups
            if (NumVisibleGroups == -1 || NavMapDirty)
            {
                visibleGroupList = GetVisibleGroups();
                if (visibleGroupList != null)
                    NumVisibleGroups = visibleGroupList.Count;
                else
                    NumVisibleGroups = 0;
            }

            //ToDo: Case when visiblegroupheight < available height
            //pixPerGroup :: displayHeight

            //ToDo: Case when visiblegroupheight >= available height
            //PixPerGroup :: scrollableHeight

            float x, y, w, h;
            Color c;
            long ticks = DateTime.Now.Ticks;
            int numDays = timeLine1.MaxDate.Subtract(timeLine1.MinDate).Days;
            float scrollHeight = timeLine1.Height - timeLine1.TimeScalesHeight;
            float dataHeight = (NumVisibleGroups * (timeLine1.RowHeight - 1));
            float visiblePercent = (float)Math.Min(1.0, scrollHeight / dataHeight);
            float pixPerDay = (float)(NavMap.Width) / numDays;
            // float pixPerGroup = NumVisibleGroups > 0 ? (float)(NavMap.Height) / NumVisibleGroups : NavMap.Height;
            float pixPerGroup = (float)(NavMap.Height - MonthHeadHeight) / NumVisibleGroups;
            if (dataHeight < scrollHeight) pixPerGroup = pixPerGroup * (dataHeight / scrollHeight);

            if (NavMapDirty)
            {

                NavPaintDirtyCalls++;

                // Create a new image
                if (NavMapImage != null) NavMapImage.Dispose();
                NavMapImage = new Bitmap(NavMap.Width, NavMap.Height, e.Graphics);
                Graphics g = Graphics.FromImage(NavMapImage);

                // Create the blinkImageOverlay
                Bitmap blinkImageOverlay = new Bitmap(NavMap.Width, NavMap.Height, e.Graphics);
                blinkImageOverlay.MakeTransparent();

                //Draw line under monthhead
                g.DrawLine(new Pen(Color.FromArgb(0xff, 0, 0, 0), 1f), 0, MonthHeadHeight - 1, NavMap.Width, MonthHeadHeight - 1);

                //Draw Today
                x = pixPerDay * DateTime.Now.Subtract(timeLine1.MinDate).Days;
                y = MonthHeadHeight;
                //w = pixPerDay * timeLine1.LastDate.Subtract(timeLine1.FirstDate).Days;
                w = pixPerDay * 2;
                x -= w / 2; //center the today block horizontally
                h = NavMap.Height - MonthHeadHeight;
                c = Color.Brown;
                g.FillRectangle(new SolidBrush(c), x, y, w, h);

                ScheduleDataSet.AppointmentsRow appt;

                //Draw Holidays
                foreach (ScheduleDataSet.HolidaysRow holiday in scheduleDataSet1.Holidays.Rows)
                {
                    x = pixPerDay * holiday.StartDate.Subtract(timeLine1.MinDate).Days;
                    y = MonthHeadHeight;
                    w = pixPerDay * (float)holiday.EndDate.Subtract(holiday.StartDate).TotalDays;
                    h = NavMap.Height - MonthHeadHeight;
                    c = GetFormat("EventType", "HOLIDAY").GetBackColor();
                    g.FillRectangle(new SolidBrush(c), x, y, w, h);
                }

                //Draw Holidays                
                //if (tre_Filters.Nodes.Find("HOLIDAYWORK", true).Length > 0 && tre_Filters.Nodes.Find("HOLIDAY", true).Length > 0)
                //{
                //    foreach (ScheduleDataSet.AppointmentsRow holiday in AllB2TEmployee.GetAppointmentsRows())
                //    {
                //        if (holiday.PendingDelete) continue;
                //        if ((holiday.AppointmentCategoriesRow.AppointmentCategoryID == "HOLIDAYWORK"
                //            && tre_Filters.Nodes.Find("HOLIDAYWORK", true)[0].Checked)
                //            ||
                //            (holiday.AppointmentCategoriesRow.AppointmentCategoryID == "HOLIDAY"
                //            && tre_Filters.Nodes.Find("HOLIDAY", true)[0].Checked)
                //            )
                //        {
                //            x = pixPerDay * holiday.StartDate.Subtract(timeLine1.MinDate).Days;
                //            y = MonthHeadHeight;
                //            w = pixPerDay * holiday.EndDate.Date.Subtract(holiday.StartDate.Date).Days;
                //            h = NavMap.Height - MonthHeadHeight;
                //            //                        c = CustomColors.DayHoliday;
                //            c = GetFormat("EventType", "HOLIDAY").GetBackColor();
                //            //                        c = scheduleDataSet1.Formats.FindByParentTypeParentID("Day","Holiday").GetBackColor();
                //            g.FillRectangle(new SolidBrush(c), x, y, w, h);
                //        }
                //    }
                //}

                //Draw the appointments
                foreach (TimeLineItem i in timeLine1.Items)
                {
                    if (i.ParentRow.Value == DBNull.Value) continue;
                    if (!visibleGroupList.ContainsKey(i.ParentRow.Value ?? 0)) continue;
                    x = pixPerDay * i.StartTime.Subtract(timeLine1.MinDate).Days;
                    y = MonthHeadHeight + pixPerGroup * visibleGroupList.IndexOfKey(i.ParentRow.Value);
                    w = pixPerDay * i.Duration.Days;
                    h = pixPerGroup;

                    if (i.FormatStyle.BackColor.IsEmpty)
                        c = Color.DimGray;
                    else
                        c = AdjustColor(i.FormatStyle.BackColor, 1, 10);

                    appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(i.DataRow)).Row);

                    //Draw the dot
                    g.FillRectangle(new SolidBrush(c), x, y, w, h);

                    //Highlight warnings and errors:
                    if (appt.EndDate > DateTime.Today)
                    {
                        if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Violation && tre_DisplayOptions.Nodes["NavMap"].Nodes["HighlightWarnings"].Checked)
                            Graphics.FromImage(blinkImageOverlay).FillRectangle(new SolidBrush(Color.FromArgb(64, Color.Firebrick)), x, y, w, h);
                        //Graphics.FromImage(blinkImageOverlay).FillRectangle(new SolidBrush(Color.FromArgb(64, Color.Firebrick)), x - 1, y - 1, w + 2, h + 2);
                        if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Error && tre_DisplayOptions.Nodes["NavMap"].Nodes["HighlightErrors"].Checked)
                            Graphics.FromImage(blinkImageOverlay).FillRectangle(new SolidBrush(Color.FromArgb(64, Color.Firebrick)), x, y, w, h);
                        //Graphics.FromImage(blinkImageOverlay).FillRectangle(new SolidBrush(Color.FromArgb(64, Color.Firebrick)), x - 1, y - 1, w + 2, h + 2);
                    }
                }

                //Draw month names 2: Along the top
                DateTime d = timeLine1.MinDate.AddMonths(-1);
                Font font = new Font("Microsoft Sans Serif", (float)8);
                // Brush brush = new System.Drawing.SolidBrush(CustomColors.NavMapMonthLabel);
                Color headFontColor = Color.FromArgb(0x80, 0x00, 0x00, 0x00);
                Color evenMonthColor = Color.FromArgb(0x10, 0x00, 0x00, 0x00);
                Color oddMonthColor = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
                //                Brush stringBrush = new System.Drawing.SolidBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));
                Brush evenMonthBrush = new System.Drawing.SolidBrush(evenMonthColor);
                Brush oddMonthBrush = new System.Drawing.SolidBrush(oddMonthColor);
                Pen monthPen = new Pen(evenMonthColor, 2);
                monthPen.Alignment = PenAlignment.Center;

                NavMap.Controls.Clear();
                while (d < timeLine1.MaxDate.AddMonths(2))
                {
                    String t;
                    x = pixPerDay * d.Subtract(timeLine1.MinDate).Days;
                    y = 0;
                    w = pixPerDay * 28;
                    h = NavMap.Height;
                    t = d.Month == 1 ? d.ToString("yyyy") : d.ToString("MMM");

                    //g.DrawRectangle(monthPen, x - (w / 2), y, w, h);
                    g.DrawLine(monthPen, x - (w / 2), 0, x - (w / 2), h);
                    //g.FillRectangle(d.Month % 2 == 1 ? oddMonthBrush : evenMonthBrush, x - (w / 2), y, w, h);
                    //g.FillRectangle(d.Month % 2 == 1 ? oddMonthBrush : evenMonthBrush, x - (w / 2), y, 2, h);

                    Label l = new Label();
                    l.Text = t;
                    l.Font = font;
                    l.Location = new Point((int)(x - (w / 2)), (int)y);
                    l.Size = new Size((int)w, (int)MonthHeadHeight);
                    l.TextAlign = ContentAlignment.TopCenter;
                    l.BackColor = Color.Transparent;
                    //                    l.BackColor = d.Month % 2 == 1 ? Color.FromArgb(0xff, 0x00, 0x00, 0x00) : Color.FromArgb(0xff, 0x20, 0x20, 0x20);
                    NavMap.Controls.Add(l);

                    //g.DrawString(t,font,stringBrush,x-(w/4),y);

                    d = d.AddMonths(1);
                }

                //draw weekends
                d = timeLine1.MinDate.AddMonths(-1);
                d = d.AddDays(-(int)d.DayOfWeek - 1); //get the first saturday
                var weekendBrush = new System.Drawing.SolidBrush(Color.FromArgb(0x8, 0x00, 0x00, 0x00));
                var weekendRects = new List<RectangleF>();
                w = pixPerDay * 2;
                h = NavMap.Height;
                for (; d < timeLine1.MaxDate.AddMonths(2); d = d.AddDays(7))
                    weekendRects.Add(new RectangleF(pixPerDay * d.Subtract(timeLine1.MinDate).Days, 0, w, h));
                g.FillRectangles(weekendBrush, weekendRects.ToArray());

                //Create the alternate navmap image for blinking
                if (NavMapImage1 != null) NavMapImage1.Dispose();
                NavMapImage1 = new Bitmap(NavMapImage);
                Graphics.FromImage(NavMapImage1).DrawImage(blinkImageOverlay, 0, 0);

                NavMap.BackgroundImage = NavMapImage1;
            }
            else
            {
                NavPaintCalls++;
            }

            //Draw the viewport
            x = pixPerDay * timeLine1.FirstDate.Subtract(timeLine1.MinDate).Days;
            y = MonthHeadHeight + timeLine1.VerticalScrollbarPosition * pixPerGroup;
            if (y < MonthHeadHeight || float.IsNaN(y)) y = MonthHeadHeight;
            w = pixPerDay * timeLine1.LastDate.Subtract(timeLine1.FirstDate).Days;
            h = ((NavMap.Height - MonthHeadHeight) * visiblePercent) - 3;

            //Draw a translucent solid rectangle
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(64, Color.Blue)), x, y, w, h);

            //Draw the opaque border
            e.Graphics.DrawRectangle(new Pen(Color.FromArgb(0xA0, Color.Blue), 1f), x, y, w, h);

            NavMapDirty = false;
            NavMapLocked = false;

            //Performance tuning stuff            
            if (NavMapDirty) NavPaintDirtyTicks += (DateTime.Now.Ticks - ticks);
            else NavPaintTicks += (DateTime.Now.Ticks - ticks);
            txtNavPaint.Text = string.Format("Clean Calls:{0} Time:{1} DirtyCalls:{2} Time:{3} ",
                NavPaintCalls,
                NavPaintTicks / TimeSpan.TicksPerMillisecond,
                NavPaintDirtyCalls,
                NavPaintDirtyTicks / TimeSpan.TicksPerMillisecond);

        }

        //This routine is called once and executes continuously in a separate thread
        //Alternating which background image to use on the navmap to produce a blink 
        //effect
        private void bgWorker_AnimateNavBar_DoWork(object sender, DoWorkEventArgs e)
        {
            bool useAltImage = false;
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                if (NavMapImage == null || NavMapImage1 == null || NavMapLocked == true) continue;
                NavMap.BackgroundImage = useAltImage ? NavMapImage1 : NavMapImage;
                useAltImage = !useAltImage;
            }
        }

        // Move the timeline to where the mouse clicked in the navMap
        private void WarpTimeLine(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            //Move the timeline horizontally
            DateTime minDate = timeLine1.MinDate;
            DateTime maxDate = timeLine1.MaxDate;
            float viewPortDays = timeLine1.LastDate.Subtract(timeLine1.FirstDate).Days;
            int numDays = maxDate.Subtract(minDate).Days;
            float pixPerDay = (float)(NavMap.Width) / numDays;
            DateTime newFirstDate = minDate.AddDays(e.X / pixPerDay).AddDays(viewPortDays / -2);
            timeLine1.FirstDate = newFirstDate;

            //Move the timeline vertically
            float scrollHeight = timeLine1.Height - timeLine1.TimeScalesHeight;
            float dataHeight = (NumVisibleGroups * (timeLine1.RowHeight - 1));
            float visiblePercent = (float)Math.Min(1.0, scrollHeight / dataHeight);
            float viewPortHeight = ((NavMap.Height - 14) * visiblePercent);
            float cursorPercent = ((float)e.Y - (viewPortHeight / 2)) / (NavMap.Height);
            if (cursorPercent < 0) cursorPercent = 0;
            timeLine1.VerticalScrollbarPosition = (int)Math.Round(cursorPercent * NumVisibleGroups);
        }

        // Start Dragging
        private void NavMap_MouseDown(object sender, MouseEventArgs e)
        {
            IsDraggingTime = true;
            NavMap.Cursor = Cursors.NoMove2D;
            WarpTimeLine(e);
        }

        // Dragging within the nav map
        private void NavMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDraggingTime)
                WarpTimeLine(e);
        }

        // Done Dragging
        private void NavMap_MouseUp(object sender, MouseEventArgs e)
        {
            IsDraggingTime = false;
            NavMap.Cursor = Cursors.Hand;
            WarpTimeLine(e);
        }

        // Size has changed, needs to be redrawn
        private void NavMap_SizeChanged(object sender, EventArgs e)
        {
            NavMapDirty = true;
        }


        #endregion



        #region Manage the edit form

        private void InitEditForm()
        {
            cmb_Event_EventName.DataSource = new DataView(
                scheduleDataSet1.AppointmentCategories,
                "Type not in('Classes')",
                "CategoryName",
                DataViewRowState.CurrentRows);
            cmb_Event_EventName.ValueMember = "AppointmentCategoryID";
            cmb_Event_EventName.DisplayMember = "CategoryName";
            //ToDo: Clear the edit form

        }

        //When the account is changed, repopulate the location combo
        private void cmb_Account_SelectedValueChanged(object sender, EventArgs e)
        {
            if (IsLoadingEditor) return;
            DataView accountLocations = new DataView(scheduleDataSet1.ClassLocations);
            if (cmb_Account.SelectedValue != null)
                accountLocations.RowFilter = String.Format("AccountID is null or AccountID='{0}'", cmb_Account.SelectedValue.ToString());

            cmb_Location.DataSource = accountLocations;
            cmb_Location.DisplayMember = "Name";
            cmb_Location.ValueMember = "ClassLocationID";
        }

        //Show the appropriate editor in the bottom panel
        private void ShowEditor()
        {
            ScheduleDataSet.AppointmentsRow appt;
            SetMenuBarState();
            if ((timeLine1.SelectedItems.Count == 0) ||
                (GetAppointmentFromTimeLineItem((TimeLineItem)(timeLine1.SelectedItems[0])) == null))
            {
                //DateTime clickDate = timeLine1.GetDateTimeAt(LastClickPoint);
                //appt = getHoliday(clickDate);
                //if (appt != null)
                //{
                //    ShowHolidayEditor(appt);
                //    return;
                //}
                ShowAppointmentStatus(null);
                ShowFreePanel();
                return;
            }

            appt = GetAppointmentFromTimeLineItem(timeLine1.SelectedItems[0].Item);
            SelectedEmployee = appt.EmployeeListRow;



            ShowAppointmentStatus(appt);

            if (appt.Deleted == true)
            {
                timeLine1.SelectedItems.Remove(timeLine1.SelectedItems[0].Item);
                ShowFreePanel();
                return;
            }

            //Make sure only appointments of the same class type can be selected
            if (timeLine1.SelectedItems.Count > 1)
                for (int i = timeLine1.SelectedItems.Count - 1; i > 0; i--)
                    if (GetAppointmentFromTimeLineItem(timeLine1.SelectedItems[i].Item).IsEvent !=
                    GetAppointmentFromTimeLineItem(timeLine1.SelectedItems[i - 1].Item).IsEvent)
                        timeLine1.SelectedItems.Remove(timeLine1.SelectedItems[i]);



            //Show the appropriate editor form
            if (GetAppointmentFromTimeLineItem(timeLine1.SelectedItems[timeLine1.SelectedItems.Count - 1].Item).IsEvent)
            {
                //this.timeLine1.AllowItemDrag = AllowItemDrag.Move | AllowItemDrag.GroupChange | AllowItemDrag.Resize;
                ShowEventEditor();
            }
            else
            {
                //this.timeLine1.AllowItemDrag = AllowItemDrag.None;
                ShowClassEditor();
            }

            //Select the editor tab if the user was viewing the notifications tab
            //then clicked on an appointment that doesn't have an error
            if (uiPanel_AppointmentDetail.SelectedPanel == uiPanel_AppointmentStatus
                && appt.ErrorLevel == ScheduleDataSet.ErrorLevels.None)
                uiPanel_AppointmentDetail.SelectedPanel = uiPanel_AppointmentEditor;

            //Populate the employee tab
            //uiPanel_EmployeeEditor.Text = appt.EmployeeListRow[GetEmployeeNameFieldPreference()].ToString();
            //uiPanel_EmployeeEditor2.Text = appt.EmployeeListRow[GetEmployeeNameFieldPreference()].ToString();
            uiPanel_EmployeeEditor3.Text = appt.EmployeeListRow[GetEmployeeNameFieldPreference()].ToString();

            //Tell the instructor editor to load itself
            instructorEditor1.Show(appt.EmployeeID, appt.StartDate, appt.EndDate);


        }

        private void ShowFreePanel()
        {
            panel_HolidayEditor.Visible = false;
            panel_ClassEditor.Visible = false;
            panel_EventEditor.Visible = false;
            panel_FreeEditor.Dock = DockStyle.Fill;
            panel_FreeEditor.Visible = true;
            //            uiPanel_AppointmentEditor.Text = "";
            uiPanel_AppointmentEditor.Image = null;
        }

        private void ShowClassEditor()
        {
            IsLoadingEditor = true;
            panel_FreeEditor.Visible = false;
            panel_HolidayEditor.Visible = false;
            panel_EventEditor.Visible = false;
            panel_ClassEditor.Visible = true;
            panel_ClassEditor.Dock = DockStyle.Fill;
            foreach (TimeLineItem tli in timeLine1.SelectedItems)
            {
                PopulateEditForm(tli);
            }
            IsLoadingEditor = false;
        }

        private void ShowHolidayEditor(ScheduleDataSet.HolidaysRow row)
        {
            IsLoadingEditor = true;
            panel_FreeEditor.Visible = false;
            panel_HolidayEditor.Visible = true;
            panel_EventEditor.Visible = false;
            panel_ClassEditor.Visible = false;
            panel_HolidayEditor.Dock = DockStyle.Fill;

            dtp_Holiday_StartDate.Value = row.StartDate;
            dtp_Holiday_EndDate.Value = row.EndDate;

            IsLoadingEditor = false;
        }

        //private void ShowHolidayEditor()
        //{
        //    //Which Holiday was clicked?
        //}


        //ToDo: Load event editor assumes startdate is beginning of day and end date is end of day, so same date = 1 day
        //ToDo: Apply event editor works wrt dates

        private void EnableEventEditor(bool enabled)
        {
            //foreach(Control c in uiPanel_EditEventContainer.Controls)
            foreach (Control c in panel_EventEditor.Controls)
                if (c is TextBox)
                    ((TextBox)c).ReadOnly = !enabled;
                else if (c is ComboBox)
                    ((ComboBox)c).Enabled = enabled;
                else if (c is Button)
                    ((Button)c).Enabled = enabled;
                else if (c is DateTimePicker)
                    ((DateTimePicker)(c)).Enabled = enabled;
                else if (c is NumericUpDown)
                    c.Enabled = enabled;
                else if (c is Label)
                    c.Enabled = true;
                else
                    c.Enabled = enabled;
            ;


            //                c.Enabled = enabled;
        }


        Boolean IsLoadingEditor = false;
        private void ShowEventEditor()
        {
            IsLoadingEditor = true;
            panel_FreeEditor.Visible = false;
            panel_ClassEditor.Visible = false;
            panel_EventEditor.Dock = DockStyle.Fill;
            panel_EventEditor.Visible = true;

            //Tab Icon:
            uiPanel_AppointmentEditor.Image = imageList1.Images["Event"];

            for (int i = 0; i < timeLine1.SelectedItems.Count; i++)
            {
                ScheduleDataSet.AppointmentsRow appt = GetAppointmentFromTimeLineItem(timeLine1.SelectedItems[i].Item);
                ScheduleDataSet.AppointmentsRow prevAppt = GetAppointmentFromTimeLineItem(timeLine1.SelectedItems[i == 0 ? 0 : i - 1].Item);
                //if (appt.RowState == DataRowState.Modified) btn_event_undo.Enabled = true;

                if (appt == null || prevAppt == null) continue;

                EnableEventEditor(appt.IsEditable & !IsReadOnly);

                btn_event_undo.Enabled = (appt.RowState == DataRowState.Modified && appt.IsEditable);
                btn_event_delete.Text = appt.PendingDelete ? "UN-Delete" : "Delete";
                btn_event_delete.ImageKey = appt.PendingDelete ? "DeleteGray" : "Delete";

                //Select the employee
                if (appt.EmployeeID == prevAppt.EmployeeID)
                {
                    cmb_Event_Instructor.SelectedValue = appt.IsEmployeeIDNull() ? "" : appt.EmployeeID;
                }
                else
                {
                    cmb_Event_Instructor.SelectedValue = "";
                    cmb_Event_Instructor.Text = "Multiple-Mixed Values";
                }

                //Select the event type
                if (appt.AppointmentCategoryID == prevAppt.AppointmentCategoryID)
                {
                    cmb_Event_EventName.SelectedValue = appt.IsAppointmentCategoryIDNull() ? "0" : appt.AppointmentCategoryID;
                }
                else
                {
                    cmb_Event_EventName.SelectedValue = "0";
                    cmb_Event_EventName.Text = "Multiple-Mixed Values";
                }

                //Subject
                txt_Event_Subject.Text = "";
                if (!appt.IsSubjectNull() && appt.Subject == prevAppt.Subject)
                    txt_Event_Subject.Text = appt.Subject;

                //Description
                txt_Event_Description.Text = "";
                if (!appt.IsDescriptionNull() & !prevAppt.IsDescriptionNull() && appt.Description == prevAppt.Description)
                    txt_Event_Description.Text = appt.Description;

                //Start Date
                //ToDo: Get the Start/End Date bracketing working again
                //ToDo: Use the dateformat hack to hide the date if its blank
                if (appt.StartDate == prevAppt.StartDate)
                {
                    //dtp_Event_StartDate.MinDate = new DateTime(1900, 1, 1);
                    //dtp_Event_StartDate.MaxDate = new DateTime(2100, 1, 1);
                    //dtp_Event_StartDate.MinDate = timeLine1.FirstDate;
                    //dtp_Event_StartDate.MaxDate = timeLine1.LastDate;
                    if (!appt.IsStartDateNull())
                    {
                        dtp_Event_StartDate.Checked = true;
                        dtp_Event_StartDate.Value = appt.StartDate;
                    }
                    else
                    {
                        dtp_Event_StartDate.Checked = false;
                    }

                }
                else
                {
                    dtp_Event_StartDate.Checked = false;
                }


                //End Date
                if (appt.EndDate == prevAppt.EndDate)
                {
                    //dtp_Event_EndDate.MinDate = timeLine1.FirstDate;
                    //dtp_Event_EndDate.MaxDate = timeLine1.LastDate;
                    if (!appt.IsEndDateNull())
                    {
                        dtp_Event_EndDate.Checked = true;
                        if (appt.EndDate.TimeOfDay == new TimeSpan(0))
                        {
                            dtp_Event_EndDate.Value = appt.EndDate.AddDays(-1);
                        }
                        else
                        {
                            dtp_Event_EndDate.Value = appt.EndDate;
                        }
                    }
                    else
                    {
                        dtp_Event_EndDate.Checked = false;
                    }

                }
                else
                {
                    dtp_Event_EndDate.Checked = false;
                }

                //LastModified by /date
                lbl_ModificationHistory.Text = "";
                if (!appt.IsLastModifiedByNull())
                {
                    ScheduleDataSet.EmployeeListRow ee = scheduleDataSet1.EmployeeList.FindByEmployeeID(appt.LastModifiedBy);
                    if (ee != null)
                        lbl_ModificationHistory.Text = "by " + ee[GetEmployeeNameFieldPreference()].ToString() + " ";
                    if (!appt.IsLastModifiedDateNull())
                        lbl_ModificationHistory.Text += "on " + appt.LastModifiedDate.ToLongDateString() + " " + appt.LastModifiedDate.ToLongTimeString();
                }
                if (lbl_ModificationHistory.Text.Length > 0)
                    lbl_ModificationHistory.Text = "Last modified " + lbl_ModificationHistory.Text;


            }
            IsLoadingEditor = false;
        }

        //Populate the appointment panel
        private void ShowAppointmentStatus(ScheduleDataSet.AppointmentsRow appt)
        {
            String title = "";
            TreeNode node;
            // ScheduleDataSet.ErrorLevels maxSeverity = ScheduleDataSet.ErrorLevels.None;
            tre_Violations.Nodes.Clear();
            if (appt == null)
            {
                uiPanel_AppointmentStatus.Image = null;
                uiPanel_AppointmentStatus.Text = null;
                return;
            }

            foreach (ScheduleDataSet.AppointmentNotificationsRow note in appt.GetAppointmentNotificationsRows())
            {
                // if (note.Severity > (short)maxSeverity) maxSeverity = (ScheduleDataSet.ErrorLevels)note.Severity;
                if (title != "") title += ", ";
                title += note.Summary;

                switch ((ScheduleDataSet.ErrorLevels)(note.Severity))
                {
                    case ScheduleDataSet.ErrorLevels.Info:
                        node = tre_Violations.Nodes.Add(note.RuleID, note.Summary, "Optimum", "Optimum");
                        node.Nodes.Add("Detail", note.Detail, "DottedLine", "DottedLine");
                        node.Expand();
                        break;
                    case ScheduleDataSet.ErrorLevels.PotentialRecalc:
                        node = tre_Violations.Nodes.Add(note.RuleID, note.Summary, "Violation.Schedule", "Violation.Schedule");
                        node.Nodes.Add("Detail", note.Detail, "DottedLine", "DottedLine");
                        node.Expand();
                        break;
                    case ScheduleDataSet.ErrorLevels.PotentialViolation:
                        node = tre_Violations.Nodes.Add(note.RuleID, note.Summary, "Violation.Potential", "Violation.Potential");
                        node.Nodes.Add("Detail", note.Detail, "DottedLine", "DottedLine");
                        node.Expand();
                        break;
                    case ScheduleDataSet.ErrorLevels.Violation:
                        node = tre_Violations.Nodes.Add(note.RuleID, note.Summary, "Violation", "Violation");
                        node.Nodes.Add("Detail", note.Detail, "DottedLine", "DottedLine");
                        node.Expand();
                        break;
                    case ScheduleDataSet.ErrorLevels.Error:
                        if (note.RuleID == "UNQ")
                        {
                            node = tre_Violations.Nodes.Add(note.RuleID, note.Summary, "NotQualified", "NotQualified");
                            node.Nodes.Add("Detail", note.Detail, "DottedLine", "DottedLine");
                            node.Expand();
                        }
                        else
                            node = tre_Violations.Nodes.Add(note.RuleID, note.Summary, "Error", "Error");
                        break;
                    default:
                        break;

                }
            }

            uiPanel_AppointmentStatus.Text = title;
            switch (appt.ErrorLevel)
            {
                case ScheduleDataSet.ErrorLevels.Info:
                    uiPanel_AppointmentStatus.Image = imageList1.Images["Optimum"];
                    break;
                case ScheduleDataSet.ErrorLevels.PotentialRecalc:
                    uiPanel_AppointmentStatus.Image = imageList1.Images["Violation.Schedule"];
                    break;
                case ScheduleDataSet.ErrorLevels.PotentialViolation:
                    uiPanel_AppointmentStatus.Image = imageList1.Images["Violation.Potential"];
                    break;
                case ScheduleDataSet.ErrorLevels.Violation:
                    uiPanel_AppointmentStatus.Image = imageList1.Images["Violation"];
                    break;
                case ScheduleDataSet.ErrorLevels.Error:
                    uiPanel_AppointmentStatus.Image = imageList1.Images["Error.Unmodified.Future"];
                    break;
                default:
                    uiPanel_AppointmentStatus.Image = null;
                    break;
            }
        }

        private void EnableEditForm(bool enabled)
        {
            foreach (Control c in uiPanel1Container.Controls)
            {
                //if (c.Name == "txt_ShippingContactAddress") continue;
                c.Enabled = enabled;
            }
        }

        /// <summary>
        /// Displays the values from the specified TimeLineItem in the class edit form
        /// </summary>
        /// <param name="tmlItem"></param>
        private void PopulateEditForm(TimeLineItem tmlItem)
        {
            // Show the edit form
            panel_EventEditor.Visible = false;
            panel_FreeEditor.Visible = false;
            panel_ClassEditor.Visible = true;
            //Tab Icon:
            uiPanel_AppointmentEditor.Image = imageList1.Images["Course"];

            ScheduleDataSet.AppointmentsRow appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(tmlItem.DataRow)).Row);

            EnableEditForm(false);

            //ToDo: uncomment this to allow editing of events.
            //EnableEditForm(appt.Editable & !IsReadOnly);

            //Paint the item as selected: Bold, opaque
            //tmlItem.FormatStyle.Alpha = 255;
            //tmlItem.FormatStyle.FontBold = TriState.True;
            //tmlItem.TimeLine.SelectedFormatStyle.BackColor = tmlItem.FormatStyle.BackColor;
            //tmlItem.FormatStyle.BackgroundImage = this.imageList1.Images[14];
            //tmlItem.FormatStyle.BackgroundImageDrawMode = BackgroundImageDrawMode.Stretch;

            //switch (appt.ErrorLevel)
            //{
            //    case ScheduleDataSet.ErrorLevels.None:
            //        uiPanel_Status.Image = null;
            //        uiPanel_Status.InfoText = "";
            //        break;
            //    case ScheduleDataSet.ErrorLevels.Warning:
            //        uiPanel_Status.Image = imageList1.Images["Warning.Unmodified.Future"];
            //        uiPanel_Status.Text = appt.GetErrors()[0];
            //        break;
            //    case  ScheduleDataSet.ErrorLevels.Error:
            //        uiPanel_Status.Image = imageList1.Images["Error.Unmodified.Future"];
            //        uiPanel_Status.Text = appt.GetErrors()[0];
            //        break;
            //}

            //Onsite Account
            cmb_Account.SelectedValue = appt.IsAccountIDNull() ? "0" : appt.AccountID;

            //Course
            //ToDo: re-enable this
            cmb_CourseName.SelectedValue = appt.IsAppointmentCategoryIDNull() ? "0" : appt.AppointmentCategoryID;

            //MaxStudents
            if (!appt.IsMaxStudentsNull())
                nud_MaxStudents.Value = appt.MaxStudents;
            else if (appt.AppointmentCategoriesRow != null && !appt.AppointmentCategoriesRow.IsMaxStudentsNull())
                nud_MaxStudents.Value = appt.AppointmentCategoriesRow.MaxStudents;

            //Class Contact:
            cmb_ClassContactName.Text = appt.IsClassContactNameNull() ? "" : appt.ClassContactName;
            mtb_ClassContactPhone.Text = appt.IsClassContactPhoneNull() ? "" : appt.ClassContactPhone;
            mtb_ClassContactEmail.Text = appt.IsClassContactEmailNull() ? "" : appt.ClassContactEmail;

            //Status:
            cmb_Status.Text = appt.IsStatusNull() ? "" : appt.Status;

            //Type:
            cmb_LocationType.Text = appt.IsClassTypeNull() ? "" : appt.ClassType;

            //Start Date
            if (!appt.IsStartDateNull() && appt.StartDate > dtp_StartDate.MinDate && appt.StartDate < dtp_StartDate.MaxDate)
            {
                dtp_StartDate.Checked = true;
                dtp_StartDate.Value = appt.StartDate;
            }
            else
            {
                dtp_StartDate.Text = "";
                dtp_StartDate.Checked = false;
            }

            //End Date
            if (!appt.IsEndDateNull() && appt.EndDate > dtp_EndDate.MinDate && appt.EndDate < dtp_EndDate.MaxDate)
            {
                dtp_EndDate.Checked = true;
                if (appt.EndDate.TimeOfDay == new TimeSpan(0))
                {
                    dtp_EndDate.Value = appt.EndDate.AddDays(-1);
                }
                else
                {
                    dtp_EndDate.Value = appt.EndDate;
                }
            }
            else
            {
                dtp_EndDate.Text = "";
                dtp_EndDate.Checked = false;
            }


            //Instructor

            cmb_InstructorName.SelectedValue = appt.IsEmployeeIDNull() ? "" : appt.EmployeeID;
            if ((string)(cmb_InstructorName.SelectedValue) == "0")
            {
                cmb_InstructorName.Text = "";
                img_InstructorNameError.Visible = true;
            }
            else
            {
                img_InstructorNameError.Visible = false;
            }


            //Duration
            TimeSpan duration = appt.EndDate.Date.Subtract(appt.StartDate.Date);
            nud_Duration.Value = ((int)(duration.TotalDays));

            //Class Location
            cmb_Location.SelectedValue = appt.IsClassLocationIDNull() ? "0" : appt.ClassLocationID;

            //NumRegistered
            txt_NumStudents.Text = appt.IsNumStudentsNull() ? "" : appt.NumStudents.ToString();

            //NumStudents
            //nud_NumRegistered.Value = appt.IsNumRegisteredNull() ? 0 : (decimal)appt.NumRegistered;

            //Material Ship Date
            if (!appt.IsMaterialShipDateNull() && appt.MaterialShipDate > dtp_MaterialShipDate.MinDate && appt.MaterialShipDate < dtp_MaterialShipDate.MaxDate)
            {
                dtp_MaterialShipDate.Checked = true;
                dtp_MaterialShipDate.Value = appt.MaterialShipDate;
            }
            else
            {
                dtp_MaterialShipDate.Text = "";
                dtp_MaterialShipDate.Checked = false;
            }

            //Shipping Contact
            cmb_ShippingContactName.Text = appt.IsShippingContactNameNull() ? "" : appt.ShippingContactName;

            //Tracking Number
            txt_TrackingNumbers.Text = appt.IsShipmentTrackingNumbersNull() ? "" : appt.ShipmentTrackingNumbers;
            string trackingNumbers = txt_TrackingNumbers.Text.Replace("\r", "").Replace("\n", ",").Replace(" ", ",").TrimEnd(" ,".ToCharArray());
            while (trackingNumbers.Contains(",,"))
                trackingNumbers = trackingNumbers.Replace(",,", ",");

            linkLabel_TrackFedEx.Links.Clear();
            linkLabel_TrackUPS.Links.Clear();
            if (trackingNumbers.Length > 3)
            {
                label_TrackShipment.Enabled = true;

                linkLabel_TrackFedEx.Links.Add(new LinkLabel.Link());
                linkLabel_TrackFedEx.Links[0].LinkData = trackingNumbers;
                linkLabel_TrackFedEx.Links[0].Enabled = true;
                linkLabel_TrackFedEx.Enabled = true;

                linkLabel_TrackUPS.Links.Add(new LinkLabel.Link());
                linkLabel_TrackUPS.Links[0].LinkData = trackingNumbers;
                linkLabel_TrackUPS.Links[0].Enabled = true;
                linkLabel_TrackUPS.Enabled = true;
            }
            else
            {
                label_TrackShipment.Enabled = false;
                linkLabel_TrackFedEx.Enabled = false;
                linkLabel_TrackUPS.Enabled = false;
            }

            //Billing Contact
            cmb_BillingContactName.Text = appt.IsBillingContactNameNull() ? "" : appt.BillingContactName;

            //Class Fee
            neb_ClassFee.Text = appt.IsClassFeeNull() ? "" : appt.ClassFee.ToString();

            //Expense Type
            cmb_ExpenseType.Text = appt.IsExpenseModeNull() ? "" : appt.ExpenseMode;

            //Invoice Terms

            if (appt.IsInvoiceTermsNull())
            {
                cmb_InvoiceTerms.SelectedIndex = 0;
            }
            else
            {

                switch (appt.InvoiceTerms)
                {
                    case "After":
                        cmb_InvoiceTerms.SelectedIndex = 1;
                        break;
                    case "7":
                        cmb_InvoiceTerms.SelectedIndex = 2;
                        break;
                    case "45":
                        cmb_InvoiceTerms.SelectedIndex = 3;
                        break;
                    default:
                        cmb_InvoiceTerms.SelectedIndex = 0;
                        break;
                }
            }

            //Student Price
            neb_StudentFee.Text = appt.IsStudentPriceNull() ? "" : appt.StudentPrice.ToString();

            //IsCustomized
            ckb_IsClassCustom.Checked = appt.IsIsCustomClassNull() ? false : appt.IsCustomClass;

            //Material Version
            cmb_MaterialVersion.Text = appt.IsMaterialVersionNull() ? "" : appt.MaterialVersion;

            //Start
            if (!appt.IsStartTimeNull() && appt.StartTime > dtp_StartTime.MinDate && appt.StartTime < dtp_StartTime.MaxDate)
            {
                dtp_StartTime.Checked = true;
                dtp_StartTime.Value = appt.StartTime;
            }
            else
            {
                dtp_StartTime.Text = "";
                dtp_StartTime.Checked = false;
            }

            //End Time
            if (!appt.IsEndTimeNull() && appt.EndTime > dtp_EndTime.MinDate && appt.EndTime < dtp_EndTime.MaxDate)
            {
                dtp_EndTime.Checked = true;
                dtp_EndTime.Value = appt.EndTime;
            }
            else
            {
                dtp_EndTime.Text = "";
                dtp_EndTime.Checked = false;
            }

            //Room
            txt_Room.Text = appt.IsRoomNull() ? "" : appt.Room;

            //White Paper Sent Date
            if (!IsDateEmpty(appt.WhitePaperSentDate) && appt.WhitePaperSentDate > dtp_WhitePaperSent.MinDate && appt.WhitePaperSentDate < dtp_WhitePaperSent.MaxDate)
            {
                dtp_WhitePaperSent.Checked = true;
                dtp_WhitePaperSent.Value = appt.WhitePaperSentDate;
            }
            else
            {
                dtp_WhitePaperSent.Text = "";
                dtp_WhitePaperSent.Checked = false;
            }

            //Long Descripton
            txt_LongDescription.Text = appt.IsClassNotesNull() ? "" : appt.ClassNotes;

            //Billing Notes
            txt_BillingNotes.Text = appt.IsBillingNotesNull() ? "" : appt.BillingNotes;

            //Shipping Address
            txt_ShippingContactAddress.Text = appt.IsShippingContactStreetNull() ? "" :
                appt.ShippingContactStreet + Environment.NewLine +
                appt.ShippingContactCity + " " +
                appt.ShippingContactState + ", " +
                appt.ShippingContactZip;

        }

        private void dtp_WhitePaperSent_ValueChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_WhitePaperSent_VisibleChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_StartDate_ValueChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_StartDate_VisibleChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_EndDate_ValueChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_EndDate_VisibleChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_MaterialShipDate_ValueChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_MaterialShipDate_VisibleChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Short);
        }

        private void dtp_StartTime_ValueChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Custom, "hh:mm tt");
        }

        private void dtp_StartTime_VisibleChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Custom, "hh:mm tt");
        }

        private void dtp_EndTime_ValueChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Custom, "hh:mm tt");
        }

        private void dtp_EndTime_VisibleChanged(object sender, EventArgs e)
        {
            FormatDateTimePicker((DateTimePicker)sender, DateTimePickerFormat.Custom, "hh:mm tt");
        }

        #endregion



        #region Manage the toolbar and menus

        /// <summary>
        /// Called when any toolbar menu item is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ToolStripItemClicked(object sender, EventArgs e)
        {
            //if (e.ClickedItem.Tag == null) return;
            String tag = "";
            if (sender.GetType() == typeof(ToolStripDropDownItem))
                tag = ((ToolStripDropDownItem)sender).Tag.ToString();
            if (sender.GetType() == typeof(ToolStripButton))
                tag = ((ToolStripItem)sender).Tag.ToString();
            if (sender.GetType() == typeof(ToolStripMenuItem))
                tag = ((ToolStripMenuItem)sender).Tag.ToString();
            //((System.Windows.Forms.ToolStripDropDownItem)sender).Enabled = false;

            switch (tag)
            {
                case "Exit":
                    ExitPlease();
                    break;
                case "Timescale":
                    // this.ShowTimescaleForm();
                    break;
                case "GroupBy":
                    // this.ShowGroupsForm();
                    break;
                case "Formatting":
                    // this.ShowFormatsForm();
                    break;
                case "Settings":
                    // this.ShowSettingsForm();
                    break;
                case "Print":
                    ShowPrintForm();
                    break;
                case "Refresh":
                    if (scheduleDataSet1.IsDirty)
                    {
                        if (MessageBox.Show(
                            "You have modified the schedule.\n\n" +
                            "Click \"OK\" to load any changes that may have been made to the schedule\n" +
                            "Your modifications will not be affected unless someone else has\n" +
                            "modified the same appointment that you modified.\n\n" +
                            "or Click \"Cancel\" to continue working with the schedule.",
                            "Are You Sure?",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) != DialogResult.OK)
                        {
                            return;
                        }
                    }

                    Application.DoEvents();
                    toolStripProgressBar1.Maximum = SalesForceDataAccessor.ReloadDataStepCount + 3;
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Visible = true;
                    showProgress("Reloading...");
                    ReloadData();
                    toolStripProgressBar1.Visible = false;
                    showProgress("Ready");
                    SetMenuBarState();
                    break;
                case "ReadOnly":
                    if (((ToolStripButton)sender).Checked)
                    {
                        tlb_ReadOnly.Image = imageList1.Images["Locked"];
                        tlb_ReadOnly.Text = "Read-Only";
                        tlb_ReadOnly.ToolTipText = "Click to enable editing";
                    }
                    else
                    {
                        tlb_ReadOnly.Image = imageList1.Images["Unlocked"];
                        tlb_ReadOnly.Text = "Editable";
                        tlb_ReadOnly.ToolTipText = "Click to prevent editing";
                    }
                    SetReadOnlyMode(((ToolStripButton)sender).Checked);
                    break;
                case "Export_List":
                    new ReportEngine(this).ExportTabular();
                    break;
                case "Export_Calendar":
                    new ReportEngine(this).ExportCalendar();
                    break;
                case "Export_ListAndCalendar":
                    new ReportEngine(this).ExportCombined();
                    break;
                case "Help_ReleaseNotes":
                    new AboutBox1().ShowDialog();
                    break;
                case "Help_About":
                    new AboutBox1().ShowDialog();
                    break;
                case "Help_TipOfTheDay":
                    new TipOfTheDay().ShowDialog();
                    break;
                case "Save":
                    ApplyEventEditorToSelectedItems();
                    Application.DoEvents();
                    if (!scheduleDataSet1.IsDirty) return;
                    toolStripProgressBar1.Maximum = (2 * scheduleDataSet1.DirtyCount) + SalesForceDataAccessor.ReloadDataStepCount + 3;
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Visible = true;
                    showProgress("Saving...");
                    int recordCountSaved = 0;
                    recordCountSaved = await SalesForceDA.SaveData();
                    if (recordCountSaved > 0)
                        RenderAllTimeLineItems();

                    toolStripProgressBar1.Visible = false;
                    showProgress("Ready");
                    ShowEditor();
                    string msg;
                    switch (recordCountSaved)
                    {
                        case 0:
                            msg = "There were no modifications to save";
                            break;
                        case 1:
                            msg = "Successfully saved 1 modification to the schedule";
                            break;
                        default:
                            msg = "Successfully saved " + recordCountSaved + " modifications to the schedule";
                            break;
                    }

                    MessageBox.Show(msg, "Save Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                case "Undo":
                    if (scheduleDataSet1.IsDirty)
                    {
                        if (MessageBox.Show(
                            "Click Ok to revert all of your changes.\n\n" +
                            "Click cancel to leave your changes intact.",
                            "Are You Sure?",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.OK)
                        {
                            Application.DoEvents();
                            RevertAll();
                            SetMenuBarState();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void RevertAll()
        {
            ArrayList dirtyEmployeeIDs = new ArrayList();
            foreach (ScheduleDataSet.AppointmentsRow appt in (ScheduleDataSet.AppointmentsDataTable)(scheduleDataSet1.Appointments.GetChanges(DataRowState.Added | DataRowState.Modified)))
                if (!dirtyEmployeeIDs.Contains(appt.EmployeeID))
                    dirtyEmployeeIDs.Add(appt.EmployeeID);

            scheduleDataSet1.Appointments.RejectChanges();
            scheduleDataSet1.EmployeeList.RejectChanges();

            ValidateSchedule(dirtyEmployeeIDs);
            RenderAllTimeLineItems();
        }

        private bool ExitPlease()
        {
            if (scheduleDataSet1.IsDirty)
            {
                if (MessageBox.Show(
                    "You have not saved your changes to the schedule.\n" +
                    "          Click \"OK\" to close without saving or\n" +
                    "          Click \"Cancel\" then save your changes.",
                    "Are You Sure?",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.OK) return false;
            }
            LocalStorage.SaveState(this);
            Environment.Exit(0);
            return true;
        }

        private void SetMenuBarState()
        {
            tsb_Print.Enabled = (scheduleDataSet1.Appointments.Rows.Count > 0);
            tsb_Refresh.Enabled = (scheduleDataSet1.Appointments.Rows.Count > 0);
            tsb_Save.Enabled = (scheduleDataSet1.Appointments.Rows.Count > 0 && scheduleDataSet1.IsDirty == true);
            tsb_Undo.Enabled = (scheduleDataSet1.Appointments.Rows.Count > 0 && scheduleDataSet1.IsDirty == true);
            ddb_Export.Enabled = (scheduleDataSet1.Appointments.Rows.Count > 0);
        }

        private void SetReadOnlyMode(Boolean isreadonly)
        {
            IsReadOnly = isreadonly;
            //this.timeLine1.AllowEdit = !IsReadOnly;
            this.timeLine1.AllowDrop = !IsReadOnly;
            this.timeLine1.AllowItemDrag = IsReadOnly ? AllowItemDrag.None : AllowItemDrag.Move | AllowItemDrag.GroupChange | AllowItemDrag.Resize;
            tre_Courses.Enabled = !IsReadOnly;

            EnableEditForm(!IsReadOnly);
            EnableEventEditor(!IsReadOnly);

            RefreshTitle();
        }

        private void ShowPrintForm()
        {
            PrintOptions frm = new PrintOptions();
            frm.ShowPrintForm(this.timeLine1);
        }

        private void linkLabel_ResetToDefault_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // uiPanel_MiddleTop.Height = 100;
            IsFilterCheckEnabled = false;
            ResetTree(tre_DisplayOptions);
            IsFilterCheckEnabled = true;
            SetTimeLineScale(75);
            ApplyDisplayOptions();
        }

        /// <summary>
        /// When the user clicks the "Cancel" button next to the progress bar to cancel a background operation,
        /// this function sets a flag which is asks the background operation to exit cleanly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbl_CancelLongOperation_Click(object sender, EventArgs e)
        {
            IsLongOperationCanceled = true;
        }

        #endregion



        #region Manage the Filters

        //Redraw the timeline when user changes filter options
        private void tre_Filters_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //If we are in the middle of initializaing the filters, then dont respond to each option being changed
            if (!IsFilterCheckEnabled) return;

            //Deal with mutually exclusive children:
            if (e.Node.Name == "ClassStatusWhitePaperNotSent" && e.Node.Checked == false && tre_Filters.Nodes.Find("ClassStatusWhitePaperSent", true)[0].Checked == false)
                tre_Filters.Nodes.Find("ClassStatusWhitePaperSent", true)[0].Checked = true;
            else if (e.Node.Name == "ClassStatusWhitePaperSent" && e.Node.Checked == false && tre_Filters.Nodes.Find("ClassStatusWhitePaperNotSent", true)[0].Checked == false)
                tre_Filters.Nodes.Find("ClassStatusWhitePaperNotSent", true)[0].Checked = true;
            else
            {
                IsFilterCheckEnabled = false;
                SetChildrenCheckState(e.Node, e.Node.Checked);
                IsFilterCheckEnabled = true;
            }
            DeferLoadTimer.Tag = "ApplyFilter";
            DeferLoadTimer.Enabled = false;
            DeferLoadTimer.Interval = 10;
            DeferLoadTimer.Enabled = true;
        }

        /// <summary>
        /// Constructs TimeLineFilterConditions corresponding to the options selected
        /// in the filter option tree control and applies them to the timeline.
        /// </summary>
        private void ApplyFilter()
        {
            if (tre_Filters.Nodes.Count == 0) return;

            //Build the filter:
            var filterCondition = Filter.GetFilterCondition();

            //ArrayList selectedInstructors = null;

            ////find any newly exposed instructors
            //var instructorCondition = Filter.GetChildCondition(filterCondition, "Employees");
            //if (instructorCondition != null)
            //{
            //    selectedInstructors = instructorCondition.Value1 as ArrayList;
            //    if (selectedInstructors != null)
            //    {
            //        var previousInstructorCondition = Filter.GetChildCondition(timeLine1.FilterCondition, "Employees");
            //        if (previousInstructorCondition != null)
            //        {
            //            var previouslySelectedInstructors = previousInstructorCondition.Value1 as ArrayList;
            //            //now I have the selected and prevously selected lists
            //            foreach (var psi in previouslySelectedInstructors)
            //                if (selectedInstructors.Contains(psi))
            //                    selectedInstructors.Remove(psi);
            //        }
            //    }
            //}

            //Finally apply the filter to the timeline control
            //ToDo: re-enable the filter
            timeLine1.FilterCondition = filterCondition;

            AddHolidaysToTimeLine();

            ValidateSchedule();

            RefreshNavMap(true);
        }
        #endregion


        
        #region Manage the Date Filter

        int DateFilterValueChangedCallbackStackDepth = 0;

        private void dtp_StartDateFilter_ValueChanged(object sender, EventArgs e)
        {
            button_ApplyStartDateFilter.Visible = (timeLine1.MinDate != dtp_StartDateFilter.Value);

            //if (timeLine1.MinDate != dtp_StartDateFilter.Value)
            //    button_ApplyStartDateFilter.ImageIndex = 43;
            //else
            //    button_ApplyStartDateFilter.ImageIndex = 44;
        }

        private void dtp_EndDateFilter_ValueChanged(object sender, EventArgs e)
        {
            button_ApplyEndDateFilter.Visible = (timeLine1.MaxDate != dtp_EndDateFilter.Value);
            //if (timeLine1.MaxDate != dtp_EndDateFilter.Value)
            //    button_ApplyEndDateFilter.ImageIndex = 43;
            //else
            //    button_ApplyEndDateFilter.ImageIndex = 44;
        }

        private void ApplyStartDateFilter()
        {
            if (DateFilterValueChangedCallbackStackDepth++ == 0)
            {
                //set the min date on the timeline if possible, otherwise popup 
                DateTime newStartDate = dtp_StartDateFilter.Value;
                DateTime oldStartDate = timeLine1.MinDate;
                DateTime oldEndDate = timeLine1.MaxDate;
                if (newStartDate != oldStartDate)
                {
                    if (newStartDate < oldEndDate)
                    {
                        timeLine1.MinDate = newStartDate;

                        if (dtp_EndDateFilter.Value > newStartDate)
                            dtp_EndDateFilter.MinDate = newStartDate.AddDays(1);

                        if (newStartDate < oldStartDate)
                            ValidateSchedule();

                        RefreshNavMap(true);
                    }
                    else
                    {
                        dtp_StartDateFilter.Value = oldStartDate;
                        //MessageBox.Show("You tried to set the start date after the end date", "Date Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                button_ApplyStartDateFilter.Visible = false;
                //    button_ApplyStartDateFilter.ImageIndex = 44;
            }
            DateFilterValueChangedCallbackStackDepth--;
        }

        private void ApplyEndDateFilter()
        {
            if (DateFilterValueChangedCallbackStackDepth++ == 0)
            {
                DateTime newEndDate = dtp_EndDateFilter.Value;
                DateTime oldStartDate = timeLine1.MinDate;
                DateTime oldEndDate = timeLine1.MaxDate;

                if (newEndDate != oldEndDate)
                {
                    if (newEndDate > oldStartDate)
                    {
                        timeLine1.MaxDate = newEndDate;

                        if (dtp_StartDateFilter.Value < newEndDate)
                            dtp_StartDateFilter.MaxDate = newEndDate.AddDays(-1);

                        if (newEndDate > oldEndDate)
                            ValidateSchedule();

                        RefreshNavMap(true);
                    }
                    else
                    {
                        dtp_EndDateFilter.Value = oldEndDate;
                        //MessageBox.Show("You tried to set the end date before the start date", "Date Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                button_ApplyEndDateFilter.Visible = false;
                //button_ApplyEndDateFilter.ImageIndex = 44;
            }
            DateFilterValueChangedCallbackStackDepth--;
        }

        private void ApplyDateFilter()
        {
            ApplyStartDateFilter();
            ApplyEndDateFilter();
        }

        private void dtp_EndDateFilter_Validating(object sender, CancelEventArgs e)
        {
            ApplyEndDateFilter();
        }

        private void dtp_StartDateFilter_Validating(object sender, CancelEventArgs e)
        {
            ApplyStartDateFilter();
        }

        private void dtp_StartDateFilter_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 13)
                dtp_EndDateFilter.Focus();
        }

        private void dtp_EndDateFilter_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 13)
                dtp_StartDateFilter.Focus();
        }

        private void button_ApplyStartDateFilter_Click(object sender, EventArgs e)
        {
            ApplyStartDateFilter();
        }

        private void button_ApplyEndDateFilter_Click(object sender, EventArgs e)
        {
            ApplyEndDateFilter();
        }
        #endregion



        #region Manage the courses tree

        //Load the courses into the courses tree
        //Can be called multiple times and will only add new nodes it discovers, it wont duplicate entries
        private void InitCourses()
        {
            var publicNode = tre_Courses.Nodes.Find("Public", false)[0];
            //TreeNode onsiteNode = tre_Courses.Nodes.Find("Onsite", false)[0];
            var eventNode = tre_Courses.Nodes.Find("Events", false)[0];

            foreach (DataRowView drv in scheduleDataSet1.AppointmentCategories.DefaultView)
            {
                var course = (ScheduleDataSet.AppointmentCategoriesRow)drv.Row;
                var id = course.AppointmentCategoryID;
                var type = course.Type;
                var name = course.CategoryName;
                TreeNode newNode;

                if (id.StartsWith("PROPOSED")) continue;

                //Create the new node in the appropriate spot in the tree
                if (type == "Classes")
                    newNode = publicNode.Nodes.Add(id, name);
                //newNode = publicNode.Nodes.Add("Courses", "Courses", "Courses", "Courses").Nodes.Add(id, name);
                else if (publicNode.Nodes.ContainsKey(type))
                    newNode = publicNode.Nodes.Find(type, true)[0].Nodes.Add(id, name);
                else
                    newNode = eventNode.Nodes.Add(id, name);
                newNode.Checked = true;

                //Format the node
                if (course.FormatsRow != null)
                {
                    newNode.ForeColor = course.FormatsRow.GetForeColor(Color.Black);
                    newNode.BackColor = course.FormatsRow.GetBackColor(Color.White);
                }
                if (course.Image.Length > 0)
                    newNode.ImageKey = course.Image;
                else if (course.FormatsRow != null && !course.FormatsRow.IsBackgroundImageKeyNull())
                    newNode.ImageKey = course.FormatsRow.BackgroundImageKey;
                else if (type == "Event")
                    newNode.ImageKey = "Event";
                else if (type == "Proposed")
                    newNode.ImageKey = "";
                else
                    newNode.ImageKey = "Course";
                newNode.SelectedImageKey = newNode.ImageKey;
            }
        }


        //When user starts dragging a course from the courses tree
        private void tre_Courses_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            // Bail out if readonly
            if (IsReadOnly) return;

            //Bail out if I am not a leaf
            if (((System.Windows.Forms.TreeNode)e.Item).Nodes.Count > 0) return;
            DraggingCourse = (System.Windows.Forms.TreeNode)(e.Item);

            //IsCreatingClass = true;
            DragDropEffects dde = DoDragDrop(e.Item, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.All);

            //     DraggingCourse = null;
        }

        #endregion



        #region Manage the display options

        /// <summary>
        /// Applys the display option user interface selections to the timeline
        /// </summary>
        private void ApplyDisplayOptions()
        {
            this.Cursor = Cursors.WaitCursor;

            //Show hide the apppointment detail
            uiPanel_AppointmentDetail.Closed = !tre_DisplayOptions.Nodes["Details"].Checked;

            //Set the label of the EmployeeName option according to the selected child:
            foreach (TreeNode n in tre_DisplayOptions.Nodes.Find("EmployeeName", true)[0].Nodes)
                if (n.Checked)
                    n.Parent.Text = "Employee Name: " + n.Text;

            //Set the label of the EmployeeName option according to the selected child:
            foreach (TreeNode n in tre_DisplayOptions.Nodes.Find("FontSize", true)[0].Nodes)
                if (n.Checked)
                    n.Parent.Text = "Font Size: " + n.Text;

            //Left Menu Style:
            if (tre_DisplayOptions.Nodes.Find("LeftMenuStyleOutlook", true)[0].Checked)
            {
                tre_DisplayOptions.Nodes.Find("LeftMenuStyle", true)[0].Text = "Left Menu Style: Outlook";
                if (uiPanel_LeftMenu.GroupStyle != Janus.Windows.UI.Dock.PanelGroupStyle.OutlookNavigator)
                {
                    uiPanel_LeftMenu.GroupStyle = Janus.Windows.UI.Dock.PanelGroupStyle.OutlookNavigator;
                    uiPanel_LeftMenu.SelectedPanel = uiPanel_Options;
                }
            }
            if (tre_DisplayOptions.Nodes.Find("LeftMenuStyleTabs", true)[0].Checked)
            {
                tre_DisplayOptions.Nodes.Find("LeftMenuStyle", true)[0].Text = "Left Menu Style: Tabs";
                if (uiPanel_LeftMenu.GroupStyle != Janus.Windows.UI.Dock.PanelGroupStyle.Tab)
                {
                    uiPanel_LeftMenu.GroupStyle = Janus.Windows.UI.Dock.PanelGroupStyle.Tab;
                    uiPanel_LeftMenu.SelectedPanel = uiPanel_Options;
                }
            }


            //Change the employee display name only if it was changed
            if (timeLine1.Fields["DisplayOrder"] != null)
            {
                //Instructor name in the course editor
                cmb_InstructorName.DisplayMember = "EmployeeList." + GetEmployeeNameFieldPreference();

                //Instructor name in the event editor
                cmb_Event_Instructor.DisplayMember = "EmployeeList." + GetEmployeeNameFieldPreference();

                //Instructor anem in the timeline
                timeLine1.Fields["DisplayOrder"].ValueList.Clear();
                timeLine1.Fields["DisplayOrder"].ValueList.PopulateValueList(
                    scheduleDataSet1.EmployeeList, "DisplayOrder", GetEmployeeNameFieldPreference());

                //Instructor name in the filter
                Filter.InitEmployeeFilter();

                GroupByInstructor();
            }

            switch (GetEmployeeNameFieldPreference())
            {
                case "FirstName":
                    timeLine1.RowHeaderWidth = 100;
                    break;
                case "LastName":
                    timeLine1.RowHeaderWidth = 100;
                    break;
                case "Username":
                    timeLine1.RowHeaderWidth = 110;
                    break;
                default:
                    timeLine1.RowHeaderWidth = 140;
                    break;
            }

            timeLine1.ShowItemsEstimatedDurationBar = tre_DisplayOptions.Nodes.Find("ClassSizeBar", true)[0].Checked;
            if (tre_DisplayOptions.Nodes.Find("AllowResize", true)[0].Checked)
            {
                timeLine1.ShowItemsDurationBar = true;
                timeLine1.AllowItemDrag = timeLine1.AllowItemDrag | AllowItemDrag.Move | AllowItemDrag.GroupChange | AllowItemDrag.Resize;
            }
            else
            {
                timeLine1.ShowItemsDurationBar = false;
                timeLine1.AllowItemDrag = timeLine1.AllowItemDrag | AllowItemDrag.Move | AllowItemDrag.GroupChange;
            }

            uiPanel_NavBarTest.Closed = !tre_DisplayOptions.Nodes["NavMap"].Checked;

            foreach (System.Windows.Forms.Control c0 in pnl_Legend.Controls)
                foreach (System.Windows.Forms.Control c1 in c0.Controls)
                    if (c1.Tag != null)
                        if (c1.Tag.ToString() == "ResizeHandle")
                            c1.Visible = timeLine1.ShowItemsDurationBar;
                        else if (c1.Tag.ToString() == "ClassSizeBar")
                            c1.Visible = timeLine1.ShowItemsEstimatedDurationBar;

            RenderAllTimeLineItems();
            if (tre_DisplayOptions.Nodes["TimeLine"].Nodes["Classes"].Nodes["Label"].Nodes["Icon"].Checked != PreviousShowErrorsChecked)
            {
                PreviousShowErrorsChecked = tre_DisplayOptions.Nodes["TimeLine"].Nodes["Classes"].Nodes["Label"].Nodes["Icon"].Checked;
                ValidateSchedule();
            }

            RefreshNavMap(true);
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Initializes the label under the TimeLineScale track bar
        /// </summary>
        private void trk_TimeLineScale_Layout(object sender, LayoutEventArgs e)
        {
            lbl_TimeLineWidthLabel.Text = trk_TimeLineScale.Value.ToString();
        }

        /// <summary>
        /// Calls SettimeLineScale when the user changes the value of the TimeLine scale track bar
        /// </summary>
        private void trk_TimeLineScale_Scroll(object sender, EventArgs e)
        {
            SetTimeLineScale(trk_TimeLineScale.Value);
        }

        /// <summary>
        /// Refreshes the NavMap only after the user is done fiddling with the
        /// TimeLineScale track bar
        /// </summary>
        private void trk_TimeLineScale_MouseUp(object sender, MouseEventArgs e)
        {
            RefreshNavMap(false);
        }

        /// <summary>
        /// Sets the horizontal scale of the TimeLine
        /// </summary>
        /// <param name="value">TimeLine horizontal scale</param>
        public void SetTimeLineScale(int value)
        {
            if (trk_TimeLineScale.Value != value) trk_TimeLineScale.Value = value;
            timeLine1.IntervalSize = value;
            lbl_TimeLineWidthLabel.Text = trk_TimeLineScale.Value.ToString();
        }

        /// <summary>
        /// Handles the user checking or unchecking a node in the 
        /// Display Options tree
        /// </summary>
        private void tre_DisplayOptions_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //Prevent unintented recursion
            if (!IsFilterCheckEnabled) return;
            IsFilterCheckEnabled = false;

            //this.Cursor = Cursors.WaitCursor;

            //Changing the checkstate of a parent changes all of its children
            //Unless the parent is the "FontSize" node
            switch (e.Node.Name)
            {
                case "EmployeeName":
                    break;
                case "FontSize":
                    e.Node.Checked = false;
                    break;
                case "LeftMenuStyle":
                    e.Node.Checked = false;
                    break;
                case "Reports":
                    e.Node.Checked = false;
                    break;
                case "NavMap":
                    uiPanel_NavBarTest.Closed = !e.Node.Checked;
                    break;
                case "Details":
                    uiPanel_MiddleBottom.Closed = !e.Node.Checked;
                    break;
                default:
                    SetChildrenCheckState(e.Node, e.Node.Checked);
                    break;
            }

            IsFilterCheckEnabled = true;

            //Deal with mutually exclusive children of the "FontSize" and "EmployeeName" nodes 
            if (e.Node.Parent != null &&
               (e.Node.Parent.Name == "FontSize" ||
                e.Node.Parent.Name == "EmployeeName" ||
                e.Node.Parent.Name == "LeftMenuStyle"))
            {
                IsFilterCheckEnabled = false;
                foreach (TreeNode n in e.Node.Parent.Nodes)
                    n.Checked = false;
                e.Node.Checked = true;
                if (e.Node.Parent.Name == "FontSize")
                    e.Node.Parent.Text = "Font Size: " + e.Node.Text;
                if (e.Node.Parent.Name == "EmployeeName")
                    e.Node.Parent.Text = "Employee Name: " + e.Node.Text;
                if (e.Node.Parent.Name == "LeftMenuStyle")
                    e.Node.Parent.Text = "Left Menu Style: " + e.Node.Text;

                IsFilterCheckEnabled = true;
            }

            //Apply the changes to the display options unless it is a report option,
            //because report options dont have any effect on the timeline display
            if (IsNodeChildOf(e.Node, tre_DisplayOptions.Nodes["Reports"]) == false)
            {
                DeferLoadTimer.Tag = "ApplyDisplayOptions";
                DeferLoadTimer.Enabled = false;
                DeferLoadTimer.Interval = 750;
                DeferLoadTimer.Enabled = true;
            }


        }

        /// <summary>
        /// Returns true if the specified childNode is an decendent of the specified parent
        /// </summary>
        private bool IsNodeChildOf(TreeNode childNode, TreeNode parentNode)
        {
            if (!childNode.TreeView.Equals(parentNode.TreeView)) return false;
            while (childNode.Parent != null)
            {
                if (childNode.Parent.Equals(parentNode)) return true;
                childNode = childNode.Parent;
            }
            return false;
        }

        //return an item description string formatted according to the specified options
        /// <summary>
        /// returns a label for the specified TimeLineItem according to the 
        /// options selected in the specified TreeNode branch.  This function is
        /// is called to create both the label and the tool-tip.
        /// </summary>
        private string TimeLineLabel(TreeNode options, TimeLineItem item)
        {
            ScheduleDataSet.AppointmentsRow appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(item.DataRow)).Row);

            if (appt.AppointmentCategoriesRow == null)
                return ("appt.AppointmentCategoriesRow == null");

            ArrayList values = new ArrayList();

            //switch (appt.AppointmentCategoriesRow.Type)
            switch (appt.ClassType)
            {
                case "Violation":
                    break;
                case "Event":
                    if (options.Nodes.ContainsKey("EventName"))
                        if (options.Nodes["EventName"].Checked)
                            if (appt.AppointmentCategoriesRow != null)
                                values.Add(appt.AppointmentCategoriesRow.CategoryName);

                    if (options.Nodes.ContainsKey("Subject"))
                        if (options.Nodes["Subject"].Checked)
                            if (!appt.IsSubjectNull())
                                values.Add(appt.Subject);

                    if (options.Nodes.ContainsKey("Description"))
                        if (options.Nodes["Description"].Checked)
                            if (!appt.IsDescriptionNull())
                                values.Add(appt.Description);

                    if (options.Nodes.ContainsKey("Violations"))
                        if (options.Nodes["Violations"].Checked)
                            if (appt.ErrorLevel != ScheduleDataSet.ErrorLevels.None)
                            {
                                values.Add("............................................................");
                                foreach (ScheduleDataSet.AppointmentNotificationsRow note in appt.GetAppointmentNotificationsRows())
                                    values.Add(note.Summary);
                            }
                    break;

                default:
                    if (appt.AccountsRow != null)
                    {
                        if (options.Nodes.ContainsKey("Account"))
                            if (options.Nodes["Account"].Checked)
                                values.Add(appt.AccountsRow.Name);

                        if (options.Nodes.ContainsKey("AccountAbbreviation"))
                            if (options.Nodes["AccountAbbreviation"].Checked)
                            {
                                if (appt.AccountsRow.Abbreviation != null && appt.AccountsRow.Abbreviation.Trim().Length > 0)
                                {
                                    values.Add(appt.AccountsRow.Abbreviation);
                                }
                                else
                                {
                                    values.Add(Abbreviate(appt.AccountsRow.Name, 10));
                                }
                            }
                    }

                    if (options.Nodes.ContainsKey("Type"))
                        if (options.Nodes["Type"].Checked)
                            values.Add(appt.IsClassTypeNull() ? "Type=?" : appt.ClassType);

                    if (options.Nodes.ContainsKey("Public@Account"))
                        if (options.Nodes["Public@Account"].Checked)
                            if (!appt.IsClassTypeNull() && appt.ClassType == "Public")
                                if (!appt.IsAccountIDNull() && appt.AccountsRow != null && appt.AccountsRow.Name != "Public")
                                    values.Add("/Pub");

                    if (appt.ClassLocationsRow != null)
                    {
                        String locationInfo = "";

                        if (options.Nodes.ContainsKey("MetroArea"))
                            if (options.Nodes["MetroArea"].Checked && appt.ClassLocationsRow.MetroArea.Trim().Length > 0)
                                locationInfo += appt.ClassLocationsRow.MetroArea + " ";

                        if (options.Nodes.ContainsKey("LocationName"))
                            if (options.Nodes["LocationName"].Checked && appt.ClassLocationsRow.Name.Trim().Length > 0)
                                locationInfo += appt.ClassLocationsRow.Name + " ";

                        if (options.Nodes.ContainsKey("City"))
                            if (options.Nodes["City"].Checked && appt.ClassLocationsRow.City.Trim().Length > 0)
                                locationInfo += appt.ClassLocationsRow.City + " ";

                        if (options.Nodes.ContainsKey("State"))
                            if (options.Nodes["State"].Checked && appt.ClassLocationsRow.State.Trim().Length > 0)
                                locationInfo += appt.ClassLocationsRow.State + " ";

                        if (locationInfo.Trim().Length > 0)
                            values.Add(locationInfo.Trim());
                    }

                    if (options.Nodes.ContainsKey("Course"))
                        if (options.Nodes["Course"].Checked)
                            if (appt.AppointmentCategoriesRow != null)
                                values.Add(appt.AppointmentCategoriesRow.CategoryName);

                    if (options.Nodes.ContainsKey("CourseAbbreviation"))
                        if (options.Nodes["CourseAbbreviation"].Checked)
                            if (appt.AppointmentCategoriesRow != null)
                                values.Add(appt.AppointmentCategoriesRow.CategoryName.Substring(0, 3) + ".");

                    if (options.Nodes.ContainsKey("IsCustom"))
                        if (options.Nodes["IsCustom"].Checked)
                            if (!appt.IsIsCustomClassNull() && appt.IsCustomClass)
                                values.Add("Custom");

                    if (options.Nodes.ContainsKey("NumStudents"))
                        if (options.Nodes["NumStudents"].Checked)
                        {
                            int numStudents = 0;
                            if (!appt.IsNumStudentsNull()) numStudents = appt.NumStudents;
                            if (!appt.IsNumRegisteredNull() && appt.NumRegistered > numStudents) numStudents = appt.NumRegistered;
                            values.Add(numStudents.ToString());
                        }

                    if (options.Nodes.ContainsKey("NumMaxStudents"))
                        if (options.Nodes["NumMaxStudents"].Checked)
                        {
                            int numStudents = 0;
                            if (!appt.IsNumStudentsNull()) numStudents = appt.NumStudents;
                            if (!appt.IsNumRegisteredNull() && appt.NumRegistered > numStudents) numStudents = appt.NumRegistered;
                            String s = numStudents.ToString() + "/";

                            if (!appt.IsMaxStudentsNull())
                            {
                                s += appt.MaxStudents.ToString();
                            }
                            else if (appt.ClassLocationsRow != null)
                            {
                                if (!appt.AppointmentCategoriesRow.IsMaxStudentsNull())
                                {
                                    s += appt.AppointmentCategoriesRow.MaxStudents.ToString();
                                }
                                else
                                {
                                    s += "?";
                                }
                            }
                            else
                            {
                                s += "?";
                            }
                            values.Add(s);
                        }

                    if (options.Nodes.ContainsKey("LongDescription"))
                        if (options.Nodes["LongDescription"].Checked)
                        {
                            if (appt.AppointmentCategoriesRow != null)
                            {
                                if (!appt.AppointmentCategoriesRow.IsLongDescriptionNull())
                                {
                                    values.Add(appt.AppointmentCategoriesRow.LongDescription.ToString());
                                }
                            }
                        }

                    if (options.Nodes.ContainsKey("ClassNotes"))
                        if (options.Nodes["ClassNotes"].Checked)
                            if (!appt.IsClassNotesNull() && appt.ClassNotes.Trim().Length > 0)
                                values.Add(appt.ClassNotes);

                    if (options.Nodes.ContainsKey("Violations"))
                        if (options.Nodes["Violations"].Checked)
                            if (appt.ErrorLevel != ScheduleDataSet.ErrorLevels.None)
                            {
                                values.Add("............................................................");
                                foreach (ScheduleDataSet.AppointmentNotificationsRow note in appt.GetAppointmentNotificationsRows())
                                    values.Add(note.Summary);
                            }
                    break;
            }

            //Cludge to display /Pub instead of ,Pub:
            return CollectionToCsv(values).Replace(", /Pub", "/Pub");
        }

        /// <summary>
        /// Expands all instructor rows to expose overlapping classes when 
        /// the "Expand All" button in Display Options is clicked
        /// </summary>
        private void ExpandAllInstructors_Click(object sender, EventArgs e)
        {
            timeLine1.ExpandGroups();
        }

        /// <summary>
        /// Collapses all timeline instructor rows when the "Collaps all" 
        /// button in the Display Options is clicked
        /// </summary>
        private void CollapseAllInstructors_Click(object sender, EventArgs e)
        {
            timeLine1.CollapseGroups();
        }

        #endregion



        #region Manage the Legend
        private void InitLegend()
        {

            pnl_Legend.Controls.Clear();

            addLegendHeading("Classes");

            foreach (DataRow row in scheduleDataSet1.Formats.Select("ParentType = 'ClassStatus'", "SortOrder"))
            {
                ScheduleDataSet.FormatsRow fmt = (ScheduleDataSet.FormatsRow)row;
                string name = "", description = "";
                switch (fmt.ParentID.ToUpper())
                {
                    case "HOLD":
                        name = "Hold";
                        description = "The date and the instructor are being held pending client confirmation";
                        break;
                    case "TENTATIVE":
                        name = "Tentative";
                        description = "Waiting for contract to be signed by client or to fill a public class";
                        break;
                    case "CONFIRMED":
                        continue;
                    case "CONFIRMEDNOTSENT":
                        name = "Confirmed, Whitepaper NOT Sent";
                        description = "Customer commitment, White paper not sent to the instructor yet";
                        break;
                    case "CONFIRMEDSENT":
                        name = "Confirmed, Whitepaper Sent";
                        description = "Customer commitment/White Paper sent";
                        break;
                    default:
                        break;
                }

                addTimeLineLegendItem(
                    name, description, fmt.GetForeColor(), fmt.GetBackColor(), null,
                    fmt.IsBackgroundImageKeyNull() ? null : fmt.BackgroundImageKey,
                    timeLine1.ShowItemsEstimatedDurationBar, timeLine1.ShowItemsDurationBar);
            }

            addLegendHeading("Events");

            foreach (DataRow row in scheduleDataSet1.AppointmentCategories.Select("Type in ('Event') and AppointmentCategoryID not like 'PROPOSED%'", "SortOrder"))
            {
                ScheduleDataSet.AppointmentCategoriesRow event_type = (ScheduleDataSet.AppointmentCategoriesRow)row;

                if (event_type.FormatsRow != null)
                {

                    addTimeLineLegendItem(
                        event_type.CategoryName, event_type.Description,
                        event_type.FormatsRow.GetForeColor(), event_type.FormatsRow.GetBackColor(),
                        null, event_type.FormatsRow.IsBackgroundImageKeyNull() ? null : event_type.FormatsRow.BackgroundImageKey,
                        false, false);
                }
                else
                {
                    Log($"no format for category: {event_type.CategoryName}");
                    //Console.WriteLine($"no format for category: {event_type.CategoryName}");
                }

            }

            addLegendHeading("Schedule Optomizer Proposals");


            foreach (DataRow row in scheduleDataSet1.AppointmentCategories.Select("Type in ('Event') and AppointmentCategoryID like 'PROPOSED%'", "SortOrder"))
            {
                ScheduleDataSet.AppointmentCategoriesRow event_type = (ScheduleDataSet.AppointmentCategoriesRow)row;

                addTimeLineLegendItem(
                    event_type.CategoryName, event_type.Description,
                    event_type.FormatsRow.GetForeColor(), event_type.FormatsRow.GetBackColor(),
                    null, event_type.FormatsRow.IsBackgroundImageKeyNull() ? null : event_type.FormatsRow.BackgroundImageKey,
                    false, false);
            }

            //"Status Indicators" submenu
            addLegendHeading("Status Indicators");

            addIconLegendItem("Assignment Modified", "Assignment has been modified but not saved.", "Modified");
            addIconLegendItem("Warning (in the  past)", "Assignments that violated one or more secondary business rules.", "Warning.Unmodified.Past");
            addIconLegendItem("Warning (present/future)", "Assignments that will violate one or more secondary business rules.", "Warning.Unmodified.Future");
            addIconLegendItem("Error (in the past)", "Assignments that violated one or more primary business rules.", "Error.Unmodified.Past");
            addIconLegendItem("Error (present/future)", "Assignments that will violate one or more primary business rules.", "Error.Unmodified.Future");
            addIconLegendItem("", "", null);
        }

        private void addLegendHeading(string name)
        {
            Label l = new Label();
            l.AutoSize = true;
            l.Margin = new Padding(5);
            l.Text = name;
            l.TextAlign = ContentAlignment.MiddleLeft;
            pnl_Legend.Controls.Add(l);
        }

        private void addTimeLineLegendItem(string name, string description, Color foreColor, Color backColor, string iconKey, string backgroundImageKey, bool showClassSizeBar, bool showResizeHandle)
        {
            Panel p = new Panel();
            Panel resizeBar = null;
            Panel sizeBar = null;
            Label l = null;

            if (backColor.Name == "Transparent") backColor = Color.White;

            p.Margin = new Padding(20, 1, 20, 1);
            p.Height = 20;
            p.Dock = DockStyle.Fill;
            p.AccessibleName = name;
            p.AccessibleDescription = description;
            p.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            p.BackColor = backColor;
            p.ForeColor = AdjustColor(backColor, 1, 10);

            if (backgroundImageKey != null)
                p.BackgroundImage = imageList1.Images[backgroundImageKey];

            if (showClassSizeBar)
            {
                sizeBar = new Panel();
                sizeBar.Tag = "ClassSizeBar";
                sizeBar.Margin = new Padding(0, 0, 0, 2);
                sizeBar.BackColor = AdjustColor(backColor, 1, 10);
                sizeBar.AccessibleName = "Class Size Bar";
                sizeBar.AccessibleDescription = "Indicates number of enrolled students as a percentage of maximum class size";
                sizeBar.Location = new Point(0, 0);
                sizeBar.Size = new Size(p.Width, 7);
                sizeBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                sizeBar.MouseEnter += new EventHandler(pnl_Legend_MouseEnter);
                sizeBar.MouseLeave += new EventHandler(pnl_Legend_MouseLeave);
                p.Height += sizeBar.Height;
                p.Controls.Add(sizeBar);
            }

            if (showResizeHandle)
            {
                resizeBar = new Panel();
                resizeBar.Tag = "ResizeHandle";
                resizeBar.Margin = new Padding(0, 0, 0, 2);
                resizeBar.BackColor = AdjustColor(backColor, 1, 10);
                resizeBar.AccessibleName = "Resize Handle";
                resizeBar.AccessibleDescription = "Drag the left or right edge of this bar to change the start or end date of a class";
                resizeBar.Location = new Point(0, showClassSizeBar ? sizeBar.Height : 0);
                resizeBar.Size = new Size(p.Width, 7);
                resizeBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                resizeBar.MouseEnter += new EventHandler(pnl_Legend_MouseEnter);
                resizeBar.MouseLeave += new EventHandler(pnl_Legend_MouseLeave);

                p.Height += resizeBar.Height;
                p.Controls.Add(resizeBar);
            }

            //Add the label to the item
            l = new Label();
            l.Location = new Point(0, 3 + (showClassSizeBar ? sizeBar.Height : 0) + (showResizeHandle ? resizeBar.Height : 0));
            l.AutoSize = true;
            l.ForeColor = foreColor;
            l.Margin = new Padding(4, 1, 4, 1);
            l.Text = name;
            l.AccessibleName = name;
            l.AccessibleDescription = description;
            l.TextAlign = ContentAlignment.MiddleLeft;
            l.MouseEnter += new EventHandler(pnl_Legend_MouseEnter);
            l.MouseLeave += new EventHandler(pnl_Legend_MouseLeave);
            p.Controls.Add(l);

            p.MouseEnter += new EventHandler(pnl_Legend_MouseEnter);
            p.MouseLeave += new EventHandler(pnl_Legend_MouseLeave);
            //Add the item to the legend
            pnl_Legend.Controls.Add(p);

        }

        private void addIconLegendItem(string name, string description, string iconKey)
        {
            Label l = new Label();
            l.ImageKey = iconKey;
            l.Text = "       " + name;
            l.AccessibleName = name;
            l.AccessibleDescription = description;
            l.Margin = new Padding(2);
            l.Padding = new Padding(20, 2, 2, 2);
            l.TextAlign = ContentAlignment.MiddleLeft;
            l.ImageAlign = ContentAlignment.MiddleLeft;
            l.Dock = DockStyle.Fill;
            l.AutoSize = true;
            l.ImageList = imageList1;
            l.Location = new System.Drawing.Point(3, 5);
            l.MouseEnter += new EventHandler(pnl_Legend_MouseEnter);
            l.MouseLeave += new EventHandler(pnl_Legend_MouseLeave);
            pnl_Legend.Controls.Add(l);
        }

        private void pnl_Legend_MouseEnter(object sender, EventArgs e)
        {
            lbl_Lengend_Description_Title.Text = ((Control)sender).AccessibleName;
            txt_Legend_Description.Text = ((Control)sender).AccessibleDescription;
        }

        private void pnl_Legend_MouseLeave(object sender, EventArgs e)
        {
            ClearLegendDesription();
        }

        private void uiPanel_Legend_MouseLeave(object sender, EventArgs e)
        {
            //    ClearLegendDesription();
        }

        private void ClearLegendDesription()
        {

            lbl_Lengend_Description_Title.Text = "Mouse over an item";
            txt_Legend_Description.Text = "To view description of it";

        }
        #endregion


        #region Manage the Log

        private void linkLabel_ClearLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            rtb_Log.Clear();
        }

        private long prevTicks = DateTime.Now.Ticks;
        public void Log(string text)
        {
            long et = DateTime.Now.Ticks - prevTicks;
            rtb_Log.Text += String.Format("{0:000.000}\t{1}\n", ((float)et / ((float)(TimeSpan.TicksPerSecond))), text);
            prevTicks = DateTime.Now.Ticks;
        }

        #endregion



        #region Manage the unscheduled classes tree

        //display mouse drag effects if course is dragged over "unscheduled courses tree
        private void tre_UnscheduledClasses_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        //Handle dropping a course on the "unscheduled classes" tree
        private void tre_UnscheduledClasses_DragDrop(object sender, DragEventArgs e)
        {
            //We can only accept dragging a course
            if (DraggingCourse == null) return;

            //ToDo: why not allow dragging a class too?

            //figure out what was dragged
            String courseID = DraggingCourse.Name;
            DataRow courseDataRow = scheduleDataSet1.Tables["AppointmentCategories"].Rows.Find(courseID);
            String courseName = courseDataRow["CategoryName"].ToString();
            Double durationDays = double.Parse(courseDataRow["DurationDays"].ToString());
            DraggingCourse = null;

            //Where was it dropped
            TreeNode parentNode = tre_UnscheduledClasses.GetNodeAt(tre_UnscheduledClasses.PointToClient(new Point(e.X, e.Y)));
            if (parentNode == null) return;
            parentNode.Expand();

            //create a new appointment
            TimeLineItem tmlItem = new TimeLineItem();
            tmlItem.StartTime = DateTime.MinValue;
            tmlItem.EndTime = tmlItem.StartTime.AddDays(durationDays);
            //tmlItem.Duration = durationDays;
            tmlItem.Text = courseName;
            tmlItem.ToolTipText = "";
            tmlItem.AllDayEvent = true;
            tmlItem.FormatStyle.BackColor = Color.Green;
            tmlItem.FormatStyle.BackColorAlphaMode = AlphaMode.UseAlpha;
            tmlItem.FormatStyle.Alpha = 25;
            tmlItem.SetValue("AppointmentCategoryID", courseID);
            string guid = CreateGUID();
            tmlItem.SetValue("AppointmentID", guid);

            //Hang the appointment on the tree node's tag
            TreeNode newNode = parentNode.Nodes.Add(courseID, courseName);
            newNode.Tag = tmlItem;

            //SetDirty(true);

        }

        #endregion



        #region Manage persistance of user options

        public void LoadDisplayOptionState()
        {
            IsFilterCheckEnabled = false;
            LocalStorage.LoadDisplayOptionState(this);
            IsFilterCheckEnabled = true;
            ApplyDisplayOptions();
        }

        public void LoadFilterState()
        {
            IsFilterCheckEnabled = false;
            LocalStorage.LoadFilterState(this);
            IsFilterCheckEnabled = true;
        }


        /// <summary>
        /// retrieves a string describing the check state and expanded state of 
        /// all node children of the specified TreeView, suitable for saving to the 
        /// registry.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        //public string GetTreeViewValues(TreeView tree)
        //{
        //    string s = "";
        //    foreach (TreeNode child in tree.Nodes)
        //        s += GetTreeViewValues(child);
        //    return s;
        //}

        ///// <summary>
        ///// retrieves a string describing the check state and expanded state of 
        ///// all node children of the specified node, suitable for saving to the 
        ///// registry.
        ///// </summary>
        ///// <param name="n"></param>
        ///// <returns></returns>
        //public string GetTreeViewValues(TreeNode n)
        //{
        //    string s = "";
        //    TreeNode n0 = n;
        //    while (n0 != null)
        //    {
        //        s += n0.Name;
        //        n0 = n0.Parent;
        //        if (n0 != null) s += "\\";
        //    }
        //    s += "," + (n.Checked ? "checked" : "notchecked");
        //    s += "," + (n.IsExpanded ? "expanded" : "collapsed");
        //    s += "|";
        //    foreach (TreeNode child in n.Nodes)
        //        s += GetTreeViewValues(child);
        //    return s;
        //}

        #endregion



        #region Utility Functions


        //C# implementation of the create_guid function in sugar/include/utils.php
        public string CreateGUID()
        {
            //Ugly.
            long sec = (long)(Math.Truncate((double)(DateTime.Now.Ticks / TimeSpan.TicksPerSecond)));
            long dec = DateTime.Now.Ticks % TimeSpan.TicksPerSecond;
            string dec_hex = string.Format("{0:x4}000000", dec).Substring(0, 5);
            string sec_hex = string.Format("{0:x4}00000", sec).Substring(0, 6);
            string guid = string.Format(
                "{0}{1:x}-{2:x}-{3:x}-{4:x}-{5}{6:x}",
                dec_hex,
                randomGUIDSeed.Next(0x100, 0xfff),
                randomGUIDSeed.Next(0x1000, 0xffff),
                randomGUIDSeed.Next(0x1000, 0xffff),
                randomGUIDSeed.Next(0x1000, 0xffff),
                sec_hex,
                randomGUIDSeed.Next(0x100000, 0xffffff));
            return guid;
        }


        /// <summary>
        /// retrieves formatting information from the formats MySql table
        /// </summary>
        /// <param name="type">a value from the parent_type field of the formats table</param>
        /// <param name="id">a value from the parent_id field of the formats table</param>
        /// <returns>Colors and icon information to be applied to a user interface element</returns>
        private ScheduleDataSet.FormatsRow GetFormat(String type, String id)
        {
            return scheduleDataSet1.Formats.FindByParentTypeParentID(type, id);
        }


        /// <summary>
        /// Returns a person's full name from the first and last name
        /// </summary>
        /// <param name="first">first name</param>
        /// <param name="last">last name</param>
        /// <returns>full name</returns>
        private static String formatName(Object first, Object last)
        {
            //ToDo: Honor some user preference for name formats, ie. first last or last, first, etc.
            return (first.ToString() + " " + last.ToString());
        }


        /// <summary>
        /// returns a comma separated string from a collection
        /// </summary>
        /// <param name="o">ArrayList of values</param>
        /// <returns>Comma separated string</returns>
        private static String CollectionToCsv(ArrayList o)
        {
            String s = "";
            foreach (object i in o)
                if (i != null)
                    if (i.ToString().Length > 0)
                        s += i.ToString() + ", ";
            return s.TrimEnd(", ".ToCharArray());
        }


        /// <summary>
        /// Abbreviates the specified string by 
        /// progressivly removing spaces, noise words, vowels then consonents 
        /// from the end until the string length is less than or equal to the 
        /// specified length
        /// </summary>
        /// <param name="s">string to abbreviate</param>
        /// <param name="len">desired string length</param>
        /// <returns></returns>
        private string Abbreviate(String s, int len)
        {
            if (s.Length <= len) return s;
            s = s.Replace(" ", "");
            if (s.Length <= len) return s;
            s = s.Replace("The", "");
            if (s.Length <= len) return s;
            s = s.Replace("Corporation", "");
            if (s.Length <= len) return s;
            s = s.Replace("Incorporated", "");
            if (s.Length <= len) return s;
            s = s.Replace("Enterprises", "");
            if (s.Length <= len) return s;
            while (s.Length >= len)
            {
                int i = s.LastIndexOfAny("aeiou".ToCharArray());
                if (i < 0) break;
                s = s.Remove(i, 1);
            }
            if (s.Length > len) s = s.Substring(0, len);
            return (s);
        }


        /// <summary>
        /// Displays a label and increments a the progress bar in the 
        /// toolstrip at the very bottom of the main window
        /// </summary>
        /// <param name="label">Text to display in the toolstrip next to the progress bar</param>
        public void showProgress(string label = ".")
        {
            showProgress(label, this.toolStripProgressBar1.Value + 1);
        }


        /// <summary>
        /// Displays a label and increments a the progress bar in the 
        /// toolstrip at the very bottom of the main window
        /// </summary>
        /// <param name="label">Text to display in the toolstrip next to the progress bar</param>
        /// <param name="progressValue">Progress bar value</param>
        public void showProgress(string label, int progressValue)
        {
            Log(label);
            //            rtb_Log.Text += "\n" + DateTime.Now.ToString("HH:mm:ss.ffff") + "\t" + label;
            this.toolStripStatusLabel1.Text = label;
            if (progressValue > toolStripProgressBar1.Maximum)
                toolStripProgressBar1.Maximum = progressValue;

            this.toolStripProgressBar1.Value = progressValue;
            Application.DoEvents();
        }


        /// <summary>
        /// Returns the SortedList of visible groups
        /// </summary>
        public SortedList GetVisibleGroups()
        {
            SortedList visibleGroupList = new SortedList();
            //ToDo: re-enable this
            foreach (TimeLineItem tli in timeLine1.Items)
            {
                var instructorDisplayOrder = (short)(tli.ParentRow.Value != DBNull.Value ? tli.ParentRow.Value : (short)0);
                if (!visibleGroupList.ContainsKey(instructorDisplayOrder))
                    visibleGroupList.Add(instructorDisplayOrder, tli.ParentRow);
            }
            return visibleGroupList;
        }


        /// <summary>
        /// Sets the check state of all children of the TreeNode to the specified value
        /// </summary>
        /// <param name="n"></param>
        /// <param name="isChecked"></param>
        private void SetChildrenCheckState(TreeNode n, Boolean isChecked)
        {
            n.Checked = isChecked;
            foreach (TreeNode childNode in n.Nodes)
                SetChildrenCheckState(childNode, isChecked);
        }

        private TreeNode[] getCheckedChildren(TreeNode parentNode, Boolean checkDecendants, Boolean leavesOnly)
        {
            ArrayList checkedChildren = new ArrayList();
            foreach (TreeNode n in parentNode.Nodes)
            {
                if (n.Nodes.Count > 0 && checkDecendants == true)
                    checkedChildren.AddRange(getCheckedChildren(n, checkDecendants, leavesOnly));
                if (n.Checked)
                    if (n.Nodes.Count == 0 || !leavesOnly)
                        checkedChildren.Add(n);
            }
            return (TreeNode[])(checkedChildren.ToArray(typeof(TreeNode)));
        }


        /// <summary>
        /// applies a format string to a DateTimePicker control.  If the 
        /// checkbox of the control is not checked (i.e. the date is not 
        /// valid) then the date is not displayed
        /// </summary>
        /// <param name="dtp">control to format</param>
        /// <param name="fmt">the format to apply to valid date values</param>
        private void FormatDateTimePicker(DateTimePicker dtp, DateTimePickerFormat fmt)
        {
            FormatDateTimePicker(dtp, fmt, "");
        }

        /// <summary>
        /// applies a format string to a DateTimePicker control.  If the 
        /// checkbox of the control is not checked (i.e. the date is not 
        /// valid) then the date is formatted using a custom format
        /// </summary>
        /// <param name="dtp">control to format</param>
        /// <param name="fmt">the format to apply to valid date values</param>
        /// <param name="cFmt">the custom format string to use if the checkbox is not checked</param>
        private void FormatDateTimePicker(DateTimePicker dtp, DateTimePickerFormat fmt, String cFmt)
        {
            if (dtp.Checked)
            {
                dtp.Format = fmt;
                if (cFmt.Length > 0) dtp.CustomFormat = cFmt;
            }
            else
            {
                dtp.Format = DateTimePickerFormat.Custom;
                dtp.CustomFormat = " ";
            }
        }

        private string GetEmployeeIdFromDisplayOrder(string displayOrder)
        {
            int i;
            if (int.TryParse(displayOrder, out i))
            {
                return GetEmployeeIdFromDisplayOrder(i);
            }
            return null;
        }
        private string GetEmployeeIdFromDisplayOrder(int displayOrder)
        {
            foreach (ScheduleDataSet.EmployeeListRow ee in scheduleDataSet1.EmployeeList)
                if (ee.DisplayOrder == displayOrder)
                    return ee.EmployeeID;
            return null;
        }


        private Color AdjustColor(Color c, double lightness, double saturation)
        {

            //find range:
            int range = Max(c.R, c.G, c.B) - Min(c.R, c.G, c.B);
            int avg = Avg(c.R, c.G, c.B);
            double satuationFactor = (saturation * 100) / (255 - range);
            double lightnessFactor = 255 - (255 * lightness);

            Color newColor = Color.FromArgb(
                clipInt((c.R + ((c.R - avg) * satuationFactor) + lightnessFactor), 0, 255),
                clipInt((c.G + ((c.G - avg) * satuationFactor) + lightnessFactor), 0, 255),
                clipInt((c.B + ((c.B - avg) * satuationFactor) + lightnessFactor), 0, 255));



            return newColor;
            /*
                                    Color newColor = Color.FromArgb(
                            255,
                            (int)(c.R * percentChange),
                            (int)(c.G * percentChange),
                            (int)(c.B * percentChange));

                        Color newColor = new Color();
                        //Cant saturate gray:
                        if(c.R == c.G && c.R == c.B) return c;

                        //Find the min and max channels
                        string minChannel="", maxChannel="";
                        int minValue=0, maxValue=0,delta=0;
                        if (c.R >= c.G && c.R >= c.B) { maxChannel = "R"; maxValue = c.R; }
                        if (c.G >= c.R && c.G >= c.B) { maxChannel = "G"; maxValue = c.G; }
                        if (c.B >= c.R && c.B >= c.G) { maxChannel = "B"; maxValue = c.B; }
                        if (c.R <= c.G && c.R <= c.B) { minChannel = "G"; minValue = c.G; }
                        if (c.G <= c.R && c.G <= c.B) { minChannel = "G"; minValue = c.G; }
                        if (c.B <= c.R && c.B <= c.G) { maxChannel = "B"; minValue = c.B; }
                        delta = maxValue - minValue;


                        newColor.

                        return c;
             * 
             */
        }

        private int Max(int a, int b, int c)
        {
            if (a >= b && a >= c) return a;
            if (b >= a && b >= c) return b;
            return c;
        }

        private int Min(int a, int b, int c)
        {
            if (a <= b && a <= c) return a;
            if (b <= a && b <= c) return b;
            return c;
        }

        private int Avg(int a, int b, int c)
        {
            return (a + b + c) / 3;
        }

        private int clipInt(double value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return (int)Math.Round(value);
        }

        private TimeLineItem GetTimeLineItemFromAppointment(ScheduleDataSet.AppointmentsRow appt)
        {
            foreach (TimeLineItem tmlItem in timeLine1.Items)
                if (appt == (ScheduleDataSet.AppointmentsRow)(((DataRowView)(tmlItem.DataRow)).Row))
                    return tmlItem;
            return null;
        }

        private ScheduleDataSet.AppointmentsRow GetAppointmentFromTimeLineItem(TimeLineItem item)
        {
            if ((item == null) || (item.DataRow == null))
                return null;
            return (ScheduleDataSet.AppointmentsRow)(((DataRowView)(item.DataRow)).Row);
        }

        /// <summary>
        /// Scans all children of the specified Tree or Node and Checks nodes that end with an asterisk,
        /// and clears those without
        /// </summary>
        /// <param name="TreeOrTreeNode"></param>
        private void ResetTree(object TreeOrTreeNode)
        {
            if (TreeOrTreeNode.GetType() == typeof(TreeView))
            {
                foreach (TreeNode n in ((TreeView)(TreeOrTreeNode)).Nodes)
                {
                    n.Checked = n.Text.Trim().EndsWith("*");
                    ResetTree(n);
                }
            }
            else if (TreeOrTreeNode.GetType() == typeof(TreeNode))
            {
                foreach (TreeNode n in ((TreeNode)(TreeOrTreeNode)).Nodes)
                {
                    n.Checked = n.Text.Trim().EndsWith("*");
                    ResetTree(n);
                }
            }
        }


        #endregion


        private void DeltaPollTimer_Tick(object sender, EventArgs e)
        {
            DeltaPollTimer.Stop();
            showProgress("Polling...");
            backgroundWorker3.RunWorkerAsync();
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            //ToDo: re-enable
            //SalesForceDA.LoadData();
            //DeltaPollTimer.Start();
        }

        private void StartBackgroundDataLoad()
        {
            this.toolStripProgressBar1.Maximum = 100;
            this.toolStripProgressBar1.Value = 0;
            this.toolStripProgressBar1.Visible = true;
            showProgress("Initiating Background Data Load");
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = LoadDataIteratively(((System.ComponentModel.BackgroundWorker)sender));
        }

        private string LoadDataIteratively(System.ComponentModel.BackgroundWorker worker)
        {
            if (!SalesForceDA.IsAuthenticated()) return "Oops, I'm not authenticated!";

            DateTime BgLoadBaseDate = DateTime.Today.AddMonths(0).AddDays(1 - DateTime.Today.Day);
            DateTime BgLoadStartDate = BgLoadBaseDate;
            DateTime BgLoadEndDate;
            double totalMonths = ((TimeSpan)timeLine1.MaxDate.Subtract(timeLine1.MinDate)).TotalDays / 12;
            int monthsProcessed = 0;

            //Fetch from last month forward
            while (BgLoadStartDate < timeLine1.MaxDate)
            {
                BgLoadEndDate = BgLoadStartDate.AddMonths(1);
                long prevTicks = DateTime.Now.Ticks;
                int recordCount = SalesForceDA.AppointmentsDAO.GetClassesBetweenDates(BgLoadStartDate, BgLoadEndDate);
                BgLoadProgressMessage = String.Format(
                    "Loaded {0} records between {1:MM/dd/yy} and {2:MM/dd/yy} in {3:000.000} secs",
                    recordCount,
                    BgLoadStartDate,
                    BgLoadEndDate,
                    ((float)(DateTime.Now.Ticks - prevTicks) / TimeSpan.TicksPerSecond));
                worker.ReportProgress((int)(100.0 * ++monthsProcessed / totalMonths));
                BgLoadStartDate = BgLoadEndDate;
            }

            ////Fetch from last month backward
            //BgLoadEndDate = BgLoadBaseDate;
            //while (BgLoadStartDate >= timeLine1.MinDate)
            //{
            //    BgLoadStartDate = BgLoadEndDate.AddMonths(-1);
            //    long prevTicks = DateTime.Now.Ticks;
            //    int recordCount = SugarDA.GetClassesBetweenDates(BgLoadStartDate, BgLoadEndDate);
            //    BgLoadProgressMessage = String.Format(
            //        "Loaded {0} records between {1:MM/dd/yy} and {2:MM/dd/yy} in {3:000.000} secs",
            //        recordCount,
            //        BgLoadStartDate,
            //        BgLoadEndDate,
            //        ((float)(DateTime.Now.Ticks - prevTicks) / TimeSpan.TicksPerSecond));
            //    worker.ReportProgress((int)(100.0 * ++monthsProcessed / totalMonths));
            //    BgLoadEndDate = BgLoadStartDate;
            //}
            return "Background Data Load Complete.";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RenderAllTimeLineItems();
            RefreshNavMap(true);
            this.toolStripProgressBar1.Visible = false;
            showProgress("Background Data Load Complete");
            showProgress("Initiating Delta Polling");
            DeltaPollTimer.Start();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.toolStripProgressBar1.Visible = true;
            showProgress(BgLoadProgressMessage, e.ProgressPercentage);
            RenderAllTimeLineItems();
            ValidateSchedule();
            RefreshNavMap(true);
            Log("Processed Background Results.");
        }

        DateTime GetLastAppointmentModifiedDate()
        {
            DateTime lastModifiedDate = DateTime.MinValue;
            foreach (ScheduleDataSet.AppointmentsRow appt in scheduleDataSet1.Appointments)
                if (!appt.IsLastModifiedByNull())
                    if (appt.LastModifiedDate > lastModifiedDate)
                        lastModifiedDate = appt.LastModifiedDate;
            return lastModifiedDate;
        }

        //This one polls for changes
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!SalesForceDA.IsAuthenticated())
            {
                BgLoadProgressMessage = "Oops, I'm not authenticated!";
                e.Result = -1;
                return;
            }
            DateTime lastAppointmentModifiedDate = GetLastAppointmentModifiedDate();
            long prevTicks = DateTime.Now.Ticks;
            int recordCount = SalesForceDA.AppointmentsDAO.GetClassesModifiedSince(lastAppointmentModifiedDate);
            BgLoadProgressMessage = String.Format(
                   "Loaded {0} records modified since {1:MM/dd hh:mm:ss} in {2:000.000} secs",
                   recordCount,
                   lastAppointmentModifiedDate,
                   ((float)(DateTime.Now.Ticks - prevTicks) / TimeSpan.TicksPerSecond));
            e.Result = recordCount;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log(BgLoadProgressMessage);
            if ((int)(e.Result) > 0)
            {
                RenderAllTimeLineItems();
                ValidateSchedule();
                RefreshNavMap(true);
            }
            showProgress("Ready");
            DeltaPollTimer.Start();
        }

        private void btn_ApplyEventEditor_Click(object sender, EventArgs e)
        {
            ApplyEventEditorToSelectedItems();
        }

        private void ApplyEventEditorToSelectedItems()
        {
            if (IsLoadingEditor) return;
            if (timeLine1.SelectedItems.Count == 0) return;
            Boolean requiresValidation = false;

            //Get the last item selected
            ScheduleDataSet.AppointmentsRow appt = GetAppointmentFromTimeLineItem(((TimeLineItem)(timeLine1.SelectedItems[timeLine1.SelectedItems.Count - 1])));
            string oldAppointmentCategoryID = appt.AppointmentCategoryID;
            string newAppointmentCategoryID = cmb_Event_EventName.SelectedValue.ToString();
            string oldAppointmentClassType = appt.ClassType;
            string newAppointmentClassType = scheduleDataSet1.AppointmentCategories.FindByAppointmentCategoryID(newAppointmentCategoryID).Type;

            //This shouldn't happen because I'll catch it in the valuechaging callback
            if (newAppointmentCategoryID.StartsWith("PROPOSED"))
            {
                MessageBox.Show("Can't change to a proposed event type");
                return;
            }

            DateTime newStartDate = dtp_Event_StartDate.Value;
            DateTime newEndDate = dtp_Event_EndDate.Value;

            //Kludge the date forward if necessary
            if (newEndDate.TimeOfDay == new TimeSpan(0)) newEndDate = newEndDate.AddDays(1);

            //Must check to see if the value was modified to avoid marking the rows rowstate as "Modified" unnecessarily
            if (appt.StartDate != newStartDate)
            {
                appt.StartDate = newStartDate;
                requiresValidation = true;
            }

            if (appt.EndDate != newEndDate)
            {
                appt.EndDate = newEndDate;
                requiresValidation = true;
            }

            if (appt.Subject != txt_Event_Subject.Text)
                appt.Subject = txt_Event_Subject.Text;

            if (appt.Description != txt_Event_Description.Text)
                appt.Description = txt_Event_Description.Text;

            //If the event type has changed
            if (newAppointmentCategoryID != null && newAppointmentCategoryID != appt.AppointmentCategoryID)
            {
                requiresValidation = true;
                //If the user changes from a "Proposed" type to something else
                if (appt.AppointmentCategoryID.StartsWith("PROPOSED") && !newAppointmentCategoryID.StartsWith("PROPOSED"))
                {
                    ScheduleDataSet.AppointmentCategoriesRow appointmentCategoriesRow =
                        scheduleDataSet1.AppointmentCategories.FindByAppointmentCategoryID(
                        newAppointmentCategoryID);
                    appt = scheduleDataSet1.Appointments.AddAppointmentsRow(
                            CreateGUID(),
                            appt.EmployeeListRow,
                            appt.AccountsRow,
                            appt.ClassLocationsRow,
                            "", //Status
                            "", //Subject
                            "", //Description
                            newStartDate.Date,
                            newEndDate.Date,
                            appointmentCategoriesRow,
                            null, //appointment layout
                            null, //recurrence pattern
                            0, //numStudents
                            appointmentCategoriesRow.MaxStudents,
                            DateTime.MinValue, //whitePaperSentDate
                            "*", //room
                            newStartDate.Add(new TimeSpan(8, 30, 0)),
                            newEndDate.Add(new TimeSpan(17, 30, 0)),
                            appt.DisplayOrder,
                            "Event", DateTime.MinValue, 0, 0, "", "",
                            "", "", "", "", "", "", "", "", "", "", false,
                            "", "", "", "", "", "", "", 0, false, false,
                            scheduleDataSet1.EmployeeList.FindByEmployeeID(CurrentUser.Id), DateTime.Now);

                    timeLine1.SelectedItems.Clear();
                    timeLine1.SelectedItems.Add(GetTimeLineItemFromAppointment(appt));
                    RefreshNavMap(true);
                }
                else
                {
                    appt.AppointmentCategoryID = cmb_Event_EventName.SelectedValue.ToString();
                    appt.ClassType = "Event";
                }
            }

            //What if the employee is changed
            String oldEmployeeID = appt.EmployeeID;
            String newEmployeeID = cmb_Event_Instructor.SelectedValue.ToString();
            if (oldEmployeeID != newEmployeeID)
            {
                requiresValidation = true;
                appt.EmployeeID = newEmployeeID;
                appt.DisplayOrder = scheduleDataSet1.EmployeeList.FindByEmployeeID(newEmployeeID).DisplayOrder;
                ValidateEmployeeSchedule(oldEmployeeID);
            }

            if (requiresValidation)
            {
                if (!DirtyAppointments.Contains(appt))
                    DirtyAppointments.Enqueue(appt);

                if (oldAppointmentCategoryID == "HOLIDAY" ||
                    oldAppointmentCategoryID == "HOLIDAYWORK" ||
                    newAppointmentCategoryID == "HOLIDAY" ||
                    newAppointmentCategoryID == "HOLIDAYWORK" ||
                    newAppointmentCategoryID == "NOTAVAILABLE"
                    )
                {
                    AddHolidaysToTimeLine();
                    //                    ValidateSchedule();
                    ValidateEmployeeSchedule(newEmployeeID); //Is this better then validating the whole schedule?
                }
                else
                {
                    ValidateEmployeeSchedule(newEmployeeID);
                }
            }
            else
            {
                RenderAppointment(appt);
            }
            ShowEventEditor();
            SetMenuBarState();
        }

        private void dtp_Event_StartDate_ValueChanged(object sender, EventArgs e)
        {
            //  dtp_Event_EndDate.MinDate = dtp_Event_StartDate.Value;
            TimeSpan duration = dtp_Event_EndDate.Value.Subtract(dtp_Event_StartDate.Value);
            if (duration.Days >= nud_Event_Duration.Minimum && duration.Days < nud_Event_Duration.Maximum)
                nud_Event_Duration.Value = duration.Days;
            ApplyEventEditorToSelectedItems();
        }

        private void dtp_Event_EndDate_ValueChanged(object sender, EventArgs e)
        {
            //    dtp_Event_StartDate.MaxDate = dtp_Event_EndDate.Value;
            TimeSpan duration = dtp_Event_EndDate.Value.Subtract(dtp_Event_StartDate.Value);
            if (duration.Days < 0) return;
            if (duration.Days + 1 >= nud_Event_Duration.Minimum && duration.Days < nud_Event_Duration.Maximum)
                nud_Event_Duration.Value = duration.Days + 1;
            ApplyEventEditorToSelectedItems();
        }

        private void nud_Event_Duration_ValueChanged(object sender, EventArgs e)
        {
            var endTime = dtp_Event_EndDate.Value.TimeOfDay;
            var endDate = dtp_Event_StartDate.Value.Date.AddDays((double)(nud_Event_Duration.Value) - 1);
            dtp_Event_EndDate.Value = endDate.Add(endTime);
        }

        private void cmb_Event_EventName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_Event_EventName.SelectedItem == null) return;
            ScheduleDataSet.AppointmentCategoriesRow category = ((ScheduleDataSet.AppointmentCategoriesRow)(((DataRowView)(cmb_Event_EventName.SelectedItem)).Row));

            //if(((ScheduleDataSet.AppointmentCategoriesRow)(cmb_Event_EventName.SelectedItem.Row)) == "Violation" ((ScheduleDataSet.AppointmentCategoriesRow)(cmb_Event_EventName.SelectedItem)).CategoryName == "Off")
            if (category.Type == "Violation" && category.CategoryName == "Off")
            {
                cmb_Event_Instructor.Enabled = false;
                // cmb_Event_EventName.Enabled = false;
                txt_Event_Subject.Enabled = false;
                txt_Event_Description.Enabled = false;
                dtp_Event_StartDate.Enabled = false;
                dtp_Event_EndDate.Enabled = false;
                nud_Event_Duration.Enabled = false;
            }
            else
            {
                cmb_Event_Instructor.Enabled = true;
                //cmb_Event_EventName.Enabled = true;
                txt_Event_Subject.Enabled = true;
                txt_Event_Description.Enabled = true;
                dtp_Event_StartDate.Enabled = true;
                dtp_Event_EndDate.Enabled = true;
                nud_Event_Duration.Enabled = true;
            }
            ApplyEventEditorToSelectedItems();
        }

        private void txt_Event_Subject_TextChanged(object sender, EventArgs e)
        {
            ApplyEventEditorToSelectedItems();
        }

        private void txt_Event_Description_TextChanged(object sender, EventArgs e)
        {
            ApplyEventEditorToSelectedItems();
        }

        private void timeLine1_SelectedItemsChanged(object sender, EventArgs e)
        {
            ShowEditor();
        }

        private void btn_event_undo_Click(object sender, EventArgs e)
        {
            foreach (TimeLineItem tli in timeLine1.SelectedItems)
            {
                undoTimeLineItem(tli);
            }
            ShowEditor();
        }

        //Undo all modifications to an appointment
        private void undoTimeLineItem(TimeLineItem tli)
        {
            ScheduleDataSet.AppointmentsRow appt = GetAppointmentFromTimeLineItem(tli);
            String employeeID = appt.EmployeeID;

            if (appt.RowState == DataRowState.Added && appt.PendingDelete == true)
                appt.PendingDelete = false;
            else
                appt.RejectChanges();

            if (appt.RowState != DataRowState.Detached)
            {
                RenderAppointment(appt);
                ValidateEmployeeSchedule(employeeID);
                if (appt.EmployeeID != employeeID)
                    ValidateEmployeeSchedule(appt.EmployeeID);
            }
            RefreshNavMap(true);
        }

        private void deleteTimeLineItem(TimeLineItem tli)
        {
            ScheduleDataSet.AppointmentsRow appt = GetAppointmentFromTimeLineItem(tli);
            ScheduleDataSet.EmployeeListRow ee = appt.EmployeeListRow;

            if (appt.RowState == DataRowState.Added)
                appt.Delete();
            else
                appt.PendingDelete = !appt.PendingDelete;
            if (ee.Username == "allb2t")
            {
                AddHolidaysToTimeLine();
                ValidateSchedule();
            }
            else
                ValidateEmployeeSchedule(ee.EmployeeID);
        }

        private void btn_event_delete_Click(object sender, EventArgs e)
        {
            Queue<TimeLineItem> SelectedItems = new Queue<TimeLineItem>();
            foreach (TimeLineItem tli in timeLine1.SelectedItems)
                SelectedItems.Enqueue(tli);

            while (SelectedItems.Count > 0)
                deleteTimeLineItem(SelectedItems.Dequeue());

            ShowEditor();
        }

        private async void btn_save_Click(object sender, EventArgs e)
        {
            int recordCount = await SalesForceDA.SaveData().ConfigureAwait(false);
            if (recordCount > 0)
            {
                RenderAllTimeLineItems();
            }
        }

        /// <summary>
        /// Returns the name of the field that the user has chosen to use as the employee name
        /// </summary>
        /// <returns></returns>
        public String GetEmployeeNameFieldPreference()
        {
            foreach (TreeNode childNode in tre_DisplayOptions.Nodes.Find("EmployeeName", true)[0].Nodes)
                if (childNode.Checked)
                    return childNode.Name;
            return "FullName";
        }

        private void timeLine1_ItemDrag(object sender, Janus.Windows.TimeLine.ItemDragEventArgs e)
        {
            switch (GetAppointmentFromTimeLineItem(e.Item).AppointmentCategoriesRow.Type)
            {
                case "Event":
                    break;
                case "Old Event":
                    e.Cancel = true;
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }

        private void linkLabel_ResetFilter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //    IsFilterCheckEnabled = false;
            foreach (TreeNode n in tre_Filters.Nodes)
                n.Checked = true;
            //    IsFilterCheckEnabled = true;
            //    ApplyFilter();

            if (DateTime.Today.AddMonths(-3) < dtp_StartDateFilter.MaxDate)
                dtp_StartDateFilter.Value = DateTime.Today.AddMonths(-3);
            if (DateTime.Today.AddYears(1) > dtp_EndDateFilter.MinDate)
                dtp_EndDateFilter.Value = DateTime.Today.AddYears(1);
        }

        private void linkLabel_ReleaseNotes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new AboutBox1().ShowDialog();
        }

        private void dtp_Holiday_StartDate_ValueChanged(object sender, EventArgs e)
        {
            TimeSpan duration = dtp_Holiday_EndDate.Value.Subtract(dtp_Holiday_StartDate.Value);
            if (duration.Days < 0) return;
            nud_Holiday_Duration.Value = duration.Days + 1;
            ApplyEventEditorToSelectedItems();
        }

        private void dtp_Holiday_EndDate_ValueChanged(object sender, EventArgs e)
        {
            TimeSpan duration = dtp_Holiday_EndDate.Value.Subtract(dtp_Holiday_StartDate.Value);
            if (duration.Days < 0) return;
            nud_Holiday_Duration.Value = duration.Days + 1;
            ApplyEventEditorToSelectedItems();

        }

        private void nud_Holiday_Duration_ValueChanged(object sender, EventArgs e)
        {
            dtp_Holiday_EndDate.Value = dtp_Holiday_StartDate.Value.AddDays((double)(nud_Holiday_Duration.Value) - 1);
        }

        private void tre_Courses_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ApplyFilter();
        }

        private void ck_ShowQualifiedInstructors_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void instructorEditor1_DataChanged(object sender, EventArgs e)
        {
            SalesForceDA.SaveData();
            ValidateEmployeeSchedule(instructorEditor1.EmployeeID);
        }

        private void uiPanel_MiddleTopContainer_Resize(object sender, EventArgs e)
        {
            Log(((Control)sender).Height.ToString());
        }

        private void linkLabel_TrackFedEx_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.fedex.com/Tracking?ascend_header=1&clienttype=dotcom&mi=n&cntry_code=us&language=english&tracknumbers=" + e.Link.LinkData);
        }

        private void linkLabel_TrackUPS_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwapps.ups.com/WebTracking/track?HTMLVersion=5.0&loc=en_US&Requester=UPSHome&WBPM_lid=homepage%2Fct1.html_pnl_trk&track.x=Track&trackNums=" + e.Link.LinkData);
        }
    }
}