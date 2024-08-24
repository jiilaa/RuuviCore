using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.jommy.RuuviCore.Gateway.Models;
using net.jommy.RuuviCore.Interfaces;
using Orleans;

namespace net.jommy.RuuviCore.Gateway.Controllers;

[ApiController]
[Route("api/v1/ruuvigateway")]
public class RuuviGatewayController : ControllerBase
{
    private readonly ILogger<RuuviGatewayController> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly RestApiOptions _options;

    public RuuviGatewayController(ILogger<RuuviGatewayController> logger, IOptions<RestApiOptions> options, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _options = options.Value;
    }

    [HttpGet("")]
    public IActionResult TestAPI()
    {
        return new JsonResult(new
        {
            Message = "If you see this message, it means your Ruuvi HTTP gateway is working!",
            Instruction = "Now just setup the gateway address in your RuuviApp to point to the same url but with /addmeasurements postfix."
        });
    }

    [HttpPost("addmeasurements")]
    public async Task<IActionResult> AddMeasurements([FromBody] RootObject measurementData)
    {
        if (!_options.AllowedDeviceIds.Contains(measurementData.deviceId))
        {
            _logger.LogWarning("Received measurement data from a device {deviceId} which is not whitelisted.", measurementData.deviceId);
            return Unauthorized("Device not whitelisted to submit measurement data.");
        }

        var storeTasks = measurementData.tags.Select(StoreMeasurements);

        await Task.WhenAll(storeTasks);

        return new OkResult();
    }

    private async Task StoreMeasurements(Tag tag)
    {
        var ruuviTag = _clusterClient.GetGrain<IRuuviTag>(tag.id);
        if (await ruuviTag.MeasurementsAllowedThroughGateway())
        {
            await ruuviTag.StoreMeasurementData(new MeasurementDTO
            {
                Acceleration = new Acceleration
                {
                    XAxis = (decimal) tag.accelX,
                    YAxis = (decimal) tag.accelY,
                    ZAxis = (decimal) tag.accelZ
                },
                Humidity = (decimal) tag.humidity,
                Pressure = (decimal) tag.pressure,
                Temperature = (decimal) tag.temperature,
                Timestamp = tag.updateAt.ToUniversalTime(),
                BatteryVoltage = (int) (tag.voltage * 1000), // convert to mV
                RSSI = (short) tag.rssi,
                MovementCounter = tag.movementCounter,
                TransmissionPower = (int) tag.txPower,
                SequenceNumber = tag.measurementSequenceNumber.GetValueOrDefault()
            });
        }
    }
}