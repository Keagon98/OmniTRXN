using Microsoft.Extensions.Logging;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Infrastructure.ExternalServices
{
    public class OzowApiClient : IVendorApiClient
    {
        private readonly ApiGatewayClient _gatewayClient;
        private readonly ILogger<OzowApiClient> _logger;
        public VendorName Vendor => VendorName.Ozow;

        public OzowApiClient(ApiGatewayClient gatewayClient, ILogger<OzowApiClient> logger)
        {
            _gatewayClient = gatewayClient;
            _logger = logger;
        }

        public async Task<RawVendorResponse> FetchTransactionsAsync(string vendorCustomerId)
        {
            // The gateway forwards to the actual Ozow API.
            var relativeUrl = $"external/rest/api/v1/Transactions/{vendorCustomerId}";
            var json = await _gatewayClient.GetAsync(relativeUrl);
            return new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Ozow,
                VendorCustomerId = vendorCustomerId
            };
        }
    }
}
