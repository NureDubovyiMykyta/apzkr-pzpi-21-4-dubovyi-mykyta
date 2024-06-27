using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SmartOrangeryApi.Models;
using SmartOrangeryApi.Services;
using System.Security.Claims;


namespace SmartOrangeryApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly SensorService _sensorService;
        private readonly LogService _logService;
        private readonly RegulationService _regulationService;

        public SensorController(SensorService sensorService, LogService logService, RegulationService regulationService)
        {
            _sensorService = sensorService;
            _logService = logService;
            _regulationService = regulationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sensor>>> Get() =>
            Ok(await _sensorService.GetAsync());

        [HttpGet("{id:length(24)}", Name = "GetSensor")]
        public async Task<ActionResult<Sensor>> Get(string id)
        {
            var sensor = await _sensorService.GetAsync(id);

            if (sensor == null)
            {
                return NotFound();
            }

            return sensor;
        }

        [HttpGet("orangery/{orangeryId:length(24)}")]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetByOrangeryId(string orangeryId) =>
            Ok(await _sensorService.GetByOrangeryIdAsync(orangeryId));

        [HttpPost("create")]
        public async Task<ActionResult<Sensor>> Create([FromBody] Sensor sensor)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _sensorService.CreateAsync(sensor, userId);

            return CreatedAtRoute("GetSensor", new { id = sensor.Id.ToString() }, sensor);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] Sensor updatedSensor)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _sensorService.UpdateAsync(id, updatedSensor, userId);

            return NoContent();
        }

        [HttpPost("log-data")]
        [AllowAnonymous]
        public async Task<IActionResult> LogSensorData([FromBody] CreateLogModel sensorData)
        {
            var sensor = await _sensorService.GetAsync(sensorData.SensorId);
            if (sensor == null)
            {
                return NotFound("Sensor not found");
            }

            var log = new Log
            {
                SensorId = new ObjectId(sensorData.SensorId),
                Value = sensorData.Value,
                Time = DateTime.UtcNow
            };

            await _logService.CreateAsync(log);

            sensor.LastValue = sensorData.Value;
            sensor.LastUpdated = DateTime.UtcNow;
            await _sensorService.UpdateAsync(sensor.Id.ToString(), sensor, ObjectId.Empty);

            await _regulationService.RegulateConditions(sensor);

            return Ok();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _sensorService.RemoveAsync(id, userId);

            return NoContent();
        }
    }

}