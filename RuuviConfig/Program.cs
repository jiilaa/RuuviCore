using System;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Hosting;

namespace net.jommy.RuuviCore;

public class OptionBase
{
    [Option('m', "mac", Required = true, HelpText = "MAC address in the following format: AA:BB:CC:DD:EE:FF")]
    public string MACAddress { get; set; }
}

[Verb("edit", HelpText = "Edits options of an existing RuuviTag. Will not create a new if MAC address does not match an existing one.")]
public class EditOptions : OptionBase
{
    [Option('n', "name", Required = false, HelpText = "RuuviTag name.")]
    public string Name { get; set; }

    [Option('i', Required = false, HelpText = "Data saving interval (seconds). Defines how often the measurements are delivered onwards.")]
    public int? Interval { get; set; }

    [Option('a', Required = false, HelpText = "Whether the RuuviTag should calculate average of the measurements received during the last interval before delivering them.")]
    public bool? Average { get; set; }

    [Option('g', Required = false, HelpText = "Whether the RuuviTag should store acceleration values.")]
    public bool? Acceleration { get; set; }

    [Option('h', longName: "allowHttp", Required = false, HelpText = "Whether the RuuviTag allows incoming values through the RuuviGateway (HTTP).")]
    public bool? AllowHttp { get; set; }

    [Option('c', longName: "checkValidity", Required = false, HelpText = "Whether the RuuviTag should do some crude sanity checks for incoming data.")]
    public bool? CheckValidity { get; set; }

    [Option('b', longName: "bucketSize", Required = false, HelpText = "Bucket size in minutes. Ignored if Average is false. Otherwise, defines the bucket size to calculate the averages. Must be even hours or 1/2/3/5/10/15/20/30 minutes", Default = 15)]
    public int BucketSizeMinutes { get; set; }
}

[Verb("add", HelpText = "Adds a new RuuviTag with given options. If a RuuviTag with specified MAC is already added, returns an error, unless overwrite option is used.")]
public class AddOptions : OptionBase
{
    [Option('n', "name", Required = true, HelpText = "RuuviTag name.")]
    public string Name { get; set; }

    [Option('i', Required = false, HelpText = "Data saving interval (seconds). Defines how often the measurements are delivered onwards.", Default = 60)]
    public int Interval { get; set; }

    [Option('a', Required = false, HelpText = "Whether the RuuviTag should calculate average of the measurements received during the last interval before delivering them.", Default = false)]
    public bool Average { get; set; }

    [Option('g', Required = false, HelpText = "Whether the RuuviTag should store acceleration values.", Default = false)]
    public bool Acceleration { get; set; }

    [Option('h', longName: "allowHttp", Required = false, HelpText = "Whether the RuuviTag allows incoming values through the RuuviGateway (HTTP).", Default = false)]
    public bool AllowHttp { get; set; }

    [Option('c', longName: "checkValidity", Required = false, HelpText = "Whether the RuuviTag should do some crude sanity checks for incoming data.", Default = false)]
    public bool CheckValidity { get; set; }

    [Option('o', Required = false, HelpText = "If a RuuviTag is registered with the same MAC address, overwrite the configuration (even with the default values).", Default = false)]
    public bool Overwrite { get; set; }

    [Option('b', longName: "bucketSize", Required = false, HelpText = "Bucket size in minutes. Ignored if Average is false. Otherwise, defines the bucket size to calculate the averages. Must be even hours or 1/2/3/5/10/15/20/30 minutes", Default = 15)]
    public int BucketSizeMinutes { get; set; }
}

[Verb("view", HelpText = "Display information of a RuuviTag. As a side effect, creates a new (unconfigured) RuuviTag with specified MAC address if one does not exist.")]
public class ViewOptions : OptionBase
{
    [Option('c', "cacheddata", Required = false, HelpText = "Display cached data (i.e. data not yet delivered onwards).", Default = false)]
    public bool CachedMeasurements { get; set; }

    [Option('o', Required = false, HelpText = "Display the RuuviTag options.", Default = false)]
    public bool Options { get; set; }
}

class Program
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
        using var host = Host.CreateDefaultBuilder(args)
            .UseOrleansClient(clientBuilder =>
                clientBuilder.UseLocalhostClustering(serviceId: "RuuviCore", clusterId: "rc"))
            .ConfigureServices(
                services => services
                    .Configure<ConsoleLifetimeOptions>(sp => sp.SuppressStatusMessages = true))
            .Build();
        await host.StartAsync();

        Console.Write(Logo);
        Console.WriteLine($"RuuviCore configuration utility. Version: {Assembly.GetEntryAssembly()?.GetName().Version}");

        try
        {
            var client = host.Services.GetRequiredService<IClusterClient>();
            await Parser.Default.ParseArguments<AddOptions, EditOptions, ViewOptions>(args)
                .MapResult((AddOptions options) => AddRuuviTag(client, options),
                    (EditOptions options) => EditRuuviTag(client, options),
                    (ViewOptions options) => ViewData(client, options),
                    _ => Task.FromResult(1));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<int> ViewData(IGrainFactory client, ViewOptions options)
    {
        var ruuviTag = client.GetGrain<IRuuviTag>(options.MACAddress);

        if (options.CachedMeasurements)
        {
            var measurementList = await ruuviTag.GetCachedMeasurements();
            foreach (var measurements in measurementList)
            {
                Console.WriteLine(measurements);
            }
        }

        if (options.Options)
        {
            var currentOptions = await ruuviTag.GetDataSavingOptions();

            Console.WriteLine($"Options:{currentOptions}");
        }

        return 0;
    }

    private static async Task<int> EditRuuviTag(IGrainFactory client, EditOptions options)
    {
        var ruuviTag = client.GetGrain<IRuuviTag>(options.MACAddress);
        if (options.Name != null)
        {
            await ruuviTag.SetName(options.Name);
        }

        if (options.AllowHttp.HasValue)
        {
            await ruuviTag.AllowMeasurementsThroughGateway(options.AllowHttp.Value);
        }

        if (options.Acceleration.HasValue || options.Average.HasValue || options.Interval.HasValue)
        {
            var existingOptions = await ruuviTag.GetDataSavingOptions();
            existingOptions.StoreAcceleration = options.Acceleration ?? existingOptions.StoreAcceleration;
            existingOptions.CalculateAverages = options.Average ?? existingOptions.CalculateAverages;
            existingOptions.DataSavingInterval = options.Interval ?? existingOptions.DataSavingInterval;
            existingOptions.DiscardMinMaxValues = options.CheckValidity ?? existingOptions.DiscardMinMaxValues;
            existingOptions.BucketSize = TimeSpan.FromMinutes(options.BucketSizeMinutes);

            await ruuviTag.SetDataSavingOptions(existingOptions);
            Console.WriteLine($"Saved options: {existingOptions}.");
        }
        else if (!options.AllowHttp.HasValue)
        {
            Console.WriteLine("No options specified.");
        }

        return 0;
    }

    private static async Task<int> AddRuuviTag(IGrainFactory client, AddOptions options)
    {
        var ruuviTag = client.GetGrain<IRuuviTag>(options.MACAddress);

        if (!options.Overwrite)
        {
            var name = await ruuviTag.GetName();
            if (name != null)
            {
                throw new Exception($"RuuviTag already registered with name {name}.");
            }
        }

        await ruuviTag.Initialize(options.MACAddress, options.Name, new DataSavingOptions
        {
            CalculateAverages = options.Average,
            DataSavingInterval = options.Interval,
            StoreAcceleration = options.Acceleration,
            DiscardMinMaxValues = options.CheckValidity
        });

        if (options.AllowHttp)
        {
            await ruuviTag.AllowMeasurementsThroughGateway(true);
        }

        return 0;
    }
}
