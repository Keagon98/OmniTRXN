using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Application.DTOs
{
    public class TransactionQueryFilter
    {
        public string? CustomerNumber { get; set; }
        public TransactionCategory? Category { get; set; }
        public DebitCreditStatus? DebitCredit { get; set; }
        public VendorName? Vendor { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
