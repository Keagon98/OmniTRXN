using OmniTrxnService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxnService.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty; // vendor's transaction ID
        public TransactionCategory Category { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public DebitCreditStatus DebitCredit { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public DateTime TransDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public VendorName Vendor { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
