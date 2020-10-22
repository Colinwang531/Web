using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SmartWeb.DB;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using SmartWeb.Helpers;
using DinkToPdf.Contracts;
using DinkToPdf;
using SmartWeb.Interface;
using NetMQ.Sockets;
using SmartWeb.ProtoBuffer.Init;
using Smartweb.Hubs;

namespace SmartWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddDistributedMemoryCache();
            //获取配置的session丢失时间
            var timeSpan = AppSettingHelper.GetSectionValue("IdleTimeout");
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(Convert.ToDouble(timeSpan));
                options.Cookie.HttpOnly = false;
                options.Cookie.IsEssential = true;
            });
            try
            {
                services.AddDbContext<MyContext>(options => options.UseMySQL(AppSettingHelper.GetConnectionString("dbconn")));

            }
            catch (Exception)
            {
                return;
            }
            services.AddRazorPages(options =>
            {
                //options.Conventions.Add(new DefaultRouteRemovalPageRouteModelConvention(String.Empty));
                options.Conventions.AddPageRoute("/Login", "Index");
            });
            services.AddMvc().AddRazorPagesOptions(o =>
            {
                o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            //注册心跳
            services.AddSingleton<IHostedService, HeartService>();
            //DinkToPdf注入
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddTransient<IPDFService, PDFService>();
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
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AlarmVoiceHub>("/chatHub");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Index}/{id?}");
            });
            app.UseCors();
        }
    }
}
