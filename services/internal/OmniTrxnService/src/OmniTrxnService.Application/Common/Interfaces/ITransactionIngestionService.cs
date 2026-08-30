
namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface ITransactionIngestionService
    {
        Task IngestAsync(string customerNumber, CancellationToken cancellationToken = default);
    }
}
