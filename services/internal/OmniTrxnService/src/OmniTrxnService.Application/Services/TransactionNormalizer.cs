using Microsoft.Extensions.Logging;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Entities;
using OmniTrxnService.Domain.Enums;
using System.Globalization;
using System.Text.Json;

namespace OmniTrxnService.Application.Services
{
    public class TransactionNormalizer : ITransactionNormalizer
    {
        private readonly ILogger<TransactionNormalizer> _logger;

        public TransactionNormalizer(ILogger<TransactionNormalizer> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Transaction> Normalize(RawVendorResponse rawResponse, VendorName vendor, string customerNumber, int customerId)
        {
            using var doc = JsonDocument.Parse(rawResponse.Content);
            var root = doc.RootElement;

            var transactionsElement = FindTransactionsArray(root);
            if (transactionsElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("No transactions array found in response from vendor {Vendor}", vendor);
                yield break;
            }

            foreach (var item in transactionsElement.EnumerateArray())
            {
                var txn = new Transaction
                {
                    Vendor = vendor,
                    CustomerNumber = customerNumber,
                    CustomerId = customerId
                };

                if (vendor == VendorName.Ozow)
                {
                    txn.TransactionId = item.GetProperty("transactionId").GetString() ?? "";
                    txn.Reference = item.GetProperty("merchantRef").GetString() ?? "";
                    txn.TransDate = DateTime.Parse(item.GetProperty("date").GetString()!);

                    var amountElement = item.GetProperty("amount");
                    if (amountElement.ValueKind == JsonValueKind.String)
                    {
                        txn.Amount = decimal.Parse(amountElement.GetString()!, CultureInfo.InvariantCulture);
                    }
                    else if (amountElement.ValueKind == JsonValueKind.Number)
                    {
                        txn.Amount = amountElement.GetDecimal();
                    }
                    else
                    {
                        txn.Amount = 0;
                    }

                    txn.Currency = item.GetProperty("currency").GetString() ?? "ZAR";
                    txn.Category = MapCategory(item.GetProperty("category").GetString());
                    var direction = item.GetProperty("direction").GetString();
                    txn.DebitCredit = direction == "inbound" ? DebitCreditStatus.Credit : DebitCreditStatus.Debit;
                }
                else 
                {
                    txn.TransactionId = item.GetProperty("txId").GetString() ?? "";
                    txn.Reference = item.GetProperty("bankReference").GetString() ?? "";
                    txn.TransDate = DateTime.Parse(item.GetProperty("bookingDate").GetString());
                    txn.Amount = Math.Abs(decimal.Parse(item.GetProperty("amount").GetString()!, CultureInfo.InvariantCulture));
                    txn.Currency = item.GetProperty("currency").GetString() ?? "ZAR";
                    txn.Category = MapCategory(item.GetProperty("category").GetString());
                    var creditDebit = item.GetProperty("creditDebit").GetString();
                    txn.DebitCredit = creditDebit == "CREDIT" ? DebitCreditStatus.Credit : DebitCreditStatus.Debit;
                }

                yield return txn;
            }
        }

        private JsonElement FindTransactionsArray(JsonElement root)
        {
            if (root.TryGetProperty("transactions", out var arr) && arr.ValueKind == JsonValueKind.Array)
                return arr;

            return SearchRecursive(root, "transaction");
        }

        private JsonElement SearchRecursive(JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Name.EndsWith(propertyName) && prop.Value.ValueKind == JsonValueKind.Array)
                        return prop.Value;
                    var found = SearchRecursive(prop.Value, propertyName);
                    if (found.ValueKind != JsonValueKind.Undefined)
                        return found;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var found = SearchRecursive(item, propertyName);
                    if (found.ValueKind != JsonValueKind.Undefined)
                        return found;
                }
            }
            return default;
        }

        private TransactionCategory MapCategory(string? category)
        {
            if (string.IsNullOrEmpty(category)) return TransactionCategory.Uncategorized;
            var normalized = category.Replace(" ", "").Replace("/", "");
            if (Enum.TryParse<TransactionCategory>(normalized, true, out var result))
                return result;
            return TransactionCategory.Uncategorized;
        }
    }
}
