using Library;
using Library.Models;
using Microsoft.AspNetCore.Mvc;
using RustIDE.Server.Models;
using RustIDE.Server.Services;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace RustIDE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompletionController : ControllerBase
    {
        private readonly CompletionService _completionService;

        public CompletionController(CompletionService completionService)
        {
            _completionService = completionService;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<CompletionItem>>> GetCompletions([FromBody] CompletionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Invalid request or empty code.");
            }

            var completions = await _completionService.GetCompletionsAsync(request.Code, request.Position);
            return Ok(completions);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssemblyModel>>> GetAssemblys()
        {
            var directoryPath = Path.Combine(AppContext.BaseDirectory, "Managed");

            List<AssemblyModel> assemblies = new List<AssemblyModel>();

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                assemblies.Add(AssemblyDataSerializer.ConvertToModel(file));
            }

            return assemblies;
        }
    }
}
