using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using RustIDE.Server.Models;
using RustIDE.Server.Services;

namespace RustIDE.Server.Controllers
{
    [ApiController]
    [Route("api/code")]
    public class CodeController : ControllerBase
    {
        private readonly CodeExecutorService _codeExecutorService;

        public CodeController(CodeExecutorService codeExecutorService)
        {
            _codeExecutorService = codeExecutorService;
        }

        [HttpPost("compile")]
        public async Task<IActionResult> Compile([FromBody] CodeRequest request)
        {
            var result = await _codeExecutorService.Compile(request.Code);
            return Ok(result);
        }
    }
}
