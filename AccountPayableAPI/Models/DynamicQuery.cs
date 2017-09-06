using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqKit;

namespace AccountPayableAPI.Models
{
    /// <summary>
    /// So many times Database developers break the rules and hold int values in varchar[], or DateTime in varchar[].  If DB designers actually used the correct data types
    /// then we would not need the CompareDataType property within our SearchCriteria object.  we could just GetType() of the target property or field and then do the
    /// specified compare operator with that given type.  But if one needs to do a ">" greaterThan on 2 DateTime values, but in the Database they are stored in an NVarChar[] field, then
    /// that blows that out of the water.  that's why to be safe I added the CompareDataType so that we cold specidy the compare type that we really want, not just rely upon the Entity Framework object
    /// or DBML property type.  That rule is broken so often that it's safer to explicitly state the compare data type.
    /// </summary>
    /// <typeparam name="TDbType"></typeparam>
    public sealed class DynamicQuery<TDbType>
    {
        private TDbType localType;

        //We need just 1 instantiated object of the target type to start all the work with.  Just an empty type of the EntityFramework or DBML target query type
        public DynamicQuery(TDbType type) { localType = type; }

        /// <summary>
        //// For all CompareOperators other than "<" and ">", use "string"
        /// </summary>
        ////<param name="searchCriteria">
        /// SearchCriteria Params and Descriptions
        ///         ObjectOperator          =   A basic operator for constructing the underlying TSQL statement (or Linq predicate/expression)  can be "and" or "or"
        ///         SortColumn              =   The field or column name in the Database or Entity Framework object property or DBML property that you want to search or sort on 
        ////        CompareOperator         =   The standard compare operator to use.  Values can be:  "=", "in", "like", "StartWith", "Contains", ">", "<" 
        ///         SortValue               =   The actualy value that you want to compare the Database column against.  Can be any valid value that could be use in a TSQL statement
        ///         CompareDataType:        =   Because Database designers ALWAYS break the rules and hold improper datatypes in incorrect coumn datatypes.  IE.  Like storing a DateTime in an NVarChar(12)
        ///                                     Since those rules are alwaysw broken, we need to explicitly state what type of comparison data type we want to use.  Can be: "int", "int64", "decimal", "DateTime", "double", "long"
        ////                                    These different values are really only needed for ">" and "<" then comparisons so that the correct comparison will be calculated.  All the other CompareOperators need not specify CompareDataType"
        /// </param>
        /// <returns> Whichever EntityFramework or DBML type you are querying against </returns>
        public TDbType[] Query(SearchCriteria[] searchCriteria)
        {
            if (searchCriteria == null) return null;

            AccountsPayableDataContext db = new AccountsPayableDataContext();
            
            IQueryable<TDbType> query = (IQueryable<TDbType>)db.GetTable(typeof(TDbType));
            ExpressionStarter<TDbType> andPredicate = PredicateBuilder.New<TDbType>(true); 
            ExpressionStarter<TDbType> orPredicate = PredicateBuilder.New<TDbType>(false);
            TDbType instance = localType; bool orPass = false;

            foreach (SearchCriteria sort in searchCriteria)
            {
                PropertyInfo prop = null;
                //Retrieve the PropertyIfo object from the .dbml generated class matching on the incoming SortColumn Name to be safe
                foreach (PropertyInfo propInfo in instance.GetType().GetProperties())
                    if (propInfo.Name.ToLower() == sort.SortColumn.ToLower()) { prop = propInfo; break; }

                //make sure the sent in SortColumn is actually a property on the .dbml generated object file for the given table/view
                if (prop != null)
                {
                    //the predicate statement which will be appended to main predicate statement as an AND or OR SQL block
                    Expression<Func<TDbType, bool>> expression = null;

                    switch (sort.CompareOperator.ToLower())
                    {
                        case "in":
                            expression = ap => sort.SortValue.ToString().Contains(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString());
                            break;
                        case "=":
                            expression = ap => ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString() == (sort.SortValue);
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
                                case "long":
                                    expression = ap => long.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) > long.Parse(sort.SortValue.ToLower());
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
                                case "long":
                                    expression = ap => long.Parse(ap.GetType().GetProperty(prop.Name).GetValue(ap).ToString()) < long.Parse(sort.SortValue.ToLower());
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

            return query.AsExpandable().Where(andPredicate.Compile()).ToArray();
        }

    }
}