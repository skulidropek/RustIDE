using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace RustIDE.Server.Services
{
    public class AIInteractionService
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly Kernel _kernel;
        private readonly ChatHistory _chatMessages;

        public AIInteractionService()
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                "gpt-4o-mini",
                Environment.GetEnvironmentVariable("openai-api-key")
            );

            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            _chatMessages = new ChatHistory();
        }

        public async Task<string> SendMessageToAI(string userMessage)
        {
            _chatMessages.AddUserMessage(userMessage);

            var result = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                _chatMessages,
                executionSettings: new OpenAIPromptExecutionSettings()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                    Temperature = 0
                },
                kernel: _kernel
            );

            StringBuilder assistantResponse = new StringBuilder();
            await foreach (var content in result)
            {
                assistantResponse.Append(content.Content);
            }

            string fullResponse = assistantResponse.ToString();
            _chatMessages.AddAssistantMessage(fullResponse);

            return fullResponse;
        }
    }

}
