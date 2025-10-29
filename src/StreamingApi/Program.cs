var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Welcome to the streaming CDK demo!\n");

app.MapGet("/stream", async context =>
{
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Content-Type", "text/plain");

    for (var i = 0; i < 5; i++)
    {
        await context.Response.WriteAsync($"data: tick {i}\n\n");
        await context.Response.Body.FlushAsync();
        await Task.Delay(500);
    }
});

app.Run();
