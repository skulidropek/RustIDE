using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using RustIDE.Server.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace RustIDE.Server.Services
{
    public class HooksService
    {
        private readonly List<HookModel> _hooks;

        public HooksService()
        {
            _hooks = JsonConvert.DeserializeObject<List<HookModel>>(File.ReadAllText("allhooks.json"));
        }

        public IEnumerable<HookModel> Hooks() => _hooks;
    }
}