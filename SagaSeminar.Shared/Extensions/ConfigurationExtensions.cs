using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Extensions
{
    public static class ConfigurationExtensions
    {
        public static ApiInfo ConfigureApiInfo(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = Constants.ConfigurationSections.ApiInfo)
        {
            ApiInfo apiInfo = configuration.GetSection(section).Get<ApiInfo>();

            services.Configure<ApiInfo>(opt =>
            {
                opt.PaymentApiBase = apiInfo.PaymentApiBase;
                opt.OrderApiBase = apiInfo.OrderApiBase;
                opt.GlobalApiBase = apiInfo.GlobalApiBase;
                opt.InventoryApiBase = apiInfo.InventoryApiBase;
                opt.ShippingApiBase = apiInfo.ShippingApiBase;
            });

            return apiInfo;
        }
    }
}
