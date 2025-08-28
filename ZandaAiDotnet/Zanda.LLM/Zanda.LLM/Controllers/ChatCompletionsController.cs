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
            _logger.LogInformation("--- NEXT TURN ---");
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
        return $"""
               You are Bizzy, a helpful assistant. 

               IMPORTANT: You have access to the conversation history so far. Use this information to provide contextually relevant responses 
               and remember details about the user from previous interactions.

               Conversation History:
               {conversationHistory}

               Please respond naturally and reference previous parts of the conversation when appropriate.
               """;
    }


    [HttpPost("ConversationSummarizationHistory")]
    public async Task<IActionResult> ConversationSummarizationHistory()
    {
        var conversationSummary = "";
        var userMessages = new List<string>
        {
            "Hi! I'm Dawid and I work as a software developer for Zanda. What about you?",
            "Great! And how are you today?",
            "What do you know about me?"
        };
        
        foreach(var userMessage in userMessages)
        {
            _logger.LogInformation("--- NEXT TURN ---");
            _logger.LogInformation("User asks: {Question}", userMessage);
            
            var systemPrompt = CreateSystemPromptWithSummary(conversationSummary);
            var llmAnswer = await _openAiClient.GetCompletionAsync(systemPrompt, userMessage);
            _logger.LogInformation("LLM answers: {Answer}", llmAnswer);
            
            // Generate new summarization
            conversationSummary = await GenerateConversationSummary(userMessage, llmAnswer, conversationSummary);
        }

        return Ok(new { 
            Message = "Conversation completed with summarization tracking",
            FinalSummary = conversationSummary
        });
    }
    

    private string CreateSystemPromptWithSummary(string conversationSummary)
    {
        if (string.IsNullOrEmpty(conversationSummary))
        {
            return """
                   You are Bizzy, a helpful assistant who speaks naturally and remembers details about the user from previous interactions.

                   Let's chat!
                   """;
        }

        return $"""
               You are Bizzy, a helpful assistant who speaks naturally and remembers details about the user from previous interactions.

               Here is a summary of the conversation so far:
               <conversation_summary>
               {conversationSummary}
               </conversation_summary>

               Use this information to provide contextually relevant responses.

               Let's chat!
               """;
    }

    private async Task<string> GenerateConversationSummary(string userMessage, string assistantResponse, string previousSummary)
    {
        var summarizationPrompt = 
            $"""
              Please summarize the following conversation in a concise manner, incorporating the previous summary if available:

              <previous_summary>{previousSummary ?? "No previous summary"}</previous_summary>
              <current_turn> User: {userMessage}
              Assistant: {assistantResponse} </current_turn>

              Please create/update our conversation summary.
            """;

        try
        {
            var summary = await _openAiClient.GetCompletionAsync(summarizationPrompt, "Please create/update our conversation summary.");
            return summary ?? "No conversation history";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conversation summary");
            return previousSummary; // Fallback to previous summary if generation fails
        }
    }
    
}