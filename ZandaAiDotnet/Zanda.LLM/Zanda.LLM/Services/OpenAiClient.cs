using System.Diagnostics.CodeAnalysis;
using OpenAI.Chat;
using OpenAI.Responses;
using Zanda.LLM.Models;

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

        #pragma warning disable OPENAI001
        public async Task<ResponsesAnswer> GetResponseAsync(string systemPrompt, string userMessage, string previousResponseId = "", string model = "gpt-4.1-mini")
        {
            OpenAIResponseClient client = new(model: model, apiKey: _settings.OpenAiApiKey);
            var responseOpt = new ResponseCreationOptions()
            {
                Instructions = systemPrompt,
            };
            
            if (!string.IsNullOrWhiteSpace(previousResponseId))
            {
                responseOpt.PreviousResponseId = previousResponseId;
            }
            
            OpenAIResponse response = await client.CreateResponseAsync(userMessage, responseOpt);
            var responseText = response.GetOutputText();
            return new ResponsesAnswer
            {
                ConversationId = response.Id,
                Response = responseText
            };

        }
    }
}