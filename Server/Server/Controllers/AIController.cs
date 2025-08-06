using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AIService _aiService;

        public AIController(AIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<AIPromptResponse>> GenerateDrawing([FromBody] AIPromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt is required.");

            var result = await _aiService.GenerateDrawingAsync(request);

            return Ok(result);
        }

    }
}
