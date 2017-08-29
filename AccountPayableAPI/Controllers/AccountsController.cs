using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Reflection;
using AccountPayableAPI.Models;

namespace AccountPayableAPI.Controllers
{
    public class AccountsController : ApiController
    { 
        /// <summary>
        /// Simple HTTPGet.  Going to just use the string params in query string for quick implementation.  An Enum would be better
        /// for the sortColumn and sortField, but this is a simple access method for UI dev.  Once all the chart, controls, etc are defined on the UI 
        /// layer, then more specific JSON returns will be formatted and returned
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

            var predicate = PredicateBuilder.True<AP_Measure_Final>();

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
            if (predicate.CanReduce) predicate = (Expression<Func<AP_Measure_Final, bool>>)predicate.Reduce();

            retData = db.AP_Measure_Finals.Where(predicate).ToArray();

            return retData;
        }
    }
}