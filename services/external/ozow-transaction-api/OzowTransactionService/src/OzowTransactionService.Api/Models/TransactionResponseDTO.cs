namespace OzowTransactionService.Api.Models
{
    public record TransactionResponseDTO(string MerchandId, string CustomerId, string CustomerEmail, List<Transaction> Transactions);
}
