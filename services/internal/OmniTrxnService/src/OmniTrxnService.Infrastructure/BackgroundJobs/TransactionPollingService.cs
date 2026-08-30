using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniTrxnService.Application.Common.Interfaces;

namespace OmniTrxnService.Infrastructure.BackgroundJobs
{
    public class TransactionPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransactionPollingService> _logger;
        private readonly IConfiguration _configuration;

        public TransactionPollingService(IServiceScopeFactory scopeFactory, ILogger<TransactionPollingService> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Transaction Polling Service started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
                    var ingestionService = scope.ServiceProvider.GetRequiredService<ITransactionIngestionService>();

                    var customers = await customerRepo.GetAllAsync();
                    foreach (var customer in customers)
                    {
                        await ingestionService.IngestAsync(customer.CustomerNumber, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during polling cycle.");
                }

                var intervalMinutes = _configuration.GetValue<int>("Polling:IntervalMinutes", 60);
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }
    }
}
