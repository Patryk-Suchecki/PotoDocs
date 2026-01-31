namespace PotoDocs.API.Options;

public class OpenAIOptions
{
    public string APIKey { get; set; } = string.Empty;
    public string SystemMessage { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
