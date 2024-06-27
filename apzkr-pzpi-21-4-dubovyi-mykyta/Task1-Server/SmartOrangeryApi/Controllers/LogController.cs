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
    public class LogController : ControllerBase
    {
        private readonly LogService _logService;

        public LogController(LogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> Get() =>
            Ok(await _logService.GetAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<Log>> Get(string id)
        {
            var log = await _logService.GetAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return log;
        }

        [HttpGet("sensor/{sensorId}")]
        public async Task<ActionResult<IEnumerable<Log>>> GetBySensorId(string sensorId) =>
            Ok(await _logService.GetBySensorIdAsync(sensorId));

        [HttpPost]
        public async Task<ActionResult<Log>> Create([FromBody] CreateLogModel model)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var log = new Log
            {
                SensorId = new ObjectId(model.SensorId),
                Value = model.Value,
                Time = DateTime.UtcNow
            };

            await _logService.CreateAsync(log);

            return CreatedAtRoute("GetLog", new { id = log.Id.ToString() }, log);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Log updatedLog)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _logService.UpdateAsync(id, updatedLog, userId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _logService.RemoveAsync(id, userId);

            return NoContent();
        }
    }

}
