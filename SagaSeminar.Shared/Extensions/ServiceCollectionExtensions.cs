using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service;
using SagaSeminar.Shared.Service.Interfaces;
using SagaSeminar.Shared.Services;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogClient(this IServiceCollection services,
            string defaultColor, string source)
        {
            return services.AddTransient<ILogClient>(provider =>
            {
                IOptions<ApiInfo> apiOptions = provider.GetRequiredService<IOptions<ApiInfo>>();

                return new LogClient(apiOptions, defaultColor, source);
            });
        }

        public static IServiceCollection AddServiceClients(this IServiceCollection services)
        {
            services.AddHttpClient<IOrderClient, OrderClient>((provider, http) =>
            {
                IOptions<ApiInfo> apiOptions = provider.GetRequiredService<IOptions<ApiInfo>>();

                http.BaseAddress = new Uri(apiOptions.Value.OrderApiBase);
            });

            services.AddHttpClient<IPaymentClient, PaymentClient>((provider, http) =>
            {
                IOptions<ApiInfo> apiOptions = provider.GetRequiredService<IOptions<ApiInfo>>();

                http.BaseAddress = new Uri(apiOptions.Value.PaymentApiBase);
            });

            services.AddHttpClient<IGlobalClient, GlobalClient>((provider, http) =>
            {
                IOptions<ApiInfo> apiOptions = provider.GetRequiredService<IOptions<ApiInfo>>();

                http.BaseAddress = new Uri(apiOptions.Value.GlobalApiBase);
            });

            services.AddHttpClient<IInventoryClient, InventoryClient>((provider, http) =>
            {
                IOptions<ApiInfo> apiOptions = provider.GetRequiredService<IOptions<ApiInfo>>();

                http.BaseAddress = new Uri(apiOptions.Value.InventoryApiBase);
            });

            services.AddHttpClient<IShippingClient, ShippingClient>((provider, http) =>
            {
                IOptions<ApiInfo> apiOptions = provider.GetRequiredService<IOptions<ApiInfo>>();

                http.BaseAddress = new Uri(apiOptions.Value.ShippingApiBase);
            });

            return services;
        }
    }
}
