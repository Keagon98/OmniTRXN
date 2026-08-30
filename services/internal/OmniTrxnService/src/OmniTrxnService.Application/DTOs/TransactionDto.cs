using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxnService.Application.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string DebitCredit { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public DateTime TransDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
    }
}
