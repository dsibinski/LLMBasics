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
    
}