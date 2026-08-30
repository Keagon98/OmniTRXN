using OmniTrxnService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OmniTrxnService.Api.Models
{
    public class TransactionQueryParameters
    {
        public string? CustomerNumber { get; set; }

        public TransactionCategory? Category { get; set; }

        public DebitCreditStatus? DebitCredit { get; set; }

        public VendorName? Vendor { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 50;
    }
}
