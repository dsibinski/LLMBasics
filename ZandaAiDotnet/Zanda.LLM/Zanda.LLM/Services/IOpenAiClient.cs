namespace Zanda.LLM.Services
{
    public interface IOpenAiClient
    {
        Task<string> GetCompletionAsync(
            string systemPrompt,
            string userMessage,
            string model = "gpt-4.1-mini"
        );
    }
}