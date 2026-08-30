using OmniTrxnService.Application.Common.Interfaces;

namespace OmniTrxnService.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OmniTrxnDbContext _context;

        public ITransactionRepository Transactions { get; }
        public IVendorCustomerMapRepository VendorCustomerMaps { get; }

        public UnitOfWork(
            OmniTrxnDbContext context,
            ITransactionRepository transactionRepository,
            IVendorCustomerMapRepository vendorCustomerMapRepository)
        {
            _context = context;
            Transactions = transactionRepository;
            VendorCustomerMaps = vendorCustomerMapRepository;
        }

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
