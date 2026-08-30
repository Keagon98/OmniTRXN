using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface IVendorApiClient
    {
        VendorName Vendor { get; }
        Task<RawVendorResponse> FetchTransactionsAsync(string vendorCustomerId);
    }
}
