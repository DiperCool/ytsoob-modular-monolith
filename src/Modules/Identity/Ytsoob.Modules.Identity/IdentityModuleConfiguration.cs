using Asp.Versioning;
using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using Ytsoob.Modules.Identity.Identity;
using Ytsoob.Modules.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using Ytsoob.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;
using Ytsoob.Modules.Identity.Users;

namespace Ytsoob.Modules.Identity;

public class IdentityModuleConfiguration : IModuleDefinition
{
    public const string IdentityModulePrefixUri = "api/v{version:apiVersion}/identity";
    public const string ModuleName = "Identity";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    public void AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        services.AddInfrastructure(configuration);

        // Add Sub Modules Endpoints
        services.AddIdentityServices(configuration);
    }

    public async Task ConfigureModule(
        IApplicationBuilder app,
        IConfiguration configuration,
        ILogger logger,
        IWebHostEnvironment environment
    )
    {
        if (environment.IsEnvironment("test") == false)
        {
            await app.ApplicationServices.StartHostedServices();

            app.UseIdentityServer();
        }

        await app.UsePostgresPersistenceMessage<IdentityModuleConfiguration>();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var v1 = new ApiVersion(1, 0);
        var v2 = new ApiVersion(2, 0);
        var v3 = new ApiVersion(3, 0);

        VersionSet = endpoints.NewApiVersionSet().HasApiVersion(v1).HasApiVersion(v2).HasApiVersion(v3).Build();

        endpoints
            .MapGet(
                "identity",
                (HttpContext context) =>
                {
                    var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                        ? requestIdHeader.FirstOrDefault()
                        : string.Empty;

                    return $"Identity Service Apis, RequestId: {requestId}";
                }
            )
            .ExcludeFromDescription();

        // Add Sub Modules Endpoints
        endpoints.MapIdentityEndpoints();
        endpoints.MapUsersEndpoints();
    }
}
