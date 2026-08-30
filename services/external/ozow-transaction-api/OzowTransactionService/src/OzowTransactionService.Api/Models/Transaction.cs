using CsvHelper.Configuration.Attributes;

namespace OzowTransactionService.Api.Models
{
    public class Transaction
    {
        [Name("transaction_id")]
        public required string TransactionId { get; set; }

        [Name("ozow_payment_id")]
        public required string OzowPaymentId { get; set; }

        [Name("merchant_reference")]
        public required string MerchantRef { get; set; }

        [Name("date")]
        public required DateTime Date { get; set; }

        [Name("status")]
        public string? Status { get; set; }

        [Name("type")]
        public required string Type { get; set; }

        [Name("direction")]
        public required string Direction { get; set; }

        [Name("amount")]
        public required decimal Amount { get; set; }

        [Name("currency")]
        public required string Currency { get; set; }

        [Name("category")]
        public required string Category { get; set; }
    }
}
