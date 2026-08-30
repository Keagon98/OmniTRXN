using Microsoft.EntityFrameworkCore;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Entities;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly OmniTrxnDbContext _context;
        public TransactionRepository(OmniTrxnDbContext context) => _context = context;

        public async Task<Transaction?> GetByTransactionsIdAsync(int id, CancellationToken ct = default) =>
            await _context.Transactions.FindAsync(id, ct);

        public async Task<IEnumerable<Transaction>> GetTransactionsByFilterAsync(TransactionQueryFilter filter, CancellationToken ct = default)
        {
            var query = _context.Transactions.AsNoTracking();

            if (!string.IsNullOrEmpty(filter.CustomerNumber))
                query = query.Where(t => t.CustomerNumber == filter.CustomerNumber);

            if (filter.Category.HasValue)
                query = query.Where(t => t.Category == filter.Category.Value);

            if (filter.DebitCredit.HasValue)
                query = query.Where(t => t.DebitCredit == filter.DebitCredit.Value);

            if (filter.Vendor.HasValue)
                query = query.Where(t => t.Vendor == filter.Vendor.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.TransDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.TransDate <= filter.ToDate.Value);

            query = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);

            return await query.ToListAsync(ct);
        }

        public async Task UpsertAsync(IEnumerable<Transaction> transactions)
        {
            foreach (var txn in transactions)
            {
                var existing = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Vendor == txn.Vendor && t.TransactionId == txn.TransactionId);

                if (existing == null)
                {
                    await _context.Transactions.AddAsync(txn);
                }
                else
                {
                    existing.Category = txn.Category;
                    existing.DebitCredit = txn.DebitCredit;
                    existing.Amount = txn.Amount;
                    existing.Reference = txn.Reference;
                    existing.TransDate = txn.TransDate;
                    existing.Currency = txn.Currency;
                    existing.CustomerId = txn.CustomerId;
                    existing.CustomerNumber = txn.CustomerNumber;
                }
            }
        }

        public async Task<bool> ExistsAsync(string transactionId, VendorName vendor) =>
            await _context.Transactions.AnyAsync(t => t.TransactionId == transactionId && t.Vendor == vendor);
    }
}
