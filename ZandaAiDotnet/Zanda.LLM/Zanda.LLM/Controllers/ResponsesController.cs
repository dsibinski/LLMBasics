using Microsoft.AspNetCore.Mvc;
using Zanda.LLM.Services;
using Zanda.LLM.Models;

namespace Zanda.LLM.Controllers;

[ApiController]
[Route("[controller]")]
public class ResponsesController : ControllerBase
{
    private readonly ILogger<ResponsesController> _logger;
    private readonly IOpenAiClient _openAiClient;

    public ResponsesController(ILogger<ResponsesController> logger, IOpenAiClient openAiClient)
    {
        _logger = logger;
        _openAiClient = openAiClient;
    }

    [HttpPost("ConversationWithContext")]
    public async Task<IActionResult> ConversationWithContext()
    {
        var systemMessage = "You are Bizzy, a helpful assistant who remembers the conversation context and speaks naturally.";
        var userMessages = new List<string>
        {
            "Hi! I'm Dawid and I work as a software developer for Zanda. What about you?",
            "Great! And how are you today?",
            "What do you know about me?",
            "Can you remind me what we talked about earlier?"
        };
        
        string previousResponseId = "";
        
        foreach(var userMessage in userMessages)
        {
            _logger.LogInformation("--- NEXT TURN ---");
            _logger.LogInformation("User asks: {Question}", userMessage);
            _logger.LogInformation("Previous Response ID: {PreviousId}", previousResponseId);
            
            // Get response using conversation context
            var response = await _openAiClient.GetResponseAsync(systemMessage, userMessage, previousResponseId);
            
            _logger.LogInformation("LLM answers: {Answer}", response.Response);
            _logger.LogInformation("Conversation ID: {ConversationId}", response.ConversationId);
            
            // Store the response for next turn
            previousResponseId = response.ConversationId;
        }

        return Ok(new { 
            Message = "Conversation completed with OpenAI Responses API context tracking",
            FinalConversationId = previousResponseId,
        });
    }

    [HttpPost("SingleTurn")]
    public async Task<IActionResult> SingleTurn([FromBody] SingleTurnRequest request)
    {
        var systemMessage = "You are Bizzy, a helpful assistant who remembers the conversation context and speaks naturally.";
        
        _logger.LogInformation("User asks: {Question}", request.UserMessage);
        _logger.LogInformation("Previous Response ID: {PreviousId}", request.PreviousResponseId);
        
        var response = await _openAiClient.GetResponseAsync(systemMessage, request.UserMessage, request.PreviousResponseId);
        
        _logger.LogInformation("LLM answers: {Answer}", response.Response);
        _logger.LogInformation("Conversation ID: {ConversationId}", response.ConversationId);
        
        return Ok(new { 
            Response = response.Response,
            ConversationId = response.ConversationId,
            PreviousResponseId = request.PreviousResponseId
        });
    }
}

public class SingleTurnRequest
{
    public string UserMessage { get; set; } = "";
    public string PreviousResponseId { get; set; } = "";
}
