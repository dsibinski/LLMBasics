using System.Text.Json.Serialization;

namespace Zanda.LLM.Models
{
    public class MathReasoningResponse
    {
        [JsonPropertyName("steps")]
        public List<ReasoningStep> Steps { get; set; } = new();
        
        [JsonPropertyName("finalAnswer")]
        public string FinalAnswer { get; set; } = "";
    }

    public class ReasoningStep
    {
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = "";
        
        [JsonPropertyName("output")]
        public string Output { get; set; } = "";
    }
}
