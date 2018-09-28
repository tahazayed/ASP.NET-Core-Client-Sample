using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.API.Entities;

namespace Products.API
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddMvcOptions(o => o.OutputFormatters.Add(
                    new XmlDataContractSerializerOutputFormatter()));
            var hostname = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "172.17.85.61";
            var password = Environment.GetEnvironmentVariable("SQLSERVER_SA_PASSWORD") ?? "dodido_2008";
            var database = Environment.GetEnvironmentVariable("SQLSERVER_DATABASE") ?? "ProductsDB";

            var connectionString = $"Server={hostname};Database={database};user id=sa;pwd={password};MultipleActiveResultSets=true;Min Pool Size=100;Max Pool Size=2000;Timeout=90;Connection Lifetime=900;";//configuration["ConnectionStrings:DefaultConnection"];

            services.AddDbContext<ProductsDbContext>(o =>o.UseSqlServer(connectionString),ServiceLifetime.Scoped);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();

            app.UseMvc();
        }
    }
}
