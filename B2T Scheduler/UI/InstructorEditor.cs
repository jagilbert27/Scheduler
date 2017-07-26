using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using B2T_Scheduler.Data;

namespace B2T_Scheduler
{
    public partial class InstructorEditor : UserControl
    {
        String modifiedByFormat = "Last modified by {0}\non {1:MM/dd/yyyy}";
        DateTime? StartDate = null;
        DateTime? EndDate = null;

        /// <summary>
        /// Format string for the modified by and date modified fields
        /// </summary>
        [Category("Behavior")]
        [Description("Format string for the modified by and date modified fields")]
        public String ModifiedByFormatString 
        { 
            get 
            { 
                return modifiedByFormat; 
            } 
            set 
            { 
                modifiedByFormat = value; 
            } 
        }

        public InstructorEditor()
        {
            InitializeComponent();
            Clear();
        }


        public ScheduleDataSet myScheduleDataSet;
        private ScheduleDataSet.EmployeeListRow employeeRow;
        //private SalesForceDataAccessor.SalesForceUser currentUserDetail = null;
        private string employeeNameFieldPreference = "FullName";

        public delegate void DescriptionChangedHandler(object sender, EventArgs e);
        [Category("Action")]
        [Description("Fires when the Description is changed.")]
        public event DescriptionChangedHandler DescriptionChanged;

        public delegate void DataChangedHandler(object sender, EventArgs e);
        [Category("Action")]
        [Description("Fires when user changes any data.")]
        public event DataChangedHandler DataChanged;


        /// <summary>
        /// The name of the field in the dataset to display for the employee
        /// </summary>
        [Category("Behavior")]
        [Description("The name of the field in the dataset to display for the employee")]
        public string NameField
        {
            get { return employeeNameFieldPreference; }
            set
            {
                if (value != null)
                    employeeNameFieldPreference = value;
            }
        }

        protected virtual void OnDataChanged(object sender, EventArgs e)
        {
            if (DataChanged != null)
                DataChanged.Invoke(this,e);
        }

        /// <summary>
        /// Reference to the schedule dataset containing the employee data
        /// </summary>
        [Category("Behavior")]
        [Description("Reference to the schedule dataset containing the employee data")]
        public ScheduleDataSet DataSet
        {
            get { return myScheduleDataSet; }
            set
            {
                myScheduleDataSet = value;
                if (myScheduleDataSet != null)
                    InitData();
            }
        }
	

        /// <summary>
        /// Employee ID to display and/or modifiy
        /// </summary>
        [Category("Behavior")]
        [Description("Employee ID to display and/or modifiy")]
        public string EmployeeID
        {
            get {
                if (employeeRow == null) return "";
                return employeeRow.EmployeeID; 
            }
            set 
            { 
                if (value == null || myScheduleDataSet == null || myScheduleDataSet.EmployeeList == null)
                    Employee = null;
                else
                    Employee = myScheduleDataSet.EmployeeList.FindByEmployeeID(value);
            }
        }

        /// <summary>
        /// Employee to display and/or modifiy
        /// </summary>
        [Category("Behavior")]
        [Description("Employee to display and/or modifiy")]
        public ScheduleDataSet.EmployeeListRow Employee
        {
            get { return employeeRow; }
            set
            {
                employeeRow = value;
            }
        }

        /// <summary>
        /// The user that will be recored in the modified by fields when a record is modified"
        /// </summary>
        [Category("Behavior")]
        [Description("The user that will be recored in the modified by fields when a record is modified")]

        public Salesforce.Common.Models.UserInfo CurrentUserDetail { get; set; }

        public void Clear()
        {
            Contact_TextBox.Clear();
            SchedulePattern_PictureBox.Image = SchedulePatternImageList.Images["0000"];
            Notes_TextBox.Clear();
            Qualifications_TextBox.Clear();
        }

        private void InitData()
        {
        //    employeeCourseQualificationsBindingSource.DataSource = myScheduleDataSet;
        //    employeeCourseQualificationsBindingSource.DataMember = "EmployeeCourseQualifications";
        //    metroAreaPreferencesBindingsource.DataSource = myScheduleDataSet;
        //    metroAreaPreferencesBindingsource.DataMember = "EmployeeMetroAreaPreferences";
        //    employeeScheduleFactorsBindingSource.DataSource = myScheduleDataSet;
        //    employeeScheduleFactorsBindingSource.DataMember = "EmployeeScheduleFactors";

        //    gridEX_InstructorQualifications.RootTable.Columns["CourseID"].DropDown.DataSource = myScheduleDataSet;
        //    gridEX_InstructorQualifications.RootTable.Columns["CourseID"].DropDown.DataMember = "AppointmentCategories";

        //    gridEX_MetroAreaPreferences.RootTable.Columns[0].DropDown.DataSource = myScheduleDataSet;
        //    gridEX_MetroAreaPreferences.RootTable.Columns[0].DropDown.DataMember = "MetroAreas";

        //    gridEX_InstructorSchedulePattern.RootTable.Columns["PatternID"].DropDown.DataSource = myScheduleDataSet;
        //    gridEX_InstructorSchedulePattern.RootTable.Columns["PatternID"].DropDown.DataMember = "SchedulePatterns";

        //    // Init schedule pattern selector dropdown menu 
        //    // by iterating over each of its rows and setting the image or imagekey property
        //    // of the name column to the image from the imageList_SchedulePatterns.
        //    foreach (Janus.Windows.GridEX.GridEXRow row in gridEX_InstructorSchedulePattern.DropDowns["DropDown_SchedulePatterns"].GetDataRows())
        //    {
        //        row.Cells["Name"].ImageKey = row.Cells["IconKey"].Text;
        //    }
        }

        public void Show(String EmployeeID, DateTime StartDate, DateTime EndDate)
        {
            this.EmployeeID = EmployeeID;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            LoadUser();
        }

        public void Show(String EmployeeID)
        {
            this.EmployeeID = EmployeeID;
            this.StartDate = null;
            this.EndDate = null;
            LoadUser();
        }


        private void LoadUser()
        {
            Clear();

            if (Employee == null)
            {
                this.Enabled = false;
                return;
            }

            this.Enabled = true;
            Contact_TextBox.Text = "";
            //Contact Info
            if (!Employee.IsAddressStreetNull())
                Contact_TextBox.Text += Employee.AddressStreet.Replace("\t", "").Replace("\r", "").Replace("\n", "") + "\r\n";

            if (!Employee.IsAddressCityNull())
                Contact_TextBox.Text += Employee.AddressCity;

            if (!Employee.IsAddressStateNull())
                Contact_TextBox.Text += " " + Employee.AddressState;

            if (!Employee.IsAddressZipNull())
                Contact_TextBox.Text += ", " + Employee.AddressZip;

            if (!Employee.IsPhoneHomeNull() && Employee.PhoneHome.Length > 0)
                Contact_TextBox.Text += "\r\n(H) " + Employee.PhoneHome;

            if (!Employee.IsPhoneMobileNull() && Employee.PhoneMobile.Length > 0)
                Contact_TextBox.Text += "\r\n(M) " + Employee.PhoneMobile;

            if (!Employee.IsEmail1Null() && employeeRow.Email1.Length > 0)
                Contact_TextBox.Text += "\r\n" + Employee.Email1;

            if (!Employee.IsEmail2Null() && employeeRow.Email2.Length > 0)
                Contact_TextBox.Text += "\r\n" + Employee.Email2;

            if (!Employee.IsDescriptionNull())
                Notes_TextBox.Text = Employee.Description;

            if (!Employee.IsLastModifiedDateNull() && !Employee.IsLastModifiedByNull())
            {
                var lastModifiedBy = myScheduleDataSet.EmployeeList.FindByEmployeeID(Employee.LastModifiedBy);
                var lastModifiedByName = lastModifiedBy == null ? Employee.LastModifiedBy : lastModifiedBy.FullName;
                NotesModifiedBy_Label.Text = String.Format(
                    modifiedByFormat,
                    lastModifiedByName,
                    Employee.LastModifiedDate);
            }
            //Pattern
            SchedulePattern_PictureBox.Image = SchedulePatternImageList.Images["0"];
            DateTime patternDate = StartDate.HasValue ? StartDate.Value : DateTime.Today;
            ScheduleDataSet.EmployeeScheduleFactorsRow esfr = getEmployeeSchedulePattern(Employee, patternDate);
            SchedulePatternModifiedBy_Label.Text = "";
            SchedulePattern_Label.Text = "";
            if (esfr != null)
            {
                SchedulePattern_PictureBox.Visible = true;
                SchedulePattern_PictureBox.Image = SchedulePatternImageList.Images[esfr.SchedulePatternsRow.IconKey];
                SchedulePattern_Label.Text = esfr.SchedulePatternsRow.Description;
                if (!esfr.IsLastModifiedByNull() & !esfr.IsLastModifiedDateNull())
                    SchedulePatternModifiedBy_Label.Text = String.Format(
                        modifiedByFormat,
                        myScheduleDataSet.EmployeeList.FindByEmployeeID(esfr.LastModifiedBy).FullName,
                        esfr.LastModifiedDate);
                SchedulePatternModifiedBy_Label.Text += " Since " + esfr.EffectiveDate.ToShortDateString();
            }
            else
            {
                SchedulePattern_PictureBox.Visible = false;
            }

            //Course Qualifications
            //Get this list of qualifications that are active for the selected appt.
            //If no appt is specified, get all current.  NO: If no date is known then get current. If date but no appt then honor that date.

            //first all:
            DateTime mostRecentModificationDate = DateTime.MinValue;
            String modString = "";
            foreach (ScheduleDataSet.EmployeeCourseQualificationsRow r in Employee.GetEmployeeCourseQualificationsRows())
            {
                DateTime qualStart = r.IsStartDateNull() ? DateTime.MinValue : r.StartDate;
                DateTime qualEnd = r.IsEndDateNull() ? DateTime.MaxValue : r.EndDate;

                if(this.StartDate == null && this.EndDate == null && IsDateBetween(DateTime.Today, qualStart, qualEnd))
                    Qualifications_TextBox.Text += qualStart.ToShortDateString() + " " + r.AppointmentCategoriesRow.Description + "\r\n";
                else if (this.StartDate != null && this.EndDate != null && DateRangeOverlap(this.StartDate, this.EndDate, qualStart, qualEnd).Days > 0)
                    Qualifications_TextBox.Text += qualStart.ToShortDateString() + " " + r.AppointmentCategoriesRow.Description + "\r\n";

                if (!r.IsLastModifiedDateNull() &! r.IsLastModifiedByNull() && r.LastModifiedDate >= mostRecentModificationDate)
                    modString = String.Format(modifiedByFormat, r.EmployeeListRowByEmployeeList_EmployeeCourseQualificationsLastModifiedBy.FullName, r.LastModifiedDate);
            }
            QualificationsModifiedBy_Label.Text = modString;


            //            employeeList-

            //Location Preferences

            //    employeeScheduleFactorsBindingSource.Filter = "EmployeeID = '" + this.employeeRow.EmployeeID + "'";
            //    employeeCourseQualificationsBindingSource.Filter = "EmployeeID = '" + this.employeeRow.EmployeeID + "'";
            //    metroAreaPreferencesBindingsource.Filter = "EmployeeID = '" + this.employeeRow.EmployeeID + "'";
            //    rtb_EmployeeNotes.Enabled = true;
            //    gridEX_InstructorSchedulePattern.Enabled = true;
            //    gridEX_InstructorQualifications.Enabled = true;
            //    gridEX_MetroAreaPreferences.Enabled = true;
            //    btn_NewCourseQualification.Enabled = true;
            //    btn_NewInstructorSchedulePattern.Enabled = true;
            //    btn_NewMetroAreaPreference.Enabled = true;


            //if (appt.EmployeeListRow.GetEmployeeScheduleFactorsRows().Length > 0)
            //{
            //    cmb_SchedulePattern.SelectedValue = String.Format("{0},{1}",
            //        appt.EmployeeListRow.GetEmployeeScheduleFactorsRows()[0].WorkWeeksInPeriod,
            //        appt.EmployeeListRow.GetEmployeeScheduleFactorsRows()[0].WeeksInPeriod -
            //        appt.EmployeeListRow.GetEmployeeScheduleFactorsRows()[0].WorkWeeksInPeriod);
            //}
            //else
            //{
            //    cmb_SchedulePattern.SelectedValue = "Select";
            //}
        }

        private ScheduleDataSet.EmployeeScheduleFactorsRow getEmployeeSchedulePattern(ScheduleDataSet.EmployeeListRow employeeListRow, DateTime? effectiveDate)
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

        //ToDo: Move this to a util class
        public TimeSpan DateRangeOverlap(DateTime? StartDate1, DateTime? EndDate1, DateTime? StartDate2, DateTime? EndDate2)
        {
            DateTime start1 = StartDate1 == null? DateTime.MinValue : (DateTime)StartDate1;
            DateTime end1 = EndDate1 == null? DateTime.MaxValue : (DateTime)EndDate1;
            DateTime start2 = StartDate2 == null? DateTime.MinValue : (DateTime)StartDate2;
            DateTime end2 = EndDate2 == null? DateTime.MaxValue : (DateTime)EndDate2;

            // 1 is totally inside 2
            if (start1 >= start2 && end1 <= end2)
                return end1.Subtract(start1); 

            // 2 is totally inside 1
            if (start1 <= start2 && end1 >= end2)
                return end2.Subtract(start2); 

            if(start1 >= start2 && end1 >= end2)
                return end2.Subtract(start1);
             
            if(start1 <= start2 && end1 <= end2)
                return end1.Subtract(start2);
       
            return new TimeSpan(0);
        }

        public Boolean IsDateBetween( DateTime? ThisDate, DateTime? RangeStartDate, DateTime? RangeEndDate)
        {
            DateTime start = RangeStartDate == null ? DateTime.MinValue : (DateTime)RangeStartDate;
            DateTime end = RangeEndDate == null ? DateTime.MaxValue : (DateTime)RangeEndDate;
            //            if(Date2 != null) return false;
            if (ThisDate >= start && ThisDate <= end) return true;
            return false;
        }

        //private void gridEX_InstructorQualifications_AddingRecord(object sender, CancelEventArgs e)
        //{
        //    ScheduleDataSet.EmployeeCourseQualificationsRow iqRow = null;
        //    try
        //    {
        //        iqRow = ((ScheduleDataSet.EmployeeCourseQualificationsRow)
        //            ((DataRowView)(gridEX_InstructorQualifications.SelectedItems[0].GetRow().DataRow)).Row);
        //    }
        //    catch
        //    {
        //        e.Cancel = true;
        //        return;
        //    }
        //    iqRow.EmployeeListRow = employeeRow;
        //    if (iqRow.IsStartDateNull()) iqRow.StartDate = DateTime.Now;
        //    if (iqRow.IsEndDateNull()) iqRow.EndDate = DateTime.MaxValue;
        //    iqRow.LastModifiedBy = CurrentUserDetail.id;
        //    iqRow.LastModifiedDate = DateTime.Now;
        //    btn_NewCourseQualification.Text = "New";
        //    gridEX_InstructorQualifications.AllowAddNew = Janus.Windows.GridEX.InheritableBoolean.False;
        //}



        private void cmb_SchedulePattern_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if(SelectedEmployeeID == null) return;
            //int workWeeksInPeriod = 0;
            //int offWeeks = 0;
            //int weeksInPeriod = 0;
            //string selectedValue = cmb_SchedulePattern.SelectedValue;
            //ScheduleDataSet.EmployeeScheduleFactorsRow scheduleFactorsRow = null;


            //if (cmb_SchedulePattern.SelectedValue != "Select")
            //{
            //    workWeeksInPeriod = int.Parse(cmb_SchedulePattern.SelectedValue.ToString().Split(",")[0]);
            //    offWeeks = int.Parse(cmb_SchedulePattern.SelectedValue.ToString().Split(",")[1]);
            //    weeksInPeriod = workWeeksInPeriod + offWeeks;
            //}


            //if (scheduleDataSet1.EmployeeList.FindByEmployeeID(SelectedEmployeeID).GetEmployeeScheduleFactorsRows().Length > 0)
            //{
            //    scheduleFactorsRow = scheduleDataSet1.EmployeeList.FindByEmployeeID(SelectedEmployeeID).GetEmployeeScheduleFactorsRows()[0];
            //    scheduleFactorsRow.WorkWeeksInPeriod = workWeeksInPeriod;
            //    scheduleFactorsRow.WeeksInPeriod = weeksInPeriod;

            //}
            //else
            //{
            //    scheduleFactorsRow = scheduleDataSet1.EmployeeScheduleFactors.AddEmployeeScheduleFactorsRow(
            //        scheduleDataSet1.EmployeeList.FindByEmployeeID(SelectedEmployeeID),
            //        workWeeksInPeriod,
            //        weeksInPeriod,
            //        DateTime.Today,
            //        false,
            //        CurrentUserDetail.id,
            //        DateTime.Now);
            //}




            //if (scheduleDataSet1.EmployeeList.FindByEmployeeID(SelectedEmployeeID).GetEmployeeScheduleFactorsRows().Length > 0)
            //{
            //    scheduleDataSet1.EmployeeList.FindByEmployeeID(SelectedEmployeeID).GetEmployeeScheduleFactorsRows()[0].WeeksInPeriod = weeksInPeriod;
            //    scheduleDataSet1.EmployeeList.FindByEmployeeID(SelectedEmployeeID).GetEmployeeScheduleFactorsRows()[0].WorkWeeksInPeriod = workWeeksInPeriod;
            //}
        }

        private void gridEX_InstructorSchedulePattern_LoadingRow(object sender, Janus.Windows.GridEX.RowLoadEventArgs e)
        {
            //Bind the Preferred Schedule Pattern PatternID column's iconKey to the ImageList's ImageKey property
            e.Row.Cells["PatternID"].ImageKey = e.Row.Cells["PatternID"].Value.ToString();
        }



        //private void btn_NewInstructorSchedulePattern_Click(object sender, EventArgs e)
        //{
        //    gridEX_InstructorSchedulePattern.AllowAddNew = Janus.Windows.GridEX.InheritableBoolean.True;
        //    btn_NewInstructorSchedulePattern.Text = "Ok";
        //}

        //private void btn_NewCourseQualification_Click(object sender, EventArgs e)
        //{
        //    gridEX_InstructorQualifications.AllowAddNew = Janus.Windows.GridEX.InheritableBoolean.True;
        //    btn_NewCourseQualification.Text = "Save";
        //}


        ////Schedule Patterns: Display last modified info
        //private void gridEX_InstructorSchedulePattern_SelectionChanged(object sender, EventArgs e)
        //{
        //    ScheduleDataSet.EmployeeScheduleFactorsRow row = null;
        //    try
        //    {
        //        row = ((ScheduleDataSet.EmployeeScheduleFactorsRow)
        //            ((DataRowView)(gridEX_InstructorSchedulePattern.SelectedItems[0].GetRow().DataRow)).Row);
        //    }
        //    catch
        //    {
        //        return;
        //    }

        //    ScheduleDataSet.EmployeeListRow eeModifiedBy = row.EmployeeListRowByEmployeeList_EmployeeScheduleFactorsLastModifiedBy;
        //    if (eeModifiedBy != null)
        //        lbl_InstructorSchedulePatternModifiedBy.Text =
        //            "Last modified by " +
        //            eeModifiedBy[employeeNameFieldPreference];
        //    else
        //        lbl_InstructorSchedulePatternModifiedBy.Text = "";

        //    if (!row.IsLastModifiedByNull())
        //        lbl_InstructorSchedulePatternModifiedDate.Text =
        //            row.LastModifiedDate.ToLongDateString();
        //    else
        //        lbl_InstructorSchedulePatternModifiedDate.Text = "";
        //}


        ////Course Qualifications: Display last modified info
        //private void gridEX_InstructorQualifications_SelectionChanged(object sender, EventArgs e)
        //{
        //    ScheduleDataSet.EmployeeCourseQualificationsRow row = null;
        //    try
        //    {
        //        row = ((ScheduleDataSet.EmployeeCourseQualificationsRow)
        //            ((DataRowView)(gridEX_InstructorQualifications.SelectedItems[0].GetRow().DataRow)).Row);
        //    }
        //    catch
        //    {
        //        return;
        //    }

        //    ScheduleDataSet.EmployeeListRow eeModifiedBy = row.EmployeeListRowByEmployeeList_EmployeeCourseQualificationsLastModifiedBy;
        //    if (eeModifiedBy != null)
        //        lbl_InstructorCourseQualificationModifiedBy.Text = 
        //            "Last modified by " +
        //            eeModifiedBy[employeeNameFieldPreference];
        //    else
        //        lbl_InstructorCourseQualificationModifiedBy.Text = "";

        //    if (!row.IsLastModifiedByNull())
        //        lbl_InstructorCourseQualificationModifiedDate.Text = 
        //            row.LastModifiedDate.ToLongDateString();
        //    else
        //        lbl_InstructorCourseQualificationModifiedDate.Text = "";
        //}


        ////Metro Area Preferences: Display last modified info
        //private void gridEX_MetroAreaPreferences_SelectionChanged(object sender, EventArgs e)
        //{
        //    ScheduleDataSet.EmployeeMetroAreaPreferencesRow row = null;
        //    try
        //    {
        //        row = ((ScheduleDataSet.EmployeeMetroAreaPreferencesRow)
        //            ((DataRowView)(gridEX_MetroAreaPreferences.SelectedItems[0].GetRow().DataRow)).Row);
        //    }
        //    catch
        //    {
        //        return;
        //    }

        //    ScheduleDataSet.EmployeeListRow eeModifiedBy = row.EmployeeListRowByEmployeeList_EmployeeMetroAreaPreferencedLastModifiedBy;

        //    if (eeModifiedBy != null)
        //        lbl_InstructorMetroAreaPreferenceModifiedBy.Text = 
        //            "Last modified by " +
        //            eeModifiedBy[employeeNameFieldPreference].ToString();
        //    else
        //        lbl_InstructorMetroAreaPreferenceModifiedBy.Text = "";

                
        //    if (!row.IsLastModifiedByNull())
        //        lbl_InstructorMetroAreaPreferenceLastModifiedDate.Text =
        //            row.LastModifiedDate.ToLongDateString();
        //    else
        //        lbl_InstructorMetroAreaPreferenceLastModifiedDate.Text = "";
        //}

        private void btn_NewInstructorSchedulePattern_Click_1(object sender, EventArgs e)
        {
            //I could either AllowAddNew or
            //Add a record to the datasource  //sposed to
            myScheduleDataSet.EmployeeScheduleFactors.AddEmployeeScheduleFactorsRow(
                this.employeeRow,
                myScheduleDataSet.SchedulePatterns.FindByPatternID("1,4"),
                0,
                0,
                DateTime.Today,
                false,
                myScheduleDataSet.EmployeeList.FindByEmployeeID(CurrentUserDetail.Id),
                DateTime.Now);
        }

        Color bg;
        private void NotesEdit_Button_Click(object sender, EventArgs e)
        {
            bg = Notes_TextBox.BackColor;
            Notes_TextBox.ReadOnly = false;
            Notes_TextBox.BackColor = Color.White;
            NotesEdit_Button.Visible = false;
            NotesOK_Button.Visible = true;
            NotesRevert_Button.Visible = true;
        }

        private void NotesOK_Button_Click(object sender, EventArgs e)
        {
            //raise the callback
            Notes_TextBox.ReadOnly = true;
            Notes_TextBox.BackColor = Notes_TextBox.Parent.BackColor;
            NotesEdit_Button.Visible = true;
            NotesOK_Button.Visible = false;
            NotesRevert_Button.Visible = false;
            if (Notes_TextBox.Text != (Employee.IsDescriptionNull() ? "" : Employee.Description))
            {
                Employee.Description = Notes_TextBox.Text;
                Employee.LastModifiedBy = CurrentUserDetail.Id;
                Employee.LastModifiedDate = DateTime.Now;
                OnDataChanged(this,e);
                LoadUser();
            }
        }

        private void NotesRevert_Button_Click(object sender, EventArgs e)
        {
            //revert to the saved text
            Notes_TextBox.Text = Employee.Description;
            Notes_TextBox.ReadOnly = true;
            NotesEdit_Button.Visible = true;
            NotesOK_Button.Visible = false;
            NotesRevert_Button.Visible = false;

        }

        private void SchedulePatternEdit_Button_Click(object sender, EventArgs e)
        {
            SchedulePatternEditor dlg = new SchedulePatternEditor();
            dlg.scheduleDataSet = myScheduleDataSet;
            dlg.EmployeeID = EmployeeID;
            dlg.CurrentUserID = CurrentUserDetail.Id;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                OnDataChanged(this, e);
                LoadUser();
            }
        }

        private void CourseQualificationEdit_Button_Click(object sender, EventArgs e)
        {
            CourseQualificationsEditor dlg = new CourseQualificationsEditor();
            dlg.scheduleDataSet = myScheduleDataSet;
            dlg.EmployeeID = EmployeeID;
            dlg.CurrentUserID = CurrentUserDetail.Id;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                OnDataChanged(this, e);
                LoadUser();
            }
        }
    }
}
