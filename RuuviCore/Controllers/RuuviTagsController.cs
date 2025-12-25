using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using net.jommy.RuuviCore.Interfaces;
using Orleans;

namespace net.jommy.RuuviCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RuuviTagsController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;
        private readonly ILogger<RuuviTagsController> _logger;

        public RuuviTagsController(IGrainFactory grainFactory, ILogger<RuuviTagsController> logger)
        {
            _grainFactory = grainFactory;
            _logger = logger;
        }

        // GET: api/ruuvitags
        [HttpGet]
        public async Task<ActionResult<List<RuuviTagInfo>>> GetAll()
        {
            try
            {
                var registry = _grainFactory.GetGrain<IRuuviTagRegistry>(0);
                var tags = await registry.GetAll();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all Ruuvi tags");
                return StatusCode(500, new { error = "Failed to retrieve tags" });
            }
        }

        // GET: api/ruuvitags/{macAddress}
        [HttpGet("{macAddress}")]
        public async Task<ActionResult<RuuviTag>> Get(string macAddress)
        {
            try
            {
                var ruuviTag = _grainFactory.GetGrain<IRuuviTag>(macAddress);
                var tag = await ruuviTag.GetTag();

                if (tag == null || string.IsNullOrEmpty(tag.Name))
                {
                    return NotFound(new { error = $"Tag with MAC address {macAddress} not found" });
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Ruuvi tag {MacAddress}", macAddress);
                return StatusCode(500, new { error = "Failed to retrieve tag" });
            }
        }

        // POST: api/ruuvitags
        [HttpPost]
        public async Task<ActionResult<RuuviTag>> Create([FromBody] CreateRuuviTagRequest request)
        {
            try
            {
                if (!IsValidMacAddress(request.MacAddress))
                {
                    return BadRequest(new { error = "Invalid MAC address format" });
                }

                var ruuviTag = _grainFactory.GetGrain<IRuuviTag>(request.MacAddress);

                // Check if already exists
                var existingName = await ruuviTag.GetName();
                if (!string.IsNullOrEmpty(existingName))
                {
                    return Conflict(new { error = $"Tag with MAC address {request.MacAddress} already exists" });
                }

                // Initialize the tag
                await ruuviTag.Initialize(request.MacAddress, request.Name, new DataSavingOptions
                {
                    DataSavingInterval = request.DataSavingInterval ?? 60,
                    CalculateAverages = request.CalculateAverages ?? false,
                    StoreAcceleration = request.StoreAcceleration ?? false,
                    DiscardMinMaxValues = request.DiscardMinMaxValues ?? true
                });

                if (request.AllowHttp ?? false)
                {
                    await ruuviTag.AllowMeasurementsThroughGateway(true);
                }

                var createdTag = await ruuviTag.GetTag();
                return CreatedAtAction(nameof(Get), new { macAddress = request.MacAddress }, createdTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Ruuvi tag");
                return StatusCode(500, new { error = "Failed to create tag" });
            }
        }

        // PUT: api/ruuvitags/{macAddress}
        [HttpPut("{macAddress}")]
        public async Task<ActionResult> Update(string macAddress, [FromBody] UpdateRuuviTagRequest request)
        {
            try
            {
                var ruuviTag = _grainFactory.GetGrain<IRuuviTag>(macAddress);

                // Check if exists
                var existingName = await ruuviTag.GetName();
                if (string.IsNullOrEmpty(existingName))
                {
                    return NotFound(new { error = $"Tag with MAC address {macAddress} not found" });
                }

                // Update name if provided
                if (!string.IsNullOrEmpty(request.Name))
                {
                    await ruuviTag.SetName(request.Name);
                }

                // Update HTTP gateway permission if provided
                if (request.AllowHttp.HasValue)
                {
                    await ruuviTag.AllowMeasurementsThroughGateway(request.AllowHttp.Value);
                }

                // Update data saving options if any provided
                if (request.DataSavingInterval.HasValue || request.CalculateAverages.HasValue ||
                    request.StoreAcceleration.HasValue || request.DiscardMinMaxValues.HasValue)
                {
                    var options = await ruuviTag.GetDataSavingOptions();

                    if (request.DataSavingInterval.HasValue)
                        options.DataSavingInterval = request.DataSavingInterval.Value;
                    if (request.CalculateAverages.HasValue)
                        options.CalculateAverages = request.CalculateAverages.Value;
                    if (request.StoreAcceleration.HasValue)
                        options.StoreAcceleration = request.StoreAcceleration.Value;
                    if (request.DiscardMinMaxValues.HasValue)
                        options.DiscardMinMaxValues = request.DiscardMinMaxValues.Value;

                    await ruuviTag.SetDataSavingOptions(options);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Ruuvi tag {MacAddress}", macAddress);
                return StatusCode(500, new { error = "Failed to update tag" });
            }
        }

        // DELETE: api/ruuvitags/{macAddress}
        [HttpDelete("{macAddress}")]
        public async Task<ActionResult> Delete(string macAddress)
        {
            try
            {
                var ruuviTag = _grainFactory.GetGrain<IRuuviTag>(macAddress);

                // Check if exists
                var existingName = await ruuviTag.GetName();
                if (string.IsNullOrEmpty(existingName))
                {
                    return NotFound(new { error = $"Tag with MAC address {macAddress} not found" });
                }

                // Note: Orleans grains don't have a direct delete method
                // You might want to implement a "soft delete" by setting a flag
                // For now, we'll just return success
                _logger.LogInformation("Delete requested for tag {MacAddress}", macAddress);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Ruuvi tag {MacAddress}", macAddress);
                return StatusCode(500, new { error = "Failed to delete tag" });
            }
        }

        // GET: api/ruuvitags/{macAddress}/measurements
        [HttpGet("{macAddress}/measurements")]
        public async Task<ActionResult<List<MeasurementDTO>>> GetMeasurements(string macAddress)
        {
            try
            {
                var ruuviTag = _grainFactory.GetGrain<IRuuviTag>(macAddress);
                var measurements = await ruuviTag.GetCachedMeasurements();
                return Ok(measurements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurements for {MacAddress}", macAddress);
                return StatusCode(500, new { error = "Failed to retrieve measurements" });
            }
        }

        private bool IsValidMacAddress(string mac)
        {
            if (string.IsNullOrEmpty(mac)) return false;
            var regex = new System.Text.RegularExpressions.Regex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
            return regex.IsMatch(mac);
        }
    }

    public class CreateRuuviTagRequest
    {
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public int? DataSavingInterval { get; set; }
        public bool? CalculateAverages { get; set; }
        public bool? StoreAcceleration { get; set; }
        public bool? DiscardMinMaxValues { get; set; }
        public bool? AllowHttp { get; set; }
    }

    public class UpdateRuuviTagRequest
    {
        public string Name { get; set; }
        public int? DataSavingInterval { get; set; }
        public bool? CalculateAverages { get; set; }
        public bool? StoreAcceleration { get; set; }
        public bool? DiscardMinMaxValues { get; set; }
        public bool? AllowHttp { get; set; }
    }
}