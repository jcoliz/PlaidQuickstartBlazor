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
    };

    public class Column
    {
        public string Title { get; set; } = String.Empty;

        public bool IsRight { get; set; }
    }

    public class Row
    {
        public string[] Cells { get; set; } = new string[0];
    }
}
