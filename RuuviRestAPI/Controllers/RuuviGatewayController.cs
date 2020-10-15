using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using RuuviRestAPI.models;

namespace RuuviRestAPI.Controllers
{
    [ApiController]
    [Route("api/v1/ruuvigateway")]
    public class RuuviGatewayController : ControllerBase
    {
        private readonly ILogger<RuuviGatewayController> _logger;
        private readonly IClusterClient _clusterClient;
        private readonly RestApiOptions _options;

        public RuuviGatewayController(ILogger<RuuviGatewayController> logger, IOptionsSnapshot<RestApiOptions> options, IClusterClient clusterClient)
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
                await ruuviTag.StoreMeasurementData(new Measurements
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
                    RSSI = (short) tag.rssi
                });
            }
        }
    }
}