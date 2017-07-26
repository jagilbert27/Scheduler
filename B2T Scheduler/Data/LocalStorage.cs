using System;
using System.Collections;
using System.Windows.Forms;

namespace B2T_Scheduler.Data
{
    public static class LocalStorage
    {
        public static void SaveState(MainForm form)
        {
            Application.UserAppDataRegistry.SetValue("WindowState", form.WindowState);

            if (form.WindowState == FormWindowState.Normal)
            {
                Application.UserAppDataRegistry.SetValue("Width", form.Size.Width);
                Application.UserAppDataRegistry.SetValue("Height", form.Size.Height);
                Application.UserAppDataRegistry.SetValue("Left", form.Location.X);
                Application.UserAppDataRegistry.SetValue("Top", form.Location.Y);
            }
            else if (form.WindowState == FormWindowState.Maximized)
            {
                Application.UserAppDataRegistry.SetValue("Width", form.RestoreBounds.Width);
                Application.UserAppDataRegistry.SetValue("Height", form.RestoreBounds.Height);
                Application.UserAppDataRegistry.SetValue("Left", form.RestoreBounds.Left);
                Application.UserAppDataRegistry.SetValue("Top", form.RestoreBounds.Top);
            }

            Application.UserAppDataRegistry.SetValue("uiPanel_MiddleTopHeight", form.uiPanel_MiddleTop.Height);
            Application.UserAppDataRegistry.SetValue("uiPanel_AppointmentDetailHeight", form.uiPanel_AppointmentDetail.Height);
            Application.UserAppDataRegistry.SetValue("uiPanel_LeftMenuWidth", form.uiPanel_LeftMenu.Width);

            Application.UserAppDataRegistry.SetValue("DatabaseUsername", form.txt_Username.Text);

            Application.UserAppDataRegistry.SetValue(
                "TimeLine.StartDate.OffsetDays",
                form.dtp_StartDateFilter.Value.Subtract(DateTime.Today).Days);

            Application.UserAppDataRegistry.SetValue(
                "TimeLine.EndDate.OffsetDays",
                form.dtp_EndDateFilter.Value.Subtract(DateTime.Today).Days);

            Application.UserAppDataRegistry.SetValue("TimeLineIntervalSize", form.trk_TimeLineScale.Value);

            //Which left menu is displayed
            Application.UserAppDataRegistry.SetValue("LeftMenu.SelectedPanel", form.uiPanel_LeftMenu.SelectedPanel.Name);

            //Save display options:
            Application.UserAppDataRegistry.SetValue("DisplayOptionsTree", GetTreeViewValues(form.tre_DisplayOptions));

            //Save filters
            Application.UserAppDataRegistry.SetValue("FilterOptionsTree", GetTreeViewValues(form.tre_Filters));
        }

        public static void LoadDisplayOptionState(MainForm form)
        {
            form.SetTimeLineScale(int.Parse(Application.UserAppDataRegistry.GetValue("TimeLineIntervalSize", 75).ToString()));
            SetTreeViewValues(form.tre_DisplayOptions, Application.UserAppDataRegistry.GetValue("DisplayOptionsTree", "").ToString());
        }

        public static void LoadFilterState(MainForm form)
        {
            SetTreeViewValues(form.tre_Filters, Application.UserAppDataRegistry.GetValue("FilterOptionsTree", "").ToString());
        }
        
        public static void LoadGeometryState(MainForm form)
        {
            int savedWidth = int.Parse(Application.UserAppDataRegistry.GetValue("Width", "869").ToString());
            int savedHeight = int.Parse(Application.UserAppDataRegistry.GetValue("Height", "608").ToString());
            int savedLeft = int.Parse(Application.UserAppDataRegistry.GetValue("Left", "175").ToString());
            int savedTop = int.Parse(Application.UserAppDataRegistry.GetValue("Top", "175").ToString());
            int savedUiPanel_MiddleTopHeight = int.Parse(Application.UserAppDataRegistry.GetValue("uiPanel_MiddleTopHeight", "225").ToString());
            int savedUiPanel_AppointmentDetailHeight = int.Parse(Application.UserAppDataRegistry.GetValue("uiPanel_AppointmentDetailHeight", "200").ToString());
            int savedUiPanel_LeftMenuWidth = int.Parse(Application.UserAppDataRegistry.GetValue("uiPanel_LeftMenuWidth", "250").ToString());

            //Which left menu is displayed
            foreach (Janus.Windows.UI.Dock.UIPanelBase child in form.uiPanel_LeftMenu.Panels)
                if (Application.UserAppDataRegistry.GetValue("LeftMenu.SelectedPanel", "").ToString() == child.Name)
                    form.uiPanel_LeftMenu.SelectedPanel = child;

            if (Application.UserAppDataRegistry.GetValue("WindowState", "Normal").ToString() == "Maximized")
                form.WindowState = FormWindowState.Maximized;

            if (savedWidth > 100 && savedHeight > 100)
            {
                form.Width = savedWidth;
                form.Height = savedHeight;
            }

            //            if (savedLeft < 800 && savedLeft > 0)
            form.Left = savedLeft;

            //          if (savedTop < 600 && savedTop > 0)
            form.Top = savedTop;

            Application.DoEvents();


            if (savedUiPanel_MiddleTopHeight < 20) savedUiPanel_MiddleTopHeight = 20;
            if (savedUiPanel_AppointmentDetailHeight < 20) savedUiPanel_AppointmentDetailHeight = 20;
            if (savedUiPanel_LeftMenuWidth < 20) savedUiPanel_LeftMenuWidth = 20;

            for (int i = 0; i < 10; i++)
            {
                if (form.uiPanel_MiddleTop.Height == savedUiPanel_MiddleTopHeight &&
                     form.uiPanel_AppointmentDetail.Height == savedUiPanel_AppointmentDetailHeight &&
                     form.uiPanel_LeftMenu.Width == savedUiPanel_LeftMenuWidth)
                    break;

                form.uiPanel_MiddleTop.Height = savedUiPanel_MiddleTopHeight;
                form.uiPanel_AppointmentDetail.Height = savedUiPanel_AppointmentDetailHeight;
                form.uiPanel_LeftMenu.Width = savedUiPanel_LeftMenuWidth;
                Application.DoEvents();
                //System.Threading.Thread.Sleep(1000);
            }
        }

        public static void LoadDateState(MainForm form)
        {
            int startOffsetDays = (int)(Math.Round(decimal.Parse(Application.UserAppDataRegistry.GetValue(
                    "TimeLine.StartDate.OffsetDays", "-90").ToString())));
            int endOffsetDays = (int)(Math.Round(decimal.Parse(Application.UserAppDataRegistry.GetValue(
                    "TimeLine.EndDate.OffsetDays", "365").ToString())));
            form.dtp_StartDateFilter.Value = DateTime.Today.AddDays(startOffsetDays);
            form.dtp_EndDateFilter.Value = DateTime.Today.AddDays(endOffsetDays);
        }

        public static void LoadLoginState(MainForm form)
        {
            //int savedSelectedDatabase = int.Parse(Application.UserAppDataRegistry.GetValue("SelectedDatabase", "-1").ToString());
            //if (savedSelectedDatabase >= 0) cmb_SelectDatabase.SelectedIndex = savedSelectedDatabase;

            //string savedDatabaseUsername = Application.UserAppDataRegistry.GetValue("DatabaseUsername", "").ToString();
            //if (savedDatabaseUsername.Length > 0) txt_Username.Text = savedDatabaseUsername;
            //SugarCrmHost = Application.UserAppDataRegistry.GetValue("SugarCrmHost", "b2ttraining.com").ToString();
            //int.TryParse(Application.UserAppDataRegistry.GetValue("SugarCrmPort", "3306").ToString(), out SugarCrmPort);
            //SugarCrmDatabase = Application.UserAppDataRegistry.GetValue("SugarCrmDatabase", "bttrai_sugarcrm").ToString();
            //SchedulerDatabase = Application.UserAppDataRegistry.GetValue("SchedulerDatabase", "bttrai_teachworks").ToString();
            //bool.TryParse(Application.UserAppDataRegistry.GetValue("EnableSshPortForwarding", "false").ToString(), out EnableSshPortForwarding);
            //SshPortForwardingLocalHost = Application.UserAppDataRegistry.GetValue("SshPortForwardingLocalHost", "127.0.0.1").ToString();
            //int.TryParse(Application.UserAppDataRegistry.GetValue("SshPortForwardingLocalPort", "3307").ToString(), out SshPortForwardingLocalPort);

            //dev
            //txt_Username.Text = "jeff@b2ttraining.com.dev";
            //txt_Password.Text = "DeerFoot27";

            //prod
            form.txt_Username.Text = "salesforce-tech@b2ttraining.com";
            form.txt_Password.Text = "B2T17admin";
        }

        public static bool ShowTipOnStartup(MainForm form) {
            return bool.Parse(Application.UserAppDataRegistry.GetValue("ShowTipOnStartup", "True").ToString());
        }

        private static void SetTreeViewValues(TreeView tree, string settings)
        {
            foreach (string nodeSetting in settings.Split("|".ToCharArray()))
            {
                Stack fullPath = new Stack(
                    nodeSetting.Split(",".ToCharArray())[0].Split("\\".ToCharArray()));
                TreeNode n = tree.Nodes[fullPath.Pop().ToString()];
                while (fullPath.Count > 0 && n != null)
                {
                    n = n.Nodes[fullPath.Pop().ToString()];
                }
                if (n == null) continue;
                n.Checked = nodeSetting.Split(",".ToCharArray())[1] == "checked" ? true : false;
                if (nodeSetting.Split(",".ToCharArray())[2] == "expanded") n.Expand();
            }
        }

        private static string GetTreeViewValues(TreeView tree)
        {
            string s = "";
            foreach (TreeNode child in tree.Nodes)
                s += GetTreeViewValues(child);
            return s;
        }

        private static string GetTreeViewValues(TreeNode n)
        {
            string s = "";
            TreeNode n0 = n;
            while (n0 != null)
            {
                s += n0.Name;
                n0 = n0.Parent;
                if (n0 != null) s += "\\";
            }
            s += "," + (n.Checked ? "checked" : "notchecked");
            s += "," + (n.IsExpanded ? "expanded" : "collapsed");
            s += "|";
            foreach (TreeNode child in n.Nodes)
                s += GetTreeViewValues(child);
            return s;
        }

    }
}
