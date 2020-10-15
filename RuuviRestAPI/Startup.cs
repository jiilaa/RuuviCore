using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using RuuviRestAPI.Utilities;

namespace RuuviRestAPI
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
            var orleansClient = CreateOrleansClient();
            services.AddSingleton(orleansClient);
            services.Configure<RestApiOptions>(Configuration.GetSection("APISettings"));
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    // Ruuvi app (at least android version) submits the datetime in a non ISO 8601-1:2019 format,
                    // i.e. the timezone is in format +0200 instead of +02:00
                    // So add a fallback to deserialize datetime strings using DateTime 
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParseAsFallback());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private IClusterClient CreateOrleansClient()
        {
            var clientBuilder = new ClientBuilder()
                .UseLocalhostClustering(serviceId: "RuuviCore", clusterId: "rc")
                .ConfigureLogging(logging => logging.AddConsole());

            var client = clientBuilder.Build();
            client
                .Connect()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            return client;
        }
    }
}