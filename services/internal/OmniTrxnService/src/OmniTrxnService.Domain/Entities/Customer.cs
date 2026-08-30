
namespace OmniTrxnService.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerNumber { get; set; } = string.Empty; // e.g., "cust42158"
        public string IdNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public ICollection<VendorCustomerMap> VendorMappings { get; set; } = new List<VendorCustomerMap>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
