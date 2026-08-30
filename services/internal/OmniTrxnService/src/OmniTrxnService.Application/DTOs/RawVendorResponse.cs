using OmniTrxnService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxnService.Application.DTOs
{
    public class RawVendorResponse
    {
        public string Content { get; set; } = string.Empty;
        public ContentType Type { get; set; }
        public VendorName Vendor { get; set; }
        public string VendorCustomerId { get; set; } = string.Empty;
    }

    public enum ContentType
    {
        Json,
        Xml
    }
}
