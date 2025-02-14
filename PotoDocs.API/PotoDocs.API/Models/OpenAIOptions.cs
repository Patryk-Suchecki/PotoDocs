namespace PotoDocs.API.Models;

public class OpenAIOptions
{
    public string APIKey { get; set; }
    public string SystemMessage { get; set; }
    public string PromptTemplate { get; set; }
}
