using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AccountPayableAPI.Models;
using LinqKit;

namespace AccountPayableAPI.Controllers
{
    public class PropertiesController : ApiController
    {
        [HttpPost]
        public Abandoned_Property_Combined_Source[] GetAbandonedProperties([FromBody] SearchCriteria[] sortCriteria)
        {
            var ret = new DynamicQuery<Abandoned_Property_Combined_Source>(new Abandoned_Property_Combined_Source()).Query(sortCriteria);
            return ret;
        }
    }
}