using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.TimeLine;

namespace B2T_Scheduler
{
    public partial class PrintOptions : Form
    {
        private Janus.Windows.TimeLine.TimeLine timeLine = null;
        private Boolean ShowPreview = true;

        public PrintOptions()
        {
            InitializeComponent();
            this.AcceptButton = this.btnPrint;
            this.CancelButton = this.btnPrintCancel;
        }




        public void ShowPrintForm(TimeLine timeLine)
        {
            this.timeLine = timeLine;
            dtpPrintStartDate.Value = this.timeLine.FirstDate;
            dtpPrintEndDate.Value = this.timeLine.LastDate;
            this.timeLinePrintDocument1.TimeLine = this.timeLine;
            this.ShowDialog();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            ShowPreview = false;
            PrintTimeline();
        }

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            ShowPreview = true;
            PrintTimeline();
        }

    
        private void PrintTimeline()
        {
            if (rbPrintAll.Checked)
            {
                this.timeLinePrintDocument1.StartDate = this.dtpPrintStartDate.Value;
                this.timeLinePrintDocument1.EndDate = this.dtpPrintEndDate.Value;
            }
            else if(rbPrintMonth.Checked)
            {
                this.timeLinePrintDocument1.StartDate = this.dtpPrintStartDate.Value.Date.AddDays(0 - this.dtpPrintStartDate.Value.Day + 1);
                while (this.timeLinePrintDocument1.StartDate <= this.dtpPrintEndDate.Value)
                {
                    this.timeLinePrintDocument1.EndDate = this.timeLinePrintDocument1.StartDate.AddMonths(1);
                    if (ShowPreview)
                        this.printPreviewDialog1.ShowDialog();
                    else
                        this.timeLinePrintDocument1.Print();
                    this.timeLinePrintDocument1.StartDate = this.timeLinePrintDocument1.EndDate;
                }

            }
            else if (rbPrintWeek.Checked)
            {
                //Back up to the previous monday
                this.timeLinePrintDocument1.StartDate = this.dtpPrintStartDate.Value.Date.AddDays(0 - this.dtpPrintStartDate.Value.DayOfWeek +1 );
                while (this.timeLinePrintDocument1.StartDate <= this.dtpPrintEndDate.Value)
                {
                    this.timeLinePrintDocument1.EndDate = this.timeLinePrintDocument1.StartDate.AddDays(5);
                    if(ShowPreview)
                        this.printPreviewDialog1.ShowDialog();
                    else
                        this.timeLinePrintDocument1.Print();
                    this.timeLinePrintDocument1.StartDate = this.timeLinePrintDocument1.StartDate.AddDays(7);
            }
            
            }
            else if (rbPrintDay.Checked)
            {

            }
        }

        private void btnPrintPageSetup_Click(object sender, EventArgs e)
        {
            this.pageSetupDialog1.ShowDialog();
        }

        private void btnPrintPrinter_Click(object sender, EventArgs e)
        {
            this.printDialog1.ShowDialog();
        }

    }
}
