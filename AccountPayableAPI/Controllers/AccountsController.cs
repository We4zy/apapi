using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Reflection;
using AccountPayableAPI.Models;
using LinqKit;
using Newtonsoft;

namespace AccountPayableAPI.Controllers
{
    public class AccountsController : ApiController
    { 
        /// <summary>
        /// Simple HTTPGet.  Going to just use the string params in query string for quick implementation.  An Enum would be better
        /// for the sortColumn and sortField, but this is a simple access method for UI dev.  Once all the chart, controls, etc are defined on the UI 
        /// layer, then more specific JSON returns will be formatted and returned.  I figured out after this method how to use the reflection within the
        /// LINQ predicate statements with a .Compile() when we actually execute the statement, so the switch...case on the field name is now un-needed
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortFilter"></param>
        /// <returns></returns>
        [HttpGet]
        public AP_Measure_Final[] GetAPMeasureData(string sortColumn, string sortFilter)
        {
            AccountsPayableDataContext db = new AccountsPayableDataContext();
                     
            if (sortColumn.ToLower() == "all") return db.AP_Measure_Finals.Where(ap => ap.RowId == ap.RowId).ToArray();
            
            PropertyInfo prop = null; AP_Measure_Final[] retData = null;
            AP_Measure_Final instance = new AP_Measure_Final();

            //running through the public properties on the .dbml type so that we can match property name ToLower() for safety         
            foreach (PropertyInfo propInfo in instance.GetType().GetProperties())
                if (propInfo.Name.ToLower() == sortColumn.ToLower()) { prop = propInfo; break; }

            var predicate = PredicateBuilder.New<AP_Measure_Final>(true);//.True<AP_Measure_Final>();

            if (prop != null)
            {
                switch (prop.Name.ToLower())
                {
                    case "rowid":
                        long rowId;
                        long.TryParse(sortFilter, out rowId);
                        predicate = predicate.And(ap => ap.RowId == rowId);
                        break;
                    case "indxctrl":
                        predicate = predicate.And(ap => ap.IndxCtrl == sortFilter);
                        break;
                    case "ap_document":
                        predicate = predicate.And(ap => ap.AP_document == sortFilter);
                        break;
                    case "vendor_number":
                        predicate = predicate.And(ap => ap.vendor_number == sortFilter);
                        break;
                    case "invoice_date":
                        predicate = predicate.And(ap => ap.invoice_date == sortFilter);
                        break;
                    case "received_date":
                        predicate = predicate.And(ap => ap.received_date == sortFilter);
                        break;
                    case "entered_date":
                        predicate = predicate.And(ap => ap.entered_date == sortFilter);
                        break;
                    case "check_date":
                        predicate = predicate.And(ap => ap.check_date == sortFilter);
                        break;
                    case "check_month":
                        int checkMonth;
                        int.TryParse(sortFilter, out checkMonth);
                        predicate = predicate.And(ap => ap.check_month == checkMonth);
                        break;
                    case "invoice_to_received":
                        int invoiceReceived;
                        int.TryParse(sortFilter, out invoiceReceived);
                        predicate = predicate.And(ap => ap.invoice_to_received == invoiceReceived);
                        break;
                    case "received_to_check":
                        int receivedCheck;
                        int.TryParse(sortFilter, out receivedCheck);
                        predicate = predicate.And(ap => ap.received_to_check == receivedCheck);
                        break;
                    case "total_days":
                        int totalDays;
                        int.TryParse(sortFilter, out totalDays);
                        predicate = predicate.And(ap => ap.total_days == totalDays);
                        break;
                    case "payment_name":
                        predicate = predicate.And(ap => ap.payment_name == sortFilter);
                        break;
                    case "total_amount":
                        decimal totalAmount;
                        decimal.TryParse(sortFilter, out totalAmount);
                        predicate = predicate.And(ap => ap.total_amount == totalAmount);
                        break;
                    case "gl_dept":
                        predicate = predicate.And(ap => ap.GL_dept == sortFilter);
                        break;
                    case "requestor":
                        predicate = predicate.And(ap => ap.requestor == sortFilter);
                        break;
                    case "last_access":
                        predicate = predicate.And(ap => ap.last_access == sortFilter);
                        break;
                    case "department":
                        predicate = predicate.And(ap => ap.department == sortFilter);
                        break;
                    case "division":
                        predicate = predicate.And(ap => ap.division == sortFilter);
                        break;
                    case "total_days_group":
                        predicate = predicate.And(ap => ap.total_days_group == sortFilter);
                        break;
                }
            }
            //if (predicate.CanReduce) predicate = (Expression<Func<AP_Measure_Final, bool>>)predicate.Reduce();

            retData = db.AP_Measure_Finals.Where(predicate).ToArray();

            return retData;
        }

        /// <summary>
        ///  We could easily make the return a T[] so that this methdo will also dynamically work with any table/view within the given DBML
        /// </summary>
        /// <param name="sortColumns"></param>
        /// <param name="sortFilters"></param>
        /// <returns></returns>
        [HttpPost]
        public AP_Measure_Final[] GetAPMeasureData([FromBody]SearchCriteria[] sortInfo)
        {
            if (sortInfo == null) return null;

            AccountsPayableDataContext db = new AccountsPayableDataContext();
            AP_Measure_Final[] retData = null;
            AP_Measure_Final instance = new AP_Measure_Final();
            PropertyInfo prop = null;

            ExpressionStarter<AP_Measure_Final> predicate = PredicateBuilder.New<AP_Measure_Final>(true);
            foreach (SearchCriteria sort in sortInfo)
            {
                //Retrieve the PropertyIfo object from the .dbml generated class matching on the incoming SortColumn Name to be safe
                // prop = instance.GetType().GetProperty(sort.SortColumn, BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly );
                foreach (PropertyInfo propInfo in instance.GetType().GetProperties())
                    if (propInfo.Name.ToLower() == sort.SortColumn.ToLower()) { prop = propInfo; break; }

                //make sure the sent in SortColumn is actually a property on the .dbml generated object file for the given table/view
                if (prop != null)
                {
                    //the predicate statement which will be appended to main predicate statement as an AND or OR SQL block
                    Expression<Func<AP_Measure_Final, bool>> innerPredicate = null;

                    switch (sort.CompareOperator.ToLower())
                    {
                        case "=":
                            innerPredicate = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString().Equals(sort.SortValue.ToString());
                            break;
                        case "like":
                            innerPredicate = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString().Contains(sort.SortValue.ToString());
                            break;
                        case "startswith":
                            innerPredicate = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString().StartsWith(sort.SortValue.ToString());
                            break;
                        case "contains":
                            innerPredicate = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString().Contains(sort.SortValue.ToString());
                            break;
                        case ">":
                            switch (Type.GetTypeCode(prop.PropertyType))
                            {
                                case TypeCode.DateTime:
                                    innerPredicate = ap => DateTime.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > DateTime.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Int32:
                                    innerPredicate = ap => Int32.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > Int32.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Int64:
                                    innerPredicate = ap => Int64.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > Int64.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Decimal:
                                    innerPredicate = ap => decimal.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > decimal.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Double:
                                    innerPredicate = ap => double.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > double.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Byte:
                                    innerPredicate = ap => byte.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > byte.Parse(sort.SortValue.ToLower());
                                    break;
                            }
                            break;
                        case "<":
                            switch (Type.GetTypeCode(prop.PropertyType))
                            {
                                case TypeCode.DateTime:
                                    innerPredicate = ap => DateTime.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < DateTime.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Int32:
                                    innerPredicate = ap => Int32.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < Int32.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Int64:
                                    innerPredicate = ap => Int64.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < Int64.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Decimal:
                                    innerPredicate = ap => decimal.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < decimal.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Double:
                                    innerPredicate = ap => double.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < double.Parse(sort.SortValue.ToLower());
                                    break;
                                case TypeCode.Byte:
                                    innerPredicate = ap => byte.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < byte.Parse(sort.SortValue.ToLower());
                                    break;
                            }
                            break;
                    }

                    if (sort.ObjectOperator.ToLower().Equals("and") && innerPredicate != null) predicate = predicate.And(innerPredicate);
                    else if (sort.ObjectOperator.ToLower().Equals("or") && innerPredicate != null) predicate = predicate.Or(innerPredicate);
                }
            }

            retData = db.AP_Measure_Finals.Where(predicate.Compile()).ToArray();
            return retData;
        }
    }
}