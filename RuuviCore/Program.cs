using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using net.jommy.Orleans;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.GrainServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using BlazorStrap;
using Serilog;
using Serilog.Events;

namespace net.jommy.RuuviCore;

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

        Console.WriteLine("Starting RuuviCore...");
        var siloHost = BootstrapSilo(useHttpGateway);

        try
        {
            await siloHost.RunAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("RuuviCore execution interrupted.");
        }
    }

    private static IHost BootstrapSilo(bool useHttpGateway)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", false, false)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();

        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                };
                services.AddSingleton(jsonSerializerOptions);
            })
            .ConfigureAppConfiguration(builder => builder.AddConfiguration(configuration))
            .UseSerilog((_, loggerConfiguration) =>
            {
                loggerConfiguration
                    .WriteTo.Console()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Orleans", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.Orleans", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Error);
            })
            .UseOrleans(ConfigureOrleans)
            .ConfigureLogging(ConfigureLogging())
            .ConfigureServices(collection =>
            {
                collection.AddBlazorStrap();
                collection.AddSingleton<IInfluxSettingsFactory, InfluxSettingsFactory>();
                collection.Configure<InfluxBridgeList>(configuration.GetSection("InfluxSettings"));
            })
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
                                (_, _) => X509CertificateLoader.LoadPkcs12FromFile(
                                    certificateFile,
                                    certificateKey));
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

    private static void ConfigureOrleans(ISiloBuilder siloHostBuilder)
    {
        siloHostBuilder.Configure<FileGrainStorageOptions>(options => options.RootDirectory = "RuuviTags");
        siloHostBuilder
            .UseLocalhostClustering(serviceId: "RuuviCore", clusterId: "rc")
            .AddGrainService<DBusListener>()
            .AddJsonFileGrainStorage(RuuviCoreConstants.GrainStorageName,
                options => { options.RootDirectory = "RuuviTags"; });
    }

    private static Action<ILoggingBuilder> ConfigureLogging()
    {
        return builder =>
        {
            builder
                .AddSerilog()
                .AddFilter("Orleans", LogLevel.Warning);
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
