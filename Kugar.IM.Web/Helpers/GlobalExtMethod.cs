using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.Services;
using Kugar.IM.Services;
using Kugar.IM.Web.Cache;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Kugar.IM.Web.Helpers
{
    public static  class IMGlobalExtMethod
    {
        public static IServiceCollection AddIM(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IUserIdProvider, DefaultJWTUserProvider>());

            services.AddCors(options =>
            {
                // CorsPolicy 是自訂的 Policy 名稱
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddScoped<ISessionService,SessionService>();
            services.AddScoped<IMessageService,MessageService>();

            services.AddSignalR(x =>
            {
                x.EnableDetailedErrors = true;
                x.KeepAliveInterval=TimeSpan.FromMinutes(2);
            });

            services.AddMemoryCache();

            services.AddSingleton<IIMConnectionManager, DefaultMemoryUserConnectManager>();

            services.AddHostedService<InitUserConnectionManagerTask>();

            return services;
        }
        
    }

    public class InitUserConnectionManagerTask : BackgroundService
    {
        private IServiceProvider _container;

        public InitUserConnectionManagerTask(IServiceProvider container)
        {
            _container = container;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var service = (IIMConnectionManager) _container.GetService(typeof(IIMConnectionManager));

            await service.Init();
        }
    }

}
