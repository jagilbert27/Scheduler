using System.Drawing;

namespace B2T_Scheduler
{


    public partial class ScheduleDataSet
    {
        partial class FormatCategoriesDataTable
        {
        }

        partial class EmployeeMetroAreaPreferencesDataTable
        {
        }

        partial class EmployeeScheduleFactorsDataTable
        {
        }

        partial class AccountEmployeePreferencesDataTable
        {
        }

        partial class EmployeeListDataTable
        {
        }

        partial class PreferenceTypesDataTable
        {
        }



        partial class FormatsDataTable
        {
        }

        partial class EmployeeCourseQualificationsDataTable
        {
        }

        partial class AppointmentNotificationsDataTable
        {
        }

        partial class AppointmentCategoriesDataTable
        {
        }

        public bool IsDirty
        {
            get
            {
                if (this.Appointments.GetChanges(System.Data.DataRowState.Modified) != null) return true;
                if (this.Appointments.GetChanges(System.Data.DataRowState.Added) != null) return true;
                if (this.EmployeeList.GetChanges(System.Data.DataRowState.Modified) != null) return true;
                if (this.EmployeeScheduleFactors.GetChanges(System.Data.DataRowState.Modified) != null) return true;
                if (this.EmployeeScheduleFactors.GetChanges(System.Data.DataRowState.Added) != null) return true;
                if (this.EmployeeCourseQualifications.GetChanges(System.Data.DataRowState.Modified) != null) return true;
                if (this.EmployeeCourseQualifications.GetChanges(System.Data.DataRowState.Added) != null) return true;
                if (this.AccountEmployeePreferences.GetChanges(System.Data.DataRowState.Modified) != null) return true;
                if (this.AccountEmployeePreferences.GetChanges(System.Data.DataRowState.Added) != null) return true;

                return false;
            }
        }


        public int DirtyCount
        {
            get
            {
                int dirtyCount = 0;
                if (this.Appointments.GetChanges(System.Data.DataRowState.Added) != null)
                    dirtyCount += this.Appointments.GetChanges(System.Data.DataRowState.Added).Rows.Count;
                if (this.Appointments.GetChanges(System.Data.DataRowState.Modified) != null)
                    dirtyCount += this.Appointments.GetChanges(System.Data.DataRowState.Modified).Rows.Count;
                return dirtyCount;
            }
        }

        public enum ErrorLevels
        {
            None = 0,
            Info = 1,
            PotentialRecalc = 2,
            PotentialViolation = 3,
            Violation = 4,
            Error = 5
        }


        partial class EventCategoriesDataTable
        {
        }

        partial class AppointmentsDataTable
        {
        }
        public partial class EventsRow
        {
        }

        public partial class FormatsRow
        {
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public Color GetBackColor()
            {
                //If you dont choose a default color, a stupid one will be provided for you
                return GetBackColor(Color.Fuchsia);
            }

            /// <summary>
            /// Efficiently calculates and caches the background color specified in the formats table
            /// </summary>
            /// <param name="defaultColor">the color to be returned if the background color is not defined</param>
            /// <returns></returns>
            public Color GetBackColor(Color defaultColor)
            {
                if (IsBackColorNull())
                    if (!IsBackcolorNameNull())
                        BackColor = (Color)new ColorConverter().ConvertFromString(BackcolorName);
                    else
                        BackColor = defaultColor;
                return BackColor;
            }

            public Color GetForeColor()
            {
                //If you dont choose a default color, a stupid one will be provided for you
                return GetForeColor(Color.Yellow);
            }

            public Color GetForeColor(Color defaultColor)
            {
                if (IsForeColorNull())
                    if (!IsForecolorNameNull())
                        ForeColor = (Color)new ColorConverter().ConvertFromString(ForecolorName);
                    else
                        ForeColor = defaultColor;
                return ForeColor;
            }

            public Image GetBackgroundImage(System.Windows.Forms.ImageList imageList)
            {
                if (IsBackgroundImageKeyNull())
                    return null;
                return imageList.Images[BackgroundImageKey];
            }

            public Image GetBackgroundImage(System.Windows.Forms.ImageList imageList, string defaultKey)
            {
                if (IsBackgroundImageKeyNull())
                    return imageList.Images[defaultKey];
                return imageList.Images[BackgroundImageKey];
            }
        }


        public partial class AppointmentsRow
        {
            private System.Collections.Hashtable errorHash;
            private System.Collections.SortedList errorList;


            /// <summary>
            /// Returns true if the user is allowed to edit this item
            /// </summary>
            public bool IsEditable
            {
                get { return (this.AppointmentCategoriesRow.Type != "Classes"); }
            }


            /// <summary>
            /// Returns the error level of the most severe error
            /// </summary>
            public ErrorLevels ErrorLevel
            {
                get
                {
                    ErrorLevels maxSeverity = ErrorLevels.None;
                    foreach (AppointmentNotificationsRow note in this.GetAppointmentNotificationsRows())
                        if (note.Severity > (short)maxSeverity) maxSeverity = (ErrorLevels)note.Severity;
                    return maxSeverity;
                }
            }

            public bool IsEvent
            {
                get
                {
                    if (AppointmentCategoriesRow == null) return false;
                    if (AppointmentCategoriesRow.Type.ToUpper() == "CLASSES") return false;
                    return true;
                }
            }

            public bool IsProposed
            {
                get
                {
                    if (this.AppointmentCategoriesRow == null) return false;
                    if (this.AppointmentCategoriesRow.AppointmentCategoryID.StartsWith("PROPOSED")) return true;
                    return false;
                }

            }
            /// <summary>
            /// Clears the error level and erases all errors
            /// </summary>
            //public void ClearErrors()
            //{
            //    this.RowError = "";
            //}



            /// <summary>
            /// Adds an error to this appointment
            /// </summary>
            /// <param name="errorLevel">severity level of the error</param>
            /// <param name="description">description of the error</param>
            //public void AddError( ErrorLevels errorLevel, string description)
            //{
            //    string msg = "";
            //    switch (errorLevel)
            //    {
            //        case ErrorLevels.None:
            //            msg = "FYI:" + description;
            //            break;
            //        case ErrorLevels.Violation:
            //            msg = "Warning:" + description;
            //            break;
            //        case ErrorLevels.Error:
            //            msg = "Error:" + description;
            //            break;
            //    }
            //    if(this.RowError.Length == 0)
            //        this.RowError = msg;
            //    else if (this.RowError.IndexOf(msg) < 0)
            //        this.RowError += "|" + msg;
            //}

            /// <summary>
            /// Returns an array of strings describing each error
            /// </summary>
            /// <returns></returns>
            //public string[] GetErrors()
            //{
            //    if (this.RowError.Length == 0) return null;
            //    return this.RowError.Split("|".ToCharArray());
            //    // return this.RowError.Split("|".ToCharArray().To
            //}
        }

        public partial class EmployeeListRow
        {
            /// <summary>
            /// Returns the severity level of the most severe error of all future appointments
            /// </summary>
            public ErrorLevels ErrorLevel
            {
                get
                {
                    ErrorLevels maxErrorLevel = ErrorLevels.None;
                    foreach (ScheduleDataSet.AppointmentsRow appt in this.GetAppointmentsRows())
                        if (appt.EndDate > System.DateTime.Now)
                            if (appt.ErrorLevel > maxErrorLevel)
                                maxErrorLevel = appt.ErrorLevel;
                    return maxErrorLevel;
                }
            }
        }
    }
}
