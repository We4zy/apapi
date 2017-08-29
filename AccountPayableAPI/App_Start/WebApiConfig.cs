using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net.Http.Headers;

namespace AccountPayableAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //Setting Json as default return format for all WebAPI calls
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));


            config.Routes.MapHttpRoute(
                name: "AccountsApi",
                routeTemplate: "api/{controller}/{action}"
                );

            config.Routes.MapHttpRoute(
                name: "AccountsApiGet",
                routeTemplate: "api/{controller}/{action}/{sortColumn}/{sortFilter}",
                defaults: null
                );
        }
    }
}
