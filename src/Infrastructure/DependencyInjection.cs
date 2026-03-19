using CleanArchitecture.VerticalSlice.Application.Abstractions.Data;
using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Infrastructure.Database;
using CleanArchitecture.VerticalSlice.Infrastructure.Interceptors;
using CleanArchitecture.VerticalSlice.Infrastructure.Repository;
using CleanArchitecture.VerticalSlice.Infrastructure.Authentication;
using CleanArchitecture.VerticalSlice.Infrastructure.FeatureManagement;
using CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Configuration;
using CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Producers;
using CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace CleanArchitecture.VerticalSlice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
            options.UseNpgsql(configuration.GetConnectionString("connection"));
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddForgeRockAuthentication(configuration);

        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        services.AddScoped<IFeatureService, FeatureService>();

        // Kafka configuration
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        services.AddTransient(typeof(IKafkaConsumer<>), typeof(KafkaConsumer<>));

        return services;
    }
}
