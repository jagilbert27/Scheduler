namespace B2T_Scheduler
{
    partial class TipOfTheDay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.label1 = new System.Windows.Forms.Label();
            this.rtb_TipText = new System.Windows.Forms.RichTextBox();
            this.btn_Ok = new System.Windows.Forms.Button();
            this.lbl_PreviousTip = new System.Windows.Forms.LinkLabel();
            this.lbl_NextTip = new System.Windows.Forms.LinkLabel();
            this.ckb_ShowTipOnStartup = new System.Windows.Forms.CheckBox();
            this.lbl_TipNumber = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Edwardian Script ITC", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(216, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(165, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tip of the Day";
            // 
            // rtb_TipText
            // 
            this.rtb_TipText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb_TipText.BackColor = System.Drawing.SystemColors.Control;
            this.rtb_TipText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtb_TipText.Font = new System.Drawing.Font("Comic Sans MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb_TipText.Location = new System.Drawing.Point(223, 50);
            this.rtb_TipText.Name = "rtb_TipText";
            this.rtb_TipText.ReadOnly = true;
            this.rtb_TipText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtb_TipText.Size = new System.Drawing.Size(388, 195);
            this.rtb_TipText.TabIndex = 26;
            this.rtb_TipText.Text = "Loading Tip of the Day...";
            // 
            // btn_Ok
            // 
            this.btn_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Ok.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Ok.Location = new System.Drawing.Point(540, 268);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(71, 23);
            this.btn_Ok.TabIndex = 27;
            this.btn_Ok.Text = "OK";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Ok_Click);
            // 
            // lbl_PreviousTip
            // 
            this.lbl_PreviousTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl_PreviousTip.AutoSize = true;
            this.lbl_PreviousTip.Location = new System.Drawing.Point(31, 273);
            this.lbl_PreviousTip.Name = "lbl_PreviousTip";
            this.lbl_PreviousTip.Size = new System.Drawing.Size(75, 13);
            this.lbl_PreviousTip.TabIndex = 28;
            this.lbl_PreviousTip.TabStop = true;
            this.lbl_PreviousTip.Text = "< Previous Tip";
            this.lbl_PreviousTip.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbl_PreviousTip_LinkClicked);
            // 
            // lbl_NextTip
            // 
            this.lbl_NextTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl_NextTip.AutoSize = true;
            this.lbl_NextTip.Location = new System.Drawing.Point(118, 273);
            this.lbl_NextTip.Name = "lbl_NextTip";
            this.lbl_NextTip.Size = new System.Drawing.Size(56, 13);
            this.lbl_NextTip.TabIndex = 28;
            this.lbl_NextTip.TabStop = true;
            this.lbl_NextTip.Text = "Next Tip >";
            this.lbl_NextTip.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbl_NextTip_LinkClicked);
            // 
            // ckb_ShowTipOnStartup
            // 
            this.ckb_ShowTipOnStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ckb_ShowTipOnStartup.AutoSize = true;
            this.ckb_ShowTipOnStartup.Checked = true;
            this.ckb_ShowTipOnStartup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckb_ShowTipOnStartup.Location = new System.Drawing.Point(237, 272);
            this.ckb_ShowTipOnStartup.Name = "ckb_ShowTipOnStartup";
            this.ckb_ShowTipOnStartup.Size = new System.Drawing.Size(293, 17);
            this.ckb_ShowTipOnStartup.TabIndex = 29;
            this.ckb_ShowTipOnStartup.Text = "Show \"Tip of the Day\" while connecting to the database";
            this.ckb_ShowTipOnStartup.UseVisualStyleBackColor = true;
            // 
            // lbl_TipNumber
            // 
            this.lbl_TipNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_TipNumber.AutoSize = true;
            this.lbl_TipNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_TipNumber.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lbl_TipNumber.Location = new System.Drawing.Point(582, 9);
            this.lbl_TipNumber.Name = "lbl_TipNumber";
            this.lbl_TipNumber.Size = new System.Drawing.Size(29, 13);
            this.lbl_TipNumber.TabIndex = 30;
            this.lbl_TipNumber.Text = "Tip #";
            // 
            // TipOfTheDay
            // 
            this.AcceptButton = this.btn_Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::B2T_Scheduler.Properties.Resources.Splash;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.CancelButton = this.btn_Ok;
            this.ClientSize = new System.Drawing.Size(623, 303);
            this.Controls.Add(this.lbl_TipNumber);
            this.Controls.Add(this.ckb_ShowTipOnStartup);
            this.Controls.Add(this.lbl_NextTip);
            this.Controls.Add(this.lbl_PreviousTip);
            this.Controls.Add(this.btn_Ok);
            this.Controls.Add(this.rtb_TipText);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TipOfTheDay";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tip of the Day";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TipOfTheDay_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox rtb_TipText;
        private System.Windows.Forms.Button btn_Ok;
        private System.Windows.Forms.LinkLabel lbl_PreviousTip;
        private System.Windows.Forms.LinkLabel lbl_NextTip;
        private System.Windows.Forms.CheckBox ckb_ShowTipOnStartup;
        private System.Windows.Forms.Label lbl_TipNumber;

    }
}
