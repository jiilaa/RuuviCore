using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using BlazorStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using net.jommy.Orleans;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Grains;
using net.jommy.RuuviCore.GrainServices;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime.Configuration;

namespace net.jommy.RuuviCore
{
    public class Program
    {
        private const string Logo = @"
  _____                   _  _____               
 |  __ \                 (_)/ ____|              
 | |__) |   _ _   ___   ___| |     ___  _ __ ___ 
 |  _  / | | | | | \ \ / / | |    / _ \| '__/ _ \
 | | \ \ |_| | |_| |\ V /| | |___| (_) | | |  __/
 |_|  \_\__,_|\__,_| \_/ |_|\_____\___/|_|  \___|
";

        public static async Task Main(string[] args)
        {
            Console.Write(Logo);
            Console.WriteLine($"Version: {Assembly.GetEntryAssembly()?.GetName().Version}");
            
            IHost siloHost;

            if (args != null && args.Length > 0 && args.First() == "--http")
            {
                Console.WriteLine("Starting RuuviCore with HTTP gateway...");
                siloHost = BootstrapSiloWithHttpGateway();
            }
            else
            {
                Console.WriteLine("Starting RuuviCore...");
                siloHost = CreateSilo();
            }

            try
            {
                await siloHost.RunAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("RuuviCore execution interrupted.");
            }
        }

        private static IHost BootstrapSiloWithHttpGateway()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var configurationSection = context.Configuration.GetSection("ListeningSettings");
                        if (UseHttps(configurationSection, out var port, out var certificateFile,
                            out var certificateKey))
                        {
                            options.Listen(IPAddress.Any, port, listenOptions =>
                            {
                                listenOptions.UseHttps(adapterOptions => adapterOptions.ServerCertificateSelector =
                                    (connectionContext, s) => new X509Certificate2(certificateFile, certificateKey));
                            });
                        }
                        else
                        {
                            options.Listen(IPAddress.Any, port);
                        }
                    });

                    webBuilder.UseStartup<Startup>();
                })
                .UseOrleans(siloHostBuilder =>
                {
                    siloHostBuilder
                        .UseLocalhostClustering(serviceId: "RuuviCore", clusterId: "rc")
                        .ConfigureApplicationParts(parts =>
                        {
                            parts.AddApplicationPart(typeof(RuuviTagGrain).Assembly).WithReferences();
                            parts.AddApplicationPart(typeof(InfluxBridge).Assembly).WithReferences();
                        })
                        .Configure<DeploymentLoadPublisherOptions>(options =>
                            options.DeploymentLoadPublisherRefreshTime = TimeSpan.FromHours(1))
                        .Configure<StatisticsOptions>(options =>
                        {
                            options.CollectionLevel = StatisticsLevel.Critical;
                            options.LogWriteInterval = TimeSpan.FromHours(1);
                            options.PerfCountersWriteInterval = TimeSpan.FromDays(1);
                        })
                        .AddGrainService<DBusListener>()
                        .AddFileStorageGrainStorage(RuuviCoreConstants.GrainStorageName,
                            options => options.Directory = "RuuviTags")
                        .AddMemoryGrainStorage("PubSubStore")
                        .AddSimpleMessageStreamProvider("SimpleStreamProvider");
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .AddSerilog()
                        .AddFilter("Orleans", LogLevel.Warning)
                        .AddFilter("Orleans.Runtime.NoOpHostEnvironmentStatistics", LogLevel.Error)
                        .AddFilter("Orleans.Runtime.MembershipService", LogLevel.Error)
                        .AddFilter("Microsoft.Orleans.Messaging", LogLevel.Error)
                        .AddFilter("Microsoft.Orleans.Networking", LogLevel.Error)
                        .AddFilter("Orleans.Runtime.Scheduler.WorkItemGroup", LogLevel.Error)
                        .AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning)
                        .AddFilter("Runtime", LogLevel.Warning);
                })
                .ConfigureServices(collection => collection.AddBootstrapCss())
                .UseConsoleLifetime(options => options.SuppressStatusMessages = true) 
                .Build();
        }

        private static IHost CreateSilo()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile(
                        "appsettings.core.json", optional: true, reloadOnChange: true);
                })
                .UseOrleans(siloHostBuilder =>
                {
                    siloHostBuilder
                        .UseLocalhostClustering(serviceId: "RuuviCore", clusterId: "rc")
                        .ConfigureApplicationParts(parts =>
                        {
                            parts.AddApplicationPart(typeof(RuuviTagGrain).Assembly).WithReferences();
                            parts.AddApplicationPart(typeof(InfluxBridge).Assembly).WithReferences();
                        })
                        .ConfigureServices(services =>
                        {
                            services.Configure<DBusSettings>(configuration.GetSection("DBusSettings"));
                            services.Configure<InfluxSettings>(configuration.GetSection("InfluxSettings"));
                        })
                        .AddGrainService<DBusListener>()
                        .AddFileStorageGrainStorage("RuuviStorage", options => options.Directory = "RuuviTags")
                        .AddMemoryGrainStorage("PubSubStore")
                        .AddSimpleMessageStreamProvider("SimpleStreamProvider");
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .AddSerilog()
                        .AddFilter("Orleans", LogLevel.Warning)
                        .AddFilter("Orleans.Runtime.NoOpHostEnvironmentStatistics", LogLevel.Error)
                        .AddFilter("Orleans.Runtime.MembershipService", LogLevel.Error)
                        .AddFilter("Microsoft.Orleans.Messaging", LogLevel.Error)
                        .AddFilter("Microsoft.Orleans.Networking", LogLevel.Error)
                        .AddFilter("Orleans.Runtime.Scheduler.WorkItemGroup", LogLevel.Error)
                        .AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning)
                        .AddFilter("Runtime", LogLevel.Warning);
                })
                .UseConsoleLifetime(options => options.SuppressStatusMessages = true) 
                .Build();
        }

        private static bool UseHttps(IConfigurationSection configurationSection, out int port,
            out string certificateFile, out string certificateKey)
        {
            port = configurationSection.GetValue<int>("ListeningPort");
            var useHttps = configurationSection.GetValue<bool>("UseHttps");
            if (useHttps)
            {
                certificateFile = configurationSection["CertificateFile"];
                certificateKey = configurationSection["CertificateKey"];
            }
            else
            {
                certificateFile = null;
                certificateKey = null;
            }

            return useHttps;
        }
    }
}