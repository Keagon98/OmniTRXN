using OzowTransactionService.Api.Models;

namespace OzowTransactionService.Api.Data
{
    public interface ITransactionsRepository
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken ct = default);
    }
}
