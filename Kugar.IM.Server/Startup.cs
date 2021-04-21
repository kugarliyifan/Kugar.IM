using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FreeSql;
using Kugar.Core.BaseStruct;
using Kugar.Core.Configuration;
using Kugar.Core.Web;
using Kugar.Core.Web.Authentications;
using Kugar.IM.Server.Areas.IM.Controllers;
using Kugar.IM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace Kugar.IM.Server
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
            //services.AddSingleton<IUserIdProvider, SignalrUserProvider>();

            var freesql = new  FreeSqlBuilder().UseAutoSyncStructure(true)
                .UseConnectionString(DataType.SqlServer, CustomConfigManager.Default["ConnStr"])
                .UseExitAutoDisposePool(true)
                .Build();

            

            services.AddSingleton<IFreeSql>(freesql);
            services.Replace(ServiceDescriptor.Singleton<IUserIdProvider, SignalrUserProvider>());
            services.AddCors(options =>
            {
                // CorsPolicy 是自訂的 Policy 名稱
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddScoped<SessionService>();
            services.AddScoped<MessageService>();
            

            //services.AddSingleton<SomeControllerModelConvention>();

            services.AddMemoryCache();

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

        public class SignalrUserProvider:IUserIdProvider
        {
            public string GetUserId(HubConnectionContext connection)
            {
                return connection.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
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
