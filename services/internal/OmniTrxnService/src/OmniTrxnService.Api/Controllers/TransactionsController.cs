using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniTrxnService.Api.Models;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.Common.Models;
using OmniTrxnService.Application.DTOs;

namespace OmniTrxnService.Api.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionQueryService _queryService;

        public TransactionsController(ITransactionQueryService queryService)
        {
            _queryService = queryService;
        }

        /// <summary>
        /// Retrieves transactions with optional filters.
        /// </summary>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionQueryParameters parameters)
        {
            var filter = new TransactionQueryFilter
            {
                CustomerNumber = parameters.CustomerNumber,
                Category = parameters.Category,
                DebitCredit = parameters.DebitCredit,
                Vendor = parameters.Vendor,
                FromDate = parameters.FromDate,
                ToDate = parameters.ToDate,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            };
            var result = await _queryService.GetTransactionsAsync(filter);
            return Ok(result);
        }
    }
}
