using System;
using System.Collections;
using System.Windows.Forms;
using Janus.Windows.TimeLine;
using System.Data;
using System.Text;

namespace B2T_Scheduler
{
    class ReportEngine
    {
        public string TemplatePath;
        public string CombinedTemplateFile = "CombinedTemplate.xls";
        public Double FontSize = 10.0;
        public System.Windows.Forms.IWin32Window ParentWindow;
        public System.Windows.Forms.ToolStripProgressBar ProgressBar;
        public System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        public System.Windows.Forms.ToolStripStatusLabel CancelLabel;
        public DateTime StartDate;
        public DateTime EndDate;

        public MainForm ParentForm;
        private Janus.Windows.TimeLine.TimeLine timeLine1;
        private Boolean ExcelVisible = true;

        private System.Windows.Forms.SaveFileDialog saveFileDialog1;

        public ReportEngine(MainForm parent)
        {
            ParentForm = parent;
            timeLine1 = ParentForm.timeLine1;
            ExcelVisible = ParentForm.tre_DisplayOptions.Nodes["Reports"].Nodes["ExcelVisible"].Checked;
            ProgressBar = ParentForm.toolStripProgressBar1;
            StatusLabel = ParentForm.toolStripStatusLabel1;
            CancelLabel = ParentForm.lbl_CancelLongOperation;

            //Set the font size:
            foreach (TreeNode n in ParentForm.tre_DisplayOptions.Nodes["Reports"].Nodes["FontSize"].Nodes)
                if (n.Checked)
                    FontSize = double.Parse(n.Name);

            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.DefaultExt = "xls";
            saveFileDialog1.Title = "Export to file";
            TemplatePath = Application.StartupPath + "\\Templates\\";
        }

        public void ExportTabular()
        {
            string outputFileName = "";

            if (timeLine1.Items.Count == 0)
            {
                MessageBox.Show("No Data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Fire up excell and open the template file
            Microsoft.Office.Interop.Excel.Application xl = new Microsoft.Office.Interop.Excel.Application();
            String templatePath = TemplatePath + "ScheduleTemplate.xls";

            if (!System.IO.File.Exists(templatePath))
            {
                MessageBox.Show("Error opening output template file \n" + templatePath,
                    "Missing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            xl.Workbooks.Open(templatePath, Type.Missing, true, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing);

            // Honor the "show excel..." option 
            if (ExcelVisible == true)
            {
                xl.Visible = true;
            }
            else
            {
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                outputFileName = saveFileDialog1.FileName;
            }

            bool foo = ExportTabular(xl.Worksheets[1]);

            // If we are generating the report silently, then silently save and exit
            if (xl.Visible == false && outputFileName.Length > 0)
            {
                xl.Workbooks[1].SaveAs(outputFileName, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                xl.Workbooks.Close();
                xl.Quit();
            }
            xl = null;
            ProgressBar.Visible = false;
            CancelLabel.Visible = false;
            StatusLabel.Text = "Ready";

        }

        public void ExportCalendar()
        {
            string outputFileName = "";

            if (timeLine1.Items.Count == 0)
            {
                MessageBox.Show("No Data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Fire up excel and open the template file
            Microsoft.Office.Interop.Excel.Application xl = new Microsoft.Office.Interop.Excel.Application();
            String templatePath = TemplatePath + "CalendarTemplate.xls";

            if (!System.IO.File.Exists(templatePath))
            {
                MessageBox.Show("Error opening output template file \n" + templatePath,
                    "Missing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            xl.Workbooks.Open(templatePath, Type.Missing, true, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing);

            // Honor the "show excel..." option 
            //if (this.tre_DisplayOptions.Nodes["Reports"].Nodes["ExcelVisible"].Checked)
            if (ExcelVisible == true)
            {
                xl.Visible = true;
            }
            else
            {
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                outputFileName = saveFileDialog1.FileName;
            }

            ExportCalendar((Microsoft.Office.Interop.Excel.Worksheet)xl.Worksheets[1]);

            // If we are generating the report silently, then silently save and exit
            if (xl.Visible == false && outputFileName.Length > 0)
            {
                xl.Workbooks[1].SaveAs(outputFileName, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                xl.Workbooks.Close();
                xl.Quit();
            }
            xl = null;
            ProgressBar.Visible = false;
            CancelLabel.Visible = false;
            StatusLabel.Text = "Ready";
        }

        public void ExportCombined()
        {
            string outputFileName = "";

            if (timeLine1.Items.Count == 0)
            {
                MessageBox.Show("No Data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Fire up excell and open the template file
            Microsoft.Office.Interop.Excel.Application xl = new Microsoft.Office.Interop.Excel.Application();
            String templatePath = TemplatePath + CombinedTemplateFile;

            if (!System.IO.File.Exists(templatePath))
            {
                MessageBox.Show("Error opening output template file \n" + templatePath,
                    "Missing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            xl.Workbooks.Open(templatePath, Type.Missing, true, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing);

            // Honor the "show excel..." option 
            //if (this.tre_DisplayOptions.Nodes["Reports"].Nodes["ExcelVisible"].Checked)
            if (ExcelVisible == true)
            {
                xl.Visible = true;
            }
            else
            {
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                outputFileName = saveFileDialog1.FileName;
            }


            if (ExportTabular((Microsoft.Office.Interop.Excel.Worksheet)xl.Worksheets["Schedule"]) == true)
                ExportCalendar((Microsoft.Office.Interop.Excel.Worksheet)xl.Worksheets["Calendar"]);

            // If we are generating the report silently, then silently save and exit
            if (xl.Visible == false && outputFileName.Length > 0)
            {
                xl.Workbooks[1].SaveAs(outputFileName, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                xl.Workbooks.Close();
                xl.Quit();
            }
            xl = null;
            ProgressBar.Visible = false;
            CancelLabel.Visible = false;
            StatusLabel.Text = "Ready";
        }

        private Boolean ExportTabular(Microsoft.Office.Interop.Excel.Worksheet ws)
        {
            Boolean IsSuccessful = true;
            Microsoft.Office.Interop.Excel.Range rng;
            Microsoft.Office.Interop.Excel.Range r;

            // Initialize the progress bar
            ProgressBar.Maximum = (int)timeLine1.GetItemsFromRange(
                new DateRange(timeLine1.MinDate, timeLine1.MaxDate)).Length;
            ProgressBar.Value = 0;
            ProgressBar.Visible = true;
            CancelLabel.Visible = true;

            ws.Select(Type.Missing);

            //Set the font size:
            ws.get_Range("TopLeftData", "IV65536").Font.Size = FontSize;

            // Render the report generated date
            r = ws.get_Range("DateGenerated", Type.Missing);
            r.Formula = "Generated: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

            //Start putting data here:
            rng = ws.get_Range("TopLeftData", Type.Missing);
            int colOffset = 0;
            int rowOffset = 0;

            // Draw the cell borders
            int numRows = timeLine1.GetItemsFromRange(new DateRange(timeLine1.MinDate, timeLine1.MaxDate)).Length;
            r = ws.get_Range(rng.get_Offset(rowOffset, 0), rng.get_Offset(rowOffset + numRows - 1, 2));
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].ColorIndex = 16;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].ColorIndex = 16;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = 16;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlLineStyleNone;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].ColorIndex = 15;

            r = ws.get_Range(rng.get_Offset(rowOffset, 2), rng.get_Offset(rowOffset + numRows - 1, 10));
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].ColorIndex = 15;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].ColorIndex = 16;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = 16;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical].ColorIndex = 15;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].ColorIndex = 15;

            foreach (TimeLineItem tli in timeLine1.GetItemsFromRange(new DateRange(timeLine1.MinDate, timeLine1.MaxDate)))
            {
                ParentForm.showProgress("Generating Report...");

                //Get a handle on the underlying data for this time line item
                ScheduleDataSet.AppointmentsRow appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(tli.DataRow)).Row);


                //Scroll to make the current row visible
                rng.get_Offset(rowOffset, colOffset).Select();

                if (appt.AppointmentCategoriesRow.CategoryName == "Off" || appt.AppointmentCategoriesRow.CategoryName == "Not Available")
                {
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.StartDate.ToShortDateString();
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.EndDate.AddDays(-1).ToShortDateString();
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.AppointmentCategoriesRow.CategoryName;
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.Status;
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.EmployeeListRow != null ? appt.EmployeeListRow[ParentForm.GetEmployeeNameFieldPreference()] : "?";

                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";

                    //Cell background pattern
                    if (tli.FormatStyle.BackgroundImage != null)
                    {
                        if (tli.FormatStyle.BackgroundImage.Tag != null)
                        {
                            r = ws.get_Range(rng.get_Offset(rowOffset, 0), rng.get_Offset(rowOffset, colOffset - 1));
                            switch ((string)(tli.FormatStyle.BackgroundImage.Tag))
                            {
                                case "InstructorOff":
                                    r.Interior.Pattern = Microsoft.Office.Interop.Excel.XlPattern.xlPatternGray8;
                                    r.Interior.PatternColorIndex = 48; // gray
                                    break;
                                case "InstructorNotAvailable":
                                    r.Interior.Pattern = Microsoft.Office.Interop.Excel.XlPattern.xlPatternLightUp;
                                    r.Interior.PatternColorIndex = 15; // gray
                                    break;
                            }
                        }
                    }
                }

                else if (appt.ClassType == "Event")
                {
                    int numDays = ((TimeSpan)(appt.EndDate.Subtract(appt.StartDate))).Days;
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.StartDate.ToShortDateString();
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.EndDate.AddDays(-1).ToShortDateString();
                    rng.get_Offset(rowOffset, colOffset++).Formula = "Event";
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.AppointmentCategoriesRow.CategoryName;
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.Status;
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.EmployeeListRow != null ? appt.EmployeeListRow[ParentForm.GetEmployeeNameFieldPreference()] : "?";
                    rng.get_Offset(rowOffset, colOffset++).Formula = numDays == 1 ? "1 day" : numDays + " days";
                    //rng.get_Offset(rowOffset, colOffset++).Formula = ((TimeSpan)(appt.EndDate.AddMilliseconds(-1).Subtract(appt.StartDate))).Days + " Day(s)";
                    rng.get_Offset(rowOffset, colOffset++).Formula = "";
                    r = ws.get_Range(rng.get_Offset(rowOffset, 0), rng.get_Offset(rowOffset, colOffset - 1));
                    r.Font.Italic = true;
                }
                else
                {
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.StartDate.ToShortDateString();
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.EndDate.AddDays(-1).ToShortDateString();
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.AccountsRow != null ? appt.AccountsRow.Name : "Public";
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.ClassLocationsRow != null ? (appt.ClassLocationsRow.City.Length > 0 ? appt.ClassLocationsRow.City + ", " : "") + appt.ClassLocationsRow.State : "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.ClassLocationsRow != null ? !appt.ClassLocationsRow.IsMetroAreaNull() ? appt.ClassLocationsRow.MetroArea : "" : "";
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.AppointmentCategoriesRow.CategoryName;
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.IsMaterialVersionNull() ? "" : appt.MaterialVersion;
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.NumStudents > appt.NumRegistered ? appt.NumStudents : appt.NumRegistered;
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.EmployeeListRow != null ? appt.EmployeeListRow[ParentForm.GetEmployeeNameFieldPreference()] : "?";
                    rng.get_Offset(rowOffset, colOffset++).Formula = appt.Status;
                    rng.get_Offset(rowOffset, colOffset++).Formula = !MainForm.IsDateEmpty(appt.WhitePaperSentDate) ? appt.WhitePaperSentDate.ToShortDateString() : "";
                }
                colOffset = 0;
                rowOffset++;

                if (ParentForm.IsLongOperationCanceled)
                {
                    ParentForm.IsLongOperationCanceled = false;
                    if (MessageBox.Show("Do you really want to stop generating this report?\n     Click \"Yes\" to cancel the report\n     Click \"No\" to resume generating the report.", "Cancel?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        r = rng.get_Offset(rowOffset, 1);
                        r.Formula = "Report generation canceled by user.  All above data are accurate";
                        IsSuccessful = false;
                        break;
                    }
                }
            }


            //Sort by date:
            ws.get_Range("A3", "L32000").Sort(ws.get_Range("A3", "A3"),
                Microsoft.Office.Interop.Excel.XlSortOrder.xlAscending, Type.Missing,
                Type.Missing, Microsoft.Office.Interop.Excel.XlSortOrder.xlAscending,
                Type.Missing, Microsoft.Office.Interop.Excel.XlSortOrder.xlAscending,
                Microsoft.Office.Interop.Excel.XlYesNoGuess.xlGuess, 1, false,
                Microsoft.Office.Interop.Excel.XlSortOrientation.xlSortColumns,
                Microsoft.Office.Interop.Excel.XlSortMethod.xlPinYin,
                Microsoft.Office.Interop.Excel.XlSortDataOption.xlSortTextAsNumbers,
                Microsoft.Office.Interop.Excel.XlSortDataOption.xlSortTextAsNumbers,
                Microsoft.Office.Interop.Excel.XlSortDataOption.xlSortTextAsNumbers);

            // Scroll the sheet back to the top
            ws.get_Range("A4", "A4").Select();

            return IsSuccessful;
        }

        private void ExportCalendar(Microsoft.Office.Interop.Excel.Worksheet ws)
        {
            // Adjust these to 1 and 6 to hide Saturday and Sunday
            int firstDayOfWeek = 0;
            int lastDayOfWeek = 7;
            int headColorIdx = 44;

            // Initialize the progress bar
            ProgressBar.Maximum = (int)(((TimeSpan)(timeLine1.MaxDate.Subtract(timeLine1.MinDate))).TotalDays / 7);
            ProgressBar.Value = 0;
            ProgressBar.Visible = true;
            CancelLabel.Visible = true;

            //Microsoft.Office.Interop.Excel.Application xl = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Range rng;
            Microsoft.Office.Interop.Excel.Range r;
            ws.Select(Type.Missing);

            // The start and end dates of the report are the same as the timeline
            DateTime startDate = timeLine1.MinDate.Date;
            // startDate = startDate.AddDays(1-startDate.Day); //beginning of this month
            startDate = startDate.AddDays(0 - (int)(startDate.DayOfWeek)); //back up to the beginning of the week
            DateTime endDate = timeLine1.MaxDate;


            // Get the list of employees who have data in this view.
            // These will be the only employees rendered
            SortedList visibleGroupList = ParentForm.GetVisibleGroups();

            // Render the date generated
            r = ws.get_Range("DateGenerated", Type.Missing);
            r.Formula = "Generated: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

            // Hide the warning and error legend if we are not going to display warnings & errors
            if (!ExcelVisible)
            {
                ws.get_Range("G8", Type.Missing).Formula = "";
                ws.get_Range("G9", Type.Missing).Formula = "";
            }

            //Set the font size:
            ws.get_Range("TopLeftData", "J65536").Font.Size = FontSize;

            //Start putting data here:
            rng = ws.get_Range("TopLeftData", Type.Missing);
            rng.Select();
            int colOffset = 0;
            int rowOffset = 0;

            // Iterate over every week:
            for (DateTime week = startDate; week <= endDate; week = week.AddDays(7))
            {
                ParentForm.showProgress("Generating Report");
                rng.get_Offset(rowOffset, 0).Select(); //Scroll into view.
                r = rng.get_Offset(rowOffset, 0);

                //Draw the month name text, border and background
                r.Formula = week.ToString("MMMM, yyyy");
                r.Interior.ColorIndex = headColorIdx; //Light gray?
                r.Font.Bold = true;
                r.BorderAround(Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble, Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium, Microsoft.Office.Interop.Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);

                //Week Column Heads
                for (int dow = firstDayOfWeek; dow < lastDayOfWeek; dow++)
                {
                    r = rng.get_Offset(rowOffset, dow + 1);

                    //Mark the first day of the month when it occurs within the week
                    if (week.AddDays(dow).Day == 1 && dow != firstDayOfWeek)
                    {
                        r.Formula = week.AddDays(dow).ToString("MMM dd");
                        r.Font.Bold = true;
                        r.BorderAround(Microsoft.Office.Interop.Excel.Constants.xlSolid, Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin, Microsoft.Office.Interop.Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);
                        r.Interior.ColorIndex = headColorIdx;

                        // Draw a vertical double line all the way down the week
                        r = ws.get_Range(rng.get_Offset(rowOffset, dow + 1), rng.get_Offset(rowOffset + visibleGroupList.Count, dow + 1));
                        r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].Color = 0x808080; //50% gray
                        r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble;
                    }
                    else if (ParentForm.GetHoliday(week.AddDays(dow).Date) != null)
                    {
                        var holiday = ParentForm.GetHoliday(week.AddDays(dow).Date);
                        r.Formula = week.AddDays(dow).ToString("ddd dd");
                        string comment = holiday.StartDate.ToString("ddd dd");
                        if (holiday.EndDate.Date != holiday.StartDate.Date.AddDays(1))
                            comment += " - " + holiday.EndDate.ToString("ddd dd");
                        if (!holiday.IsNameNull() && holiday.Name.Length > 0)
                            comment += "\n" + holiday.Name;
                        if (!holiday.IsDescriptionNull() && holiday.Description.Length > 0)
                            comment += "\n" + holiday.Description;
                        else
                            comment += "\nHoliday";
                        r.AddComment(comment);
                        r.Font.Bold = true;
                        r = ws.get_Range(rng.get_Offset(rowOffset, dow + 1), rng.get_Offset(rowOffset + visibleGroupList.Count, dow + 1));
                        r.Interior.ColorIndex = 10;
                    }
                    //else if (ParentForm.HolidayHash.ContainsKey(week.AddDays(dow).Date))
                    //{
                    //    r.Formula = ParentForm.HolidayHash[week.AddDays(dow).Date].ToString();
                    //    r.AddComment(week.AddDays(dow).ToString("MMM dd") + "\n" + ParentForm.HolidayHash[week.AddDays(dow).Date].ToString());
                    //    r.Font.Bold = true;
                    //    r = ws.get_Range(rng.get_Offset(rowOffset, dow + 1), rng.get_Offset(rowOffset + visibleGroupList.Count, dow + 1));
                    //    r.Interior.ColorIndex = 10;
                    //}
                    else
                    {
                        // Draw the normal week column head text, border and background color
                        r.Formula = week.AddDays(dow).ToString("ddd dd");
                        r.BorderAround(Microsoft.Office.Interop.Excel.Constants.xlSolid, Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin, Microsoft.Office.Interop.Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);
                        r.Interior.ColorIndex = headColorIdx;
                    }
                }

                // Look at each instructor which will be a row in the month
                foreach (TimeLineGroupRow g in timeLine1.GetGroupRows())
                {
                    // Dont render instructors that have no data on the calendar at all
                    if (g.Value is DBNull || !visibleGroupList.ContainsKey(g.Value)) continue;

                    rowOffset++;
                    r = rng.get_Offset(rowOffset, 0);

                    // Render the employee name
                    r.Formula = g.GroupCaption;

                    // Draw a border around the employee name
                    Int32 interalBorderColor = 0x101010;
                    r.BorderAround(Type.Missing, Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin, Microsoft.Office.Interop.Excel.XlColorIndex.xlColorIndexAutomatic, interalBorderColor);

                    // Set the background color of the instructor
                    r.Interior.ColorIndex = headColorIdx;

                    // Draw the dotted line under the whole week next to this instructor
                    r = ws.get_Range(rng.get_Offset(rowOffset, 0), rng.get_Offset(rowOffset, lastDayOfWeek - firstDayOfWeek));
                    r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlDot;
                    r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                    r.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = 48;


                    //For each day of the week
                    for (int dow = 0; dow < 7; dow++)
                    {
                        DateTime cellDate = week.AddDays(dow).Date;
                        //Iterate over every time line item for this instructor, looking for ones that belong on this day.
                        //ToDo: There must be a better way to find items for this employee for a particular day
                        foreach (TimeLineItem tli in g.GetTimeLineItems())
                        {
                            //If I find a time line item that belongs on this day
                            if ((tli.StartTime.Date == cellDate) ||
                               (dow == firstDayOfWeek && tli.StartTime.Date < cellDate && tli.EndTime.Date > cellDate))
                            {
                                Application.DoEvents();

                                DateTime tli_StartTime = cellDate;
                                DateTime tli_EndTime = tli.EndTime.Date <= week.AddDays(lastDayOfWeek) ? tli.EndTime.Date : week.AddDays(lastDayOfWeek);
                                double tli_DurationDays = ((TimeSpan)(tli_EndTime.Subtract(tli_StartTime))).TotalDays;

                                //Get a handle on the underlying data for this time line item
                                ScheduleDataSet.AppointmentsRow appt = (ScheduleDataSet.AppointmentsRow)(((DataRowView)(tli.DataRow)).Row);

                                //Which cell are we going to put this into
                                r = rng.get_Offset(rowOffset, dow + 1);

                                //Render the Class comment (This has to be done before a merge)
                                if (tli.ToolTipText != null && tli.ToolTipText.Length > 0)
                                {
                                    //ToDo: Fix this
                                    string comment = tli.ToolTipText;

                                    if (appt.RowError != null && appt.RowError.Length > 0) comment += "\n" + appt.RowError;

                                    // comment = comment.TrimEnd("\n".ToCharArray());
                                    if (r.Comment == null)
                                        r.AddComment(comment);
                                    else
                                        r.Comment.Text("\n_________________________________\n" + comment, 1000, false);


                                    if (r.Comment != null)
                                    {
                                        r.Comment.Shape.Width = 250;
                                        r.Comment.Shape.Height = r.Comment.Text("", 1, false).Split("\n".ToCharArray()).Length * 12;
                                    }
                                }


                                //if the cell is currently blank:
                                if (r.Formula.ToString().Length == 0)
                                {
                                    int prefixLength = 0;
                                    String defaultFont = r.Font.Name.ToString();
                                    int defaultColorIdx = int.Parse(r.Font.ColorIndex.ToString());
                                    Boolean defaultBold = Boolean.Parse(r.Font.Bold.ToString());

                                    //Hang a symbol on the left edge if this appt was continued from a previous week
                                    if (tli.StartTime.Date < tli_StartTime)
                                        r.Formula += "9";

                                    // Prefix with Warning Symbol
                                    if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Violation)
                                        r.Formula += "p";

                                    // Prefix with Error Symbol
                                    if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Error)
                                        r.Formula += "Ä";

                                    if (r.Formula.ToString() != "")
                                        // Put a space between the symbols and the text if necessary
                                        r.Formula += " " + tli.Text;
                                    else
                                        // Append with the actual text
                                        r.Formula += tli.Text;


                                    //Format the continuation character
                                    if (tli.StartTime.Date < tli_StartTime)
                                    {
                                        prefixLength++;
                                        r.get_Characters(prefixLength, 1).Font.ColorIndex = defaultColorIdx;
                                        r.get_Characters(prefixLength, 1).Font.Name = "Wingdings 3";
                                        r.get_Characters(prefixLength, 1).Font.Bold = true;
                                    }

                                    // Format the Warning Symbol
                                    if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Violation)
                                    {
                                        prefixLength++;
                                        r.get_Characters(prefixLength, 1).Font.Name = "Wingdings 3";
                                        r.get_Characters(prefixLength, 1).Font.Bold = false;
                                        if (appt.EndDate.Date >= DateTime.Now.Date)
                                            r.get_Characters(prefixLength, 1).Font.ColorIndex = 46; //Orange
                                        else
                                            r.get_Characters(prefixLength, 1).Font.ColorIndex = 48; // Gray
                                    }
                                    // Prefix with Error Symbol
                                    if (appt.ErrorLevel == ScheduleDataSet.ErrorLevels.Error)
                                    {
                                        prefixLength++;
                                        r.get_Characters(prefixLength, 1).Font.Name = "Wingdings 2";
                                        r.get_Characters(prefixLength, 1).Font.Bold = false;
                                        if (appt.EndDate.Date >= DateTime.Now.Date)
                                            r.get_Characters(prefixLength, 1).Font.ColorIndex = 3; // Red
                                        else
                                            r.get_Characters(prefixLength, 1).Font.ColorIndex = 48; // Gray
                                    }

                                    //Hang a symbol off the right edge to indicate an appt is continued in the next week
                                    r = rng.get_Offset(rowOffset, lastDayOfWeek + 1);
                                    if (tli.EndTime.Date > tli_EndTime)
                                    {
                                        r.Font.Name = "Wingdings 3";
                                        r.Font.Bold = true;
                                        r.Formula = "?";
                                    }
                                    else
                                    {
                                        r.Formula = "' ";
                                    }

                                    //Merge the cells to the proper number of days
                                    r = ws.get_Range(rng.get_Offset(rowOffset, dow + 1), rng.get_Offset(rowOffset, dow + tli_DurationDays));
                                    if (appt.Status != "Off")
                                        r.MergeCells = true;

                                    //Draw a border around the class
                                    r.BorderAround(Type.Missing, Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin, Microsoft.Office.Interop.Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);

                                    //Copy background color and pattern from index:
                                    Microsoft.Office.Interop.Excel.Range legendCell = null;
                                    try
                                    {
                                        if (appt.Status == "Confirmed" & !MainForm.IsDateEmpty(appt.WhitePaperSentDate))
                                            legendCell = ws.get_Range("ConfirmedSent", Type.Missing);
                                        else if (appt.Status == "Confirmed" && MainForm.IsDateEmpty(appt.WhitePaperSentDate))
                                            legendCell = ws.get_Range("ConfirmedNotSent", Type.Missing);
                                        else
                                            legendCell = ws.get_Range(appt.Status.Replace(" ", ""), Type.Missing);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            legendCell = ws.get_Range(appt.AppointmentCategoriesRow.CategoryName.Replace(" ", ""), Type.Missing);
                                        }
                                        catch { }
                                    }

                                    if (legendCell != null)
                                    {

                                        r.Interior.ColorIndex = legendCell.Interior.ColorIndex;
                                        r.Interior.Pattern = legendCell.Interior.Pattern;
                                        r.Interior.PatternColorIndex = legendCell.Interior.PatternColorIndex;
                                        if (r.Interior.Pattern.ToString() == "1")
                                            r.Font.Bold = false;
                                        else
                                            r.Font.Bold = true;
                                    }
                                    else
                                    {
                                        if (tli.FormatStyle.BackColor != null & !tli.FormatStyle.BackColor.IsEmpty)
                                            r.Interior.Color =
                                            ((tli.FormatStyle.BackColor.R) * 0x000001) +
                                            ((tli.FormatStyle.BackColor.G) * 0x000100) +
                                            ((tli.FormatStyle.BackColor.B) * 0x010000);
                                        else
                                        {
                                            r.Interior.ColorIndex = 2; //white
                                        }


                                        //Cell background pattern
                                        if (tli.FormatStyle.BackgroundImage != null)
                                        {
                                            if (tli.FormatStyle.BackgroundImage.Tag != null)
                                            {
                                                switch ((string)(tli.FormatStyle.BackgroundImage.Tag))
                                                {
                                                    case "InstructorOff":
                                                        r.Interior.Pattern = Microsoft.Office.Interop.Excel.XlPattern.xlPatternGray8;
                                                        r.Interior.PatternColorIndex = 48; // gray
                                                        break;
                                                    case "InstructorNotAvailable":
                                                        r.Interior.Pattern = Microsoft.Office.Interop.Excel.XlPattern.xlPatternLightUp;
                                                        r.Interior.PatternColorIndex = 48; // gray
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //Draw the double line around the interiour of the month
                r = ws.get_Range(rng.get_Offset(rowOffset - visibleGroupList.Count + 1, 1), rng.get_Offset(rowOffset, colOffset + lastDayOfWeek - firstDayOfWeek));
                r.BorderAround(Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble, Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium, Microsoft.Office.Interop.Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);

                //Draw the thick line around the month
                r = ws.get_Range(rng.get_Offset(rowOffset - visibleGroupList.Count, 0), rng.get_Offset(rowOffset, colOffset + lastDayOfWeek - firstDayOfWeek));
                r.BorderAround(Type.Missing, Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium, Microsoft.Office.Interop.Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);
                rowOffset++;
                rowOffset++;

                if (ParentForm.IsLongOperationCanceled)
                {
                    ParentForm.IsLongOperationCanceled = false;
                    if (MessageBox.Show("Do you really want to stop generating this report?\n     Click \"Yes\" to cancel the report\n     Click \"No\" to resume generating the report.", "Cancel?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        r = rng.get_Offset(rowOffset, 1);
                        r.Formula = "Report generation canceled by user.  All above data are accurate";
                        break;
                    }
                }
            }


            // Scroll the sheet back to the top
            ws.get_Range("A1", "A1").Select();

        }

    }
}
