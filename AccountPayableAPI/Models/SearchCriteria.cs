using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountPayableAPI.Models
{
    public class SearchCriteria
    {
        public string ObjectOperator { get; set; }
        public string SortColumn { get; set; }
        public string SortValue { get; set; }
        public string CompareOperator { get; set; }
        public string CompareDataType { get; set; }
    }

    public class ComparerOperators
    {
        public static readonly string Equal = "=";
        public static readonly string Like = "%{0}%";
        public static readonly string StartsWith = "{0}%";
        public static readonly string Contains = "%{0}%";
        public static readonly string GreaterThan = ">";
        public static readonly string LessThan = "<";
    }

    public class And
    {
        public string Operator { get { return "and"; }  }
    }

    public class Or
    {
        public string Operator { get { return "or"; } }
    }
    
    //public static class CompareOperators
    //{
    //    public static string Equals { get { return "="; } }
    //}
}