
namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITransactionRepository Transactions { get; }
        IVendorCustomerMapRepository VendorCustomerMaps { get; }
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
