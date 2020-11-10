using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureAppConfiguration((hostContext, config) =>
               {
                   var env = hostContext.HostingEnvironment;
                   config.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: $"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true);
               })
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                  webBuilder.UseIISIntegration();
                  webBuilder.UseStartup<Startup>();
              });
        // .ConfigureWebHostDefaults(webBuilder =>
        // {

        //     webBuilder.UseKestrel(c => c.AddServerHeader = false)
        //          .UseStartup<Startup>()
        //          .UseNLog();
        // });
    }
}
