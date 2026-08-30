
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Entities;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface ITransactionNormalizer
    {
        IEnumerable<Transaction> Normalize(RawVendorResponse rawResponse, VendorName vendor, string customerNumber, int customerId);
    }
}
