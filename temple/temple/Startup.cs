using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using temple.Helpers;
using temple.Models;

namespace temple
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
            
            double LoginExpireMinute = Convert.ToDouble(Configuration["LoginExpireMinute"]);
            services.AddControllersWithViews();
          
            //宣告增加驗證方式，使用 cookie 驗證
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie("Cookies", option => {
                //從組態讀取登入逾時設定

                //瀏覽器會限制cookie 只能經由HTTP(S) 協定來存取
                option.Cookie.HttpOnly = true;
                option.LoginPath = new PathString("/User/Login");//登入頁
                option.LogoutPath = new PathString("/User/Logout");//登出Action
                option.Cookie.Name = "temple";
                option.Cookie.SameSite = SameSiteMode.Strict;
                
                ////登入有效時間

            });
            // 將 Session 存在 ASP.NET Core 記憶體中
            services.AddDistributedMemoryCache();
           
            services.AddHttpContextAccessor();

            
            services.AddSession(options =>
            {
                options.Cookie.Name = "mywebsite";
                options.IdleTimeout = TimeSpan.FromMinutes(LoginExpireMinute);
                options.Cookie.HttpOnly = true;
            });
            services.AddAntiforgery(
                opts =>
                {
                    opts.Cookie.Name = "anticsrf";
                    opts.FormFieldName = "anticsrf";
                }) ;
            
            services.AddScoped<ICallApi, CallApi>();
            
            services.AddScoped<ICookieHelper, CookieHelper>();
            services.AddScoped<Ihttpclient, httpclient>();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseStatusCodePagesWithRedirects("/Home/Error"); //或直接給http開頭的絕對URL
            }
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add(
                    "Content-Security-Policy",
                    //"style-src 'self' 'sha256-aqNNdDLnnrDOnTNdkJpYlAxKVJtLt9CtFLklmInuUAE=';" +
                    "img-src 'self';" +
                    "frame-src https://calendar.google.com/;"+
                    "script-src 'self' 'nonce-KUY8VewuvyUYVEIvEFue4vwyiuf';" +
                    "frame-ancestors 'none';"

                );
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-Xss-Protection", "1");
                await next();
            });

            
            // SessionMiddleware 加入 Pipeline
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            //app.UseSession();
            //啟用 cookie 原則功能
            app.UseCookiePolicy();
            //啟用身分識別
            app.UseAuthentication();
            //啟用授權功能
            app.UseAuthorization();
            // 這三個前後次序不能對調喔
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
