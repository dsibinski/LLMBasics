using OpenAI.Chat;

namespace Zanda.LLM.Services
{
    public class OpenAiClient: IOpenAiClient
    {
        private readonly AppSettings _settings;

        public OpenAiClient(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> GetCompletionAsync(string systemPrompt, string userMessage, string model = "gpt-4.1-mini")
        {
            var openAiClient = new ChatClient(model: model, apiKey: _settings.OpenAiApiKey);
            List<ChatMessage> prompt = new()
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage),
            };

            ChatCompletion completion = await openAiClient.CompleteChatAsync(prompt);
            return completion.Content[0].Text.Trim();
        }
    }
}