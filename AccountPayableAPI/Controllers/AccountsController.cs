﻿using System;
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
        ///  I plan to upgrade this methdo to utilize generics so that it can work with any table or any EF tpye or DBML type
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
            PropertyInfo newProp = null; IQueryable<AP_Measure_Final> query = db.AP_Measure_Finals;

            ExpressionStarter<AP_Measure_Final> andPredicate = PredicateBuilder.New<AP_Measure_Final>(true); bool orPass = false;
            ExpressionStarter<AP_Measure_Final> orPredicate = PredicateBuilder.New<AP_Measure_Final>(false); 
            foreach (SearchCriteria sort in sortInfo)
            {
                //Retrieve the PropertyIfo object from the .dbml generated class matching on the incoming SortColumn Name to be safe
                // prop = instance.GetType().GetProperty(sort.SortColumn, BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly );
                foreach (PropertyInfo propInfo in instance.GetType().GetProperties())
                    if (propInfo.Name.ToLower() == sort.SortColumn.ToLower()) { newProp = propInfo; break; }

                //make sure the sent in SortColumn is actually a property on the .dbml generated object file for the given table/view
                if (newProp != null)
                {
                    //the predicate statement which will be appended to main predicate statement as an AND or OR SQL block
                    Expression<Func<AP_Measure_Final, bool>> expression = null;

                    var prop = newProp;

                    switch (sort.CompareOperator.ToLower())
                    {
                        case "in":
                            expression = ap => sort.SortValue.ToString().Contains(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString());
                            break;
                        case "=":
                            expression = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString() == (sort.SortValue.ToString());
                            break;
                        case "like":
                        case "startswith": 
                        case "contains":
                            expression = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString().Contains(sort.SortValue.ToString());
                            break;
                        case ">":
                            switch (sort.CompareDataType.ToLower())
                            {
                                case "datetime":
                                    expression = ap => DateTime.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > DateTime.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int":
                                    expression = ap => Int32.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > Int32.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int64":
                                    expression = ap => Int64.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > Int64.Parse(sort.SortValue.ToLower());
                                    break;
                                case "decimal":
                                    expression = ap => decimal.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > decimal.Parse(sort.SortValue.ToLower());
                                    break;
                                case "double":
                                    expression = ap => double.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > double.Parse(sort.SortValue.ToLower());
                                    break;
                            }
                            break;
                        case "<":
                            switch (sort.CompareDataType.ToLower())
                            {
                                case "datetime":
                                    expression = ap => DateTime.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < DateTime.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int":
                                    expression = ap => Int32.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < Int32.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int64":
                                    expression = ap => Int64.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < Int64.Parse(sort.SortValue.ToLower());
                                    break;
                                case "decimal":
                                    expression = ap => decimal.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < decimal.Parse(sort.SortValue.ToLower());
                                    break;
                                case "double":
                                    expression = ap => double.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < double.Parse(sort.SortValue.ToLower());
                                    break;
                            }
                            break;
                    }

                    if (sort.ObjectOperator.ToLower().Equals("and") && expression != null)
                        andPredicate = andPredicate.And(expression);
                    else if (sort.ObjectOperator.ToLower().Equals("or") && expression != null)
                    {
                        orPredicate = orPredicate.Or(expression);
                        orPass = true;
                    }
                }
            }
            //Depending upon how inner or statements can be formatted, we may need to determine if this should be an and or an Or.  Ir it's just a single Or clause then OR, if it's appending a few OR clauses then
            // this might need to be constructed as an AND
            if (orPass) andPredicate = andPredicate.Or(orPredicate);

            retData = db.AP_Measure_Finals.AsExpandable().Where(andPredicate.Compile()).ToArray();
            return retData;
        }


       

        //public PagedViewModel<T> Filter<TValue>(Expression<Func<T, TValue>> predicate, FilterType filterType = FilterType.Equals)
        //{
        //    var name = (predicate.Body as MemberExpression ?? ((UnaryExpression)predicate.Body).Operand as MemberExpression).Member.Name;
        //    var value = Expression.Constant(ParamsData[name].To<TValue>(), typeof(T).GetProperty(name).PropertyType);

        //    // If nothing has been set for filter, skip and don't filter data.
        //    ViewData[name] = m_QueryInternal.Distinct(predicate.Compile()).ToSelectList(name, name, ParamsData[name]);
        //    if (string.IsNullOrWhiteSpace(ParamsData[name]))
        //        return this;

        //    var nameExpression = Expression.Parameter(typeof(T), name);
        //    var propertyExpression = Expression.Property(nameExpression, typeof(T).GetProperty(name));

        //    // Create expression body based on type of filter
        //    Expression expression;
        //    MethodInfo method;
        //    switch (filterType)
        //    {
        //        case FilterType.Like:
        //            method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        //            expression = Expression.Call(propertyExpression, method, value);
        //            break;
        //        case FilterType.EndsWith:
        //        case FilterType.StartsWith:
        //            method = typeof(string).GetMethod(filterType.ToString(), new[] { typeof(string) });
        //            expression = Expression.Call(propertyExpression, method, value);
        //            break;
        //        case FilterType.GreaterThan:
        //            expression = Expression.GreaterThan(propertyExpression, value);
        //            break;
        //        case FilterType.Equals:
        //            expression = Expression.Equal(propertyExpression, value);
        //            break;
        //        default:
        //            throw new ArgumentException("Filter Type could not be determined");
        //    }

        //    // Execute the expression against Query.
        //    var methodCallExpression = Expression.Call(
        //        typeof(Queryable),
        //        "Where",
        //        new[] { Query.ElementType },
        //        Query.Expression,
        //        Expression.Lambda<Func<T, bool>>(expression, new[] { nameExpression }));

        //    // Filter the current Query data.
        //    Query = Query.Provider.CreateQuery<T>(methodCallExpression);

        //    return this;
        //}





        //var paramsData = new NameValueCollection { { "CreatedOn", DateTime.Today.ToString() } };
        //var model = m_data.ToPagedList(new ViewDataDictionary(), paramsData, 1, 10, null, x => x.LastName)
        //                  .Filters(Criteria<TrainerProfile>.New(x => x.CreatedOn, FilterType.GreaterThan))
        //                  .Setup();  


    }


    /// <summary>
    /// So many times Database developers break the rules and hold int values in varchar[], or DateTime in varchar[].  If DB designers actually used the correct data types
    /// then we would not need the CompareDataType property within our SearchCriteria object.  we could just GetType() of the target property or field and then do the
    /// specified compare operator with that given type.  But if one needs to do a ">" greaterThan on 2 DateTime values, but in the Database they are stored in an NVarChar[] field, then
    /// that blows that out of the water.  that's why to be safe I added the CompareDataType so that we cold specidy the compare type that we really want, not just rely upon the Entity Framework object
    /// or DBML property type.  That rule is broken so often that it's safer to explicitly state the compare data type.
    /// </summary>
    /// <typeparam name="TDbType"></typeparam>
    public class DynamicQuery<TDbType>
    {
        private TDbType localType;

        public DynamicQuery(TDbType type) { localType = type; }

        /// <summary>
        //// For all CompareOperators other than "<" and ">", use "string"
        /// </summary>
        ////<param name="searchCriteria">
        /// SearchCriteria Params and Descriptions
        ///         ObjectOperator          =   A basic operator for constructing the underlying TSQL statement (or Linq statment)  can be "and" or "or"
        ///         SortColumn              =   The field or column name in the Database or Entity Framework object property or DBML property that you want to search or sort on 
        ////        CompareOperator         =   The standard compare operator to use.  Values can be:  "=", "in", "like", "StartWith", "Contains", ">", "<" 
        ///         SortValue               =   The actualy value that you want to compare the Database column against.  Can be any valid value that could be use in a TSQL statement
        ///         CompareDataType:        =   Because Database designers ALWAYS break the rules and hold improper datatypes in incorrect coumn datatypes.  IE.  Like storing a DateTime in an NVarChar(12)
        ///                                     Since those rules are alwaysw broken, we need to explicitly state what type of comparison data type we want to use.  Can be: "int", "int64", "decimal", "DateTime", "double"
        ////                                    These different values are really only needed for ">" and "<" then comparisons so that the correct comparison will be calculated.  All the other CompareOperators just use type "string"
        /// </param>
        /// <returns> Whichever EntityFramework or DBML type you are querying against </returns>
        public TDbType[] Query(SearchCriteria[] searchCriteria)
        {
            if (searchCriteria == null) return null;

            AccountsPayableDataContext db = new AccountsPayableDataContext();
            TDbType[] retData = null; TDbType instance = localType; PropertyInfo newProp = null;
            IQueryable<TDbType> query = (IQueryable<TDbType>)db.GetType().GetProperty(instance.GetType().Name).GetValue(db, null);
            ExpressionStarter<TDbType> andPredicate = PredicateBuilder.New<TDbType>(true); bool orPass = false;
            ExpressionStarter<TDbType> orPredicate = PredicateBuilder.New<TDbType>(false);

            foreach (SearchCriteria sort in searchCriteria)
            {
                //Retrieve the PropertyIfo object from the .dbml generated class matching on the incoming SortColumn Name to be safe
                foreach (PropertyInfo propInfo in instance.GetType().GetProperties())
                    if (propInfo.Name.ToLower() == sort.SortColumn.ToLower()) { newProp = propInfo; break; }

                //make sure the sent in SortColumn is actually a property on the .dbml generated object file for the given table/view
                if (newProp != null)
                {
                    //the predicate statement which will be appended to main predicate statement as an AND or OR SQL block
                    Expression<Func<TDbType, bool>> expression = null;
                    var prop = newProp;

                    switch (sort.CompareOperator.ToLower())
                    {
                        case "in":
                            expression = ap => sort.SortValue.ToString().Contains(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString());
                            break;
                        case "=":
                            expression = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString() == (sort.SortValue.ToString());
                            break;
                        case "like":
                        case "startswith":
                        case "contains":
                            expression = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString().Contains(sort.SortValue.ToString());
                            break;
                        case ">":
                            switch (sort.CompareDataType.ToLower())
                            {
                                case "datetime":
                                    expression = ap => DateTime.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > DateTime.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int":
                                    expression = ap => Int32.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > Int32.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int64":
                                    expression = ap => Int64.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > Int64.Parse(sort.SortValue.ToLower());
                                    break;
                                case "decimal":
                                    expression = ap => decimal.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > decimal.Parse(sort.SortValue.ToLower());
                                    break;
                                case "double":
                                    expression = ap => double.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > double.Parse(sort.SortValue.ToLower());
                                    break;
                            }
                            break;
                        case "<":
                            switch (sort.CompareDataType.ToLower())
                            {
                                case "datetime":
                                    expression = ap => DateTime.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < DateTime.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int":
                                    expression = ap => Int32.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < Int32.Parse(sort.SortValue.ToLower());
                                    break;
                                case "int64":
                                    expression = ap => Int64.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < Int64.Parse(sort.SortValue.ToLower());
                                    break;
                                case "decimal":
                                    expression = ap => decimal.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < decimal.Parse(sort.SortValue.ToLower());
                                    break;
                                case "double":
                                    expression = ap => double.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < double.Parse(sort.SortValue.ToLower());
                                    break;
                            }
                            break;
                    }

                    if (sort.ObjectOperator.ToLower().Equals("and") && expression != null)
                        andPredicate = andPredicate.And(expression);
                    else if (sort.ObjectOperator.ToLower().Equals("or") && expression != null)
                    {
                        orPredicate = orPredicate.Or(expression);
                        orPass = true;
                    }
                }
            }

            if (orPass) andPredicate = andPredicate.Or(orPredicate);

            retData = query.AsExpandable().Where(andPredicate.Compile()).ToArray();
            return retData;
        }

    }
}