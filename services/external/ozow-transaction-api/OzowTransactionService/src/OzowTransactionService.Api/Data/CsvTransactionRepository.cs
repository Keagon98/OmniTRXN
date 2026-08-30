using CsvHelper;
using OzowTransactionService.Api.Models;
using System.Globalization;

namespace OzowTransactionService.Api.Data
{
    public class CsvTransactionRepository : ITransactionsRepository
    {
        private readonly string _csvPath;

        public CsvTransactionRepository(IConfiguration config)
        {
            _csvPath = config["Csv:TransactionsFile"] ?? throw new ArgumentNullException(nameof(_csvPath), "CSV file not found");
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken ct = default)
        {
            using var reader = new StreamReader(_csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<Transaction>().ToList();
            return await Task.FromResult(records);
        }
    }
}
