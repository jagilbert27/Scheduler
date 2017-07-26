using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using B2T_Scheduler.com.b2ttraining.www.prod;
using System.Collections;
using System.Collections.Specialized;

namespace B2T_Scheduler
{
    public class SoapSugarDataAccessor
    {
        sugarsoap sugarClient;
        string sessionId;
        string error;
        public ScheduleDataSet DataSet;
        public MainForm ParentForm;
        public user_detail[] SugarUserDetails;
        public user_detail CurrentUserDetail;
        //Create an authentication object
        private user_auth CurrentUser = new user_auth();

        public string Error
        {
            get
            {
                return this.error;
            }
        }

        public SoapSugarDataAccessor(MainForm parent)
        {
            ParentForm = parent;

            //Create a new instance of the client proxy
            this.sugarClient = new sugarsoap();

            //Set the default value
            this.sessionId = String.Empty;
        }
        
        public bool Authenticate(string Username, string Password)
        {
            ParentForm.showProgress("Logging in to sugar...");

            //Set the credentials
            CurrentUser.user_name = Username;
            CurrentUser.password = this.computeMD5String(Password);

            //Try to authenticate
            set_entry_result authentication_result = this.sugarClient.login(CurrentUser, "");

            //Check for errors
            if (Convert.ToInt32(authentication_result.error.number) != 0)
            {
                //An error occured
                this.error = String.Concat(authentication_result.error.name, ": ",
                authentication_result.error.description);
                //Clear the existing sessionId
                this.sessionId = String.Empty;
            }
            else
            {
                //Set the sessionId
                this.sessionId = authentication_result.id;
                CurrentUserDetail = GetUserById(sugarClient.get_user_id(this.sessionId));

                //Clear the existing error
                this.error = String.Empty;
                ParentForm.showProgress(String.Format("Recognized User: {0} {1}", CurrentUserDetail.first_name, CurrentUserDetail.last_name));
            }
            //Return the boolean
            return (this.sessionId != String.Empty);
        }

        public bool IsAuthenticated()
        {
            return (this.sessionId != String.Empty);
        }

        public user_detail GetUserById(string user_id)
        {
            if(SugarUserDetails == null || SugarUserDetails.Length == 0)
                SugarUserDetails = sugarClient.user_list(CurrentUser.user_name, CurrentUser.password);

            foreach(user_detail u in SugarUserDetails)
                if (u.id == user_id)
                    return u;

            return null;
        }


        public void LoadData()
        {

            //showProgress("Loading Class Locations...");
            //LoadClassLocations();

            //showProgress("Loading Instructors...");
            //LoadInstructors();

            //showProgress("Loading Courses...");
            //LoadCourses();

            //showProgress("Loading Accounts...");
            //LoadAccounts();

            //showProgress("Loading Holidays...");
            //LoadHolidays();
            //AddHolidaysToTimeLine();


                ParentForm.showProgress("Loading Events...");
                ParentForm.Log(GetEvents(new DateTime(2007, 1, 1), DateTime.MaxValue));

                ParentForm.showProgress("Loading Classes...");
                ParentForm.Log(GetClassesBetweenDates(new DateTime(2007, 1, 1), new DateTime(2007, 2, 1)).ToString());


                //ParentForm.showProgress("Loading more classes in the background...");
            //backgroundWorker1.RunWorkerAsync();
            //LoadDataIteratively();


        }

        public string GetEvents(DateTime startDate, DateTime endDate)
        {
            string query = "parent_type <> 'Holiday'"; //This is probably wrong

            if (startDate > DateTime.MinValue)
                query += " AND date_start >= '" + startDate.ToString("yyyy-MM-dd")+"'";

            if (endDate < DateTime.MaxValue)
                query += " AND date_end >= '" + endDate.ToString("yyyy-MM-dd") + "'";

            return (GetMeetings("", null, query, "", 0, 10000, false));
        }

        public string GetMeetings(string SessionId, sugarsoap SugarSoap,
            string Query, string OrderBy, int Offset, int MaxResults, bool GetDeleted)
        {
            string result = "";
            StringDictionary fieldMappings = new StringDictionary();
            fieldMappings.Add("id", "AppointmentID");
            fieldMappings.Add("instructor_b2t_id", "DisplayOrder"); //Need to fetch this
            fieldMappings.Add("parent_id", "AppointmentCategoryID");
            fieldMappings.Add("assigned_user_id", "EmployeeID");
            fieldMappings.Add("name", "Status");
            fieldMappings.Add("date_start", "StartDate");
            fieldMappings.Add("date_end", "EndDate");
            fieldMappings.Add("time_start", "StartTime");
            fieldMappings.Add("time_end", "EndTime");
            fieldMappings.Add("description", "ClassNotes");
            string[] fieldNames = new string[fieldMappings.Count];
            fieldMappings.Keys.CopyTo(fieldNames, 0);

            try
            {
                long prevTicks = DateTime.Now.Ticks;
                get_entry_list_result entryList = this.sugarClient.get_entry_list(this.sessionId, "Meetings",
                Query, OrderBy, Offset, fieldNames, MaxResults, Convert.ToInt32(GetDeleted));
                result += String.Format("Loaded {0} records in {1:000.000} seconds", entryList.result_count, ((float)(DateTime.Now.Ticks - prevTicks) / TimeSpan.TicksPerSecond));

                //Loop trough the entries
                foreach (entry_value entry in entryList.entry_list)
                {
                    ScheduleDataSet.AppointmentsRow appt = DataSet.Appointments.NewAppointmentsRow();

                    foreach (name_value nameValue in entry.name_value_list)
                    {
                        if (DataSet.Appointments.Columns[fieldMappings[nameValue.name]] != null)
                        {
                            switch (DataSet.Appointments.Columns[fieldMappings[nameValue.name]].DataType.Name)
                            {
                                case "DateTime":
                                    DateTime dateValue;
                                    if (DateTime.TryParse(nameValue.value, out dateValue))
                                        appt[fieldMappings[nameValue.name]] = dateValue;
                                    break;
                                case "Int16":
                                    int intValue;
                                    if (int.TryParse(nameValue.value, out intValue))
                                        appt[fieldMappings[nameValue.name]] = intValue;
                                    break;

                                case "Decimal":
                                    decimal decimalValue;
                                    if (decimal.TryParse(nameValue.value, out decimalValue))
                                        appt[fieldMappings[nameValue.name]] = decimalValue;
                                    break;

                                case "Boolean":
                                    bool boolValue;
                                    if (bool.TryParse(nameValue.value, out boolValue))
                                        appt[fieldMappings[nameValue.name]] = boolValue;
                                    else
                                        appt[fieldMappings[nameValue.name]] = false;
                                    break;

                                case "String":
                                    appt[fieldMappings[nameValue.name]] = nameValue.value;
                                    break;
                                default:
                                    appt[fieldMappings[nameValue.name]] = nameValue.value;
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unknown Field:" + nameValue.name);
                        }
                    }

                    DataSet.Appointments.AddAppointmentsRow(appt);
                }


            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public int GetClassesModifiedSince(DateTime modifiedDate)
        {
            string query = "classes.date_modified > '" + modifiedDate.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            return GetClasses("", null, query, "", 0, 10000, false);
        }

        public int GetClassesBetweenDates(DateTime startDate, DateTime endDate)
        {
            string query = "";

            if (startDate > DateTime.MinValue)
                query = "end_date >= '" + startDate.ToString("yyyy-MM-dd") + "'";

            if (endDate < DateTime.MaxValue)
            {
                if (query.Length > 0) query += " AND ";
                query += "start_date < '" + endDate.ToString("yyyy-MM-dd") + "'";
            }

            return GetClasses("", null, query, "", 0, 10000, false);

        }

        public int GetClasses(string SessionID, sugarsoap SugarSoap, 
            string Query, string OrderBy, int Offset, int MaxResults, bool GetDeleted)
        {
            int result = 0;
            StringDictionary fieldMappings = new StringDictionary();
            fieldMappings.Add("id", "AppointmentID");
            fieldMappings.Add("instructor_b2t_id", "DisplayOrder");
            fieldMappings.Add("course_id", "AppointmentCategoryID");
            fieldMappings.Add("instructor_id", "EmployeeID");
            fieldMappings.Add("account_id", "AccountID");
            fieldMappings.Add("location_id", "ClassLocationID");
            fieldMappings.Add("contact_id", "ClassContactID");
            fieldMappings.Add("billing_contact_id", "BillingContactID");
            fieldMappings.Add("shipping_contact_id", "ShippingContactID");
            fieldMappings.Add("status", "Status");
            fieldMappings.Add("start_date", "StartDate");
            fieldMappings.Add("end_date", "EndDate");
            fieldMappings.Add("start_time", "StartTime");
            fieldMappings.Add("end_time", "EndTime");
            fieldMappings.Add("nbr_enrolled_students", "NumRegistered");
            fieldMappings.Add("max_nbr_students", "MaxStudents");
            fieldMappings.Add("type", "ClassType");
            fieldMappings.Add("white_paper_sent_date", "WhitePaperSentDate");
            fieldMappings.Add("classroom_nbr", "Room");
            fieldMappings.Add("materials_ship_date", "MaterialShipDate");
            fieldMappings.Add("student_price", "StudentPrice");
            fieldMappings.Add("class_fee", "ClassFee");
            fieldMappings.Add("expenses", "ExpenseMode");
            fieldMappings.Add("invoice_date", "InvoiceTerms");
            fieldMappings.Add("customized_flag", "IsCustomClass");
            fieldMappings.Add("shipment_tracking_nbrs", "ShipmentTrackingNumbers");
            fieldMappings.Add("note_text", "ClassNotes");
            fieldMappings.Add("contact_name", "ClassContactName");
            fieldMappings.Add("contact_phone", "ClassContactPhone");
            fieldMappings.Add("contact_email", "ClassContactEmail");
            fieldMappings.Add("billing_contact_name", "BillingContactName");
            fieldMappings.Add("shipping_contact_name", "ShippingContactName");
            fieldMappings.Add("shipping_address_street", "ShippingContactStreet");
            fieldMappings.Add("shipping_address_city", "ShippingContactCity");
            fieldMappings.Add("shipping_address_state", "ShippingContactState");
            fieldMappings.Add("shipping_address_postalcode", "ShippingContactZip");
            fieldMappings.Add("date_modified", "LastModifiedDate");
            fieldMappings.Add("modified_user_id", "LastModifiedBy");

            string[] fieldNames = new string[fieldMappings.Count];
            fieldMappings.Keys.CopyTo(fieldNames, 0);


            //Get a list of entries
            try
            {
                get_entry_list_result entryList = this.sugarClient.get_entry_list(this.sessionId, "Classes",
                    Query, OrderBy, Offset, fieldNames, MaxResults, Convert.ToInt32(GetDeleted));
                result = entryList.result_count;
                //Loop trough the entries
                foreach (entry_value entry in entryList.entry_list)
                {
                    ScheduleDataSet.AppointmentsRow appt = DataSet.Appointments.NewAppointmentsRow();

                    foreach (name_value nameValue in entry.name_value_list)
                    {
                        if (DataSet.Appointments.Columns[fieldMappings[nameValue.name]] != null)
                        {
                            switch (DataSet.Appointments.Columns[fieldMappings[nameValue.name]].DataType.Name)
                            {
                                case "DateTime":
                                    DateTime dateValue;
                                    if (DateTime.TryParse(nameValue.value, out dateValue))
                                        appt[fieldMappings[nameValue.name]] = dateValue;
                                    break;
                                case "Int16":
                                    int intValue;
                                    if (int.TryParse(nameValue.value, out intValue))
                                        appt[fieldMappings[nameValue.name]] = intValue;
                                    break;

                                case "Decimal":
                                    decimal decimalValue;
                                    if (decimal.TryParse(nameValue.value, out decimalValue))
                                        appt[fieldMappings[nameValue.name]] = decimalValue;
                                    break;

                                case "Boolean":
                                    bool boolValue;
                                    if (bool.TryParse(nameValue.value, out boolValue))
                                        appt[fieldMappings[nameValue.name]] = boolValue;
                                    else
                                        appt[fieldMappings[nameValue.name]] = false;
                                    break;

                                case "String":
                                    appt[fieldMappings[nameValue.name]] = nameValue.value;
                                    break;
                                default:
                                    appt[fieldMappings[nameValue.name]] = nameValue.value;
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unknown Field:" + nameValue.name);
                        }
                    }
                    try
                    {
                        ScheduleDataSet.AppointmentsRow existingAppt = 
                        DataSet.Appointments.FindByAppointmentID(appt.AppointmentID);
                        if (existingAppt != null)
                        {
                            //To Do: Put conflict resolution logic here
                            existingAppt.Delete();
                        }
                        DataSet.Appointments.AddAppointmentsRow(appt);
                    }
                    catch (System.Data.ConstraintException)
                    {
                        //This just means I've already fetched this record.
                        //Just skip it and move along.
                    }
                }
            }
            catch (Exception ex )
            {
            }
            return result;
        }



        private string computeMD5String(string PlainText)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBuffer = System.Text.Encoding.ASCII.GetBytes(PlainText);
            byte[] outputBuffer = md5.ComputeHash(inputBuffer);
            //Convert the byte[] to a hex-string
            StringBuilder builder = new StringBuilder(outputBuffer.Length);
            for (int i = 0; i < outputBuffer.Length; i++)
            {
                builder.Append(outputBuffer[i].ToString("X2"));
            }
            return builder.ToString();
        }


    }
}
