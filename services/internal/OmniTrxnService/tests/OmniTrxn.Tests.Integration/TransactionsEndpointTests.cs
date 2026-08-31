using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.Common.Models;
using OmniTrxnService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace OmniTrxn.Tests.Integration
{
    public class TransactionsEndpointTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiWebApplicationFactory _factory;

        public TransactionsEndpointTests(ApiWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Helper to trigger the ingestion service manually, since hosted services are removed in tests.
        /// </summary>
        private async Task IngestTransactionsAsync(string customerNumber)
        {
            using var scope = _factory.Services.CreateScope();
            var ingestionService = scope.ServiceProvider.GetRequiredService<ITransactionIngestionService>();
            await ingestionService.IngestAsync(customerNumber);
        }

        [Fact]
        public async Task GetTransactions_WithCustomerNumber_ReturnsSeededAndIngestedData()
        {
            // Arrange: ingest data for our known customer
            await IngestTransactionsAsync("cust42158");

            // Act
            var response = await _client.GetAsync("/api/v1/Transactions?customerNumber=cust42158");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeEmpty();
            result.Items.All(t => t.CustomerNumber == "cust42158").Should().BeTrue();
        }

        [Fact]
        public async Task GetTransactions_WithCategoryFilter_ReturnsOnlyThatCategory()
        {
            // Arrange
            await IngestTransactionsAsync("cust42158");

            // Act
            var response = await _client.GetAsync("/api/v1/Transactions?customerNumber=cust42158&category=Groceries");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result!.Items.Should().NotBeEmpty();
            result.Items.All(t => t.Category == "Groceries").Should().BeTrue();
        }

        [Fact]
        public async Task GetTransactions_WithDebitCreditFilter_ReturnsOnlyDebits()
        {
            // Arrange
            await IngestTransactionsAsync("cust42158");

            // Act
            var response = await _client.GetAsync("/api/v1/Transactions?customerNumber=cust42158&debitCredit=Debit");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result!.Items.Should().NotBeEmpty();
            result.Items.All(t => t.DebitCredit == "Debit").Should().BeTrue();
        }

        [Fact]
        public async Task GetTransactions_WithVendorFilter_ReturnsOnlyThatVendor()
        {
            // Arrange
            await IngestTransactionsAsync("cust42158");

            // Act
            var response = await _client.GetAsync("/api/v1/Transactions?customerNumber=cust42158&vendor=Ozow");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result!.Items.Should().NotBeEmpty();
            result.Items.All(t => t.Vendor == "Ozow").Should().BeTrue();
        }

        [Fact]
        public async Task GetTransactions_WithDateRange_ReturnsTransactionsInRange()
        {
            // Arrange
            await IngestTransactionsAsync("cust42158");

            // Act
            var response = await _client.GetAsync("/api/v1/Transactions?customerNumber=cust42158&fromDate=2026-08-01&toDate=2026-08-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result!.Items.Should().NotBeEmpty();
            result.Items.All(t => t.TransDate >= new DateTime(2026, 8, 1) && t.TransDate <= new DateTime(2026, 8, 31)).Should().BeTrue();
        }

        [Fact]
        public async Task GetTransactions_WithoutFilters_ReturnsAllTransactions()
        {
            // Arrange
            await IngestTransactionsAsync("cust42158");

            // Act
            var response = await _client.GetAsync("/api/v1/Transactions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result!.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetTransactions_WithPagination_ReturnsOnlyPageSizeItems()
        {
            // Arrange
            await IngestTransactionsAsync("cust42158");

            // Act
            var response = await _client.GetAsync("/api/v1/Transactions?page=1&pageSize=5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result!.Items.Should().HaveCountLessOrEqualTo(5);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().BeGreaterThan(0);
        }
    }
}
