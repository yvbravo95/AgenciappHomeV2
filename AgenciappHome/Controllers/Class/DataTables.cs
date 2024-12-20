using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public class DataTables
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public List<Column> columns { get; set; }
        public Search search { get; set; }
        public List<OrderColumn> order { get; set; }
    }

    public class DataTablesOrder
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public string status { get; set; }
        public List<Column> columns { get; set; }
        public Search search { get; set; }
        public List<OrderColumn> order { get; set; }
    }

    public class OrderColumn
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; }
    }

    public class Search
    {
        public string value { get; set; }
        public string regex { get; set; }
    }
}
