using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.Services;
using Kugar.IM.DB.Entities;
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
        /// <summary>
        /// 同步IM服务所需的数据库结构到数据库中
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection SyncIMDbStruct(this IServiceCollection services)
        {
            services.AddHostedService<SyncIMDbStructTask>();

            return services;
        }

        public static IServiceCollection AddIM(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IUserIdProvider, DefaultJWTUserProvider>());

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

    public class SyncIMDbStructTask : BackgroundService
    {
        private IServiceProvider _container;

        public SyncIMDbStructTask(IServiceProvider container)
        {
            _container = container;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var freeSql = (IFreeSql)_container.GetService(typeof(IFreeSql));

            freeSql.CodeFirst.SyncStructure(typeof(im_chat_message),typeof(im_chat_session),typeof(im_chat_userInSession),typeof(im_chat_userMessageStatus),typeof(im_chat_userconnection));

            return Task.CompletedTask;
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
