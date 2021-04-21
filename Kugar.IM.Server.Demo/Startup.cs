using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql;
using Kugar.Core.BaseStruct;
using Kugar.Core.Configuration;
using Kugar.Core.Web;
using Kugar.Core.Web.Authentications;
using Kugar.IM.Services;
using Kugar.IM.Web.Cache;
using Kugar.IM.Web.Helpers;
using Kugar.IM.Web.Hub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace Kugar.IM.Server.Demo
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
            var freesql = new  FreeSqlBuilder().UseAutoSyncStructure(true)
                .UseConnectionString(DataType.SqlServer, CustomConfigManager.Default["ConnStr"])
                .UseExitAutoDisposePool(true)
                .Build();

            

            services.AddSingleton<IFreeSql>(freesql);

            services.AddCors(options =>
            {
                // CorsPolicy ����ӆ�� Policy ���Q
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
             
            services.AddControllers(opt =>
                {
                })
                .AddNewtonsoftJson((options) =>
                {
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    (options.SerializerSettings.ContractResolver as DefaultContractResolver).NamingStrategy = new CamelCaseNamingStrategy(true,true);
                }).EnableJsonValueModelBinder() ; 


            services.AddAuthentication()
                .AddWebJWT("im", new WebJWTOption()
                {
                    Audience = "sdfs",
                    Issuer = "sss",
                    LoginService = new LoginService(),
                    //TokenFactory = (context)=>context.HttpContext.Request.Query["access_token"]
                });

            services.AddIM();

            services.AddSignalR(x =>
            {
                x.EnableDetailedErrors = true;
                x.KeepAliveInterval=TimeSpan.FromMinutes(2);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("im/chatHub" );

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area}/{controller=Home}/{action=Index}");

                
            });
        }

        public class LoginService : IWebJWTLoginService
        {
            public async Task<ResultReturn<string>> Login(HttpContext context, string userName, string password, bool isNeedEncoding = false)
            {
                return new SuccessResultReturn<string>(userName);
            }
        }
    }
}
