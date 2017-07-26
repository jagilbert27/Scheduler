namespace B2T_Scheduler
{
    partial class SchedulePatternEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Janus.Windows.GridEX.GridEXLayout gridEX1_DesignTimeLayout = new Janus.Windows.GridEX.GridEXLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulePatternEditor));
            this.gridEX1 = new Janus.Windows.GridEX.GridEX();
            this.employeeScheduleFactorsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.scheduleDataSet = new B2T_Scheduler.ScheduleDataSet();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.Add_Button = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Delete_Button = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.Ok_Button = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridEX1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.employeeScheduleFactorsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scheduleDataSet)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridEX1
            // 
            this.gridEX1.AutoEdit = true;
            this.gridEX1.DataSource = this.employeeScheduleFactorsBindingSource;
            gridEX1_DesignTimeLayout.LayoutString = resources.GetString("gridEX1_DesignTimeLayout.LayoutString");
            this.gridEX1.DesignTimeLayout = gridEX1_DesignTimeLayout;
            this.gridEX1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEX1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.gridEX1.GroupByBoxVisible = false;
            this.gridEX1.ImageList = this.imageList1;
            this.gridEX1.Location = new System.Drawing.Point(3, 16);
            this.gridEX1.Name = "gridEX1";
            this.gridEX1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.Default;
            this.gridEX1.Size = new System.Drawing.Size(534, 305);
            this.gridEX1.TabIndex = 5;
            this.gridEX1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2007;
            this.gridEX1.DropDownHide += new Janus.Windows.GridEX.DropDownHideEventHandler(this.gridEX1_DropDownHide);
            // 
            // employeeScheduleFactorsBindingSource
            // 
            this.employeeScheduleFactorsBindingSource.DataMember = "EmployeeScheduleFactors";
            this.employeeScheduleFactorsBindingSource.DataSource = this.scheduleDataSet;
            // 
            // scheduleDataSet
            // 
            this.scheduleDataSet.DataSetName = "ScheduleDataSet";
            this.scheduleDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "0,4");
            this.imageList1.Images.SetKeyName(1, "1,3");
            this.imageList1.Images.SetKeyName(2, "2,3");
            this.imageList1.Images.SetKeyName(3, "1,4");
            this.imageList1.Images.SetKeyName(4, "1,2");
            this.imageList1.Images.SetKeyName(5, "2,4");
            this.imageList1.Images.SetKeyName(6, "3,4");
            this.imageList1.Images.SetKeyName(7, "0,0");
            this.imageList1.Images.SetKeyName(8, "1,12");
            // 
            // Add_Button
            // 
            this.Add_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Add_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Add_Button.Image = ((System.Drawing.Image)(resources.GetObject("Add_Button.Image")));
            this.Add_Button.Location = new System.Drawing.Point(562, 177);
            this.Add_Button.Name = "Add_Button";
            this.Add_Button.Size = new System.Drawing.Size(75, 75);
            this.Add_Button.TabIndex = 8;
            this.Add_Button.Text = "New Schedule Preference";
            this.Add_Button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Add_Button.UseVisualStyleBackColor = true;
            this.Add_Button.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.gridEX1);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(540, 324);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "History";
            // 
            // Delete_Button
            // 
            this.Delete_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Delete_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Delete_Button.Image = ((System.Drawing.Image)(resources.GetObject("Delete_Button.Image")));
            this.Delete_Button.Location = new System.Drawing.Point(562, 258);
            this.Delete_Button.Name = "Delete_Button";
            this.Delete_Button.Size = new System.Drawing.Size(75, 75);
            this.Delete_Button.TabIndex = 8;
            this.Delete_Button.Text = "Delete Selected";
            this.Delete_Button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Delete_Button.UseVisualStyleBackColor = true;
            this.Delete_Button.Click += new System.EventHandler(this.Delete_Button_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Image = ((System.Drawing.Image)(resources.GetObject("Cancel_Button.Image")));
            this.Cancel_Button.Location = new System.Drawing.Point(562, 84);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 50);
            this.Cancel_Button.TabIndex = 11;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // Ok_Button
            // 
            this.Ok_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok_Button.Image = ((System.Drawing.Image)(resources.GetObject("Ok_Button.Image")));
            this.Ok_Button.Location = new System.Drawing.Point(562, 28);
            this.Ok_Button.Name = "Ok_Button";
            this.Ok_Button.Size = new System.Drawing.Size(75, 50);
            this.Ok_Button.TabIndex = 12;
            this.Ok_Button.Text = "OK";
            this.Ok_Button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Ok_Button.UseVisualStyleBackColor = true;
            this.Ok_Button.Click += new System.EventHandler(this.Ok_Button_Click);
            // 
            // SchedulePatternEditor
            // 
            this.AcceptButton = this.Ok_Button;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(645, 348);
            this.Controls.Add(this.Ok_Button);
            this.Controls.Add(this.Add_Button);
            this.Controls.Add(this.Delete_Button);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.groupBox2);
            this.HelpButton = true;
            this.Name = "SchedulePatternEditor";
            this.Text = "SchedulePatternEditor";
            this.Load += new System.EventHandler(this.SchedulePatternEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridEX1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.employeeScheduleFactorsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scheduleDataSet)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Janus.Windows.GridEX.GridEX gridEX1;
        public ScheduleDataSet scheduleDataSet;
        private System.Windows.Forms.Button Add_Button;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button Delete_Button;
        private System.Windows.Forms.Button Ok_Button;
        private System.Windows.Forms.BindingSource employeeScheduleFactorsBindingSource;
        private System.Windows.Forms.ImageList imageList1;

    }
}