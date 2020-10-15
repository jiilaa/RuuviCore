using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace RuuviRestAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile(
                        "appsettings.gateway.json", optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var configurationSection = context.Configuration.GetSection("ListeningSettings");
                        if (UseHttps(configurationSection, out var port, out var certificateFile, out var certificateKey))
                        {
                            options.Listen(IPAddress.Any, port, listenOptions => { listenOptions.UseHttps(adapterOptions => adapterOptions.ServerCertificateSelector =
                                (connectionContext, s) => new X509Certificate2(certificateFile, certificateKey)); });
                        }
                        else
                        {
                            options.Listen(IPAddress.Any, port);
                        }
                    });
                    
                    webBuilder.UseStartup<Startup>();
                });

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