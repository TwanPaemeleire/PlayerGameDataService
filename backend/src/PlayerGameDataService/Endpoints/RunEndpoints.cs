using PlayerGameDataService.Models;
using PlayerGameDataService.Services;
namespace PlayerGameDataService.Endpoints;

public static class RunEndpoints
{
    public static void MapRunEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/runs", async (RunService runService) =>
        {
            Guid runId = await runService.StartAsync();
            return Results.Ok(new StartRunResponse(runId));
        }).WithName("StartRun").RequireRateLimiting(RateLimitPolicies.PerClient);
    }
}
