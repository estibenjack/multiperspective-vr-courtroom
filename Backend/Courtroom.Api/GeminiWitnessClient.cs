using System.Net.Http.Json;
using System.Text.Json;

public class GeminiWitnessClient
{
    // readonly = fields can only be assigned here or in constructor
    private readonly HttpClient httpClient;
    private readonly string apiKey;
    private readonly string model;

    public GeminiWitnessClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        this.httpClient = httpClient;
        // reads key from configuration (supplied locally by user-secrets)
        apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API key is missing.");
        // model name is in appsettings.json
        model = configuration["Gemini:Model"]
            ?? throw new InvalidOperationException("Gemini model is missing.");
    }

    // Task<string> represents asynchronous operation returning a string
    // optional token lets caller cancel operation
    public async Task<string> GetAnswerAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        // escape model name so it can be used in url
        var url = "https://generativelanguage.googleapis.com/v1beta/models/" + $"{Uri.EscapeDataString(model)}:generateContent";

        // anonymous objects describe the json structure gemini expects
        // "user" means the message comes from the person asking a question
        // not the courtroom role they selected
        var body = new
        {
            contents = new[]
            {
                new{
                    role = "user",
                    parts = new[]
                    {
                        new {text = question}
                    }
                }
            }
        };

        // using var disposes resources when this method exits, even on an exception
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        // authenticate through a header (keeping key out of url)
        request.Headers.Add("x-goog-api-key", apiKey);
        // convert request obj into json
        request.Content = JsonContent.Create(body);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);
        // throw HttpRequestException for an unsuccessful HTTP status
        // program.cs handles this and returns a controlled error response
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(json);

        // gemini returns candidate responses containing content + parts
        // check that an answer exists before trying to read its text
        if (!document.RootElement.TryGetProperty("candidates", out var candidates)
            || candidates.GetArrayLength() == 0
            || !candidates[0].TryGetProperty("content", out var content)
            || !content.TryGetProperty("parts", out var parts))
        {
            throw new InvalidOperationException("Gemini returned no answer.");
        }

        var answerParts = new List<string>();

        foreach (var part in parts.EnumerateArray())
        {
            // skip parts marked as model thoughts
            if (part.TryGetProperty("thought", out var thought)
                && thought.ValueKind == JsonValueKind.True)
            {
                continue;
            }

            // some parts could contain something other than text
            if (part.TryGetProperty("text", out var text))
            {
                var value = text.GetString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    answerParts.Add(value);
                }
            }
        }

        // check collected at least one text part before returning
        if (answerParts.Count == 0)
        {
            throw new InvalidOperationException("Gemini returned no text answer.");
        }

        return string.Join("\n", answerParts);
    }
}