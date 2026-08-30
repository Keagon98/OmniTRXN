using Microsoft.Extensions.Logging;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.DTOs;

namespace OmniTrxnService.Application.Services
{
    public class TransactionIngestionService : ITransactionIngestionService
    {
        private readonly IEnumerable<IVendorApiClient> _vendorClients;
        private readonly IXmlToJsonAdapter _xmlToJsonAdapter;
        private readonly ITransactionNormalizer _normalizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<TransactionIngestionService> _logger;

        public TransactionIngestionService(
            IEnumerable<IVendorApiClient> vendorClients,
            IXmlToJsonAdapter xmlToJsonAdapter,
            ITransactionNormalizer normalizer,
            IUnitOfWork unitOfWork,
            ICustomerRepository customerRepository,
            ILogger<TransactionIngestionService> logger)
        {
            _vendorClients = vendorClients;
            _xmlToJsonAdapter = xmlToJsonAdapter;
            _normalizer = normalizer;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task IngestAsync(string customerNumber, CancellationToken cancellationToken = default)
        {
            var customer = await _customerRepository.GetByCustomerNumberAsync(customerNumber);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerNumber} not found.", customerNumber);
                return;
            }

            var vendorMappings = await _unitOfWork.VendorCustomerMaps.GetByCustomerNumberAsync(customerNumber);
            foreach (var mapping in vendorMappings)
            {
                var client = _vendorClients.FirstOrDefault(c => c.Vendor == mapping.VendorName);
                if (client == null)
                {
                    _logger.LogWarning("No client configured for vendor {Vendor}", mapping.VendorName);
                    continue;
                }

                try
                {
                    var rawResponse = await client.FetchTransactionsAsync(mapping.VendorCustomerNumber);
                   
                    if (rawResponse.Type == ContentType.Xml)
                    {
                        rawResponse.Content = _xmlToJsonAdapter.Convert(rawResponse.Content);
                        rawResponse.Type = ContentType.Json;
                    }

                    var transactions = _normalizer.Normalize(rawResponse, mapping.VendorName, customerNumber, customer.Id);
                    await _unitOfWork.Transactions.UpsertAsync(transactions);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ingesting from vendor {Vendor} for customer {Customer}", mapping.VendorName, customerNumber);
                }
            }
            await _unitOfWork.CompleteAsync();
        }
    }
}
