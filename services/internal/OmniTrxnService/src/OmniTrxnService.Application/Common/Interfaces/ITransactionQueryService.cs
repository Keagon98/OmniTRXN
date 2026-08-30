using OmniTrxnService.Application.Common.Models;
using OmniTrxnService.Application.DTOs;

namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface ITransactionQueryService
    {
        Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionQueryFilter filter, CancellationToken cancellationToken = default);
    }
}
