using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SagaSeminar.Shared.Service.Extensions
{
    public static class WebApplicationExtensions
    {
        public static RouteHandlerBuilder MapGetConfigurations(this WebApplication app)
        {
            return app.MapGet("/.config", (
                [FromServices] IConfiguration configuration) =>
            {
                string configDebugView = ((IConfigurationRoot)configuration).GetDebugView();

                return Results.Content(configDebugView);
            })
            .WithName("Get configurations");
        }
    }
}
