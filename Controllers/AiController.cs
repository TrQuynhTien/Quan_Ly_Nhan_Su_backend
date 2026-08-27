using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNhanSu.API.DTOs;
using QuanLyNhanSu.API.Services;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/ai")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly AiService _aiService;

        public AiController(AiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("ask")]
        [Authorize]
        public async Task<IActionResult> Ask(AiQuestionRequest request)
        {
            var result = await _aiService.AskAsync(request.Question);

            return Ok(new
            {
                Answer = result
            });
        }
    }
}