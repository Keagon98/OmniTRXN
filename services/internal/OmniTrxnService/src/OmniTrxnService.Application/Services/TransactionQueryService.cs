using AutoMapper;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.Common.Models;
using OmniTrxnService.Application.DTOs;

namespace OmniTrxnService.Application.Services
{
    public class TransactionQueryService : ITransactionQueryService
    {
        private readonly ITransactionRepository _repository;
        private readonly IMapper _mapper;

        public TransactionQueryService(ITransactionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionQueryFilter filter, CancellationToken cancellationToken = default)
        {

            var allMatching = await _repository.GetTransactionsByFilterAsync(filter, cancellationToken);
            var totalCount = allMatching.Count();

            var pagedItems = allMatching
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<TransactionDto>>(pagedItems);

            return new PagedResult<TransactionDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }
}
