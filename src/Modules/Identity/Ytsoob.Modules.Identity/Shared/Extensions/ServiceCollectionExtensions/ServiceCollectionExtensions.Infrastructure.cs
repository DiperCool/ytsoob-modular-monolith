using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Caching.Behaviours;
using BuildingBlocks.Cap;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Email.Options;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using BuildingBlocks.Security;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;

namespace Ytsoob.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        SnowFlakIdGenerator.Configure(1);
        services.AddControllersAsServices();

        services.AddCqrs(doMoreActions: s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        });

        services.AddCore(configuration, IdentityModuleConfiguration.ModuleName, Assembly.GetExecutingAssembly());

        services.AddEmailService(configuration, $"{IdentityModuleConfiguration.ModuleName}:{nameof(EmailOptions)}");

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddJwt(configuration);

        services.AddSingleton<IBus, CapBus>();
        services.AddTransient<IMessagePersistenceService, MessagePersistenceService>();
        services.AddScoped<IMessagePersistenceRepository, PostgresMessagePersistenceRepository>();
        services.AddHostedService<MessagePersistenceBackgroundService>();
        services.AddPostgresMessagePersistence(
            $"{IdentityModuleConfiguration.ModuleName}:{nameof(MessagePersistenceOptions)}"
        );

        services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());

        return services;
    }
}
