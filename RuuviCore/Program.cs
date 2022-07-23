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
using Orleans.Providers;
using Orleans.Runtime.Configuration;
using Serilog.Events;

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
            
            var useHttpGateway = args != null && args.Contains("--http");
            var useSimpleStream = args != null && args.Contains("--simplestream");

            Console.WriteLine("Starting RuuviCore...");
            var siloHost = BootstrapSilo(useHttpGateway, useSimpleStream);

            try
            {
                await siloHost.RunAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("RuuviCore execution interrupted.");
            }
        }

        private static IHost BootstrapSilo(bool useHttpGateway, bool useSimpleStream)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false, false).Build();

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddJsonFile("appsettings.json", true, true);
                })
                .UseSerilog((context, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .WriteTo.Console()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Orleans", LogEventLevel.Error)
                        .MinimumLevel.Override("Microsoft.Orleans", LogEventLevel.Error)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Error);
                })
                .UseOrleans(siloHostBuilder => ConfigureOrleans(siloHostBuilder, useSimpleStream, configuration))
                .ConfigureLogging(ConfigureLogging())
                .ConfigureServices(collection => collection.AddBlazorStrap())
                .UseConsoleLifetime(options => options.SuppressStatusMessages = true)
                .ConfigureHostOptions(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));

            if (useHttpGateway)
            {
                hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var configurationSection = context.Configuration.GetSection("ListeningSettings");
                        if (UseHttps(configurationSection, out var port, out var certificateFile, out var certificateKey))
                        {
                            options.Listen(IPAddress.Any, port, listenOptions =>
                            {
                                listenOptions.UseHttps(adapterOptions => adapterOptions.ServerCertificateSelector =
                                    (_, _) => new X509Certificate2(certificateFile, certificateKey));
                            });
                        }
                        else
                        {
                            options.Listen(IPAddress.Any, port);
                        }
                    });

                    webBuilder.UseStartup<Startup>();
                });
            }

            return hostBuilder.Build();
        }

        private static void ConfigureOrleans(ISiloBuilder siloHostBuilder, bool useSimpleStream, IConfiguration configuration)
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
                .Configure<FileStorageProviderOptions>(options => options.Directory = "RuuviTags")
                .ConfigureServices(services =>
                {
                    services.Configure<DBusSettings>(configuration.GetSection("DBusSettings"));
                    services.Configure<InfluxSettings>(configuration.GetSection("InfluxSettings"));
                })
                .AddGrainService<DBusListener>()
                .AddFileStorageGrainStorage(RuuviCoreConstants.GrainStorageName, options => options.Directory = "RuuviTags")
                .AddMemoryGrainStorage("PubSubStore");
            if (useSimpleStream)
            {
                siloHostBuilder.AddSimpleMessageStreamProvider(RuuviCoreConstants.StreamProviderName);
            }
            else
            {
                siloHostBuilder.AddMemoryStreams<DefaultMemoryMessageBodySerializer>(RuuviCoreConstants.StreamProviderName,
                    configurator =>
                    {
                        configurator.ConfigurePartitioning(numOfQueues: 2);
                    });
            }
        }

        private static Action<ILoggingBuilder> ConfigureLogging()
        {
            return builder =>
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
            };
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