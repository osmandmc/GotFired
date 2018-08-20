using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using System.Globalization;
using System.Web.Http;
using GotFired.Model.Entities;
using Newtonsoft.Json.Converters;
using GotFired.Api.Handlers;
using System.Net.Http.Formatting;

namespace GotFired.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            // enables authentication using bearer tokens.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            // Web API routes
            config.Routes.MapHttpRoute(
               name: "DefaultApi",
               routeTemplate: "api/v1/{controller}/{id}",
               defaults: new { id = RouteParameter.Optional }
           );

            // just enables support for CORS
            //config.EnableCors();
            //config.Services.Add(typeof(IExceptionLogger), new HrmExceptionLogger());
            //config.Services.Replace(typeof(IExceptionHandler), new HrmExceptionHandler());

            // camelCase
            //var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().FirstOrDefault();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            //json.SerializerSettings.Culture = new CultureInfo("en-EN");
            //json.SerializerSettings.Converters.Add(
            // new MyDateTimeConverter());
            //JsonMediaTypeFormatter jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //JsonSerializerSettings jSettings = new Newtonsoft.Json.JsonSerializerSettings()
            //{
            //    Formatting = Formatting.Indented,
            //    DateTimeZoneHandling = DateTimeZoneHandling.Utc
            //};
            //jSettings.Converters.Add(new MyDateTimeConverter());
            //jsonFormatter.SerializerSettings = jSettings;
        }
    }
}
