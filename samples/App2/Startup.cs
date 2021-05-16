namespace App2
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Nacos.AspNetCore.V2;
    using System.Collections.Generic;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddNacosAspNetCore(Configuration);
            // services.AddNacosAspNet(Configuration);
            services.AddNacosAspNet(x =>
            {
                x.ServerAddresses = new List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs";
                x.ServiceName = "App2";
                x.GroupName = "DEFAULT_GROUP";
                x.ClusterName = "DEFAULT";
                x.Ip = "";
                x.PreferredNetworks = "";
                x.Port = 0;
                x.Weight = 100;
                x.RegisterEnabled = true;
                x.InstanceEnabled = true;
                x.Ephemeral = true;
                x.Secure = false;

                // swich to use http or rpc
                x.NamingUseRpc = true;
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
