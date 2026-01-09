using TechChallengeAPI.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiCore(builder.Configuration);

var app = builder.Build();

app.UseApiCore();
app.MapV1Endpoints();

app.MapGet("/debug/headers", (HttpContext ctx) =>
{
    var headers = ctx.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
    return Results.Ok(new
    {
        path = ctx.Request.Path.ToString(),
        headers
    });
}).AllowAnonymous();


app.Run();