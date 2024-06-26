using System.Reflection;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Caching;
using BuildingBlocks.Caching.Behaviours;
using BuildingBlocks.Cap;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Security;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ytsoob.Modules.Posts.Shared.Data;

namespace Ytsoob.Modules.Posts.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        SnowFlakIdGenerator.Configure(2);
        services.AddControllersAsServices();

        services.AddCqrs(doMoreActions: s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        });

        services.AddCore(configuration, PostsModuleConfiguration.ModuleName, Assembly.GetExecutingAssembly());

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddJwt(configuration);

        services.AddSingleton<IBus, CapBus>();
        services.AddTransient<IMessagePersistenceService, MessagePersistenceService>();
        services.AddScoped<IMessagePersistenceRepository, PostgresMessagePersistenceRepository>();
        services.AddHostedService<MessagePersistenceBackgroundService>();
        services.AddPostgresMessagePersistence(
            $"{PostsModuleConfiguration.ModuleName}:{nameof(MessagePersistenceOptions)}"
        );
        services.AddCustomCaching(configuration, PostsModuleConfiguration.ModuleName);
        services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());
        services.AddPostgresDbContext<PostsDbContext>(
            configuration,
            $"{PostsModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}"
        );
        return services;
    }
}
