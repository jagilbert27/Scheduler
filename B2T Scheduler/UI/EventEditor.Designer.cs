namespace B2T_Scheduler
{
    partial class EventEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EventEditor));
            this.cmb_Location = new System.Windows.Forms.ComboBox();
            this.cmb_CourseName = new System.Windows.Forms.ComboBox();
            this.cmb_Account = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.cmb_ClassContactName = new System.Windows.Forms.ComboBox();
            this.cmb_ShippingContactName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.mtb_ClassContactEmail = new System.Windows.Forms.MaskedTextBox();
            this.mtb_ClassContactPhone = new System.Windows.Forms.MaskedTextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.cmb_MaterialVersion = new System.Windows.Forms.ComboBox();
            this.cmb_LocationType = new System.Windows.Forms.ComboBox();
            this.label43 = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.dtp_MaterialShipDate = new System.Windows.Forms.DateTimePicker();
            this.label39 = new System.Windows.Forms.Label();
            this.dtp_EndDate = new System.Windows.Forms.DateTimePicker();
            this.dtp_StartDate = new System.Windows.Forms.DateTimePicker();
            this.label41 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.txt_Room = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nud_Duration = new System.Windows.Forms.NumericUpDown();
            this.label44 = new System.Windows.Forms.Label();
            this.cmb_Status = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label15 = new System.Windows.Forms.Label();
            this.img_InstructorNameError = new System.Windows.Forms.PictureBox();
            this.cmb_InstructorName = new System.Windows.Forms.ComboBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.nud_MaxStudents = new System.Windows.Forms.NumericUpDown();
            this.nud_NumRegistered = new System.Windows.Forms.NumericUpDown();
            this.cmb_ExpenseType = new System.Windows.Forms.ComboBox();
            this.dtp_WhitePaperSent = new System.Windows.Forms.DateTimePicker();
            this.cmb_BillingContactName = new System.Windows.Forms.ComboBox();
            this.cmb_InvoiceTerms = new System.Windows.Forms.ComboBox();
            this.neb_ClassFee = new Janus.Windows.GridEX.EditControls.NumericEditBox();
            this.neb_StudentFee = new Janus.Windows.GridEX.EditControls.NumericEditBox();
            this.txt_NumStudents = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nud_Duration)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.img_InstructorNameError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_MaxStudents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_NumRegistered)).BeginInit();
            this.SuspendLayout();
            // 
            // cmb_Location
            // 
            this.cmb_Location.DisplayMember = "ClassLocations.Name";
            this.cmb_Location.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_Location.FormattingEnabled = true;
            this.cmb_Location.Location = new System.Drawing.Point(283, 57);
            this.cmb_Location.Name = "cmb_Location";
            this.cmb_Location.Size = new System.Drawing.Size(117, 21);
            this.cmb_Location.TabIndex = 24;
            this.cmb_Location.ValueMember = "ClassLocations.ClassLocationID";
            // 
            // cmb_CourseName
            // 
            this.cmb_CourseName.DisplayMember = "AppointmentCategories.CategoryName";
            this.cmb_CourseName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_CourseName.FormattingEnabled = true;
            this.cmb_CourseName.Location = new System.Drawing.Point(283, 3);
            this.cmb_CourseName.Name = "cmb_CourseName";
            this.cmb_CourseName.Size = new System.Drawing.Size(117, 21);
            this.cmb_CourseName.TabIndex = 23;
            this.cmb_CourseName.ValueMember = "AppointmentCategories.AppointmentCategoryID";
            this.cmb_CourseName.SelectedIndexChanged += new System.EventHandler(this.cmb_CourseName_SelectedIndexChanged);
            // 
            // cmb_Account
            // 
            this.cmb_Account.DisplayMember = "Accounts.Name";
            this.cmb_Account.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_Account.FormattingEnabled = true;
            this.cmb_Account.Location = new System.Drawing.Point(87, 3);
            this.cmb_Account.Name = "cmb_Account";
            this.cmb_Account.Size = new System.Drawing.Size(117, 21);
            this.cmb_Account.TabIndex = 22;
            this.cmb_Account.ValueMember = "Accounts.AccountID";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Location = new System.Drawing.Point(3, 3);
            this.label11.Margin = new System.Windows.Forms.Padding(3);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 21);
            this.label11.TabIndex = 1;
            this.label11.Text = "Account";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label11, "The client account that is sponsoring the class, or public");
            // 
            // cmb_ClassContactName
            // 
            this.cmb_ClassContactName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_ClassContactName.FormattingEnabled = true;
            this.cmb_ClassContactName.Location = new System.Drawing.Point(87, 84);
            this.cmb_ClassContactName.Name = "cmb_ClassContactName";
            this.cmb_ClassContactName.Size = new System.Drawing.Size(117, 21);
            this.cmb_ClassContactName.TabIndex = 13;
            // 
            // cmb_ShippingContactName
            // 
            this.cmb_ShippingContactName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_ShippingContactName.FormattingEnabled = true;
            this.cmb_ShippingContactName.Location = new System.Drawing.Point(734, 84);
            this.cmb_ShippingContactName.Name = "cmb_ShippingContactName";
            this.cmb_ShippingContactName.Size = new System.Drawing.Size(117, 21);
            this.cmb_ShippingContactName.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(222, 3);
            this.label2.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 21);
            this.label2.TabIndex = 1;
            this.label2.Text = "Course";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(222, 30);
            this.label10.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(55, 21);
            this.label10.TabIndex = 1;
            this.label10.Text = "Version";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtb_ClassContactEmail
            // 
            this.mtb_ClassContactEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mtb_ClassContactEmail.Location = new System.Drawing.Point(87, 137);
            this.mtb_ClassContactEmail.Name = "mtb_ClassContactEmail";
            this.mtb_ClassContactEmail.Size = new System.Drawing.Size(117, 20);
            this.mtb_ClassContactEmail.TabIndex = 14;
            // 
            // mtb_ClassContactPhone
            // 
            this.mtb_ClassContactPhone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mtb_ClassContactPhone.Location = new System.Drawing.Point(87, 111);
            this.mtb_ClassContactPhone.Mask = "(999) 000-0000";
            this.mtb_ClassContactPhone.Name = "mtb_ClassContactPhone";
            this.mtb_ClassContactPhone.Size = new System.Drawing.Size(117, 20);
            this.mtb_ClassContactPhone.TabIndex = 14;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label46.Location = new System.Drawing.Point(869, 84);
            this.label46.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(74, 21);
            this.label46.TabIndex = 1;
            this.label46.Text = "Student Price";
            this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmb_MaterialVersion
            // 
            this.cmb_MaterialVersion.DisplayMember = "AppointmentCategories.CategoryName";
            this.cmb_MaterialVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_MaterialVersion.FormattingEnabled = true;
            this.cmb_MaterialVersion.Location = new System.Drawing.Point(283, 30);
            this.cmb_MaterialVersion.Name = "cmb_MaterialVersion";
            this.cmb_MaterialVersion.Size = new System.Drawing.Size(117, 21);
            this.cmb_MaterialVersion.TabIndex = 0;
            this.cmb_MaterialVersion.ValueMember = "AppointmentCategories.AppointmentCategoryID";
            // 
            // cmb_LocationType
            // 
            this.cmb_LocationType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_LocationType.FormattingEnabled = true;
            this.cmb_LocationType.Location = new System.Drawing.Point(87, 57);
            this.cmb_LocationType.Name = "cmb_LocationType";
            this.cmb_LocationType.Size = new System.Drawing.Size(117, 21);
            this.cmb_LocationType.TabIndex = 5;
            this.cmb_LocationType.Text = "OnSite";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label43.Location = new System.Drawing.Point(637, 30);
            this.label43.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(91, 21);
            this.label43.TabIndex = 1;
            this.label43.Text = "Expense Type";
            this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label42.Location = new System.Drawing.Point(869, 57);
            this.label42.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(74, 21);
            this.label42.TabIndex = 1;
            this.label42.Text = "Class Fee";
            this.label42.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(418, 84);
            this.label12.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(78, 21);
            this.label12.TabIndex = 8;
            this.label12.Text = "Max Class Size";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(418, 111);
            this.label13.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(58, 13);
            this.label13.TabIndex = 8;
            this.label13.Text = "Registered";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 84);
            this.label6.Margin = new System.Windows.Forms.Padding(3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 21);
            this.label6.TabIndex = 8;
            this.label6.Text = "Contact Name";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 30);
            this.label5.Margin = new System.Windows.Forms.Padding(3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 21);
            this.label5.TabIndex = 1;
            this.label5.Text = "Status";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label47.Location = new System.Drawing.Point(869, 111);
            this.label47.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(74, 20);
            this.label47.TabIndex = 8;
            this.label47.Text = "Num Students";
            this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label24.Location = new System.Drawing.Point(418, 57);
            this.label24.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(78, 21);
            this.label24.TabIndex = 8;
            this.label24.Text = "Duration";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dtp_MaterialShipDate
            // 
            this.dtp_MaterialShipDate.Checked = false;
            this.dtp_MaterialShipDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtp_MaterialShipDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtp_MaterialShipDate.Location = new System.Drawing.Point(734, 111);
            this.dtp_MaterialShipDate.MaxDate = new System.DateTime(2100, 1, 1, 0, 0, 0, 0);
            this.dtp_MaterialShipDate.Name = "dtp_MaterialShipDate";
            this.dtp_MaterialShipDate.ShowCheckBox = true;
            this.dtp_MaterialShipDate.Size = new System.Drawing.Size(117, 20);
            this.dtp_MaterialShipDate.TabIndex = 2;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label39.Location = new System.Drawing.Point(637, 84);
            this.label39.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(91, 21);
            this.label39.TabIndex = 8;
            this.label39.Text = "Shipping Contact";
            this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dtp_EndDate
            // 
            this.dtp_EndDate.Checked = false;
            this.dtp_EndDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtp_EndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtp_EndDate.Location = new System.Drawing.Point(502, 30);
            this.dtp_EndDate.Name = "dtp_EndDate";
            this.dtp_EndDate.ShowCheckBox = true;
            this.dtp_EndDate.Size = new System.Drawing.Size(117, 20);
            this.dtp_EndDate.TabIndex = 2;
            // 
            // dtp_StartDate
            // 
            this.dtp_StartDate.Checked = false;
            this.dtp_StartDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtp_StartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtp_StartDate.Location = new System.Drawing.Point(502, 3);
            this.dtp_StartDate.Name = "dtp_StartDate";
            this.dtp_StartDate.ShowCheckBox = true;
            this.dtp_StartDate.Size = new System.Drawing.Size(117, 20);
            this.dtp_StartDate.TabIndex = 2;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label41.Location = new System.Drawing.Point(869, 3);
            this.label41.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(74, 21);
            this.label41.TabIndex = 8;
            this.label41.Text = "Billing Contct";
            this.label41.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label22.Location = new System.Drawing.Point(3, 111);
            this.label22.Margin = new System.Windows.Forms.Padding(3);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(78, 20);
            this.label22.TabIndex = 8;
            this.label22.Text = "Contact Phone";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txt_Room
            // 
            this.txt_Room.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Room.Location = new System.Drawing.Point(283, 84);
            this.txt_Room.Name = "txt_Room";
            this.txt_Room.Size = new System.Drawing.Size(117, 20);
            this.txt_Room.TabIndex = 10;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Location = new System.Drawing.Point(222, 57);
            this.label17.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(55, 21);
            this.label17.TabIndex = 1;
            this.label17.Text = "Location";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(3, 57);
            this.label16.Margin = new System.Windows.Forms.Padding(3);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(78, 21);
            this.label16.TabIndex = 1;
            this.label16.Text = "Type";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label23.Location = new System.Drawing.Point(3, 137);
            this.label23.Margin = new System.Windows.Forms.Padding(3);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(78, 20);
            this.label23.TabIndex = 8;
            this.label23.Text = "Contact Email";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label20.Location = new System.Drawing.Point(222, 137);
            this.label20.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(55, 20);
            this.label20.TabIndex = 1;
            this.label20.Text = "End Time";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(637, 111);
            this.label3.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 20);
            this.label3.TabIndex = 1;
            this.label3.Text = "Matrl Ship Date";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nud_Duration
            // 
            this.nud_Duration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nud_Duration.Location = new System.Drawing.Point(502, 57);
            this.nud_Duration.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_Duration.Name = "nud_Duration";
            this.nud_Duration.Size = new System.Drawing.Size(117, 20);
            this.nud_Duration.TabIndex = 6;
            this.nud_Duration.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label44.Location = new System.Drawing.Point(869, 30);
            this.label44.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(74, 21);
            this.label44.TabIndex = 1;
            this.label44.Text = "Invoice Terms";
            this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmb_Status
            // 
            this.cmb_Status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_Status.FormattingEnabled = true;
            this.cmb_Status.Items.AddRange(new object[] {
            "Hold",
            "Tentative",
            "Confirmed"});
            this.cmb_Status.Location = new System.Drawing.Point(87, 30);
            this.cmb_Status.Name = "cmb_Status";
            this.cmb_Status.Size = new System.Drawing.Size(117, 21);
            this.cmb_Status.TabIndex = 5;
            this.cmb_Status.Text = "Hold";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label19.Location = new System.Drawing.Point(222, 111);
            this.label19.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(55, 20);
            this.label19.TabIndex = 1;
            this.label19.Text = "Start Time";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label45.Location = new System.Drawing.Point(418, 30);
            this.label45.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(78, 21);
            this.label45.TabIndex = 1;
            this.label45.Text = "End Date";
            this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Location = new System.Drawing.Point(222, 84);
            this.label18.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(55, 21);
            this.label18.TabIndex = 1;
            this.label18.Text = "Room";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(418, 3);
            this.label4.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 21);
            this.label4.TabIndex = 1;
            this.label4.Text = "Start Date";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label38.Location = new System.Drawing.Point(637, 57);
            this.label38.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(91, 21);
            this.label38.TabIndex = 1;
            this.label38.Text = "White Paper Sent";
            this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 10;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.textBox3, 8, 7);
            this.tableLayoutPanel1.Controls.Add(this.textBox2, 6, 7);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.label11, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmb_Account, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmb_Status, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label23, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.mtb_ClassContactEmail, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label22, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.mtb_ClassContactPhone, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmb_ClassContactName, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label16, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.cmb_LocationType, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.cmb_InstructorName, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmb_CourseName, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label10, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmb_MaterialVersion, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label17, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.cmb_Location, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.label18, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.txt_Room, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.label19, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.dateTimePicker1, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.label20, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.dateTimePicker2, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.label4, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.dtp_StartDate, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.label45, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.dtp_EndDate, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.label24, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.nud_Duration, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.label12, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.nud_MaxStudents, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.label13, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.nud_NumRegistered, 5, 4);
            this.tableLayoutPanel1.Controls.Add(this.label43, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmb_ExpenseType, 7, 1);
            this.tableLayoutPanel1.Controls.Add(this.label38, 6, 2);
            this.tableLayoutPanel1.Controls.Add(this.dtp_WhitePaperSent, 7, 2);
            this.tableLayoutPanel1.Controls.Add(this.label39, 6, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmb_ShippingContactName, 7, 3);
            this.tableLayoutPanel1.Controls.Add(this.label3, 6, 4);
            this.tableLayoutPanel1.Controls.Add(this.dtp_MaterialShipDate, 7, 4);
            this.tableLayoutPanel1.Controls.Add(this.label41, 8, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmb_BillingContactName, 9, 0);
            this.tableLayoutPanel1.Controls.Add(this.label44, 8, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmb_InvoiceTerms, 9, 1);
            this.tableLayoutPanel1.Controls.Add(this.label42, 8, 2);
            this.tableLayoutPanel1.Controls.Add(this.label46, 8, 3);
            this.tableLayoutPanel1.Controls.Add(this.neb_ClassFee, 9, 2);
            this.tableLayoutPanel1.Controls.Add(this.neb_StudentFee, 9, 3);
            this.tableLayoutPanel1.Controls.Add(this.label47, 8, 4);
            this.tableLayoutPanel1.Controls.Add(this.txt_NumStudents, 9, 4);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label7, 6, 6);
            this.tableLayoutPanel1.Controls.Add(this.label8, 8, 6);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1069, 499);
            this.tableLayoutPanel1.TabIndex = 27;
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Window;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.textBox3, 2);
            this.textBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox3.Location = new System.Drawing.Point(869, 190);
            this.textBox3.Margin = new System.Windows.Forms.Padding(15, 3, 3, 6);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(197, 303);
            this.textBox3.TabIndex = 16;
            // 
            // textBox2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.textBox2, 2);
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox2.Location = new System.Drawing.Point(637, 190);
            this.textBox2.Margin = new System.Windows.Forms.Padding(15, 3, 3, 6);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(214, 303);
            this.textBox2.TabIndex = 16;
            this.textBox2.TextChanged += new System.EventHandler(this.txt_TrackingNumbers_TextChanged);
            // 
            // textBox1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.textBox1, 6);
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 190);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(616, 303);
            this.textBox1.TabIndex = 15;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.label15);
            this.panel1.Controls.Add(this.img_InstructorNameError);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(622, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(109, 27);
            this.panel1.TabIndex = 29;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(15, 3);
            this.label15.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(63, 20);
            this.label15.TabIndex = 2;
            this.label15.Text = "Instructor";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // img_InstructorNameError
            // 
            this.img_InstructorNameError.Image = ((System.Drawing.Image)(resources.GetObject("img_InstructorNameError.Image")));
            this.img_InstructorNameError.Location = new System.Drawing.Point(84, 3);
            this.img_InstructorNameError.Name = "img_InstructorNameError";
            this.img_InstructorNameError.Size = new System.Drawing.Size(24, 21);
            this.img_InstructorNameError.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.img_InstructorNameError.TabIndex = 21;
            this.img_InstructorNameError.TabStop = false;
            this.img_InstructorNameError.Visible = false;
            // 
            // cmb_InstructorName
            // 
            this.cmb_InstructorName.DisplayMember = "EmployeeList.Name";
            this.cmb_InstructorName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_InstructorName.FormattingEnabled = true;
            this.cmb_InstructorName.Location = new System.Drawing.Point(734, 3);
            this.cmb_InstructorName.Name = "cmb_InstructorName";
            this.cmb_InstructorName.Size = new System.Drawing.Size(117, 21);
            this.cmb_InstructorName.TabIndex = 25;
            this.cmb_InstructorName.ValueMember = "EmployeeList.EmployeeID";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Checked = false;
            this.dateTimePicker1.CustomFormat = "hh:mm tt";
            this.dateTimePicker1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(283, 111);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(117, 20);
            this.dateTimePicker1.TabIndex = 2;
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Checked = false;
            this.dateTimePicker2.CustomFormat = "hh:mm tt";
            this.dateTimePicker2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker2.Location = new System.Drawing.Point(283, 137);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(117, 20);
            this.dateTimePicker2.TabIndex = 2;
            // 
            // nud_MaxStudents
            // 
            this.nud_MaxStudents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nud_MaxStudents.Location = new System.Drawing.Point(502, 84);
            this.nud_MaxStudents.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nud_MaxStudents.Name = "nud_MaxStudents";
            this.nud_MaxStudents.Size = new System.Drawing.Size(117, 20);
            this.nud_MaxStudents.TabIndex = 6;
            this.nud_MaxStudents.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_MaxStudents.ValueChanged += new System.EventHandler(this.nud_MaxStudents_ValueChanged);
            // 
            // nud_NumRegistered
            // 
            this.nud_NumRegistered.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nud_NumRegistered.Location = new System.Drawing.Point(502, 111);
            this.nud_NumRegistered.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nud_NumRegistered.Name = "nud_NumRegistered";
            this.nud_NumRegistered.Size = new System.Drawing.Size(117, 20);
            this.nud_NumRegistered.TabIndex = 6;
            this.nud_NumRegistered.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cmb_ExpenseType
            // 
            this.cmb_ExpenseType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_ExpenseType.FormattingEnabled = true;
            this.cmb_ExpenseType.Items.AddRange(new object[] {
            "Actual",
            "Flat"});
            this.cmb_ExpenseType.Location = new System.Drawing.Point(734, 30);
            this.cmb_ExpenseType.Name = "cmb_ExpenseType";
            this.cmb_ExpenseType.Size = new System.Drawing.Size(117, 21);
            this.cmb_ExpenseType.TabIndex = 13;
            // 
            // dtp_WhitePaperSent
            // 
            this.dtp_WhitePaperSent.Checked = false;
            this.dtp_WhitePaperSent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtp_WhitePaperSent.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtp_WhitePaperSent.Location = new System.Drawing.Point(734, 57);
            this.dtp_WhitePaperSent.Name = "dtp_WhitePaperSent";
            this.dtp_WhitePaperSent.ShowCheckBox = true;
            this.dtp_WhitePaperSent.Size = new System.Drawing.Size(117, 20);
            this.dtp_WhitePaperSent.TabIndex = 19;
            // 
            // cmb_BillingContactName
            // 
            this.cmb_BillingContactName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_BillingContactName.FormattingEnabled = true;
            this.cmb_BillingContactName.Location = new System.Drawing.Point(949, 3);
            this.cmb_BillingContactName.Name = "cmb_BillingContactName";
            this.cmb_BillingContactName.Size = new System.Drawing.Size(117, 21);
            this.cmb_BillingContactName.TabIndex = 13;
            // 
            // cmb_InvoiceTerms
            // 
            this.cmb_InvoiceTerms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_InvoiceTerms.FormattingEnabled = true;
            this.cmb_InvoiceTerms.Items.AddRange(new object[] {
            "",
            "After Class",
            "7 Days Prior",
            "45 Days Prior"});
            this.cmb_InvoiceTerms.Location = new System.Drawing.Point(949, 30);
            this.cmb_InvoiceTerms.Name = "cmb_InvoiceTerms";
            this.cmb_InvoiceTerms.Size = new System.Drawing.Size(117, 21);
            this.cmb_InvoiceTerms.TabIndex = 13;
            // 
            // neb_ClassFee
            // 
            this.neb_ClassFee.Dock = System.Windows.Forms.DockStyle.Fill;
            this.neb_ClassFee.FormatMask = Janus.Windows.GridEX.NumericEditFormatMask.Currency;
            this.neb_ClassFee.Location = new System.Drawing.Point(949, 57);
            this.neb_ClassFee.Name = "neb_ClassFee";
            this.neb_ClassFee.NullBehavior = Janus.Windows.GridEX.NumericEditNullBehavior.AllowDBNull;
            this.neb_ClassFee.Size = new System.Drawing.Size(117, 20);
            this.neb_ClassFee.TabIndex = 20;
            this.neb_ClassFee.Text = "$0.00";
            this.neb_ClassFee.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // neb_StudentFee
            // 
            this.neb_StudentFee.Dock = System.Windows.Forms.DockStyle.Fill;
            this.neb_StudentFee.FormatMask = Janus.Windows.GridEX.NumericEditFormatMask.Currency;
            this.neb_StudentFee.Location = new System.Drawing.Point(949, 84);
            this.neb_StudentFee.Name = "neb_StudentFee";
            this.neb_StudentFee.NullBehavior = Janus.Windows.GridEX.NumericEditNullBehavior.AllowDBNull;
            this.neb_StudentFee.Size = new System.Drawing.Size(117, 20);
            this.neb_StudentFee.TabIndex = 20;
            this.neb_StudentFee.Text = "$0.00";
            this.neb_StudentFee.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // txt_NumStudents
            // 
            this.txt_NumStudents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_NumStudents.Location = new System.Drawing.Point(949, 111);
            this.txt_NumStudents.Name = "txt_NumStudents";
            this.txt_NumStudents.Size = new System.Drawing.Size(117, 20);
            this.txt_NumStudents.TabIndex = 10;
            // 
            // label1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.label1, 6);
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 163);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(616, 21);
            this.label1.TabIndex = 8;
            this.label1.Text = "Class Notes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label7, 2);
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(637, 163);
            this.label7.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(214, 21);
            this.label7.TabIndex = 8;
            this.label7.Text = "Tracking No. [FedEx   |   UPS ]";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label8, 2);
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(869, 163);
            this.label8.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(197, 21);
            this.label8.TabIndex = 8;
            this.label8.Text = "Material Shipping Address";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Class Editor";
            // 
            // EventEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EventEditor";
            this.Size = new System.Drawing.Size(1069, 499);
            ((System.ComponentModel.ISupportInitialize)(this.nud_Duration)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.img_InstructorNameError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_MaxStudents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_NumRegistered)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmb_Location;
        private System.Windows.Forms.ComboBox cmb_CourseName;
        private System.Windows.Forms.ComboBox cmb_Account;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmb_ClassContactName;
        private System.Windows.Forms.ComboBox cmb_ShippingContactName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.MaskedTextBox mtb_ClassContactEmail;
        private System.Windows.Forms.MaskedTextBox mtb_ClassContactPhone;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.ComboBox cmb_MaterialVersion;
        private System.Windows.Forms.ComboBox cmb_LocationType;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.DateTimePicker dtp_MaterialShipDate;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.DateTimePicker dtp_EndDate;
        private System.Windows.Forms.DateTimePicker dtp_StartDate;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox txt_Room;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_Duration;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.ComboBox cmb_Status;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.PictureBox img_InstructorNameError;
        private System.Windows.Forms.ComboBox cmb_BillingContactName;
        private System.Windows.Forms.ComboBox cmb_InvoiceTerms;
        private Janus.Windows.GridEX.EditControls.NumericEditBox neb_ClassFee;
        private Janus.Windows.GridEX.EditControls.NumericEditBox neb_StudentFee;
        private System.Windows.Forms.TextBox txt_NumStudents;
        private System.Windows.Forms.ComboBox cmb_InstructorName;
        private System.Windows.Forms.DateTimePicker dtp_WhitePaperSent;
        private System.Windows.Forms.ComboBox cmb_ExpenseType;
        private System.Windows.Forms.NumericUpDown nud_MaxStudents;
        private System.Windows.Forms.NumericUpDown nud_NumRegistered;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
