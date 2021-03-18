using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using DatabaseController.Controller;
using DatabaseController.Interface;

namespace HomeTicketing
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string dbConn = Configuration.GetSection("ConnectionString").GetSection("Db").Value;
            string connString = Configuration.GetValue<string>("ConnectionString:Db");
            services.AddScoped<ITicketHandler, TicketHandler>(s => new TicketHandler(connString));
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("2.0", new OpenApiInfo
                {
                    Version = "2.0",
                    Title = "Ticketing API Swagger UI",
                    Description = "This is a place where the API can be try.",
                    Contact = new OpenApiContact
                    {
                        Name = "Attila Molnár",
                        Email = "molnar.attila28@gmail.com"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });    
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("./2.0/swagger.json", "Ticketing API Swagger UI");
                c.RoutePrefix = "swagger";
            });

            loggerFactory.AddFile(Configuration.GetValue<string>("LogPath"));

            app.UseRouting();

            app.UseCors("Open");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
