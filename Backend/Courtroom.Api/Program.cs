var builder = WebApplication.CreateBuilder(args);
// register gemini client with dependency injection
// ASP.NET Core supplies its httpclient and configuration
builder.Services.AddHttpClient<GeminiWitnessClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.MapGet("/", () => "Courtroom API is running.");

app.MapPost("/api/witness", async (
    WitnessRequest request,
    GeminiWitnessClient geminiClient,
    CancellationToken cancellationToken) =>
{
    // reject empty questions before making request
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest(new
        {
            error="Please provide a question."
        });
    }

    try
    {
        // await network request without blocking thread
        // pass cancellation on so work can stop if caller disconnects
        var answer = await geminiClient.GetAnswerAsync(
            request.Question,
            cancellationToken);

        // ASP.NET Core converts this anonymous object into JSON
        return Results.Ok(new {answer});
    }
    catch (HttpRequestException ex)
    {
        // log upstream status without logging api key
        app.Logger.LogWarning("Gemini request failed with HTTP status {StatusCode}.", ex.StatusCode);
        // 502: api couldn't get a successful upstream response
        return Results.Json(
            new 
            {
                error = "The AI service request failed. Check the backend logs."
            },
            statusCode: 502);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        // handles a timeout (rather than cancellation by caller)
        // 504: upstream service didn't respond in time 
        return Results.Json(
            new
            {
                error = "The AI service took too long to respond."
            },
            statusCode: 504);
    }
    catch (InvalidOperationException)
    {
        // gemini client couldn't extract a usable answer
        return Results.Json(
            new
            {
                error = "The AI service returned no usable answer."
            },
            statusCode: 502);
    }
});

app.Run();

// small data type that represents incoming json body
// eg. {"question": "where were you?"}
public record WitnessRequest(string Question);
