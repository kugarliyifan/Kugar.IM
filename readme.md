本类库提供一个通用的IM聊天功能


Web项目中注册:

public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication()
                .AddWebJWT("im", new WebJWTOption()
                {
                    Audience = "sdfs",
                    Issuer = "sss",
                    LoginService = new LoginService(),
                    //TokenFactory = (context)=>context.HttpContext.Request.Query["access_token"]
                });  //注册授信息

    services.AddMemoryCache();  //注册cache方式


	services.AddIM();
}

public void Configure(IApplicationBuilder app)
{

    app.UseAuthentication();
    app.UseAuthorization();


	app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("im/chatHub" );  //注册Hub的url

            endpoints.MapControllerRoute(
                name: "areas",
                pattern: "{area}/{controller=Home}/{action=Index}");

                
        });
}