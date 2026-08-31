using AutoMapper;
using FluentAssertions;
using Moq;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Application.Services;
using OmniTrxnService.Domain.Entities;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxn.Tests.Unit.Services
{
    public class TransactionQueryServiceTests
    {
        [Fact]
        public async Task GetTransactionsAsync_ReturnsPagedResult()
        {
            // Arrange
            var filter = new TransactionQueryFilter { CustomerNumber = "cust42158", Page = 1, PageSize = 10 };
            var transactions = new List<Transaction>
    {
        new Transaction { Id = 1, TransactionId = "tx1", Category = TransactionCategory.Groceries, Amount = 100, Vendor = VendorName.Ozow, CustomerNumber = "cust42158" },
        new Transaction { Id = 2, TransactionId = "tx2", Category = TransactionCategory.Fuel, Amount = 50, Vendor = VendorName.Fnb, CustomerNumber = "cust42158" }
    };

            var repoMock = new Mock<ITransactionRepository>();
            repoMock.Setup(r => r.GetTransactionsByFilterAsync(filter, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<List<TransactionDto>>(It.IsAny<List<Transaction>>()))
                .Returns((List<Transaction> source) => source.Select(t => new TransactionDto { Id = t.Id, TransactionId = t.TransactionId, Category = t.Category.ToString() }).ToList());

            var service = new TransactionQueryService(repoMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetTransactionsAsync(filter);

            // Assert
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }
    }
}
