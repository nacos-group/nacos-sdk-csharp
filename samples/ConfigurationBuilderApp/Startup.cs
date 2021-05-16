using ConfigurationBuilderApp;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Startup))]

namespace ConfigurationBuilderApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.StatusCode = 404;

                var key = context.Request.Query["key"];
                if (string.IsNullOrWhiteSpace(key)) return Task.CompletedTask;

                var value = ConfigurationManager.AppSettings[key];
                if (value != null) context.Response.StatusCode = 200;

                context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";

                return context.Response.WriteAsync(value ?? "undefined");
            });
        }
    }
}
