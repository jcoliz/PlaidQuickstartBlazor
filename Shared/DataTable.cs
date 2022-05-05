using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaidQuickstartBlazor.Shared
{
    public class DataTable
    {
        public Column[]? Columns { get; set; }

        public Row[]? Rows { get; set; }
    };

    public class Column
    {
        public string? Title { get; set; }

        public bool IsRight { get; set; }
    }

    public class Row
    {
        public string[]? Cells { get; set; }
    }
}
