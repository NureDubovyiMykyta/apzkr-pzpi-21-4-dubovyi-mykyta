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
    public class OrangeryController : ControllerBase
    {
        private readonly OrangeryService _orangeryService;

        public OrangeryController(OrangeryService orangeryService)
        {
            _orangeryService = orangeryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Orangery>>> Get() =>
            Ok(await _orangeryService.GetAsync());

        [HttpGet("{id:length(24)}", Name = "GetOrangery")]
        public async Task<ActionResult<Orangery>> Get(string id)
        {
            var orangery = await _orangeryService.GetAsync(id);

            if (orangery == null)
            {
                return NotFound();
            }

            return orangery;
        }

        [HttpGet("user/{userId:length(24)}")]
        public async Task<ActionResult<IEnumerable<Orangery>>> GetByUserId(string userId) =>
            Ok(await _orangeryService.GetByUserIdAsync(userId));

        [HttpPost]
        public async Task<ActionResult<Orangery>> Create([FromBody] Orangery orangery)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = new ObjectId(userIdClaim);
            await _orangeryService.CreateAsync(orangery, userId);

            return CreatedAtRoute("GetOrangery", new { id = orangery.Id.ToString() }, orangery);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] Orangery updatedOrangery)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = new ObjectId(userIdClaim);
            await _orangeryService.UpdateAsync(id, updatedOrangery, userId);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = new ObjectId(userIdClaim);
            await _orangeryService.RemoveAsync(id, userId);

            return NoContent();
        }
    }
}
