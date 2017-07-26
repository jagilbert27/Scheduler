using Janus.Windows.TimeLine;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace B2T_Scheduler.UI
{
    class Filter
    {
        MainForm mainForm { get; set; }
        TreeView tre_Filters { get; set; }
        bool IsFilterCheckEnabled { get; set; } = true;
        ScheduleDataSet dataSet { get; set; }

        public Filter(MainForm f)
        {
            mainForm = f;
            tre_Filters = f.tre_Filters;
            dataSet = f.scheduleDataSet1;
        }

        public static TimeLineFilterCondition GetChildCondition(IFilterCondition parent, string key)
        {
            try { return parent.FilterCondition.Conditions[key]; }
            catch (Exception) { return null; }
        }

        //Load the values into the filter tree
        public void InitAllFilters()
        {
            IsFilterCheckEnabled = false;
            tre_Filters.Nodes.Clear();
            InitEmployeeFilter();
            InitCourseFilter();
            InitClassFilter();
            InitAccountsFilter();
        }

        public TimeLineFilterCondition GetFilterCondition()
        {
            var timeLine = mainForm.timeLine1;

            TimeLineFilterCondition filter = new TimeLineFilterCondition();

            filter.AddCondition(LogicalOperator.And, GetDeletedClassesFilter(timeLine));
            filter.AddCondition(LogicalOperator.And, GetEmployeesFilter(timeLine));
            filter.AddCondition(LogicalOperator.And, GetCoursesFilter(timeLine));
            filter.AddCondition(LogicalOperator.And, GetEventsFilter(timeLine));
            filter.AddCondition(LogicalOperator.And, GetAccountsFilter(timeLine));
            filter.AddCondition(LogicalOperator.And, GetClassTypeFilter(timeLine));
            filter.AddCondition(LogicalOperator.And, GetClassStatusFilter(timeLine));
            filter.AddCondition(LogicalOperator.And, GetWhitePaperFilter(timeLine));

            return filter;
        }

        private TimeLineFilterCondition GetDeletedClassesFilter(TimeLine t)
        {
            return new TimeLineFilterCondition
            {
                Key = "DeletedClasses",
                Field = t.Fields["Deleted"],
                ConditionOperator = ConditionOperator.NotEqual,
                Value1 = true
            };
        }

        private TimeLineFilterCondition GetEmployeesFilter(TimeLine t)
        {
            var showQualifiedInstructors = mainForm.ck_ShowQualifiedInstructors.Checked;

            //Get the list of which instructors are checked in the filter
            int numEmployees = 0;
            var employeeFilterValues = new ArrayList();

            foreach (TreeNode n0 in tre_Filters.Nodes["People"].Nodes)
            {
                if (n0.Nodes.Count == 0)
                {
                    numEmployees++;
                    if (n0.Checked)
                        employeeFilterValues.Add(n0.Name);
                }
                foreach (TreeNode n1 in n0.Nodes)
                {
                    numEmployees++;
                    if (n1.Checked)
                        employeeFilterValues.Add(n1.Name);
                }
            }

            // Remove the unqualified instructors from the filter if necessary
            if (showQualifiedInstructors && mainForm.tre_Courses.SelectedNode != null)
            {
                var courseID = mainForm.tre_Courses.SelectedNode.Name;
                var course = dataSet.AppointmentCategories.FindByAppointmentCategoryID(courseID);
                if (course != null && course.Type != "Event")
                {
                    for (int idx = employeeFilterValues.Count - 1; idx >= 0; idx--)
                    {
                        var displayOrder = (short)employeeFilterValues[idx];
                        if (displayOrder >= 1000) continue;
                        var employeeID = GetEmployeeIdFromDisplayOrder(displayOrder.ToString());

                        var qualification = dataSet.EmployeeCourseQualifications.FindByEmployeeIDCourseID(employeeID, courseID);
                        if (qualification == null ||
                            qualification.IsQualificationLevelNull() ||
                            qualification.QualificationLevel <= 0 ||
                            (!qualification.IsEndDateNull() && qualification.EndDate < DateTime.Today))
                        {
                            employeeFilterValues.RemoveAt(idx);
                        }
                    }
                }
            }

            //Create an employee filter only if some employees are not checked.
            var condition = employeeFilterValues.Count == numEmployees
                ? new TimeLineFilterCondition { Key = "Employees" }
                : new TimeLineFilterCondition
                {
                    Key = "Employees",
                    Field = t.Fields["DisplayOrder"],
                    ConditionOperator = ConditionOperator.In,
                    Value1 = employeeFilterValues
                };

            return condition;
        }

        private TimeLineFilterCondition GetCoursesFilter(TimeLine t)
        {
            int numCourses = 0;
            ArrayList courseFilterValues = new ArrayList();
            foreach (TreeNode n0 in tre_Filters.Nodes["Courses"].Nodes)
            {
                foreach (TreeNode n1 in n0.Nodes)
                {
                    numCourses++;
                    if (!n1.Checked)
                        courseFilterValues.Add(n1.Name);
                }
            }

            return courseFilterValues.Count == 0
                ? new TimeLineFilterCondition { Key = "Courses" }
                : new TimeLineFilterCondition
                {
                    Key = "Courses",
                    Field = t.Fields["AppointmentCategoryID"],
                    ConditionOperator = ConditionOperator.NotIn,
                    Value1 = courseFilterValues
                };
        }

        private TimeLineFilterCondition GetEventsFilter(TimeLine t)
        {
            var values = new ArrayList();
            foreach (TreeNode n in tre_Filters.Nodes["Events"].Nodes)
                if (!n.Checked)
                    values.Add(n.Name);

            return values.Count == 0
                ? new TimeLineFilterCondition { Key = "Events" }
                : new TimeLineFilterCondition
                {
                    Key = "Events",
                    Field = t.Fields["AppointmentCategoryID"],
                    ConditionOperator = ConditionOperator.NotIn,
                    Value1 = values
                };
        }

        private TimeLineFilterCondition GetAccountsFilter(TimeLine t)
        {
            var values = new ArrayList();
            foreach (TreeNode n in tre_Filters.Nodes["Accounts"].Nodes)
                if (!n.Checked)
                    values.Add(n.Name);

            return values.Count == 0
                ? new TimeLineFilterCondition { Key = "Accounts" }
                : new TimeLineFilterCondition
                {
                    Key = "Accounts",
                    Field = t.Fields["AccountID"],
                    ConditionOperator = ConditionOperator.NotIn,
                    Value1 = values
                };
        }

        private TimeLineFilterCondition GetClassTypeFilter(TimeLine t)
        {
            if (tre_Filters.Nodes["ClassTypePublic"].Checked &&
                tre_Filters.Nodes["ClassTypeOnsite"].Checked)
                return new TimeLineFilterCondition { Key = "ClassType" };

            var values = new ArrayList();
            if (tre_Filters.Nodes["ClassTypePublic"].Checked == false) values.Add("Public");
            if (tre_Filters.Nodes["ClassTypeOnsite"].Checked == false) values.Add("Onsite");

            return new TimeLineFilterCondition
            {
                Key = "ClassType",
                Field = t.Fields["ClassType"],
                ConditionOperator = ConditionOperator.NotIn,
                Value1 = values
            };
        }

        private TimeLineFilterCondition GetClassStatusFilter(TimeLine t)
        {
            if (tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusHeld"].Checked &&
                tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusTentative"].Checked &&
                tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusConfirmed"].Checked)
                return new TimeLineFilterCondition { Key = "ClassStatus" };

            ArrayList values = new ArrayList();
            if (!tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusHeld"].Checked) values.Add("Hold");
            if (!tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusTentative"].Checked) values.Add("Tentative");
            if (!tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusConfirmed"].Checked) values.Add("Confirmed");

            return new TimeLineFilterCondition
            {
                Key = "ClassStatus",
                Field = t.Fields["Status"],
                ConditionOperator = ConditionOperator.NotIn,
                Value1 = values
            };
        }

        private TimeLineFilterCondition GetWhitePaperFilter(TimeLine t)
        {
            if (tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusWhitePaperNotSent"].Checked &&
                tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusWhitePaperSent"].Checked)
                return new TimeLineFilterCondition { Key = "WhitePaper" };

            return new TimeLineFilterCondition
            {
                Key = "WhitePaper",
                Field = t.Fields["WhitePaperSentDate"],
                ConditionOperator = tre_Filters.Nodes["ClassStatus"].Nodes["ClassStatusWhitePaperSent"].Checked == false
                ? ConditionOperator.IsNull
                : ConditionOperator.NotIsNull
            };
        }

        /// <summary>
        /// (Re)Initializes the employee filter tree nodes (can be called multiple times)
        /// </summary>
        public void InitEmployeeFilter()
        {
            bool prevIsFilterCheckEnabled = IsFilterCheckEnabled;
            IsFilterCheckEnabled = false;
            String prevFilterValues = GetTreeViewValues(tre_Filters);

            //Empty out the employee filter tree
            if (tre_Filters.Nodes.Find("People", true).Length > 0)
                tre_Filters.Nodes.Find("People", true)[0].Remove();

            TreeNode peopleNode = tre_Filters.Nodes.Insert(0, "People", "People", "People", "People");
            
            //Add the AllB2T Employee
            foreach (var employee in
                from employee in dataSet.EmployeeList
                where employee.DisplayOrder == 1001
                orderby employee.DisplayOrder
                select employee)
                peopleNode.Nodes.Add(
                    employee.DisplayOrder.ToString(),
                    employee[mainForm.GetEmployeeNameFieldPreference()].ToString(), "People", "People").Checked = true;

            //Add the Unassigned Employee
            foreach (var employee in
                from employee in dataSet.EmployeeList
                where employee.DisplayOrder == 1002
                orderby employee.DisplayOrder
                select employee)
                peopleNode.Nodes.Add(
                    employee.DisplayOrder.ToString(),
                    employee[mainForm.GetEmployeeNameFieldPreference()].ToString(), "PersonUnknown", "PersonUnknown").Checked = true;

            //Instructors:
            foreach (var employee in
                from employee in dataSet.EmployeeList
                where employee.DisplayOrder < 1000
                where employee.IsInstructor
                where employee.EmployeeStatus == "Active"
                orderby employee.DisplayOrder
                select employee)
                GetOrCreateTreeNode(peopleNode, "Instructors", "Instructors", "People")
                    .Nodes.Add(employee.DisplayOrder.ToString(),
                    employee[mainForm.GetEmployeeNameFieldPreference()].ToString(), "Person").Checked = true;

            //Salesforce Users:
            foreach (var employee in
                from employee in dataSet.EmployeeList
                where employee.DisplayOrder < 1000
                where employee.IsInstructor == false
                where employee.Type == "User"
                orderby employee.DisplayOrder
                select employee)
                GetOrCreateTreeNode(peopleNode, "Users", "Salesforce Users", "People")
                    .Nodes.Add(employee.DisplayOrder.ToString(),
                    employee[mainForm.GetEmployeeNameFieldPreference()].ToString(), "Person").Checked = true;
            
            SetTreeViewValues(tre_Filters, prevFilterValues);
            IsFilterCheckEnabled = prevIsFilterCheckEnabled;
        }

        private void InitCourseFilter()
        {

            //Course Filter
            var courseNode = tre_Filters.Nodes.Add("Courses", "Courses", "Courses", "Courses");
            courseNode.Checked = true;

            TreeNode eventNode = tre_Filters.Nodes.Add("Events", "Events");
            eventNode.Checked = true;


            foreach (DataRowView drv in dataSet.AppointmentCategories.DefaultView)
            {
                var course = (ScheduleDataSet.AppointmentCategoriesRow)drv.Row;
                var id = course.AppointmentCategoryID;
                var type = course.Type;
                var name = course.CategoryName;
                TreeNode newNode;// = eventNode.Nodes.Add(id, name);

                if (type == "Classes")
                    newNode = courseNode.Nodes.Add(id, name);
                else if (courseNode.Nodes.ContainsKey(type))
                    newNode = tre_Filters.Nodes.Find(type, true)[0].Nodes.Add(id, name);
                else
                    newNode = eventNode.Nodes.Add(id, name);
                newNode.Checked = true;

                //Format the node
                if (course.FormatsRow != null)
                {
                    newNode.ForeColor = course.FormatsRow.GetForeColor(Color.Black);
                    newNode.BackColor = course.FormatsRow.GetBackColor(Color.White);
                }
                if (course.Image.Length > 0)
                    newNode.ImageKey = course.Image;
                else if (course.FormatsRow != null && !course.FormatsRow.IsBackgroundImageKeyNull())
                    newNode.ImageKey = course.FormatsRow.BackgroundImageKey;
                else if (type == "Event")
                    newNode.ImageKey = "Event";
                else if (type == "Proposed")
                    newNode.ImageKey = "";
                else
                    newNode.ImageKey = "Course";
                newNode.SelectedImageKey = newNode.ImageKey;
            }

        }

        private void InitClassFilter()
        {
            //Class Type
            tre_Filters.Nodes.Add("ClassTypeOnsite", "Onsite", "Accounts", "Accounts").Checked = true;
            tre_Filters.Nodes.Add("ClassTypePublic", "Public", "Accounts", "Accounts").Checked = true;

            //Class Status
            TreeNode classStatusNode = tre_Filters.Nodes.Add("ClassStatus", "Status");
            classStatusNode.Checked = true;
            classStatusNode.Nodes.Add("ClassStatusHeld", "Hold").Checked = true;
            classStatusNode.Nodes.Add("ClassStatusTentative", "Tentative").Checked = true;
            classStatusNode.Nodes.Add("ClassStatusConfirmed", "Confirmed").Checked = true;
            classStatusNode.Nodes.Add("ClassStatusWhitePaperNotSent", "White Paper NOT Sent").Checked = true;
            classStatusNode.Nodes.Add("ClassStatusWhitePaperSent", "White Paper Sent").Checked = true;
            classStatusNode.Nodes["ClassStatusHeld"].BackColor = GetFormat("ClassStatus", "Hold").GetBackColor();
            classStatusNode.Nodes["ClassStatusTentative"].BackColor = GetFormat("ClassStatus", "Tentative").GetBackColor();
            classStatusNode.Nodes["ClassStatusConfirmed"].BackColor = GetFormat("ClassStatus", "Tentative").GetBackColor();
            classStatusNode.Nodes["ClassStatusWhitePaperNotSent"].BackColor = GetFormat("ClassStatus", "ConfirmedNotSent").GetBackColor();
            classStatusNode.Nodes["ClassStatusWhitePaperSent"].BackColor = GetFormat("ClassStatus", "ConfirmedSent").GetBackColor();
        }

        private void InitAccountsFilter()
        {
            TreeNode accountNode = tre_Filters.Nodes.Add("Accounts", "Accounts");
            accountNode.Checked = true;
            var accounts = new SortedList();
            foreach (ScheduleDataSet.AppointmentsRow appt in dataSet.Appointments.Rows)
            {
                if (appt.AccountID != null && appt.AccountsRow != null)
                {
                    if (!accounts.ContainsValue(appt.AccountID))
                    {
                        accounts.Add(appt.AccountsRow.Name, appt.AccountID);
                    }
                }
            }
            for (int i = 0; i < accounts.Count; i++)
            {
                accountNode.Nodes.Add(accounts.GetByIndex(i).ToString(), accounts.GetKey(i).ToString()).Checked = true;
            }
            IsFilterCheckEnabled = true;
        }

        private TreeNode GetOrCreateTreeNode(TreeNode parentNode, string nodeKey, string nodeName, string nodeImageKey, string nodeSelectedImageKey = null)
        {
            if (parentNode.Nodes.ContainsKey(nodeKey))
                return parentNode.Nodes[nodeKey];
            return parentNode.Nodes.Add(nodeKey, nodeName, nodeImageKey, nodeSelectedImageKey ?? nodeImageKey);
        }

        /// <summary>
        /// Sets the check state and expanded state of check box child of 
        /// the specified tree node according to the settings string
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="settings"></param>
        public void SetTreeViewValues(TreeView tree, String settings)
        {
            foreach (string nodeSetting in settings.Split("|".ToCharArray()))
            {
                var fullPath = new Stack(
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

        /// <summary>
        /// retrieves a string describing the check state and expanded state of 
        /// all node children of the specified TreeView, suitable for saving to the 
        /// registry.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public string GetTreeViewValues(TreeView tree)
        {
            string s = "";
            foreach (TreeNode child in tree.Nodes)
                s += GetTreeViewValues(child);
            return s;
        }

        /// <summary>
        /// retrieves a string describing the check state and expanded state of 
        /// all node children of the specified node, suitable for saving to the 
        /// registry.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public string GetTreeViewValues(TreeNode n)
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

        /// <summary>
        /// retrieves formatting information from the formats MySql table
        /// </summary>
        /// <param name="type">a value from the parent_type field of the formats table</param>
        /// <param name="id">a value from the parent_id field of the formats table</param>
        /// <returns>Colors and icon information to be applied to a user interface element</returns>
        private ScheduleDataSet.FormatsRow GetFormat(String type, String id)
        {
            return dataSet.Formats.FindByParentTypeParentID(type, id);
        }

        private string GetEmployeeIdFromDisplayOrder(string displayOrder)
        {
            int i;
            if (int.TryParse(displayOrder, out i))
                return GetEmployeeIdFromDisplayOrder(i);
            return null;
        }

        private string GetEmployeeIdFromDisplayOrder(int displayOrder)
        {
            foreach (ScheduleDataSet.EmployeeListRow ee in dataSet.EmployeeList)
                if (ee.DisplayOrder == displayOrder)
                    return ee.EmployeeID;
            return null;
        }

    }
}
