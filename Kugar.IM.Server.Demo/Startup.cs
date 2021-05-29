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
using Kugar.Core.Web.ActionResult;
using Kugar.Core.Web.Authentications;
using Kugar.Core.Web.JsonTemplate;
using Kugar.IM.Services;
using Kugar.IM.Web.Cache;
using Kugar.IM.Web.Helpers;
using Kugar.IM.Web.Hub;
using Kugar.Storage;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;
using NSwag;
using NSwag.Generation.Processors.Security;
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

            //services.AddCors(options =>
            //{
            //    // CorsPolicy 是自訂的 Policy 名稱
            //    options.AddPolicy("CorsPolicy", policy =>
            //    {
            //        policy.WithOrigins("http://localhost:5000")
            //            .AllowAnyHeader()
            //            .AllowAnyMethod()
            //            .AllowCredentials();
            //    });
            //});

            //services.AddScoped<IMessageService, MessageService>();
            //services.AddScoped<ISessionService, SessionService>();

            services.AddSingleton<IStorage>(x=>
            {
                var env = (IHostEnvironment) x.GetService(typeof(IHostEnvironment));
                return new LocalStorage(env.ContentRootPath);
            });

            services.AddSwaggerDocument(opt =>
            {
                opt.ApiGroupNames = new[] { "im" };
                opt.DocumentName = "api";
                opt.Title = "Api接口";
                opt.UseJsonTemplate(typeof(Startup).Assembly);

                opt.DocumentProcessors.Add(new SecurityDefinitionAppender("Authorization", new OpenApiSecurityScheme()
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "授权token",

                }));
            });

            services.AddCors(x =>
            {
                x.AddDefaultPolicy(y =>
                    y.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true));
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddControllers()
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

            //services.AddSignalR(x =>
            //{
            //    x.EnableDetailedErrors = true;
            //    x.KeepAliveInterval=TimeSpan.FromMinutes(2);
            //});
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
            app.AddPhysicalStaticFiles("/uploads", "/uploads", maxAge: new TimeSpan(5, 0, 0, 0));

            app.UseJsonTemplate();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("im/chatHub" );

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area}/{controller=Home}/{action=Index}");

                
            });

            app.UseOpenApi();       // serve OpenAPI/Swagger documents

            app.UseSwaggerUi3();    // serve Swagger UI

            app.UseSwaggerUi3(config =>  // serve ReDoc UI
            {
                // 這裡的 Path 用來設定 ReDoc UI 的路由 (網址路徑) (一定要以 / 斜線開頭)
                config.Path = "/swager";
                config.WithCredentials = true;
            });

            //app.UseOpenApi();       // serve OpenAPI/Swagger documents

            //app.UseSwaggerUi3();    // serve Swagger UI

            //app.UseSwaggerUi3(config =>  // serve ReDoc UI
            //{
            //    // 這裡的 Path 用來設定 ReDoc UI 的路由 (網址路徑) (一定要以 / 斜線開頭)
            //    config.Path = "/swager";
            //    config.WithCredentials = true;
            //});
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
