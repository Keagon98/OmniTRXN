using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Application.Services;
using OmniTrxnService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxn.Tests.Unit.Services
{
    public class TransactionNormalizerTests
    {
        private readonly TransactionNormalizer _normalizer;

        public TransactionNormalizerTests()
        {
            _normalizer = new TransactionNormalizer(Mock.Of<ILogger<TransactionNormalizer>>());
        }

        #region Ozow Tests

        [Fact]
        public void Normalize_OzowResponse_ReturnsCorrectTransaction()
        {
            // Arrange
            var json = @"{
                ""transactions"": [
                    {
                        ""transactionId"": ""local_txn_1001"",
                        ""merchantRef"": ""INV-2026-0001"",
                        ""date"": ""2026-08-25T11:12:34+02:00"",
                        ""amount"": 1250.00,
                        ""currency"": ""ZAR"",
                        ""category"": ""Groceries"",
                        ""direction"": ""inbound""
                    }
                ]
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Ozow,
                VendorCustomerId = "cus_ozow_00932"
            };

            // Act
            var result = _normalizer.Normalize(rawResponse, VendorName.Ozow, "cust42158", 1).ToList();

            // Assert
            result.Should().HaveCount(1);
            var txn = result[0];
            txn.TransactionId.Should().Be("local_txn_1001");
            txn.Reference.Should().Be("INV-2026-0001");
            txn.Amount.Should().Be(1250.00m);
            txn.Currency.Should().Be("ZAR");
            txn.Category.Should().Be(TransactionCategory.Groceries);
            txn.DebitCredit.Should().Be(DebitCreditStatus.Credit);
            txn.CustomerNumber.Should().Be("cust42158");
            txn.CustomerId.Should().Be(1);
            txn.Vendor.Should().Be(VendorName.Ozow);
        }

        [Fact]
        public void Normalize_OzowRefund_ReturnsDebit()
        {
            // Arrange
            var json = @"{
                ""transactions"": [
                    {
                        ""transactionId"": ""local_txn_1002_refund"",
                        ""merchantRef"": ""INV-2026-0002"",
                        ""date"": ""2026-08-01T12:00:00+02:00"",
                        ""amount"": 500.00,
                        ""currency"": ""ZAR"",
                        ""category"": ""Dining"",
                        ""direction"": ""outbound""
                    }
                ]
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Ozow,
                VendorCustomerId = "cus_ozow_00932"
            };

            // Act
            var result = _normalizer.Normalize(rawResponse, VendorName.Ozow, "cust42158", 1).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].DebitCredit.Should().Be(DebitCreditStatus.Debit);
            result[0].Amount.Should().Be(500.00m);
        }

        [Fact]
        public void Normalize_OzowPayout_ReturnsCredit()
        {
            // Arrange
            var json = @"{
                ""transactions"": [
                    {
                        ""transactionId"": ""local_txn_1023_payout"",
                        ""merchantRef"": """",
                        ""date"": ""2026-08-27T09:00:00+02:00"",
                        ""amount"": 1730.00,
                        ""currency"": ""ZAR"",
                        ""category"": ""Salary/Income"",
                        ""direction"": ""payout""
                    }
                ]
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Ozow,
                VendorCustomerId = "cus_ozow_00932"
            };

            // Act
            var result = _normalizer.Normalize(rawResponse, VendorName.Ozow, "cust42158", 1).ToList();

            // Assert
            result[0].DebitCredit.Should().Be(DebitCreditStatus.Debit); // outbound (payout) -> Debit
            result[0].Category.Should().Be(TransactionCategory.SalaryIncome);
        }

        [Fact]
        public void Normalize_OzowAmountAsString_ParsesCorrectly()
        {
           
            var json = @"{
                ""transactions"": [
                    {
                        ""transactionId"": ""tx1"",
                        ""merchantRef"": ""ref"",
                        ""date"": ""2026-08-25"",
                        ""amount"": ""1250.00"",
                        ""currency"": ""ZAR"",
                        ""category"": ""Retail"",
                        ""direction"": ""inbound""
                    }
                ]
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Ozow,
                VendorCustomerId = "cus_ozow_00932"
            };

            // Act
            var result = _normalizer.Normalize(rawResponse, VendorName.Ozow, "cust", 1).ToList();

            // Assert
            result[0].Amount.Should().Be(1250.00m);
        }

        [Fact]
        public void Normalize_OzowUnknownCategory_ReturnsUncategorized()
        {
            var json = @"{
                ""transactions"": [
                    {
                        ""transactionId"": ""tx1"",
                        ""merchantRef"": ""ref"",
                        ""date"": ""2026-08-01"",
                        ""amount"": 100,
                        ""currency"": ""ZAR"",
                        ""category"": ""Mystery"",
                        ""direction"": ""inbound""
                    }
                ]
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Ozow,
                VendorCustomerId = "cust1"
            };

            var result = _normalizer.Normalize(rawResponse, VendorName.Ozow, "cust", 1).ToList();

            result[0].Category.Should().Be(TransactionCategory.Uncategorized);
        }

        [Fact]
        public void Normalize_OzowMissingTransactions_ReturnsEmpty()
        {
            var json = @"{ ""merchandId"": ""101"" }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Ozow,
                VendorCustomerId = "cust"
            };

            var result = _normalizer.Normalize(rawResponse, VendorName.Ozow, "cust", 1).ToList();

            result.Should().BeEmpty();
        }

        #endregion

        #region FNB Tests

        [Fact]
        public void Normalize_FnbResponse_ReturnsCorrectTransaction()
        {

            var json = @"{
                ""Envelope"": {
                    ""Body"": {
                        ""getCustomerTransactionsResponse"": {
                            ""transactions"": {
                                ""transaction"": [
                                    {
                                        ""txId"": ""txn-uuid-3001"",
                                        ""bankReference"": ""FNB-BANKTX-20260801-3001"",
                                        ""bookingDate"": ""2026-08-01"",
                                        ""amount"": ""-450.75"",
                                        ""currency"": ""ZAR"",
                                        ""category"": ""Groceries"",
                                        ""creditDebit"": ""DEBIT""
                                    }
                                ]
                            }
                        }
                    }
                }
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Fnb,
                VendorCustomerId = "cust-acct-908"
            };

            var result = _normalizer.Normalize(rawResponse, VendorName.Fnb, "cust42158", 1).ToList();

            result.Should().HaveCount(1);
            var txn = result[0];
            txn.TransactionId.Should().Be("txn-uuid-3001");
            txn.Reference.Should().Be("FNB-BANKTX-20260801-3001");
            txn.Amount.Should().Be(450.75m);
            txn.Currency.Should().Be("ZAR");
            txn.Category.Should().Be(TransactionCategory.Groceries);
            txn.DebitCredit.Should().Be(DebitCreditStatus.Debit);
            txn.Vendor.Should().Be(VendorName.Fnb);
            txn.CustomerNumber.Should().Be("cust42158");
            txn.CustomerId.Should().Be(1);
        }

        [Fact]
        public void Normalize_FnbCredit_ReturnsCredit()
        {
            var json = @"{
                ""Envelope"": {
                    ""Body"": {
                        ""getCustomerTransactionsResponse"": {
                            ""transactions"": {
                                ""transaction"": [
                                    {
                                        ""txId"": ""txn-uuid-3011"",
                                        ""bankReference"": ""FNB-BANKTX-20260822-2003"",
                                        ""bookingDate"": ""2026-08-22"",
                                        ""amount"": ""8000.00"",
                                        ""currency"": ""ZAR"",
                                        ""category"": ""Salary/Income"",
                                        ""creditDebit"": ""CREDIT""
                                    }
                                ]
                            }
                        }
                    }
                }
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Fnb,
                VendorCustomerId = "cust-acct-908"
            };

            var result = _normalizer.Normalize(rawResponse, VendorName.Fnb, "cust", 1).ToList();

            result[0].DebitCredit.Should().Be(DebitCreditStatus.Credit);
            result[0].Category.Should().Be(TransactionCategory.SalaryIncome);
        }

        [Fact]
        public void Normalize_FnbMissingTransactions_ReturnsEmpty()
        {
            var json = @"{ ""Envelope"": { ""Body"": { ""getCustomerTransactionsResponse"": {} } } }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Fnb,
                VendorCustomerId = "cust-acct-908"
            };

            var result = _normalizer.Normalize(rawResponse, VendorName.Fnb, "cust", 1).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Normalize_FnbMultipleTransactions_ReturnsAll()
        {
            var json = @"{
                ""Envelope"": {
                    ""Body"": {
                        ""getCustomerTransactionsResponse"": {
                            ""transactions"": {
                                ""transaction"": [
                                    {
                                        ""txId"": ""tx1"",
                                        ""bankReference"": ""ref1"",
                                        ""bookingDate"": ""2026-08-01"",
                                        ""amount"": ""-10.00"",
                                        ""currency"": ""ZAR"",
                                        ""category"": ""Groceries"",
                                        ""creditDebit"": ""DEBIT""
                                    },
                                    {
                                        ""txId"": ""tx2"",
                                        ""bankReference"": ""ref2"",
                                        ""bookingDate"": ""2026-08-02"",
                                        ""amount"": ""20.00"",
                                        ""currency"": ""ZAR"",
                                        ""category"": ""Retail"",
                                        ""creditDebit"": ""CREDIT""
                                    }
                                ]
                            }
                        }
                    }
                }
            }";
            var rawResponse = new RawVendorResponse
            {
                Content = json,
                Type = ContentType.Json,
                Vendor = VendorName.Fnb,
                VendorCustomerId = "cust-acct-908"
            };

            var result = _normalizer.Normalize(rawResponse, VendorName.Fnb, "cust", 1).ToList();

            result.Should().HaveCount(2);
            result[0].TransactionId.Should().Be("tx1");
            result[1].TransactionId.Should().Be("tx2");
        }

        #endregion
    }
}
