using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaidQuickstartBlazor.Shared
{
    public class DataTable
    {
        public Column[] Columns { get; set; } = Array.Empty<Column>();

        public Row[] Rows { get; set; } = Array.Empty<Row>();

        public DataTable() { }
    };

    public class Column
    {
        public string Title { get; set; } = String.Empty;

        public bool IsRight { get; set; }
    }

    public class Row
    {
        public string[] Cells { get; set; } = Array.Empty<string>();

        public Row() { }

        public Row(params string[] cells) 
        { 
            Cells = cells;
        }
    }
}
