using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Runtime;

namespace net.jommy.RuuviCore.Grains;

public class SimulatedRuuviTag(ILogger<SimulatedRuuviTag> logger, IGrainContext context, IDBusListenerClient listener) : IGrainBase, ISimulatedTag
{
    public IGrainContext GrainContext { get; } = context;

    private readonly ILogger<SimulatedRuuviTag> _logger = logger;
    private readonly IRuuviDBusListener _listener = listener;

    private Task SimulationAsync(object state, CancellationToken arg)
    {
        _logger.LogInformation("Simulating event for tag {tag}", this.GetPrimaryKeyString());
        _listener.SimulateEvent(this.GetPrimaryKeyString());
        return Task.CompletedTask;
    }

    public Task Start()
    {
        _logger.LogInformation("Starting simulated tag {tag}", this.GetPrimaryKeyString());
        this.RegisterGrainTimer<object>(SimulationAsync, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }
}