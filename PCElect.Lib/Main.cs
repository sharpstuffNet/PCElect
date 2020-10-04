using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetEscapades.Extensions.Logging.RollingFile;
using System;
using System.Reflection;
using System.Threading;
using IO = System.IO;

namespace PCElect.Lib
{
    public static class MainHelper
    {
        public static ILogger Logger { get; private set; }
        public static IConfigurationRoot Config { get; private set; }

        public static bool Init(string[] args)
        {
            var exeName = Assembly.GetEntryAssembly().GetName();
            Thread.CurrentThread.Name = $"{exeName.Name}.Main";
            IConfigurationRoot config;

            //Version Flag
            if (args.Length > 0 && args[0].StartsWith("--v"))
            {
                System.Console.WriteLine(exeName.Version.ToString());
                System.Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
                return false;
            }

            //Config File
            if (args.Length > 1 && args[0].StartsWith("--c"))
            {
                config = new ConfigurationBuilder()
                  .AddJsonFile(args[1], optional: true)
                  .AddCommandLine(args)
                  .Build();

                Console.WriteLine($"Using Config File {args[1]}");
            }
            else
            {
                config = new ConfigurationBuilder()
                .SetBasePath(IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddCommandLine(args)
                .Build();

                Console.WriteLine($"{args.Length} -> Using appsettings.json in {IO.Directory.GetCurrentDirectory()}");
            }

            // Logger
            var lgr = LoggerFactory.Create(builder =>
            {
                builder.AddConfiguration(config.GetSection("Logging"));

                if (config.GetValue<bool>("Logging:Console"))
                    builder.AddConsole();

                if (!string.IsNullOrEmpty(config.GetValue<string>("Logging:FileDir", null)))
                {
                    var logd = string.Format(config.GetValue<string>("Logging:FileDir"), DateTime.UtcNow);
                    if (!IO.Directory.Exists(logd))
                        IO.Directory.CreateDirectory(logd);

                    builder.AddFile(options =>
                    {
                        options.FileName = $"{exeName.Name}_"; // The log file prefixes
                        options.LogDirectory = logd; // The directory to write the logs
                        options.FileSizeLimit = 1024 * 1024 * 1024; // The maximum log file size (20MB here)
                        options.Extension = "log"; // The log file extension
                        options.Periodicity = PeriodicityOptions.Hourly; // Roll log files hourly instead of daily.
                        options.RetainedFileCountLimit = 24 * 7;
                    });
                }
            });

            Logger = lgr.CreateLogger(exeName.Name);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            return true;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.LogWarning($"ProcessExit");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.LogError($"UnhandledException {ex?.Message}{Environment.NewLine}{ex?.StackTrace}", ex);
        }
    }
}
