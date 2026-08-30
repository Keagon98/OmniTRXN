using OzowTransactionService.Api.Models;

namespace OzowTransactionService.Api.Services
{
    public interface ITransactionService
    {
        Task<TransactionResponseDTO> GetCustomerTransactionsAsync(string customerId, CancellationToken ct = default);
    }
}
