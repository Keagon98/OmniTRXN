using Microsoft.Extensions.Caching.Memory;
using OzowTransactionService.Api.Data;
using OzowTransactionService.Api.Models;

namespace OzowTransactionService.Api.Services
{
    public class TransactionService : ITransactionService
    {

        private readonly ITransactionsRepository _repo;
        private readonly IMemoryCache _cache;
        private static readonly string CacheKey = "products_all";

        public TransactionService(ITransactionsRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<TransactionResponseDTO> GetCustomerTransactionsAsync(string customerId, CancellationToken ct)
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<Transaction> transactions))
            {
                transactions = await _repo.GetAllTransactionsAsync(ct);
                _cache.Set(CacheKey, transactions, TimeSpan.FromMinutes(5));
            }

            var response = new TransactionResponseDTO("101", "cus_ozow_00932", "jclarkson@mail.co.za", transactions.ToList());

            return response;
        }
    }
}
