using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.Services;
using OmniTrxnService.Infrastructure.Adapters;
using OmniTrxnService.Infrastructure.BackgroundJobs;
using OmniTrxnService.Infrastructure.ExternalServices;
using OmniTrxnService.Infrastructure.Persistence;
using OmniTrxnService.Infrastructure.Persistence.Repositories;

namespace OmniTrxnService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database context
            services.AddDbContext<OmniTrxnDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("OmniTrxnDb"),
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(OmniTrxnDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IVendorCustomerMapRepository, VendorCustomerMapRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // API Gateway HttpClient
            services.AddHttpClient<ApiGatewayClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["ApiGateway:BaseUrl"] ?? throw new ArgumentNullException("API Gateway url not found"));
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Vendor clients
            services.AddScoped<IVendorApiClient, OzowApiClient>();
            services.AddScoped<IVendorApiClient, FnbSoapClient>();

            // Adapter and Normalizer
            services.AddScoped<IXmlToJsonAdapter, XmlToJsonAdapter>();
            services.AddScoped<ITransactionNormalizer, TransactionNormalizer>();

            // Background polling service
            services.AddHostedService<TransactionPollingService>();

            return services;
        }
    }
}
