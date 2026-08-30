using OmniTrxnService.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface IVendorCustomerMapRepository
    {
        Task<VendorCustomerMap?> GetByIdAsync(int id);
        Task<IEnumerable<VendorCustomerMap>> GetByCustomerNumberAsync(string customerNumber);
        Task<IEnumerable<VendorCustomerMap>> GetAllAsync();
        Task AddAsync(VendorCustomerMap vendorCustomerMap, CancellationToken cancellationToken = default);
        void Update(VendorCustomerMap vendorCustomerMap);
        void Remove(VendorCustomerMap vendorCustomerMap);
    }
}
