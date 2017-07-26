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
    public partial class SchedulePatternEditor : Form
    {
        public string EmployeeID { get; set; }
        public string CurrentUserID { get; set; }

        public SchedulePatternEditor()
        {
            InitializeComponent();
        }

        private void SchedulePatternEditor_Load(object sender, EventArgs e)
        {
            this.gridEX1.DataSource = scheduleDataSet.EmployeeScheduleFactors;

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
            GridEXColumn patternColumn = gridEX1.RootTable.Columns["PatternID"];
            patternColumn.HasValueList = true;
            patternColumn.ColumnType = ColumnType.ImageAndText;
            patternColumn.EditType = EditType.DropDownList;
            foreach (ScheduleDataSet.SchedulePatternsRow row in scheduleDataSet.SchedulePatterns)
                patternColumn.ValueList.Add(
                    row.PatternID,
                    row.Description,
                    scheduleDataSet.SchedulePatterns.FindByPatternID(row.PatternID).IconKey);

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

        private void AddButton_Click(object sender, EventArgs e)
        {
            //Add a row with default settings - The user can then change it.
            DateTime defaultDate = DateTime.Today;
            while (scheduleDataSet.EmployeeScheduleFactors.FindByEmployeeIDEffectiveDate(EmployeeID, defaultDate) != null)
                defaultDate = defaultDate.AddDays(1);

            scheduleDataSet.EmployeeScheduleFactors.AddEmployeeScheduleFactorsRow(
                scheduleDataSet.EmployeeList.FindByEmployeeID(EmployeeID),
                scheduleDataSet.SchedulePatterns.FindByPatternID("0,0"),
                0, 0, defaultDate, false,
                scheduleDataSet.EmployeeList.FindByEmployeeID(CurrentUserID),
                DateTime.Now);
        }

        private void Delete_Button_Click(object sender, EventArgs e)
        {
            if (gridEX1.SelectedItems.Count == 0)
                return;

            gridEX1.SelectedItems[0].GetRow().Delete();
        }

        private void Ok_Button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            scheduleDataSet.EmployeeScheduleFactors.RejectChanges();
        }

        private void gridEX1_DropDownHide(object sender, DropDownHideEventArgs e)
        {
            if (e.ValueSelected)
                foreach (GridEXSelectedItem item in gridEX1.SelectedItems)
                {
                    item.GetRow().Cells["LastModifiedBy"].Value = CurrentUserID;
                    item.GetRow().Cells["LastModifiedDate"].Value = DateTime.Now;
                }
        }


    }
}
