using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaSeminar.Shared.Service.Repositories;
using SagaSeminar.Shared.Service.Repositories.Interfaces;
using SagaSeminar.Shared.Service.Services;
using SagaSeminar.Shared.Service.Services.Interfaces;
using SagaSeminar.Shared.Service.Utils;

namespace SagaSeminar.Shared.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        }

        public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            return services.AddScoped<IUnitOfWork<TDbContext>, UnitOfWork<TDbContext>>();
        }

        public static IServiceCollection AddRedis(this IServiceCollection services)
        {
            return services.AddSingleton(e =>
            {
                IConfiguration configuration = e.GetRequiredService<IConfiguration>();

                string endpoint = configuration.GetValue<string>("Redis:Endpoint");
                bool allowAdmin = configuration.GetValue<bool>("Redis:AllowAdmin");

                return RedisHelper.GetConnectionMultiplexer(endpoint, allowAdmin);
            });
        }

        public static IServiceCollection AddGlobalConfigServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IGlobalConfigService, GlobalConfigService>()
                .AddSingleton<IGlobalConfigReader, GlobalConfigReader>();
        }

        public static IServiceCollection AddGlobalConfigReader(this IServiceCollection services)
        {
            return services
                .AddSingleton<IGlobalConfigReader, GlobalConfigReader>();
        }

        public static IServiceCollection AddCorsDefaults(this IServiceCollection services)
        {
            return services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });
        }
    }
}
