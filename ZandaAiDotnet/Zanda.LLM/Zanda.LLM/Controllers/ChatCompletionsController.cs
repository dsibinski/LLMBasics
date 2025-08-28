using Microsoft.AspNetCore.Mvc;
using Zanda.LLM.Services;

namespace Zanda.LLM.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatCompletionsController : ControllerBase
{
    
    private readonly ILogger<ChatCompletionsController> _logger;
    private readonly IOpenAiClient _openAiClient;

    public ChatCompletionsController(ILogger<ChatCompletionsController> logger, IOpenAiClient openAiClient)
    {
        _logger = logger;
        _openAiClient = openAiClient;
    }

    [HttpPost]
    public async Task<IActionResult> Simple()
    {
        var systemMessage = "You are Bizzy, a helpful assistant.";
        var userMessages = new List<string>
        {
            "Hi! I'm Dawid and I work as a software developer for Zanda. What about you?",
            "Great! And how are you today?",
            "What do you know about me?"
        };
        
        foreach(var userMessage in userMessages)
        {
            _logger.LogInformation("User asks: {Question}", userMessage);
            var llmAnswer = await _openAiClient.GetCompletionAsync(systemMessage, userMessage);
            _logger.LogInformation("LLM answers: {Answer}", llmAnswer);
        }

        return Ok();
    }

    [HttpPost("ConversationNaiveHistory")]
    public async Task<IActionResult> ConversationNaiveHistory()
    {
        var conversationHistory = new List<string>();
        var userMessages = new List<string>
        {
            "Hi! I'm Dawid and I work as a software developer for Zanda. What about you?",
            "Great! And how are you today?",
            "What do you know about me?"
        };
        
        foreach(var userMessage in userMessages)
        {
            _logger.LogInformation("User asks: {Question}", userMessage);
            
            // Format the conversation history
            var conversationHistoryText = string.Join(", ", conversationHistory);
            var systemMessageWithHistory = BuildSystemPromptWithHistory(conversationHistoryText);
            
            var llmAnswer = await _openAiClient.GetCompletionAsync(systemMessageWithHistory, userMessage);
            _logger.LogInformation("LLM answers: {Answer}", llmAnswer);
            
            // Add the exchange to conversation history
            conversationHistory.Add($"User: {userMessage}");
            conversationHistory.Add($"Assistant: {llmAnswer}");
        }

        return Ok(new { 
            Message = "Conversation completed with history tracking",
            ConversationHistory = conversationHistory
        });
    }

    private string BuildSystemPromptWithHistory(string conversationHistory)
    {
        return @"You are Bizzy, a helpful assistant. 

IMPORTANT: You have access to the conversation history so far. Use this information to provide contextually relevant responses and remember details about the user from previous interactions.

Conversation History:
" + conversationHistory + @"

Please respond naturally and reference previous parts of the conversation when appropriate.";
    }
    
}