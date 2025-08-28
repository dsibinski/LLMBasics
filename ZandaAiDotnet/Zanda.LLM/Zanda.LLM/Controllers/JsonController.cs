using Microsoft.AspNetCore.Mvc;
using Zanda.LLM.Services;
using Zanda.LLM.Models;

namespace Zanda.LLM.Controllers;

[ApiController]
[Route("[controller]")]
public class JsonController : ControllerBase
{
    private readonly ILogger<JsonController> _logger;
    private readonly IOpenAiClient _openAiClient;

    public JsonController(ILogger<JsonController> logger, IOpenAiClient openAiClient)
    {
        _logger = logger;
        _openAiClient = openAiClient;
    }

    [HttpPost("MathReasoningExample")]
    public async Task<IActionResult> MathReasoningExample()
    {
        var systemPrompt = """
            You are a math tutor. When given a math problem, solve it step by step and provide your reasoning in a structured format.
            Each step should have a clear explanation of what you're doing and the output/result of that step.
            Be thorough but concise in your explanations.
            """;

        var exampleProblem = "How can I solve 8x + 7 = -23?";

        // Define the JSON schema for MathReasoningResponse
        var jsonSchema = """
            {
                "type": "object",
                "properties": {
                    "steps": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "explanation": { "type": "string" },
                                "output": { "type": "string" }
                            },
                            "required": ["explanation", "output"],
                            "additionalProperties": false
                        }
                    },
                    "finalAnswer": { "type": "string" }
                },
                "required": ["steps", "finalAnswer"],
                "additionalProperties": false
            }
            """;

        try
        {
            _logger.LogInformation("Solving example math problem: {Problem}", exampleProblem);
            
            var jsonResponse = await _openAiClient.GetStructuredCompletionAsync(
                systemPrompt, 
                exampleProblem,
                jsonSchema
            );
            
            // Deserialize the JSON response to MathReasoningResponse
            var structuredResponse = System.Text.Json.JsonSerializer.Deserialize<MathReasoningResponse>(jsonResponse);
            if (structuredResponse == null)
            {
                return BadRequest(new { Error = "Failed to deserialize structured response" });
            }
            
            _logger.LogInformation("Structured response received with {StepCount} steps", structuredResponse.Steps.Count);
            
            return Ok(new
            {
                Problem = exampleProblem,
                Solution = structuredResponse,
                Message = "Example math problem solved with structured reasoning"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error solving example math problem: {Problem}", exampleProblem);
            return BadRequest(new { Error = "Failed to solve math problem", Details = ex.Message });
        }
    }

    [HttpPost("CustomSchema")]
    public async Task<IActionResult> CustomSchema([FromBody] CustomSchemaRequest request)
    {
        try
        {
            _logger.LogInformation("Processing custom schema request: {UserMessage}", request.UserMessage);
            
            var jsonResponse = await _openAiClient.GetStructuredCompletionAsync(
                request.SystemPrompt,
                request.UserMessage,
                request.JsonSchema
            );
            
            _logger.LogInformation("Custom schema response received");
            
            return Ok(new
            {
                UserMessage = request.UserMessage,
                SystemPrompt = request.SystemPrompt,
                Response = jsonResponse,
                Message = "Custom schema request processed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing custom schema request: {UserMessage}", request.UserMessage);
            return BadRequest(new { Error = "Failed to process custom schema request", Details = ex.Message });
        }
    }
}

public class CustomSchemaRequest
{
    public string SystemPrompt { get; set; } = "";
    public string UserMessage { get; set; } = "";
    public string JsonSchema { get; set; } = "";
}