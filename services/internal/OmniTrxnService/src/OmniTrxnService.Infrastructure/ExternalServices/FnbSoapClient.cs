using Microsoft.Extensions.Logging;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Infrastructure.ExternalServices
{
    public class FnbSoapClient : IVendorApiClient
    {
        private readonly ApiGatewayClient _gatewayClient;
        private readonly ILogger<FnbSoapClient> _logger;
        public VendorName Vendor => VendorName.Fnb;

        public FnbSoapClient(ApiGatewayClient gatewayClient, ILogger<FnbSoapClient> logger)
        {
            _gatewayClient = gatewayClient;
            _logger = logger;
        }

        public async Task<RawVendorResponse> FetchTransactionsAsync(string vendorCustomerId)
        {
            var soapEnvelope = BuildSoapRequest(vendorCustomerId);
            var xmlResponse = await _gatewayClient.PostSoapAsync("external/soap/ws/CustomerTransactions.wsdl", soapEnvelope);
            return new RawVendorResponse
            {
                Content = xmlResponse,
                Type = ContentType.Xml,
                Vendor = VendorName.Fnb,
                VendorCustomerId = vendorCustomerId
            };
        }

        private string BuildSoapRequest(string accountId)
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tran=""http://transactions.fnb.soapservice.co.za"">
                        <soapenv:Header/>
                        <soapenv:Body>
                            <tran:getCustomerTransactionsRequest>
                                <tran:accountId>{accountId}</tran:accountId>
                            </tran:getCustomerTransactionsRequest>
                        </soapenv:Body>
                    </soapenv:Envelope>";
        }
    }
}
