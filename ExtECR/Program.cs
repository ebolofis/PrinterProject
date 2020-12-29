using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace ExtECR
{
    public class Program
    {
        public static string CurrentPath { get; set; }
        public static string AppName { get; set; }
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            CurrentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Directory.SetCurrentDirectory(CurrentPath);

            AppName = Assembly.GetEntryAssembly().GetName().Name;

            var pathComponents = new List<string>() { CurrentPath, "Config", "NLog.config" };
            var logPath = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();

            try
            {
                ConfigurationBuilder();
                var webHostArgs = args.Where(arg => arg != "--console").ToArray();
                var webBuilder = CreateWebHostBuilder(webHostArgs).Build();

                IWebHostEnvironment webHostEnv = (IWebHostEnvironment)webBuilder.Services.GetService(typeof(IWebHostEnvironment));
                StartLogging(logger, webHostEnv);

                var isService = !(Debugger.IsAttached || args.Contains("--console"));
                if (isService)
                {
                    webBuilder.RunAsService();
                }
                else
                {
                    webBuilder.Run();
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception.ToString());
                throw;
            }
            finally
            {
                logger.Warn($" ======== {AppName} Stopping ======== ");
                logger.Warn("");
                LogManager.Shutdown();
            }
        }

        private static void ConfigurationBuilder()
        {
            var pathComponents = new List<string>() { CurrentPath, "Config", "appsettings.json" };
            var configPath = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            var builder = new ConfigurationBuilder()
                     .SetBasePath(CurrentPath)
                     .AddJsonFile(configPath, optional: false, reloadOnChange: true);
            Configuration = builder.Build();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseConfiguration(Configuration)
            .UseNLog();

        private static void StartLogging(Logger logger, IWebHostEnvironment webHostEnv)
        {
            logger.Info("");
            logger.Info("");
            logger.Info("*****************************************");
            logger.Info("*                                       *");
            logger.Info($"*  {AppName}  Started                   ");
            logger.Info("*                                       *");
            logger.Info("*****************************************");
            logger.Info("");
            Debug.WriteLine("Application Started");
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            logger.Info("Version: " + versionInfo.FileVersion);
            logger.Info("Urls: " + Configuration["URLS"]);
            logger.Info("Environment: " + webHostEnv.EnvironmentName);
            logger.Info("Current Path: " + CurrentPath);
            logger.Info("");
        }
    }
}