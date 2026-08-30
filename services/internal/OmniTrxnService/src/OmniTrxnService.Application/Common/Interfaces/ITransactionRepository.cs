using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Entities;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByTransactionsIdAsync(int id, CancellationToken ct);
        Task<IEnumerable<Transaction>> GetTransactionsByFilterAsync(TransactionQueryFilter filter, CancellationToken ct);
        Task UpsertAsync(IEnumerable<Transaction> transactions);
        Task<bool> ExistsAsync(string transactionId, VendorName vendor);
    }
}
