using Zanda.LLM.Models;

namespace Zanda.LLM.Services
{
    public interface IOpenAiClient
    {
        Task<string> GetCompletionAsync(
            string systemPrompt,
            string userMessage,
            string model = "gpt-4.1-mini"
        );
        
        Task<ResponsesAnswer> GetResponseAsync(
            string systemPrompt,
            string userMessage,
            string previousResponseId = "",
            string model = "gpt-4.1-mini"
        );
        
        Task<string> GetStructuredCompletionAsync(
            string systemPrompt,
            string userMessage,
            string jsonSchema,
            string model = "gpt-4.1-mini"
        );
    }
}