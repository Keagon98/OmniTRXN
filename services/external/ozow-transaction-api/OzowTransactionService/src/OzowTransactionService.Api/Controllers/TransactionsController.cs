using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OzowTransactionService.Api.Attributes;
using OzowTransactionService.Api.Models;
using OzowTransactionService.Api.Services;

namespace OzowTransactionService.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Gets a list of customer transactions.
        /// </summary>
        /// <returns>Customer transaction list</returns>
        [HttpGet("{customerId}"), BasicAuthorization]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(TransactionResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerTransactions(string customerId, CancellationToken ct = default)
        {
            if (customerId == null) return BadRequest("Please provide a valid customer id");

            if (!customerId.Equals("cus_ozow_00932")) return NotFound($"Customer {customerId} not found");

            var result = await _transactionService.GetCustomerTransactionsAsync(customerId, ct);

            if (result == null) return NotFound();

            return Ok(result);
        }
    }
}
