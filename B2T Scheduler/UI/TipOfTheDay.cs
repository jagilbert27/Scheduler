using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace B2T_Scheduler
{
    partial class TipOfTheDay : Form
    {
        string TipFileNameTemplate = "{0}\\Documents\\Tips\\Tip_{1:000}.rtf";

        public TipOfTheDay()
        {
            InitializeComponent();

            //Retrieve the last displayed tip number
            this.Text = "Scheduler Tip of the day";

            ckb_ShowTipOnStartup.Checked = bool.Parse(Application.UserAppDataRegistry.GetValue("ShowTipOnStartup", "True").ToString());

            ShowNextTip();
            
        }

        private void ShowTip(int tipNumber)
        {
            //Display the tip number
            lbl_TipNumber.Text = String.Format("Tip {0}", tipNumber);

            //Construct the name of the tip file:
            string TipFileName = string.Format(TipFileNameTemplate, Application.StartupPath, tipNumber);

            //Load the file into the rich text box
            rtb_TipText.LoadFile(TipFileName);

            //Persist the last tip number that was displayed
            Application.UserAppDataRegistry.SetValue("LastTipNumber", tipNumber.ToString());

        }

        private void ShowNextTip()
        {
            int tipNumber = int.Parse(Application.UserAppDataRegistry.GetValue("LastTipNumber", "0").ToString());
            if (!System.IO.File.Exists(String.Format(TipFileNameTemplate, Application.StartupPath, ++tipNumber)))
                tipNumber = 1;
            ShowTip(tipNumber);
        }

        private void ShowPreviousTip()
        {
            int tipNumber = int.Parse(Application.UserAppDataRegistry.GetValue("LastTipNumber", "0").ToString());
            if (--tipNumber < 1)
                tipNumber = GetHighestTipNumber();
            ShowTip(tipNumber);
        }

        private void lbl_NextTip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowNextTip();
        }

        private void lbl_PreviousTip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowPreviousTip();
        }

        private int GetHighestTipNumber()
        {
            int tipNumber = 0;
            while (System.IO.File.Exists(String.Format(TipFileNameTemplate, Application.StartupPath, tipNumber+1)))
                tipNumber++;
            return tipNumber;
        }

        private void TipOfTheDay_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.UserAppDataRegistry.SetValue("ShowTipOnStartup", this.ckb_ShowTipOnStartup.Checked.ToString());

        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
