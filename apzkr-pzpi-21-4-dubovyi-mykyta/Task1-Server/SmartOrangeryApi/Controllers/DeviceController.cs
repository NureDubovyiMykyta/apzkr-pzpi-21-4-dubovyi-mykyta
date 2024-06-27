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
    public class DeviceController : ControllerBase
    {
        private readonly DeviceService _deviceService;
        private readonly RegulationService _regulationService;

        public DeviceController(DeviceService deviceService, RegulationService regulationService)
        {
            _deviceService = deviceService;
            _regulationService = regulationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> Get() =>
            Ok(await _deviceService.GetAsync());

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Device>> Get(string id)
        {
            var device = await _deviceService.GetAsync(id);

            if (device == null)
            {
                return NotFound();
            }

            return device;
        }

        [HttpGet("orangery/{orangeryId}")]
        public async Task<ActionResult<IEnumerable<Device>>> GetByOrangeryId(string orangeryId) =>
            Ok(await _deviceService.GetByOrangeryIdAsync(orangeryId));

        [HttpPost("create")]
        public async Task<ActionResult<Device>> Create([FromBody] Device device)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _deviceService.CreateAsync(device, userId);

            return CreatedAtRoute("GetDevice", new { id = device.Id.ToString() }, device);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] Device updatedDevice)
        {
            await _deviceService.UpdateAsync(id, updatedDevice);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = new ObjectId(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _deviceService.RemoveAsync(id, userId);

            return NoContent();
        }

        [HttpPost("{id:length(24)}/turn-on")]
        public async Task<IActionResult> TurnOn(string id)
        {
            var device = await _deviceService.GetAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            await _regulationService.SendCommandToDevice(device, "turn_on");
            device.Status = "on";
            await _deviceService.UpdateAsync(id, device);

            return NoContent();
        }

        [HttpPost("{id:length(24)}/turn-off")]
        public async Task<IActionResult> TurnOff(string id)
        {
            var device = await _deviceService.GetAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            await _regulationService.SendCommandToDevice(device, "turn_off");
            device.Status = "off";
            await _deviceService.UpdateAsync(id, device);

            return NoContent();
        }
    }
}
