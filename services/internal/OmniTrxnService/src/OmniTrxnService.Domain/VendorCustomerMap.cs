using OmniTrxnService.Domain.Entities;
using OmniTrxnService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxnService.Domain
{
    public class VendorCustomerMap
    {
        public int Id { get; set; }
        public string CustomerNumber { get; set; } = string.Empty; // OmniTRXN customer
        public string VendorCustomerNumber { get; set; } = string.Empty; // vendor-specific ID
        public VendorName VendorName { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
