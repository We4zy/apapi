using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountPayableAPI.Models
{
    public class SortCriteria
    {
        public string SortColumn { get; set; }
        public string SortFilter { get; set; }      
        public ComparerOperators CompareOperator { get; set; }
    }

    public enum ComparerOperators
    {
        Equals = 0,
        Like = 1,
        StartsWith = 2,
        Contains = 3,
        GreaterThan = 4,
        LessThan = 5
    }
}