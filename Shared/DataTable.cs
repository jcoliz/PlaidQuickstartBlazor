using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaidQuickstartBlazor.Shared
{
    public class DataTable
    {
        public Column[] Columns { get; set; } = new Column[0];

        public Row[] Rows { get; set; } = new Row[0];

        public DataTable() { }

        public DataTable(params string[] cols)
        {
            Columns = cols.Select(x => 
            {
                var split = x.Split("/");
                return new Column() { Title = split[0], IsRight = split.Length > 1 && split[1] == "r" };
            }).ToArray();
        }
    };

    public class Column
    {
        public string Title { get; set; } = String.Empty;

        public bool IsRight { get; set; }
    }

    public class Row
    {
        public string[] Cells { get; set; } = new string[0];

        public Row() { }

        public Row(params string[] cells) 
        { 
            Cells = cells;
        }
    }
}
