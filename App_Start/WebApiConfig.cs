using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http;
using System.Web.Http.Cors;

namespace ShopThoiTrang
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Bật CORS để app Android có thể gọi API
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // Attribute routing (nếu cần)
            config.MapHttpAttributeRoutes();

            // Conventional routing
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Đảm bảo trả về JSON (không XML)
            config.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }
    }
}