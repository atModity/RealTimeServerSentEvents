using System.Net.ServerSentEvents;
using RealTimeServerSentEvents;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TimeSeriesSpammer>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("EventClient", policy =>
        {
            policy.WithOrigins("http://localhost:63342")
                .WithMethods("GET")
                .AllowAnyHeader();
        });
    });
}

var app = builder.Build();


app.MapGet("/timeseries", (TimeSeriesSpammer timeSeriesSpammer, CancellationToken ct) =>
    TypedResults.ServerSentEvents(
        timeSeriesSpammer.GetTimeSeriesUpdates(ct),
        eventType: "timeSeriesUpdate"
    ));

app.MapGet("/timeseries/latest", (TimeSeriesSpammer timeSeriesSpammer,
    HttpRequest request,
    CancellationToken ct) =>
{
    // 1. Read Last-Event-ID (if any)
    var lastEventId = request.Headers.TryGetValue("Last-Event-ID", out var id)
        ? id.ToString()
        : null;

    // 2. Optionally log or handle resume logic
    if (!string.IsNullOrEmpty(lastEventId))
    {
        app.Logger.LogInformation("Reconnected, client last saw ID {LastId}", lastEventId);
    }

    // 3. Stream SSE with lastEventId and retry 
    var stream = timeSeriesSpammer.GenerateTimeSeriesSince(lastEventId, ct).Select(e =>
    {
        var sseItem = new SseItem<TimeSeriesEvent>(e, "timeSeriesUpdate")
        {
            EventId = e.Id
        };

        return sseItem;
    });
    return TypedResults.ServerSentEvents(
        stream,
        eventType: "timeSeriesUpdate"
    );
});

app.UseCors("EventClient");
app.Run();