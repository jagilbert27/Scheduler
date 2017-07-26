namespace B2T_Scheduler
{
    partial class PrintOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintOptions));
            this.label1 = new System.Windows.Forms.Label();
            this.dtpPrintStartDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpPrintEndDate = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbPrintAll = new System.Windows.Forms.RadioButton();
            this.rbPrintMonth = new System.Windows.Forms.RadioButton();
            this.rbPrintWeek = new System.Windows.Forms.RadioButton();
            this.rbPrintDay = new System.Windows.Forms.RadioButton();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnPrintCancel = new System.Windows.Forms.Button();
            this.timeLinePrintDocument1 = new Janus.Windows.TimeLine.TimeLinePrintDocument();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.btnPrintPreview = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbPrintLandscape = new System.Windows.Forms.RadioButton();
            this.rbPrintPortrait = new System.Windows.Forms.RadioButton();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.pageSetupDialog1 = new System.Windows.Forms.PageSetupDialog();
            this.btnPrintPageSetup = new System.Windows.Forms.Button();
            this.btnPrintPrinter = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Date";
            // 
            // dtpPrintStartDate
            // 
            this.dtpPrintStartDate.Location = new System.Drawing.Point(73, 35);
            this.dtpPrintStartDate.Name = "dtpPrintStartDate";
            this.dtpPrintStartDate.Size = new System.Drawing.Size(200, 20);
            this.dtpPrintStartDate.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "End Date";
            // 
            // dtpPrintEndDate
            // 
            this.dtpPrintEndDate.Location = new System.Drawing.Point(73, 61);
            this.dtpPrintEndDate.Name = "dtpPrintEndDate";
            this.dtpPrintEndDate.Size = new System.Drawing.Size(200, 20);
            this.dtpPrintEndDate.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbPrintAll);
            this.groupBox1.Controls.Add(this.rbPrintMonth);
            this.groupBox1.Controls.Add(this.rbPrintWeek);
            this.groupBox1.Controls.Add(this.rbPrintDay);
            this.groupBox1.Location = new System.Drawing.Point(36, 102);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(88, 114);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Fit to Page";
            // 
            // rbPrintAll
            // 
            this.rbPrintAll.AutoSize = true;
            this.rbPrintAll.Location = new System.Drawing.Point(6, 89);
            this.rbPrintAll.Name = "rbPrintAll";
            this.rbPrintAll.Size = new System.Drawing.Size(36, 17);
            this.rbPrintAll.TabIndex = 0;
            this.rbPrintAll.Text = "All";
            this.rbPrintAll.UseVisualStyleBackColor = true;
            // 
            // rbPrintMonth
            // 
            this.rbPrintMonth.AutoSize = true;
            this.rbPrintMonth.Location = new System.Drawing.Point(6, 66);
            this.rbPrintMonth.Name = "rbPrintMonth";
            this.rbPrintMonth.Size = new System.Drawing.Size(55, 17);
            this.rbPrintMonth.TabIndex = 0;
            this.rbPrintMonth.Text = "Month";
            this.rbPrintMonth.UseVisualStyleBackColor = true;
            // 
            // rbPrintWeek
            // 
            this.rbPrintWeek.AutoSize = true;
            this.rbPrintWeek.Checked = true;
            this.rbPrintWeek.Location = new System.Drawing.Point(6, 43);
            this.rbPrintWeek.Name = "rbPrintWeek";
            this.rbPrintWeek.Size = new System.Drawing.Size(54, 17);
            this.rbPrintWeek.TabIndex = 0;
            this.rbPrintWeek.TabStop = true;
            this.rbPrintWeek.Text = "Week";
            this.rbPrintWeek.UseVisualStyleBackColor = true;
            // 
            // rbPrintDay
            // 
            this.rbPrintDay.AutoSize = true;
            this.rbPrintDay.Location = new System.Drawing.Point(7, 20);
            this.rbPrintDay.Name = "rbPrintDay";
            this.rbPrintDay.Size = new System.Drawing.Size(44, 17);
            this.rbPrintDay.TabIndex = 0;
            this.rbPrintDay.Text = "Day";
            this.rbPrintDay.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(306, 12);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(95, 23);
            this.btnPrint.TabIndex = 3;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnPrintCancel
            // 
            this.btnPrintCancel.Location = new System.Drawing.Point(306, 128);
            this.btnPrintCancel.Name = "btnPrintCancel";
            this.btnPrintCancel.Size = new System.Drawing.Size(95, 23);
            this.btnPrintCancel.TabIndex = 3;
            this.btnPrintCancel.Text = "Cancel";
            this.btnPrintCancel.UseVisualStyleBackColor = true;
            // 
            // timeLinePrintDocument1
            // 
            this.timeLinePrintDocument1.PrintExpandBox = false;
            // 
            // printPreviewDialog1
            // 
            this.printPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog1.Document = this.timeLinePrintDocument1;
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog1.Icon")));
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.UseAntiAlias = true;
            this.printPreviewDialog1.Visible = false;
            // 
            // btnPrintPreview
            // 
            this.btnPrintPreview.Location = new System.Drawing.Point(306, 99);
            this.btnPrintPreview.Name = "btnPrintPreview";
            this.btnPrintPreview.Size = new System.Drawing.Size(95, 23);
            this.btnPrintPreview.TabIndex = 3;
            this.btnPrintPreview.Text = "Print Preview...";
            this.btnPrintPreview.UseVisualStyleBackColor = true;
            this.btnPrintPreview.Click += new System.EventHandler(this.btnPrintPreview_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbPrintLandscape);
            this.groupBox2.Controls.Add(this.rbPrintPortrait);
            this.groupBox2.Location = new System.Drawing.Point(130, 102);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(88, 114);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Orientation";
            // 
            // rbPrintLandscape
            // 
            this.rbPrintLandscape.AutoSize = true;
            this.rbPrintLandscape.Location = new System.Drawing.Point(6, 43);
            this.rbPrintLandscape.Name = "rbPrintLandscape";
            this.rbPrintLandscape.Size = new System.Drawing.Size(78, 17);
            this.rbPrintLandscape.TabIndex = 0;
            this.rbPrintLandscape.Text = "Landscape";
            this.rbPrintLandscape.UseVisualStyleBackColor = true;
            // 
            // rbPrintPortrait
            // 
            this.rbPrintPortrait.AutoSize = true;
            this.rbPrintPortrait.Checked = true;
            this.rbPrintPortrait.Location = new System.Drawing.Point(7, 20);
            this.rbPrintPortrait.Name = "rbPrintPortrait";
            this.rbPrintPortrait.Size = new System.Drawing.Size(58, 17);
            this.rbPrintPortrait.TabIndex = 0;
            this.rbPrintPortrait.TabStop = true;
            this.rbPrintPortrait.Text = "Portrait";
            this.rbPrintPortrait.UseVisualStyleBackColor = true;
            // 
            // printDialog1
            // 
            this.printDialog1.Document = this.timeLinePrintDocument1;
            this.printDialog1.UseEXDialog = true;
            // 
            // pageSetupDialog1
            // 
            this.pageSetupDialog1.Document = this.timeLinePrintDocument1;
            // 
            // btnPrintPageSetup
            // 
            this.btnPrintPageSetup.Location = new System.Drawing.Point(306, 70);
            this.btnPrintPageSetup.Name = "btnPrintPageSetup";
            this.btnPrintPageSetup.Size = new System.Drawing.Size(95, 23);
            this.btnPrintPageSetup.TabIndex = 3;
            this.btnPrintPageSetup.Text = "Page Setup...";
            this.btnPrintPageSetup.UseVisualStyleBackColor = true;
            this.btnPrintPageSetup.Click += new System.EventHandler(this.btnPrintPageSetup_Click);
            // 
            // btnPrintPrinter
            // 
            this.btnPrintPrinter.Location = new System.Drawing.Point(306, 41);
            this.btnPrintPrinter.Name = "btnPrintPrinter";
            this.btnPrintPrinter.Size = new System.Drawing.Size(95, 23);
            this.btnPrintPrinter.TabIndex = 3;
            this.btnPrintPrinter.Text = "Printer...";
            this.btnPrintPrinter.UseVisualStyleBackColor = true;
            this.btnPrintPrinter.Click += new System.EventHandler(this.btnPrintPrinter_Click);
            // 
            // PrintOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 266);
            this.Controls.Add(this.btnPrintCancel);
            this.Controls.Add(this.btnPrintPreview);
            this.Controls.Add(this.btnPrintPageSetup);
            this.Controls.Add(this.btnPrintPrinter);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dtpPrintEndDate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtpPrintStartDate);
            this.Controls.Add(this.label1);
            this.Name = "PrintOptions";
            this.Text = "Print ";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpPrintStartDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpPrintEndDate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbPrintAll;
        private System.Windows.Forms.RadioButton rbPrintMonth;
        private System.Windows.Forms.RadioButton rbPrintWeek;
        private System.Windows.Forms.RadioButton rbPrintDay;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnPrintCancel;
        private Janus.Windows.TimeLine.TimeLinePrintDocument timeLinePrintDocument1;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog1;
        private System.Windows.Forms.Button btnPrintPreview;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbPrintLandscape;
        private System.Windows.Forms.RadioButton rbPrintPortrait;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.PageSetupDialog pageSetupDialog1;
        private System.Windows.Forms.Button btnPrintPageSetup;
        private System.Windows.Forms.Button btnPrintPrinter;
    }
}