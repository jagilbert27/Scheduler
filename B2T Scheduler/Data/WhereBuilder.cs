using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class WhereBuilder
    {
        private List<string> items = new List<string>();
        public WhereBuilder() { }

        public WhereBuilder Where(string clause)
        {
            items.Add(" "+clause+" ");
            return this;
        }

        public WhereBuilder Where(string fieldAndOperator, string value)
        {
            if (value != null)
                items.Add(" "+fieldAndOperator + " '" + value + "'");
            return this;
        }

        public WhereBuilder Where(string fieldAndOperator, int? value)
        {
            if (value.HasValue)
                items.Add(" "+fieldAndOperator + " " + value);
            return this;
        }

        public WhereBuilder WhereDate(string fieldAndOperator, DateTime? date)
        {
            if (date.HasValue)
                items.Add(fieldAndOperator + Soql.Date(date.Value));
            return this;
        }

        public WhereBuilder WhereDateTime(string fieldAndOperator, DateTime? date)
        {
            if (date.HasValue)
                items.Add(" " + fieldAndOperator + Soql.DateTime(date.Value));
            return this;
        }

        public override string ToString()
        {
            if (items.Count == 0) return " ";
            var result = "";
            foreach (var item in items)
                result += (result == "" ? " WHERE " : " AND ") + item;
            return " "+result + " ";
        }
    }
}

