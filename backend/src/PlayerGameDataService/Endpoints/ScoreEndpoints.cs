using PlayerGameDataService.Models;
namespace PlayerGameDataService.Endpoints;

public static class ScoreEndpoints
{
    public static void MapScoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/scores");

        group.MapGet("/", () =>
        {
            Score[] items =
            [
                // Version 7 because time is a part of it, so easier to sort
                new Score(Guid.CreateVersion7(), 200, "Me", DateTimeOffset.UtcNow),
                new Score(Guid.CreateVersion7(), 400, "Myself", DateTimeOffset.UtcNow.AddDays(-1)),
            ];
            return items;
        }).WithName("GetScores");
    }
}
