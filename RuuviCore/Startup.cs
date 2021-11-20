using Append.Blazor.Notifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Gateway.Utilities;
using net.jommy.RuuviCore.Services;

namespace net.jommy.RuuviCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddNotifications();
            services.AddSingleton<IRuuviTagRepository, RuuviTagRepository>();
            services.Configure<RestApiOptions>(Configuration.GetSection("APISettings"));
            services.Configure<DBusSettings>(Configuration.GetSection("DBusSettings"));
            services.Configure<InfluxSettings>(Configuration.GetSection("InfluxSettings"));            
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
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
