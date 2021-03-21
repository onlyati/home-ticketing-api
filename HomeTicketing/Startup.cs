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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using DatabaseController.DataModel;
using HomeTicketing.Model;
using System.Collections.Generic;

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
            /*-----------------------------------------------------------------------------------*/
            /* Database configuration                                                            */
            /*-----------------------------------------------------------------------------------*/
            string dbConn = Configuration.GetSection("ConnectionString").GetSection("Db").Value;
            string connString = Configuration.GetValue<string>("ConnectionString:Db");
            services.AddScoped<IDbHandler, DbHandler>(s => new DbHandler(connString));
            services.AddControllers();

            /*-----------------------------------------------------------------------------------*/
            /* Indentify configuration                                                           */
            /*-----------------------------------------------------------------------------------*/
            // For Identity  
            services.AddDbContext<TicketDatabase>();
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<TicketDatabase>()
                .AddDefaultTokenProviders();

            // Adding Authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

            /*-----------------------------------------------------------------------------------*/
            /* CORS policy                                                                       */
            /*-----------------------------------------------------------------------------------*/
            services.AddCors(options =>
            {
                options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            /*-----------------------------------------------------------------------------------*/
            /* Swagger config                                                                    */
            /*-----------------------------------------------------------------------------------*/
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
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
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

            // app.UseHttpsRedirection();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    // swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{Configuration["ProxyPrefix"]}" } };
                    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{Configuration["HttpProtocol"]}://{httpReq.Host.Value}{Configuration["ProxyPrefix"]}" } };
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("2.0/swagger.json", "Ticketing API Swagger UI");
                c.RoutePrefix = "swagger";
            });

            loggerFactory.AddFile(Configuration.GetValue<string>("LogPath"));

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors("Open");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
