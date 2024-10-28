using Microsoft.AspNetCore.Mvc;
using RustIDE.Server.Services;
using RustIDE.Server.Models.Dto;
using RustIDE.Server.Models;

namespace RustIDE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AIInteractionService _aiService;

        public AIController(AIInteractionService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<ActionResult<string>> SendMessage([FromBody] UserMessageRequest request)
        {
            try
            {
                string response = await _aiService.SendMessageToAI(request.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
