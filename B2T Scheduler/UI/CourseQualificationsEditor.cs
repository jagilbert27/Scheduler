using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.GridEX;

namespace B2T_Scheduler
{
    public partial class CourseQualificationsEditor : Form
    {
        public string EmployeeID { get; set; }
        public string CurrentUserID { get; set; }
//        private string courseFilter = "Type in ('Core','Additional','Management')";
        private string courseFilter = "Type not in ('Event')";
        private string courseSort = "SortOrder";

        public CourseQualificationsEditor()
        {
            InitializeComponent();
        }

        private void CourseQualificationsEditor_Load(object sender, EventArgs e)
        {
            this.gridEX1.DataSource = scheduleDataSet.EmployeeCourseQualifications;

            gridEX1.RootTable.DynamicFiltering = InheritableBoolean.True;

            GridEXFilterCondition filters = new GridEXFilterCondition();

            filters.AddCondition(LogicalOperator.And, new GridEXFilterCondition(
                gridEX1.RootTable.Columns["Deleted"], ConditionOperator.Equal, false));

            filters.AddCondition(new GridEXFilterCondition(
                gridEX1.RootTable.Columns["EmployeeID"], ConditionOperator.Equal, EmployeeID));

            gridEX1.RootTable.FilterCondition = filters;

            ////Filter to show only the appropriate employee
            //gridEX1.RootTable.FilterCondition = new GridEXFilterCondition(
            //    gridEX1.RootTable.Columns["EmployeeID"], ConditionOperator.Equal, EmployeeID);

            //Configure the value list dropdown for the schedule pattern
            GridEXColumn courseIdColumn = gridEX1.RootTable.Columns["CourseID"];
            courseIdColumn.HasValueList = true;
            courseIdColumn.ColumnType = ColumnType.ImageAndText;
            courseIdColumn.EditType = EditType.DropDownList;
            foreach (ScheduleDataSet.AppointmentCategoriesRow row in scheduleDataSet.AppointmentCategories.Select(courseFilter,courseSort))
                courseIdColumn.ValueList.Add(
                    row.AppointmentCategoryID,
                    row.Type + ": " + row.CategoryName);

            //Configure the lookup for the Modified By column
            GridEXColumn modifiedByColumn = gridEX1.RootTable.Columns["LastModifiedBy"];
            modifiedByColumn.HasValueList = true;
            modifiedByColumn.ColumnType = ColumnType.Text;
            modifiedByColumn.EditType = EditType.NoEdit;
            foreach (ScheduleDataSet.EmployeeListRow row in scheduleDataSet.EmployeeList)
                modifiedByColumn.ValueList.Add(row.EmployeeID, row.FullName);

            //Make the "Last Modfied By Date" not editable
            gridEX1.RootTable.Columns["LastModifiedDate"].EditType = EditType.NoEdit;
        }

        private void Add_Button_Click(object sender, EventArgs e)
        {
            //find a default course:
            string defaultNewCategoryID = null;
            foreach (ScheduleDataSet.AppointmentCategoriesRow row in scheduleDataSet.AppointmentCategories.Select(courseFilter, courseSort))
            {
                if (scheduleDataSet.EmployeeCourseQualifications.FindByEmployeeIDCourseID(EmployeeID, row.AppointmentCategoryID) == null)
                {
                    defaultNewCategoryID = row.AppointmentCategoryID;
                    break;
                }
            }
            if (defaultNewCategoryID == null)
                return;

            //Add a row with default settings - The user can then change it.
            ScheduleDataSet.EmployeeCourseQualificationsRow newRow = scheduleDataSet.EmployeeCourseQualifications.NewEmployeeCourseQualificationsRow();
            newRow.EmployeeID = EmployeeID;
            newRow.AppointmentCategoriesRow = scheduleDataSet.AppointmentCategories.FindByAppointmentCategoryID(defaultNewCategoryID);
            newRow.StartDate = DateTime.Today;
            newRow.QualificationLevel = 1;
            newRow.Deleted = false;
            newRow.LastModifiedBy = CurrentUserID;
            newRow.LastModifiedDate = DateTime.Now;
            scheduleDataSet.EmployeeCourseQualifications.AddEmployeeCourseQualificationsRow(newRow);
        }

        private void Delete_Button_Click(object sender, EventArgs e)
        {
            if (gridEX1.SelectedItems.Count == 0)
                return;

            DataRowView drv = ((DataRowView)(gridEX1.SelectedItems[0].GetRow().DataRow));

            ScheduleDataSet.EmployeeCourseQualificationsRow row = ((ScheduleDataSet.EmployeeCourseQualificationsRow)(drv.Row));



            //ScheduleDataSet.EmployeeCourseQualificationsRow row = 
            //    ((ScheduleDataSet.EmployeeCourseQualificationsRow)(gridEX1.SelectedItems[0].GetRow().DataRow));

            row.Delete();

            // gridEX1.SelectedItems[0].GetRow().Delete();
            // Why cant I just delete the row from the ds and call update?!
            // because I want to wait till the user hits ok.
            // But I can reject changes!

        }
        
        private void Ok_Button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            scheduleDataSet.EmployeeCourseQualifications.RejectChanges();
        }

    }
}
