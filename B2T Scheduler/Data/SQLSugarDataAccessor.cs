using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text;

namespace B2T_Scheduler
{
    class SQLSugarDataAccessor
    {
        /// <summary>
        /// The number of progress steps that will be reported to the status bar during the LoadData phase
        /// </summary>
        /// <returns></returns>
        public int LoadDataStepCount
        {
            get { return 13; }
        }

        /// <summary>
        /// The number of progress steps that will be reported to the status bar during the ReloadData phase
        /// </summary>
        /// <returns></returns>
        public int ReloadDataStepCount
        {
            get { return 4; }
        }

        /// <summary>
        /// The instance of a form containing the progress bar.
        /// ToDo: Implement status updates by raising events
        /// </summary>
        public MainForm ParentForm;

        /// <summary>
        /// the instances of a ScheduleDataSet that stores the local copy of the sugar data
        /// </summary>
        public ScheduleDataSet MyDataSet;

        /// <summary>
        /// Determins the first date for which class and event data are loaded
        /// </summary>
        public DateTime MinDateFilter = new DateTime(2006, 1, 1);

        public class MySshUserInfo : Tamir.SharpSsh.jsch.UserInfo
        {
            private String passwd;
            public String getPassword() { return passwd; }
            public bool promptYesNo(String str)
            {
                return true;
            }

            // Returns the user passphrase (passwd for the private key file)
            public String getPassphrase() { return null; }

            // Prompt the user for a passphrase (passwd for the private key file)
            public bool promptPassphrase(String message) { return true; }

            // Prompt the user for a password
            public bool promptPassword(String message) { return true; }

            // Shows a message to the user
            public void showMessage(String message) { }
        }

        /// <summary>
        /// Creates a data access layer component to access the remote sugar MySQL database 
        /// </summary>
        public SQLSugarDataAccessor(MainForm parent, string sugarCrmHost, int sugarCrmPort, string sugarCrmDatabase,
            bool enableSshPortForwarding, string sshPortForwardingLocalHost, int sshPortForwardingLocalPort,
            string schedulerDatabase, int connectionTimeoutSeconds, int commandTimeoutSeconds)
        {
            ParentForm = parent;
            SugarCrmDatabase = sugarCrmDatabase;
            SchedulerDatabase = schedulerDatabase;
            SugarCrmHost = sugarCrmHost;
            SugarCrmPort = sugarCrmPort;
            EnableSshPortForwarding = enableSshPortForwarding;
            SshPortForwardingLocalHost = sshPortForwardingLocalHost;
            SshPortForwardingLocalPort = sshPortForwardingLocalPort;
            ConnectionTimeoutSeconds = connectionTimeoutSeconds;
            CommandTimeoutSeconds = commandTimeoutSeconds;

            //Poll for changes 
            CmdPoll = new MySqlCommand();
            CmdPoll.Connection = SugarCnx;
            CmdPoll.CommandType = CommandType.Text;
            CmdPoll.Parameters.Add("?StartDate", MySqlDbType.DateTime).Value = MinDateFilter;
            CmdPoll.Parameters.Add("?ClassLocationModified", MySqlDbType.DateTime).Value = ClassLocationsLastModifiedDate;
            CmdPoll.Parameters.Add("?UsersModified", MySqlDbType.DateTime).Value = InstructorsLastModifiedDate;
            CmdPoll.Parameters.Add("?EmployeeScheduleFactorsLastModifiedDate", MySqlDbType.DateTime).Value = EmployeeScheduleFactorsLastModifiedDate;
            CmdPoll.Parameters.Add("?EmployeeCourseQualificationsLastModifiedDate", MySqlDbType.DateTime).Value = EmployeeCourseQualificationsLastModifiedDate;
            CmdPoll.Parameters.Add("?CoursesModified", MySqlDbType.DateTime).Value = CoursesLastModifiedDate;
            CmdPoll.Parameters.Add("?AccountsModified", MySqlDbType.DateTime).Value = AccountsLastModifiedDate;
            CmdPoll.Parameters.Add("?ClassesModified", MySqlDbType.DateTime).Value = ClassesLastModifiedDate;
            CmdPoll.Parameters.Add("?EventTypesModified", MySqlDbType.DateTime).Value = EventTypesLastModifiedDate;
            CmdPoll.Parameters.Add("?EventsModified", MySqlDbType.DateTime).Value = EventsLastModifiedDate;
            CmdPoll.CommandText = String.Format(
              @"  Select
	                (select count(*) from class_locations where date_modified > ?ClassLocationModified ) as newClassLocations,
	                (select count(*) from users where date_modified > ?UsersModified) as newInstructors,
	                (select count(*) from {0}.instructor_schedule_factors where date_modified > ?EmployeeScheduleFactorsLastModifiedDate) as newEmployeeScheduleFactors,
	                (select count(*) from {0}.instructor_course_qualifications where date_modified > ?EmployeeCourseQualificationsLastModifiedDate) as newEmployeeCourseQualifications,
	                (select count(*) from courses where date_modified > ?CoursesModified) as newCourses,
	                (select count(*) from accounts where deleted = 0 and id in (select account_id from accounts_classes) and date_modified > ?AccountsModified) as newAccounts,
	                (select count(*) 
                     from   classes 
                     Inner  Join  courses_classes ON courses_classes.class_id  = classes.id 
                     Inner  Join  courses         ON courses_classes.course_id = courses.id
                     Where  classes.start_date    >= ?StartDate
                     And    courses.type          != 'Administrative'
                     And    classes.date_modified > ?ClassesModified) as newClasses,
	                (select count(*) from {0}.event_types where date_modified > ?EventTypesModified ) as newEventTypes,
	                (select count(*) from {0}.events where date_modified > ?EventsModified) as newEvents", SchedulerDatabase);

            //Class Locations
            SdaClassLocations = new MySqlDataAdapter();
            SdaClassLocations.SelectCommand = new MySqlCommand();
            SdaClassLocations.SelectCommand.Connection = SugarCnx;
            SdaClassLocations.SelectCommand.CommandType = CommandType.Text;
            SdaClassLocations.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = ClassLocationsLastModifiedDate;
            SdaClassLocations.SelectCommand.CommandText =
                @"  SELECT  id            as 'ClassLocationID',
                            account_id    as 'AccountID',
                            name          as 'Name',
                            metro_area    as 'MetroArea',
                            address_city  as 'City',
                            address_state as 'State',
                            date_modified as 'LastModifiedDate'
                    FROM    class_locations
                    WHERE   date_modified > ?dateModified
                    AND     deleted=0
                    ORDER   BY 3";


            //Instructors
            SdaEmployeeList = new MySqlDataAdapter();
            SdaEmployeeList.FillLoadOption = LoadOption.PreserveChanges;
            SdaEmployeeList.SelectCommand = new MySqlCommand();
            SdaEmployeeList.SelectCommand.Connection = SugarCnx;
            SdaEmployeeList.SelectCommand.CommandType = CommandType.Text;
            SdaEmployeeList.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = InstructorsLastModifiedDate;
            SdaEmployeeList.SelectCommand.CommandText =
                @"  SELECT  users.id as 'EmployeeID',
				            case when (first_name is null and last_name is null) then 'Unknown'
					        else (concat(IfNull(concat(first_name,' '),''), IfNull(last_name,''))) end as 'FullName',
                            coalesce(concat(first_name,'.',last_name,'.jpg'),'Unknown.jpg') as 'Image',
                            b2t_id as 'DisplayOrder',
			                Case when 'Instructors' in (
                                Select t.name from teams t
                                Join team_memberships tm on tm.team_id = t.id
                                Where tm.user_id = users.id) then 1 else 0 end as IsInstructor,
                            title               as Title,
                            last_name           as LastName,
                            first_name          as FirstName,
                            user_name           as Username,
                            employee_status     as EmployeeStatus,
                            address_street      as AddressStreet,
                            address_city        as AddressCity,
                            address_state       as AddressState,
                            address_postalcode  as AddressZip,
                            phone_home          as PhoneHome,
                            phone_mobile        as PhoneMobile,
                            email1              as Email1,
                            email2              as Email2,
                            description         as Description,
                            deleted             as IsDeleted,
                            date_modified       as LastModifiedDate,
                            modified_user_id    as lastModifiedBy
                    FROM    users
                    WHERE   users.date_modified > ?dateModified
                    -- AND     users.employee_status = 'Active'
                    AND     users.deleted = 0
                    ORDER   BY 4";

            //Employee Schedule Factors
            SdaEmployeeScheduleFactors = new MySqlDataAdapter();
            SdaEmployeeScheduleFactors.FillLoadOption = LoadOption.PreserveChanges;
            SdaEmployeeScheduleFactors.SelectCommand = new MySqlCommand();
            SdaEmployeeScheduleFactors.SelectCommand.Connection = SugarCnx;
            SdaEmployeeScheduleFactors.SelectCommand.CommandType = CommandType.Text;
            SdaEmployeeScheduleFactors.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = EmployeeScheduleFactorsLastModifiedDate;
            SdaEmployeeScheduleFactors.SelectCommand.CommandText = String.Format(
                @"  SELECT  isf.instructor_id               as 'EmployeeID',
                            isf.pattern_id                  as 'PatternID',
                            isf.max_work_weeks_in_period    as 'WorkWeeksInPeriod',
                            isf.weeks_in_period             as 'WeeksInPeriod',
                            isf.effective_date              as 'EffectiveDate',
                            isf.date_modified               as 'LastModifiedDate',
                            isf.deleted                     as 'Deleted'
                    FROM    {0}.instructor_schedule_factors isf
                    WHERE   isf.date_modified > ?dateModified
                    AND     isf.deleted = 0", SchedulerDatabase);

            //Instructor Course Qualifications
            SdaEmployeeCourseQualifications = new MySqlDataAdapter();
            SdaEmployeeCourseQualifications.FillLoadOption = LoadOption.PreserveChanges;
            SdaEmployeeCourseQualifications.SelectCommand = new MySqlCommand();
            SdaEmployeeCourseQualifications.SelectCommand.Connection = SugarCnx;
            SdaEmployeeCourseQualifications.SelectCommand.CommandType = CommandType.Text;
            SdaEmployeeCourseQualifications.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = EmployeeCourseQualificationsLastModifiedDate;
            SdaEmployeeCourseQualifications.SelectCommand.CommandText = String.Format(
                @"  SELECT  icq.instructor_id as 'EmployeeID',
                            icq.course_id as 'CourseID',
                            icq.start_date as 'StartDate',
                            icq.start_date as 'StartDate',
                            icq.qualification_level as 'QualificationLevel',
                            icq.modified_by as 'LastModifiedBy',
                            icq.deleted as 'Deleted',
                            icq.date_modified as 'LastModifiedDate'
                    FROM    {0}.instructor_course_qualifications icq
                    WHERE   icq.date_modified > ?dateModified
                    AND     icq.deleted = 0", SchedulerDatabase);



            SdaCourses = new MySqlDataAdapter();
            SdaCourses.FillLoadOption = LoadOption.PreserveChanges;
            SdaCourses.SelectCommand = new MySqlCommand();
            SdaCourses.SelectCommand.Connection = SugarCnx;
            SdaCourses.SelectCommand.CommandType = CommandType.Text;
            SdaCourses.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = CoursesLastModifiedDate;
            SdaCourses.SelectCommand.CommandText =
                @"  SELECT  id                     as 'AppointmentCategoryID',
                            short_name             as 'CategoryName',
                            concat(type,':',title) as 'Description',
                            id                     as 'id',
                            type                   as 'Type',
                            nbr_days               as 'DurationDays',
                            short_name             as 'CategoryAbbreviation',
                            max_nbr_students       as 'MaxStudents',
                            description            as 'LongDescription',
                            1                      as 'IsExclusive',
                            1                      as 'IsWorking',
                            ''                     as 'Image',
                            order_id               as 'SortOrder',
                            date_modified          as 'LastModifiedDate'
                    FROM    courses
                    WHERE   date_modified > ?dateModified
                    And     deleted = 0
                    ORDER   by order_id";


            SdaEventTypes = new MySqlDataAdapter();
            SdaEventTypes.FillLoadOption = LoadOption.PreserveChanges;
            SdaEventTypes.SelectCommand = new MySqlCommand();
            SdaEventTypes.SelectCommand.Connection = SugarCnx;
            SdaEventTypes.SelectCommand.CommandType = CommandType.Text;
            SdaEventTypes.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = EventTypesLastModifiedDate;
            SdaEventTypes.SelectCommand.CommandText = String.Format(
                @"  SELECT  id             as 'AppointmentCategoryID',
                            name           as 'CategoryName',
                            description    as 'Description',
                            id             as 'id',
                            'Event'        as 'Type',
                            1              as 'DurationDays',
                            name           as 'CategoryAbbreviation',
                            0              as 'MaxStudents',
                            description    as 'LongDescription',
                            is_exclusive   as 'IsExclusive',
                            is_working     as 'IsWorking',
                            image          as 'Image',
                            sort_order     as 'SortOrder',
                            date_modified  as 'LastModifiedDate'
                    FROM    {0}.event_types
                    WHERE   deleted = 0
                    ORDER   by event_types.sort_order
                    AND     date_modified > ?DateModified", SchedulerDatabase);



            //Accounts
            SdaAccounts = new MySqlDataAdapter();
            SdaAccounts.SelectCommand = new MySqlCommand();
            SdaAccounts.SelectCommand.Connection = SugarCnx;
            SdaAccounts.SelectCommand.CommandType = CommandType.Text;
            SdaAccounts.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = AccountsLastModifiedDate;
            SdaAccounts.SelectCommand.CommandText =
                @"  SELECT  DISTINCT
                            accounts.id            as 'AccountID',
                            accounts.name          as 'Name',
                            accounts.ticker_symbol as 'Abbreviation',
                            accounts.date_modified as 'LastModifiedDate'
                    FROM    accounts
                    JOIN    accounts_classes ON accounts_classes.account_id = accounts.id
                    WHERE   accounts.deleted = 0
                    AND     accounts.date_modified > ?dateModified
                    ORDER   by 2";

            SdaHolidays = new MySqlDataAdapter();
            SdaHolidays.SelectCommand = new MySqlCommand();
            SdaHolidays.SelectCommand.Connection = SugarCnx;
            SdaHolidays.SelectCommand.CommandType = CommandType.Text;
            SdaHolidays.SelectCommand.Parameters.Add("?StartDate", MySqlDbType.DateTime).Value = MinDateFilter;
            SdaHolidays.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = HolidaysLastModifiedDate;
            SdaHolidays.SelectCommand.CommandText =
                @"SELECT    m.id         as 'ID',
                            CONCAT(
                                DATE_FORMAT(m.date_start,'%m/%d/%Y'),
                                ' ',m.time_start) as 'StartDate',
                            DATE_FORMAT(
                                DATE_ADD(
                                    DATE_ADD(
                                        CONCAT(m.date_start,' ',m.time_start),
                                        INTERVAL duration_hours HOUR),
                                    INTERVAL duration_minutes MINUTE),
                            '%m/%d/%Y %T') AS 'EndDate',
                            m.name as 'Name',
                            m.date_modified as 'LastModifiedDate'
                FROM        meetings m
                Inner Join  project p on p.id = m.parent_id
                WHERE       m.date_start >= ?StartDate
                AND         m.date_modified > ?dateModified
                AND         p.name = 'Holiday'
                AND         m.deleted = 0";


            SdaClasses = new MySqlDataAdapter();
            SdaClasses.FillLoadOption = LoadOption.OverwriteChanges;
            SdaClasses.SelectCommand = new MySqlCommand();
            SdaClasses.SelectCommand.Connection = SugarCnx;
            SdaClasses.SelectCommand.CommandType = CommandType.Text;
            SdaClasses.SelectCommand.Parameters.Add("?StartDate", MySqlDbType.DateTime).Value = MinDateFilter;
            SdaClasses.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = ClassesLastModifiedDate;
            SdaClasses.AcceptChangesDuringFill = true;
            SdaClasses.SelectCommand.CommandText =
                @"SELECT    classes.id                                                          as 'AppointmentID',
                            courses.id                                                          as 'AppointmentCategoryID',
                            ifNull(instructor.id,0)                                             as 'EmployeeID',
                            accounts.id                                                         as 'AccountID',
                            class_locations.id                                                  as 'ClassLocationID',
                            classes.contact_id                                                  as 'ClassContactID',
                            classes.billing_contact_id                                          as 'BillingContactID',
                            classes.shipping_contact_id                                         as 'ShippingContactID',
                            location_contact.id                                                 as 'LocationContactID',
                            classes.status                                                      as 'Status',
                            classes.start_date                                                  as 'StartDate',
                            DATE_ADD(classes.end_date,INTERVAL 1 DAY)                           as 'EndDate',
                            ADDTIME(classes.start_date,start_time)                              as 'StartTime',
                            ADDTIME(classes.start_date,end_time)                                as 'EndTime',
                            ''                                                                  as 'Subject',
                            ''                                                                  as 'Description',
                            ifNull(classes.nbr_enrolled_students,0)                             as 'NumRegistered',
                            (select count(*) 
                             from students_classes 
                             where deleted=0 
                             and class_id = classes.id)                                         as 'NumStudents',
                            classes.max_nbr_students                                            as 'MaxStudents',
                            classes.type                                                        as 'ClassType',
                            classes.white_paper_sent_date                                       as 'WhitePaperSentDate',
                            classroom_nbr                                                       as 'Room',
                            classes.materials_ship_date                                         as 'MaterialShipDate',
                            classes.student_price                                               as 'StudentPrice',
                            classes.class_fee                                                   as 'ClassFee',
                            classes.expenses                                                    as 'ExpenseMode',
                            classes.invoice_date                                                as 'InvoiceTerms',
                            classes.material_version                                            as 'MaterialVersion',
                            classes.customized_flag                                             as 'IsCustomClass',
                            classes.shipment_tracking_nbrs                                      as 'ShipmentTrackingNumbers',
                            classes.note_text                                                   as 'ClassNotes',
                            classes.billing_notes                                               as 'BillingNotes',
                            Concat(class_contact.first_name,' ',class_contact.last_name)        as 'ClassContactName',
                            coalesce(class_contact.phone_work, class_contact.phone_home)        as 'ClassContactPhone',
                            class_contact.email1                                                as 'ClassContactEmail',
                            Concat(billing_contact.first_name,' ',billing_contact.last_name)    as 'BillingContactName',
                            coalesce(billing_contact.phone_work, billing_contact.phone_home)    as 'BillingContactPhone',
                            billing_contact.email1                                              as 'BillingContactEmail',
                            Concat(shipping_contact.first_name,' ',shipping_contact.last_name)  as 'ShippingContactName',
                            coalesce(shipping_contact.phone_work, shipping_contact.phone_home)  as 'ShippingContactPhone',
                            shipping_contact.email1                                             as 'ShippingContactEmail',
                            shipping_contact.primary_address_street                             as 'ShippingContactStreet',
                            shipping_contact.primary_address_city                               as 'ShippingContactCity',
                            shipping_contact.primary_address_state                              as 'ShippingContactState',
                            shipping_contact.primary_address_postalcode                         as 'ShippingContactZip',
                            Concat(location_contact.first_name,' ',location_contact.last_name)  as 'LocationContactName',
                            coalesce(location_contact.phone_work, location_contact.phone_home)  as 'LocationContactPhone',
                            location_contact.email1                                             as 'LocationContactEmail',
                            IfNull(instructor.b2t_id,1000)                                      as 'DisplayOrder',
                            1                                                                   as 'ViolationLevel',
                            classes.deleted                                                     as 'Deleted',
                            0                                                                   as 'PendingDelete',
                            classes.date_modified                                               as 'LastModifiedDate',
                            classes.modified_user_id                                            as 'LastModifiedBy'
                FROM        classes
                Left Join   classes_locations   ON classes_locations.class_id    = classes.id AND classes_locations.deleted = 0
                Left Join   class_locations     ON classes_locations.location_id = class_locations.id AND class_locations.deleted = 0
                Inner Join  courses_classes     ON courses_classes.class_id      = classes.id -- AND courses_classes.deleted = 0
                Inner Join  courses             ON courses_classes.course_id     = courses.id -- AND courses.deleted = 0
                Left Join   instructors_classes ON instructors_classes.class_id  = classes.id AND instructors_classes.deleted = 0
                Left Join   accounts_classes    ON accounts_classes.class_id     = classes.id And accounts_classes.deleted = 0
                Left Join   accounts            ON accounts.id                   = accounts_classes.account_id and accounts.deleted = 0
                Left Join   users AS instructor ON instructor.id                 = instructors_classes.instructor_id AND instructor.deleted = 0
                Left Join   contacts as class_contact ON class_contact.id        = classes.contact_id
                Left Join   contacts as billing_contact ON billing_contact.id    = classes.billing_contact_id
                Left Join   contacts as shipping_contact ON shipping_contact.id  = classes.shipping_contact_id
                Left Join   contacts as location_contact ON location_contact.id  = class_locations.contact_id
                WHERE       classes.start_date >= ?StartDate
                AND         classes.date_modified > ?dateModified
                AND         courses.type != 'Administrative'
                -- AND         classes.deleted = 0
                ORDER BY    11";

            SdaEvents = new MySqlDataAdapter();
            SdaEvents.AcceptChangesDuringFill = true;
            SdaEvents.SelectCommand = new MySqlCommand();
            SdaEvents.SelectCommand.Connection = SugarCnx;
            SdaEvents.SelectCommand.CommandType = CommandType.Text;
            SdaEvents.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = EventsLastModifiedDate;
            SdaEvents.SelectCommand.CommandText = String.Format(
                    @"SELECT    e.id                    as 'AppointmentID',
                                e.event_type_id         as 'AppointmentCategoryID',
                                e.instructor_id         as 'EmployeeID',
                                e.subject               as 'Status',
                                e.date_start            as 'StartDate',
                                e.date_end              as 'EndDate',
                                e.date_start            as 'StartTime',
                                e.date_end              as 'EndTime',
                                'Event'                 as 'ClassType',
                                e.description           as 'ClassNotes',
                                IfNull(u.b2t_id,1000)   as 'DisplayOrder',
                                1                       as 'ViolationLevel',
                                0                       as 'PendingDelete',
                                e.deleted               as 'Deleted',
                                e.date_modified         as 'LastModifiedDate',
                                e.modified_by           as 'LastModifiedBy'
                        FROM    {0}.events e
                        LEFT    JOIN users u on u.id = e.instructor_id
                        WHERE   e.date_modified > ?DateModified", SchedulerDatabase);


            SdaFormats = new MySqlDataAdapter();
            SdaFormats.AcceptChangesDuringFill = true;
            SdaFormats.SelectCommand = new MySqlCommand();
            SdaFormats.SelectCommand.Connection = SugarCnx;
            SdaFormats.SelectCommand.CommandType = CommandType.Text;
            SdaFormats.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = FormatsLastModifiedDate;
            SdaFormats.SelectCommand.CommandText = String.Format(
                    @"SELECT    format_id               as 'FormatID',
                                parent_id               as 'ParentID',
                                parent_type             as 'ParentType',
                                format_name             as 'FormatName',
                                forecolor_name          as 'ForecolorName',
                                forecolor_alpha         as 'ForecolorAlpha',
                                forecolor_red           as 'ForecolorRed',
                                forecolor_green         as 'ForecolorGreen',
                                forecolor_blue          as 'ForecolorBlue',
                                backcolor_name          as 'BackcolorName',
                                backcolor_alpha         as 'BackcolorAlpha',
                                backcolor_red           as 'BackcolorRed',
                                backcolor_green         as 'BackcolorGreen',
                                backcolor_blue          as 'BackcolorBlue',
                                background_image_key    as 'BackgroundImageKey',
                                icon_image_key          as 'IconImageKey',
                                sort_order              as 'SortOrder',
                                date_modified           as 'LastModifiedDate'
                        FROM    {0}.formats
                        WHERE   date_modified > ?DateModified", SchedulerDatabase);

            SdaMetroAreas = new MySqlDataAdapter();
            SdaMetroAreas.AcceptChangesDuringFill = true;
            SdaMetroAreas.SelectCommand = new MySqlCommand();
            SdaMetroAreas.SelectCommand.Connection = SugarCnx;
            SdaMetroAreas.SelectCommand.CommandType = CommandType.Text;
            SdaMetroAreas.SelectCommand.CommandText =
                @"SELECT DISTINCT metro_area as MetroArea from class_locations";


            SdaSchedulePatterns = new MySqlDataAdapter();
            SdaSchedulePatterns.AcceptChangesDuringFill = true;
            SdaSchedulePatterns.SelectCommand = new MySqlCommand();
            SdaSchedulePatterns.SelectCommand.Connection = SugarCnx;
            SdaSchedulePatterns.SelectCommand.CommandType = CommandType.Text;
            SdaSchedulePatterns.SelectCommand.CommandText = String.Format(
                @"SELECT    pattern_id           as PatternID,
                            name                 as Name,
                            description          as Description,
                            icon_key             as IconKey,
                            work_weeks_in_period as WorkWeeksInPeriod,
                            weeks_in_period      as WeeksInPeriod
                    FROM    {0}.schedule_patterns
                    WHERE   enabled = 1", SchedulerDatabase);


            SdaPreferenceTypes = new MySqlDataAdapter();
            SdaPreferenceTypes.AcceptChangesDuringFill = true;
            SdaPreferenceTypes.SelectCommand = new MySqlCommand();
            SdaPreferenceTypes.SelectCommand.Connection = SugarCnx;
            SdaPreferenceTypes.SelectCommand.CommandType = CommandType.Text;
            SdaPreferenceTypes.SelectCommand.CommandText = String.Format(
                @"SELECT    preference_type_id   as PreferenceTypeID,
                            name                 as Name,
                            description          as Description,
                            icon_key             as IconKey
                    FROM    {0}.preference_types
                    WHERE   deleted = 0", SchedulerDatabase);


            //Accounts
            SdaAccountEmployeePreferences = new MySqlDataAdapter();
            SdaAccountEmployeePreferences.SelectCommand = new MySqlCommand();
            SdaAccountEmployeePreferences.SelectCommand.Connection = SugarCnx;
            SdaAccountEmployeePreferences.SelectCommand.CommandType = CommandType.Text;
            SdaAccountEmployeePreferences.SelectCommand.Parameters.Add("?DateModified", MySqlDbType.DateTime).Value = AccountsLastModifiedDate;
            SdaAccountEmployeePreferences.SelectCommand.CommandText = String.Format(
                @"  SELECT  account_id          as AccountID,
                            instructor_id       as EmployeeID,
                            preference_type_id  as PreferenceTypeID,
                            notes               as Notes,
                            last_modified_by    as LastModifiedBy,
                            last_modified_date  as LastModifiedDate
                    FROM    {0}.instructor_account_preferences", SchedulerDatabase);
        }

        /// <summary>
        /// Login to the database using sql authentidation
        /// </summary>
        /// <returns>true if login was successful</returns>
        public bool Authenticate()
        {
            //Server = myServerAddress; Port = 1234; Database = myDataBase; Uid = myUsername; Pwd = myPassword;
            if (EnableSshPortForwarding)
                SugarCnx.ConnectionString = String.Format(
                    "Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}; old guids=true; Connection Timeout={5}; default command timeout={6};",
                    SshPortForwardingLocalHost, SshPortForwardingLocalPort, SugarCrmDatabase, "scheduler", "teachworks", ConnectionTimeoutSeconds, CommandTimeoutSeconds);
            else
                SugarCnx.ConnectionString = String.Format(
                    "Server={0}; Database={1}; Uid={2}; Pwd={3}; old guids=true; Connection Timeout={4}; default command timeout={5};",
                    SugarCrmHost, SugarCrmDatabase, "root", "f7Kc2BRak1qLX7Ni_jw9", ConnectionTimeoutSeconds, CommandTimeoutSeconds);

            //SugarCnx.ConnectionString = String.Format(
            //    "Server={0}; Database={1}; Uid={2}; Pwd={3}; old guids=true; Connection Timeout={4}; default command timeout={5};",
            //    SugarCrmHost, SugarCrmDatabase, "bttrai_sales", "class07", ConnectionTimeoutSeconds, CommandTimeoutSeconds);

            return IsAuthenticated();
            //Use compression=true;
        }

        /// <summary>
        /// Checks that the connection is valid
        /// </summary>
        /// <returns>true if the connection is authenticated and viable</returns>
        public bool IsAuthenticated()
        {
            Boolean IsOk = false;

            if (SugarCnx.ConnectionString.Length == 0)
                throw new Exception("Sugar ConnectionString not specified");

            ConnectionState prevConnectionState = SugarCnx.State;

            if (EnableSshPortForwarding)
                if (!OpenSshTunnel())
                    return false;

            if (SugarCnx.State != ConnectionState.Open) SugarCnx.Open();
            IsOk = SugarCnx.Ping();

            if (SugarCnx.State != prevConnectionState)
                if (SugarCnx.State == ConnectionState.Open)
                    SugarCnx.Close();

            return IsOk;
        }

        /// <summary>
        /// Loads or refreshes all data from the database that is required the first data are loaded.
        /// </summary>
        /// <param name="startDate">specifies the minimum date of classes and projects to be loaded</param>
        public void LoadData(DateTime startDate)
        {
            MinDateFilter = startDate;
            LoadData();
        }

        /// <summary>
        /// Loads data from the database that is required the first data are loaded.
        /// </summary>
        public int LoadData()
        {
            int tryCount = 0;
            while (true)
            {
                tryCount++;
                try
                {
                    return LoadDataInternal();
                }

                catch (Exception ex)
                {
                    ParentForm.showProgress("Lost database connection in LoadData() because\n" + ex.Message + ".\n Trying to reconnect...");
                    System.Threading.Thread.Sleep(3000);
                    try
                    {
                        Authenticate();
                    }
                    catch (Exception ex2)
                    {
                        ParentForm.showProgress("Authenticate threw an exception: " + ex2.Message);
                    }
                    if (tryCount >= 3)
                        throw (new Exception("Lost database connection in LoadData().  Unable to reestablish connection after " + tryCount + " attempts.\n", ex));
                }
            }
        }

        /// <summary>
        /// Loads or refreshes all data from the database that is required the first data are loaded.
        /// </summary>
        public int ReloadData()
        {
            int tryCount = 0;
            while (true)
            {
                tryCount++;
                try
                {
                    return ReloadDataInternal();
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
                    if (tryCount >= 3)
                        throw (new Exception("Lost database connection in ReloadData().  Unable to reestablish connection after " + tryCount + " attempts.\n", ex));
                }
            }
        }

        /// <summary>
        /// Persists all modifications that have been made to the schedule data set
        /// </summary>
        /// <returns>the number of records written</returns>
        public int SaveData()
        {
            int tryCount = 0;
            while (true)
            {
                tryCount++;
                try
                {
                    return SaveDataInternal();
                }

                catch (Exception ex)
                {
                    ParentForm.showProgress("Lost database connection in SaveData() because\n" + ex.Message + ".\n Trying to reconnect...");
                    System.Threading.Thread.Sleep(3000);
                    try
                    {
                        Authenticate();
                    }
                    catch (Exception ex2)
                    {
                        ParentForm.showProgress("Authenticate threw an exception: " + ex2.Message);
                    }
                    if (tryCount >= 3)
                        throw (new Exception("Lost database connection in SaveData().  Unable to reestablish connection after " + tryCount + " tries.\n", ex));
                }
            }
        }

        #region Private

        private string SugarCrmHost;
        private int SugarCrmPort;
        private string SugarCrmDatabase;
        private string SchedulerDatabase;
        private bool EnableSshPortForwarding;
        private string SshPortForwardingLocalHost;
        private int SshPortForwardingLocalPort;
        private int ConnectionTimeoutSeconds;
        private int CommandTimeoutSeconds;
        private Tamir.SharpSsh.jsch.Session SshSession;
        private MySqlConnection SugarCnx = new MySqlConnection();
        private MySqlCommand CmdPoll;
        private MySqlDataAdapter SdaEmployeeList;
        private MySqlDataAdapter SdaClassLocations;
        private MySqlDataAdapter SdaCourses;
        private MySqlDataAdapter SdaEventTypes;
        private MySqlDataAdapter SdaAccounts;
        private MySqlDataAdapter SdaHolidays;
        private MySqlDataAdapter SdaClasses;
        private MySqlDataAdapter SdaEmployeeScheduleFactors;
        private MySqlDataAdapter SdaEmployeeCourseQualifications;
        private MySqlDataAdapter SdaEvents;
        private MySqlDataAdapter SdaFormats;
        private MySqlDataAdapter SdaMetroAreas;
        private MySqlDataAdapter SdaSchedulePatterns;
        private MySqlDataAdapter SdaAccountEmployeePreferences;
        private MySqlDataAdapter SdaPreferenceTypes;
        private DateTime ClassLocationsLastModifiedDate = DateTime.MinValue;
        private DateTime InstructorsLastModifiedDate = DateTime.MinValue;
        private DateTime CoursesLastModifiedDate = DateTime.MinValue;
        private DateTime AccountsLastModifiedDate = DateTime.MinValue;
        private DateTime ClassesLastModifiedDate = DateTime.MinValue;
        private DateTime ProjectsLastModifiedDate = DateTime.MinValue;
        private DateTime OldEventsLastModifiedDate = DateTime.MinValue;
        private DateTime EventsLastModifiedDate = DateTime.MinValue;
        private DateTime HolidaysLastModifiedDate = DateTime.MinValue;
        private DateTime AppointmentsLastModifiedDate = DateTime.MinValue;
        private DateTime EventTypesLastModifiedDate = DateTime.MinValue;
        private DateTime EventCategoriesLastModifiedDate = DateTime.MinValue;
        private DateTime EmployeeScheduleFactorsLastModifiedDate = DateTime.MinValue;
        private DateTime EmployeeCourseQualificationsLastModifiedDate = DateTime.MinValue;
        private DateTime FormatsLastModifiedDate = DateTime.MinValue;
        private DateTime SchedulePatternsLastModifiedDate = DateTime.MinValue;
        
        private bool OpenSshTunnel()
        {
            if (SshSession != null && SshSession.isConnected())
                return true;

            ParentForm.showProgress("Establishing secure connection to host...");

            Tamir.SharpSsh.jsch.JSch jsch = new Tamir.SharpSsh.jsch.JSch();
            SshSession = jsch.getSession("scheduler", SugarCrmHost, 22);
            SshSession.setHost(SugarCrmHost);
            SshSession.setPassword("teachworks");
            Tamir.SharpSsh.jsch.UserInfo ui = new MySshUserInfo();
            SshSession.setUserInfo(ui);
            SshSession.connect();

            // port forwarding
            ParentForm.showProgress("Forwarding port connections...");
            SshSession.setPortForwardingL(SshPortForwardingLocalPort, "localhost", SugarCrmPort);

            return SshSession.isConnected();
        }

        private int LoadDataInternal()
        {
            int recordCount = 0;

            ParentForm.showProgress("Opening Connection to Database...");
            SugarCnx.Open();

            ParentForm.showProgress("Loading Formats...");
            recordCount += LoadFormats();

            ParentForm.showProgress("Loading Class Locations...");
            recordCount += LoadClassLocations();

            ParentForm.showProgress("Loading Courses...");
            recordCount += LoadCourses();

            ParentForm.showProgress("Loading Instructors...");
            recordCount += LoadInstructors();

            ParentForm.showProgress("Loading Schedule Pattern Definitions...");
            recordCount += LoadSchedulePatterns();

            ParentForm.showProgress("Loading Preference Definitions...");
            recordCount += LoadPreferenceTypes();

            ParentForm.showProgress("Loading Instructor Schedule Factors...");
            recordCount += LoadEmployeeScheduleFactors();

            ParentForm.showProgress("Loading Instructor Course Qualifications...");
            recordCount += LoadEmployeeCourseQualifications();

            ParentForm.showProgress("Loading Accounts...");
            recordCount += LoadAccounts();

            ParentForm.showProgress("Loading Instructor Account Preferences...");
            recordCount += LoadAccountEmployeePreferences();

            ParentForm.showProgress("Loading Event Types...");
            recordCount += LoadEventTypes();

            ParentForm.showProgress("Loading Classes...");
            recordCount += LoadClasses();

            ParentForm.showProgress("Loading Events...");
            recordCount += LoadEvents();

            ParentForm.showProgress("Loading Metro Areas...");
            recordCount += LoadMetroAreas();

            SugarCnx.Close();

            return recordCount;
        }

        private int ReloadDataInternal()
        {
            int newRecords = 0;
            int newClassLocations = 0;
            int newInstructors = 0;
            int newEmployeeScheduleFactors = 0;
            int newEmployeeCourseQualifications = 0;
            int newCourses = 0;
            int newAccounts = 0;
            int newClasses = 0;
            int newEventTypes = 0;
            int newEvents = 0;
            int recordCount = 0;
            int modifiedTables = 0;

            CmdPoll.Parameters["?StartDate"].Value = MinDateFilter;
            CmdPoll.Parameters["?ClassLocationModified"].Value = ClassLocationsLastModifiedDate;
            CmdPoll.Parameters["?UsersModified"].Value = InstructorsLastModifiedDate;
            CmdPoll.Parameters["?EmployeeScheduleFactorsLastModifiedDate"].Value = EmployeeScheduleFactorsLastModifiedDate;
            CmdPoll.Parameters["?EmployeeCourseQualificationsLastModifiedDate"].Value = EmployeeCourseQualificationsLastModifiedDate;
            CmdPoll.Parameters["?CoursesModified"].Value = CoursesLastModifiedDate;
            CmdPoll.Parameters["?AccountsModified"].Value = AccountsLastModifiedDate;
            CmdPoll.Parameters["?ClassesModified"].Value = ClassesLastModifiedDate;
            CmdPoll.Parameters["?EventTypesModified"].Value = EventTypesLastModifiedDate;
            CmdPoll.Parameters["?EventsModified"].Value = EventsLastModifiedDate;

            SugarCnx.Open();
            MySqlDataReader reader = CmdPoll.ExecuteReader();
            if (reader.Read())
            {
                newRecords += newClassLocations = int.Parse(reader["newClassLocations"].ToString());
                newRecords += newInstructors = int.Parse(reader["newInstructors"].ToString());
                newRecords += newEmployeeScheduleFactors = int.Parse(reader["newEmployeeScheduleFactors"].ToString());
                newRecords += newEmployeeCourseQualifications = int.Parse(reader["newEmployeeCourseQualifications"].ToString());
                newRecords += newCourses = int.Parse(reader["newCourses"].ToString());
                newRecords += newAccounts = int.Parse(reader["newAccounts"].ToString());
                newRecords += newClasses = int.Parse(reader["newClasses"].ToString());
                newRecords += newEventTypes = int.Parse(reader["newEventTypes"].ToString());
                newRecords += newEvents = int.Parse(reader["newEvents"].ToString());
            }
            reader.Close();
            if (newRecords == 0)
            {
                SugarCnx.Close();
                return 0;
            }
            modifiedTables += newClassLocations > 0 ? 1 : 0;
            modifiedTables += newInstructors > 0 ? 1 : 0;
            modifiedTables += newEmployeeScheduleFactors > 0 ? 1 : 0;
            modifiedTables += newEmployeeCourseQualifications > 0 ? 1 : 0;
            modifiedTables += newCourses > 0 ? 1 : 0;
            modifiedTables += newAccounts > 0 ? 1 : 0;
            modifiedTables += newClasses > 0 ? 1 : 0;
            modifiedTables += newEventTypes > 0 ? 1 : 0;
            modifiedTables += newEvents > 0 ? 1 : 0;

            if (newClassLocations > 0)
            {
                ParentForm.showProgress("Loading Class Locations...");
                recordCount += LoadClassLocations();

                ParentForm.showProgress("Loading Metro Areas...");
                LoadMetroAreas();
            }

            if (newInstructors > 0)
            {
                ParentForm.showProgress("Loading Instructors...");
                recordCount += LoadInstructors();
            }

            if (newEmployeeScheduleFactors > 0)
            {
                ParentForm.showProgress("Loading Employee Schedule Factors...");
                recordCount += LoadEmployeeScheduleFactors();
            }

            if (newEmployeeCourseQualifications > 0)
            {
                ParentForm.showProgress("Loading Employee Course Qualifications...");
                recordCount += LoadEmployeeCourseQualifications();
            }

            if (newCourses > 0)
            {
                ParentForm.showProgress("Loading Courses...");
                recordCount += LoadCourses();
            }

            if (newAccounts > 0)
            {
                ParentForm.showProgress("Loading Accounts...");
                recordCount += LoadAccounts();
            }
            if (newEventTypes > 0)
            {
                ParentForm.showProgress("Loading Event Types...");
                recordCount += LoadEventTypes();
            }
            if (newClasses > 0)
            {
                ParentForm.showProgress("Loading Classes...");
                recordCount += LoadClasses();
            }

            if (newEvents > 0)
            {
                ParentForm.showProgress("Loading Events...");
                recordCount += LoadEvents();
            }

            ParentForm.showProgress("Ready");
            ParentForm.toolStripProgressBar1.Visible = false;
            SugarCnx.Close();
            return recordCount;
        }

        private int SaveDataInternal()
        {

            int recordCount = 0;
            SugarCnx.Open();
            recordCount += SaveAppointments();
            recordCount += SaveInstructors();
            recordCount += SaveSchedulePatterns();
            recordCount += SaveEmployeeCourseQualifications();
            recordCount += SaveAccountEmployeePreferences();
            if (SugarCnx.State != ConnectionState.Closed)
                SugarCnx.Close();

            ReloadData();
            ParentForm.showProgress("Ready");
            return recordCount;
        }

        private int LoadClassLocations()
        {
            if (ClassLocationsLastModifiedDate == DateTime.MinValue)
                MyDataSet.ClassLocations.Clear();

            SdaClassLocations.SelectCommand.Parameters["?DateModified"].Value = ClassLocationsLastModifiedDate;
            int numLoaded = SdaClassLocations.Fill(MyDataSet.ClassLocations);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.ClassLocationsRow row in MyDataSet.ClassLocations)
                    if (row.LastModifiedDate > ClassLocationsLastModifiedDate)
                        ClassLocationsLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadMetroAreas()
        {
            int numLoaded = SdaMetroAreas.Fill(MyDataSet.MetroAreas);
            return numLoaded;
        }

        private int LoadInstructors()
        {
            if (InstructorsLastModifiedDate == DateTime.MinValue)
                MyDataSet.EmployeeList.Clear();

            SdaEmployeeList.SelectCommand.Parameters["?DateModified"].Value = InstructorsLastModifiedDate;
            int numLoaded = SdaEmployeeList.Fill(MyDataSet.EmployeeList);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.EmployeeListRow row in MyDataSet.EmployeeList)
                    if (row.LastModifiedDate > InstructorsLastModifiedDate)
                        InstructorsLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadEmployeeScheduleFactors()
        {
            if (EmployeeScheduleFactorsLastModifiedDate == DateTime.MinValue)
                MyDataSet.EmployeeScheduleFactors.Clear();

            SdaEmployeeScheduleFactors.SelectCommand.Parameters["?DateModified"].Value = EmployeeScheduleFactorsLastModifiedDate;
            int numLoaded = SdaEmployeeScheduleFactors.Fill(MyDataSet.EmployeeScheduleFactors);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.EmployeeScheduleFactorsRow row in MyDataSet.EmployeeScheduleFactors)
                    if (row.LastModifiedDate > EmployeeScheduleFactorsLastModifiedDate)
                        EmployeeScheduleFactorsLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadEmployeeCourseQualifications()
        {
            if (EmployeeCourseQualificationsLastModifiedDate == DateTime.MinValue)
                MyDataSet.EmployeeCourseQualifications.Clear();

            SdaEmployeeCourseQualifications.SelectCommand.Parameters["?DateModified"].Value = EmployeeCourseQualificationsLastModifiedDate;
            int numLoaded = SdaEmployeeCourseQualifications.Fill(MyDataSet.EmployeeCourseQualifications);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.EmployeeCourseQualificationsRow row in MyDataSet.EmployeeCourseQualifications)
                    if (row.LastModifiedDate > EmployeeCourseQualificationsLastModifiedDate)
                        EmployeeCourseQualificationsLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadCourses()
        {
            int numLoaded = SdaCourses.Fill(MyDataSet.AppointmentCategories);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.AppointmentCategoriesRow row in MyDataSet.AppointmentCategories)
                    if (row.LastModifiedDate > CoursesLastModifiedDate)
                        CoursesLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadEventTypes()
        {
            int numLoaded = SdaEventTypes.Fill(MyDataSet.AppointmentCategories);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.AppointmentCategoriesRow row in MyDataSet.AppointmentCategories)
                    if (row.Type == "Event")
                        if (row.LastModifiedDate > EventTypesLastModifiedDate)
                            EventTypesLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadAccounts()
        {
            if (AccountsLastModifiedDate == DateTime.MinValue)
                MyDataSet.Accounts.Clear();

            SdaAccounts.SelectCommand.Parameters["?DateModified"].Value = AccountsLastModifiedDate;
            int numLoaded = SdaAccounts.Fill(MyDataSet.Accounts);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.AccountsRow row in MyDataSet.Accounts)
                    if (row.LastModifiedDate > AccountsLastModifiedDate)
                        AccountsLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadClasses()
        {
            if (ClassesLastModifiedDate == DateTime.MinValue)
                MyDataSet.Appointments.Clear();

            SdaClasses.SelectCommand.Parameters["?StartDate"].Value = MinDateFilter;
            SdaClasses.SelectCommand.Parameters["?DateModified"].Value = ClassesLastModifiedDate;
            int numLoaded = SdaClasses.Fill(MyDataSet.Appointments);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.AppointmentsRow row in MyDataSet.Appointments)
                    if (row.RowState == DataRowState.Unchanged)
                        if (row.ClassType != "Event")
                            if (row.LastModifiedDate > ClassesLastModifiedDate)
                                ClassesLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadEvents()
        {
            SdaEvents.SelectCommand.Parameters["?DateModified"].Value = EventsLastModifiedDate;
            int numLoaded = SdaEvents.Fill(MyDataSet.Appointments);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.AppointmentsRow row in MyDataSet.Appointments)
                    if (row.RowState != DataRowState.Deleted)
                        if (row.ClassType == "Event")
                            if (row.LastModifiedDate > EventsLastModifiedDate)
                                EventsLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadFormats()
        {
            if (FormatsLastModifiedDate == DateTime.MinValue)
                MyDataSet.Formats.Clear();

            SdaFormats.SelectCommand.Parameters["?DateModified"].Value = FormatsLastModifiedDate;
            int numLoaded = SdaFormats.Fill(MyDataSet.Formats);
            if (numLoaded > 0)
                foreach (ScheduleDataSet.FormatsRow row in MyDataSet.Formats)
                    if (row.LastModifiedDate > FormatsLastModifiedDate)
                        FormatsLastModifiedDate = row.LastModifiedDate;
            return numLoaded;
        }

        private int LoadSchedulePatterns()
        {
            int numLoaded = SdaSchedulePatterns.Fill(MyDataSet.SchedulePatterns);
            return numLoaded;
        }

        private int LoadPreferenceTypes()
        {
            int numLoaded = SdaPreferenceTypes.Fill(MyDataSet.PreferenceTypes);
            return numLoaded;
        }

        private int LoadAccountEmployeePreferences()
        {
            int numLoaded = SdaAccountEmployeePreferences.Fill(MyDataSet.AccountEmployeePreferences);
            return numLoaded;
        }

        private int SaveInstructors()
        {
            int recordCount = 0;
            for (int i = MyDataSet.EmployeeList.Rows.Count - 1; i >= 0; i--)
            {
                ScheduleDataSet.EmployeeListRow ee =
                    (ScheduleDataSet.EmployeeListRow)(MyDataSet.EmployeeList.Rows[i]);

                if (ee.RowState == DataRowState.Modified)
                {
                    ParentForm.showProgress("Updating instructor");
                    int n = UpdateEmployee(ee);
                    recordCount += n;
                }
            }
            return recordCount;
        }

        private int SaveSchedulePatterns()
        {
            int recordCount = 0;
            //foreach (ScheduleDataSet.EmployeeScheduleFactorsRow row in MyDataSet.EmployeeScheduleFactors.GetChanges().Rows)
            //    recordCount += UpSertSchedulePattern(row);
            //return recordCount;
            for (int i = MyDataSet.EmployeeScheduleFactors.Rows.Count - 1; i >= 0; i--)
            {
                ScheduleDataSet.EmployeeScheduleFactorsRow row =
                    (ScheduleDataSet.EmployeeScheduleFactorsRow)(MyDataSet.EmployeeScheduleFactors.Rows[i]);
                if (row.RowState != DataRowState.Unchanged)
                    recordCount += UpSertSchedulePattern(row);

                //if (row.RowState == DataRowState.Deleted)
                //{
                //    row.RejectChanges();
                //    row.Deleted = true;
                //}

                //if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
                //{
                //    ParentForm.showProgress("Updating schedule pattern");
                //    int n = UpSertSchedulePattern(row);
                //    recordCount += n;
                //}
            }
            return recordCount;
        }

        private int SaveEmployeeCourseQualifications()
        {
            int recordCount = 0;
            for (int i = MyDataSet.EmployeeCourseQualifications.Rows.Count - 1; i >= 0; i--)
            {
                ScheduleDataSet.EmployeeCourseQualificationsRow row =
                    (ScheduleDataSet.EmployeeCourseQualificationsRow)(MyDataSet.EmployeeCourseQualifications.Rows[i]);

                //if (row.RowState == DataRowState.Deleted)
                //{
                //    row.RejectChanges();
                //    row.Deleted = true;
                //}

                if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added || row.RowState == DataRowState.Deleted)
                {
                    ParentForm.showProgress("Updating course qualifications...");
                    int n = UpSertEmployeeCourseQualifications(row);
                    recordCount += n;
                }
            }
            return recordCount;
        }

        private int SaveAccountEmployeePreferences()
        {
            int recordCount = 0;
            for (int i = MyDataSet.AccountEmployeePreferences.Rows.Count - 1; i >= 0; i--)
            {
                ScheduleDataSet.AccountEmployeePreferencesRow row =
                    (ScheduleDataSet.AccountEmployeePreferencesRow)(MyDataSet.AccountEmployeePreferences.Rows[i]);

                if (row.RowState == DataRowState.Deleted)
                {
                    row.RejectChanges();
                    row.Deleted = true;
                }

                if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
                {
                    ParentForm.showProgress("Updating account employee preferences");
                    int n = UpSertAccountEmployeePreferences(row);
                    recordCount += n;
                }
            }
            return recordCount;
        }

        private int SaveAppointments()
        {
            int recordCount = 0;

            for (int i = MyDataSet.Appointments.Rows.Count - 1; i >= 0; i--)
            {
                ScheduleDataSet.AppointmentsRow appt =
                    (ScheduleDataSet.AppointmentsRow)(MyDataSet.Appointments.Rows[i]);

                if (appt.RowState == DataRowState.Modified)
                {
                    ParentForm.showProgress("Updating 1 event");
                    int n = UpdateEvent(appt);
                    recordCount += n;
                    ParentForm.showProgress("Updated " + n + " event(s)");
                    if (n == 1 && appt.Deleted)
                    {
                        appt.Delete();
                        appt.AcceptChanges();
                    }
                }


                if (appt.RowState == DataRowState.Added)
                {
                    if (appt.PendingDelete)
                    {
                        appt.RejectChanges();
                        recordCount++;
                    }
                    else
                    {
                        ParentForm.showProgress("Inserting 1 event");
                        int n = InsertEvent(appt);
                        recordCount += n;
                        ParentForm.showProgress("Inserted " + n + " event(s)");
                    }
                }
            }

            return recordCount;
        }

        private int InsertEvent(ScheduleDataSet.AppointmentsRow appt)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SugarCnx;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("?id", MySqlDbType.String).Value = appt.AppointmentID;
            cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = appt.EmployeeID;
            cmd.Parameters.Add("?event_type_id", MySqlDbType.String).Value = appt.AppointmentCategoryID;
            cmd.Parameters.Add("?date_start", MySqlDbType.DateTime).Value = appt.StartDate;
            cmd.Parameters.Add("?date_end", MySqlDbType.DateTime).Value = appt.EndDate;
            cmd.Parameters.Add("?subject", MySqlDbType.String).Value = appt.Status;
            cmd.Parameters.Add("?description", MySqlDbType.String).Value = appt.ClassNotes;
            cmd.Parameters.Add("?modified_by", MySqlDbType.String).Value = ParentForm.CurrentUserDetail.id;
            cmd.CommandText = String.Format(@"
                INSERT  {0}.events(
                        id,instructor_id,event_type_id,date_start,date_end,
                        subject,description,date_modified, modified_by)
                SELECT  ?id, ?instructor_id, ?event_type_id, ?date_start, ?date_end, 
                         ?subject, ?description, current_timestamp, ?modified_by", SchedulerDatabase);
            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                recordCount = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                appt.AcceptChanges();
            }
            catch (Exception e)
            {
                recordCount = 0;
            }
            finally
            {
                if (cmd.Connection.State != ConnectionState.Closed)
                    cmd.Connection.Close();
            }
            return recordCount;
        }

        private int UpdateEvent(ScheduleDataSet.AppointmentsRow appt)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SugarCnx;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("?id", MySqlDbType.String).Value = appt.AppointmentID;
            cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = appt.EmployeeID;
            cmd.Parameters.Add("?event_type_id", MySqlDbType.String).Value = appt.AppointmentCategoryID;
            cmd.Parameters.Add("?date_start", MySqlDbType.DateTime).Value = appt.StartDate;
            cmd.Parameters.Add("?date_end", MySqlDbType.DateTime).Value = appt.EndDate;
            cmd.Parameters.Add("?subject", MySqlDbType.String).Value = appt.Status;
            cmd.Parameters.Add("?description", MySqlDbType.String).Value = appt.ClassNotes;
            cmd.Parameters.Add("?deleted", MySqlDbType.Bit).Value = appt.PendingDelete;
            cmd.Parameters.Add("?modified_by", MySqlDbType.String).Value = ParentForm.CurrentUserDetail.id;
            cmd.CommandText = String.Format(@"
                UPDATE  {0}.events
                SET     instructor_id = ?instructor_id,
                        event_type_id = ?event_type_id,
                        date_start    = ?date_start,
                        date_end      = ?date_end,
                        subject       = ?subject,
                        description   = ?description,
                        deleted       = ?deleted,
                        date_modified = current_timestamp,
                        modified_by   = ?modified_by
                WHERE   id            = ?id", SchedulerDatabase);

            try
            {
                recordCount = cmd.ExecuteNonQuery();
                appt.AcceptChanges();
            }
            catch (Exception e)
            {

            }
            return recordCount;
        }

        private int DeleteEvent(String id)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SugarCnx;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("?id", MySqlDbType.String).Value = id;
            cmd.CommandText = String.Format("DELETE  from {0}.events WHERE id = ?id", SchedulerDatabase);
            try
            {
                recordCount = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            return recordCount;
        }

        private int UpdateEmployee(ScheduleDataSet.EmployeeListRow ee)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SugarCnx;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("?id", MySqlDbType.String).Value = ee.EmployeeID;
            cmd.Parameters.Add("?description", MySqlDbType.String).Value = ee.Description;
            cmd.Parameters.Add("?modified_by", MySqlDbType.String).Value = ParentForm.CurrentUserDetail.id;
            cmd.CommandText = String.Format(@"
                UPDATE  users
                SET     description = ?description
                WHERE   id            = ?id");

            try
            {
                recordCount = cmd.ExecuteNonQuery();
                ee.AcceptChanges();
            }
            catch (Exception e)
            {
            }
            return recordCount;
        }

        private int UpSertSchedulePattern(ScheduleDataSet.EmployeeScheduleFactorsRow row)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SugarCnx;

            switch (row.RowState)
            {
                case DataRowState.Deleted:
                    cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value =
                        (String)row["EmployeeID", DataRowVersion.Original];
                    cmd.Parameters.Add("?original_effective_date", MySqlDbType.Date).Value =
                        (DateTime)row["EffectiveDate", DataRowVersion.Original];
                    cmd.CommandText = String.Format(@"
                        DELETE from {0}.instructor_schedule_factors
                        WHERE  instructor_id = ?instructor_id
                        AND    effective_date = ?original_effective_date", SchedulerDatabase);
                    EmployeeScheduleFactorsLastModifiedDate = DateTime.MinValue;
                    break;
                case DataRowState.Modified:
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = row.EmployeeID;
                    cmd.Parameters.Add("?effective_date", MySqlDbType.Date).Value = row.EffectiveDate;
                    cmd.Parameters.Add("?deleted", MySqlDbType.Bit).Value = row.Deleted;
                    cmd.Parameters.Add("?pattern_id", MySqlDbType.String).Value = row.PatternID;
                    cmd.Parameters.Add("?max_work_weeks_in_period", MySqlDbType.String).Value = row.WorkWeeksInPeriod;
                    cmd.Parameters.Add("?weeks_in_period", MySqlDbType.String).Value = row.WeeksInPeriod;
                    cmd.Parameters.Add("?modified_by", MySqlDbType.String).Value = row.IsLastModifiedByNull() ? "" : row.LastModifiedBy;
                    cmd.Parameters.Add("?date_modified", MySqlDbType.DateTime).Value = row.IsLastModifiedByNull() ? DateTime.Now : row.LastModifiedDate;
                    cmd.Parameters.Add("?original_effective_date", MySqlDbType.Date).Value = (DateTime)row["EffectiveDate", DataRowVersion.Original];
                    cmd.CommandText = String.Format(@"
                        UPDATE  {0}.instructor_schedule_factors
                        SET     pattern_id                  = ?pattern_id,
                                max_work_weeks_in_period    = ?max_work_weeks_in_period,
                                weeks_in_period             = ?weeks_in_period,
                                effective_date              = ?effective_date,
                                modified_by                 = ?modified_by,
                                date_modified               = ?date_modified,
                                deleted                     = ?deleted
                        WHERE   instructor_id               = ?instructor_id
                        AND     effective_date              = ?original_effective_date", SchedulerDatabase);
                    break;
                case DataRowState.Added:
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = row.EmployeeID;
                    cmd.Parameters.Add("?effective_date", MySqlDbType.Date).Value = row.EffectiveDate;
                    cmd.Parameters.Add("?deleted", MySqlDbType.Bit).Value = row.Deleted;
                    cmd.Parameters.Add("?pattern_id", MySqlDbType.String).Value = row.PatternID;
                    cmd.Parameters.Add("?max_work_weeks_in_period", MySqlDbType.String).Value = row.WorkWeeksInPeriod;
                    cmd.Parameters.Add("?weeks_in_period", MySqlDbType.String).Value = row.WeeksInPeriod;
                    cmd.Parameters.Add("?modified_by", MySqlDbType.String).Value = row.IsLastModifiedByNull() ? "" : row.LastModifiedBy;
                    cmd.Parameters.Add("?date_modified", MySqlDbType.DateTime).Value = row.IsLastModifiedByNull() ? DateTime.Now : row.LastModifiedDate;
                    cmd.CommandText = String.Format(@"
                        INSERT  {0}.instructor_schedule_factors(instructor_id, pattern_id, 
                                max_work_weeks_in_period, weeks_in_period, effective_date,
                                modified_by, date_modified, deleted)
                        VALUES  (?instructor_id, ?pattern_id,?max_work_weeks_in_period,
                                ?weeks_in_period,?effective_date,?modified_by,?date_modified,0)", SchedulerDatabase);
                    break;

            }

            recordCount = cmd.ExecuteNonQuery();
            if (recordCount == 1)
                row.AcceptChanges();
            return recordCount;
        }

        private int DeleteSchedulePatternRow(ScheduleDataSet.EmployeeScheduleFactorsRow row)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SugarCnx;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = row.EmployeeID;
            cmd.Parameters.Add("?effective_date", MySqlDbType.Date).Value = row.EffectiveDate;
            cmd.CommandText = String.Format(@"
                DELETE from instructor_schedule_factors
                WHERE  instructor_id = ?instructor_id
                AND    effective_date = ?effecitive_date");
            EmployeeScheduleFactorsLastModifiedDate = DateTime.MinValue;

            try
            {
                recordCount = cmd.ExecuteNonQuery();
                if (recordCount == 1)
                    row.AcceptChanges();
            }
            catch (Exception e)
            {
                recordCount = 0;
            }
            return recordCount;
        }

        private int UpSertEmployeeCourseQualifications(ScheduleDataSet.EmployeeCourseQualificationsRow row)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = SugarCnx;

            if (row.RowState == DataRowState.Deleted)
            {

                cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = row["EmployeeID", DataRowVersion.Original].ToString();
                cmd.Parameters.Add("?course_id", MySqlDbType.String).Value = row["CourseID", DataRowVersion.Original].ToString();
                cmd.CommandText = String.Format(@"
                        DELETE  FROM {0}.instructor_course_qualifications
                        WHERE   instructor_id       = ?instructor_id
                        AND     course_id           = ?course_id", SchedulerDatabase);
                EmployeeCourseQualificationsLastModifiedDate = DateTime.MinValue;
            }
            else if (row.RowState == DataRowState.Modified)
            {
                cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = row.EmployeeID;
                cmd.Parameters.Add("?course_id", MySqlDbType.String).Value = row.CourseID;
                cmd.Parameters.Add("?start_date", MySqlDbType.Date).Value = row.StartDate;
                cmd.Parameters.Add("?end_date", MySqlDbType.Date); //.Value = row.IsEndDateNull() ? DBNull.Value : row.EndDate;
                if (row.IsEndDateNull() || row.EndDate == DateTime.MaxValue)
                    cmd.Parameters["?end_date"].Value = null;
                else
                    cmd.Parameters["?end_date"].Value = row.EndDate;
                cmd.Parameters.Add("?qualification_level", MySqlDbType.String).Value = row.QualificationLevel;
                cmd.Parameters.Add("?modified_by", MySqlDbType.String).Value = row.LastModifiedBy;
                cmd.Parameters.Add("?date_modified", MySqlDbType.DateTime).Value = row.LastModifiedDate;
                cmd.Parameters.Add("?deleted", MySqlDbType.Bit).Value = row.Deleted;
                cmd.CommandText = String.Format(@"
                        UPDATE  {0}.instructor_course_qualifications
                        SET     start_date          = ?start_date,
                                end_date            = ?end_date,
                                qualification_level = ?qualification_level,
                                modified_by         = ?modified_by,
                                date_modified       = ?date_modified,
                                deleted             = ?deleted
                        WHERE   instructor_id       = ?instructor_id
                        AND     course_id           = ?course_id", SchedulerDatabase);
            }
            else if (row.RowState == DataRowState.Added)
            {
                cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = row.EmployeeID;
                cmd.Parameters.Add("?course_id", MySqlDbType.String).Value = row.CourseID;
                cmd.Parameters.Add("?start_date", MySqlDbType.Date).Value = row.StartDate;
                cmd.Parameters.Add("?end_date", MySqlDbType.Date); //.Value = row.IsEndDateNull() ? DBNull.Value : row.EndDate;
                if (row.IsEndDateNull() || row.EndDate == DateTime.MaxValue)
                    cmd.Parameters["?end_date"].Value = null;
                else
                    cmd.Parameters["?end_date"].Value = row.EndDate;
                cmd.Parameters.Add("?qualification_level", MySqlDbType.String).Value = row.QualificationLevel;
                cmd.Parameters.Add("?modified_by", MySqlDbType.String).Value = row.LastModifiedBy;
                cmd.Parameters.Add("?date_modified", MySqlDbType.DateTime).Value = row.LastModifiedDate;
                cmd.Parameters.Add("?deleted", MySqlDbType.Bit).Value = row.Deleted;

                cmd.CommandText = String.Format(@"
                        INSERT  {0}.instructor_course_qualifications(instructor_id, course_id, 
                                start_date, end_date, qualification_level,
                                modified_by, date_modified, deleted)
                        VALUES  (?instructor_id, ?course_id,?start_date,
                                ?end_date,?qualification_level,?modified_by,?date_modified,0)", SchedulerDatabase);
            }
            else
            {
                // this shouldn't happen
            }

            recordCount = cmd.ExecuteNonQuery();
            if (recordCount == 1)
                row.AcceptChanges();
            return recordCount;
        }

        private int UpSertAccountEmployeePreferences(ScheduleDataSet.AccountEmployeePreferencesRow row)
        {
            int recordCount = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SugarCnx;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("?instructor_id", MySqlDbType.String).Value = row.EmployeeID;
            cmd.Parameters.Add("?account_id", MySqlDbType.String).Value = row.AccountID;
            cmd.Parameters.Add("?preference_type_id", MySqlDbType.Int16).Value = row.PreferenceTypeID;
            cmd.Parameters.Add("?notes", MySqlDbType.String);
            if (!row.IsNotesNull())
                cmd.Parameters["?notes"].Value = row.Notes;
            cmd.Parameters.Add("?last_modified_by", MySqlDbType.String).Value = row.LastModifiedBy;
            cmd.Parameters.Add("?last_modified_date", MySqlDbType.DateTime).Value = row.LastModifiedDate;
            cmd.Parameters.Add("?deleted", MySqlDbType.Bit).Value = row.Deleted;
            cmd.CommandText = String.Format(@"
                UPDATE  {0}.instructor_account_preferences
                SET     preference_type_id  = ?preference_type_id,
                        notes               = ?notes,
                        last_modified_by    = ?last_modified_by,
                        last_modified_date  = ?last_modified_date,
                        deleted             = ?deleted
                WHERE   instructor_id       = ?instructor_id
                AND     account_id          = ?account_id", SchedulerDatabase);

            try
            {
                recordCount = cmd.ExecuteNonQuery();
                if (recordCount == 1)
                    row.AcceptChanges();
            }
            catch (Exception e)
            {
            }

            //If we updated a record then our job is done
            if (recordCount == 1) return recordCount;

            //If we didnt update a record, I guess we need to insert one:
            cmd.CommandText = String.Format(@"
                INSERT  {0}.instructor_account_preferences(instructor_id, account_id, 
                        preference_type_id, notes, last_modified_by, last_modified_date, deleted)
                VALUES  (?instructor_id, ?account_id,?preference_type_id,
                        ?notes,?last_modified_by,?last_modified_date,0)", SchedulerDatabase);

            try
            {
                recordCount = cmd.ExecuteNonQuery();
                if (recordCount == 1)
                    row.AcceptChanges();
            }
            catch (Exception e)
            {
                recordCount = 0;
            }
            return recordCount;
        }

        #endregion
    }
}
