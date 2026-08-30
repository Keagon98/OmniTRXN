using Microsoft.EntityFrameworkCore;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxnService.Infrastructure.Persistence.Repositories
{
    public class VendorCustomerMapRepository : IVendorCustomerMapRepository
    {
        private readonly OmniTrxnDbContext _context;

        public VendorCustomerMapRepository(OmniTrxnDbContext context)
        {
            _context = context;
        }

        public async Task<VendorCustomerMap?> GetByIdAsync(int id)
        {
            return await _context.VendorCustomerMaps.FindAsync(id);
        }

        public async Task<IEnumerable<VendorCustomerMap>> GetByCustomerNumberAsync(string customerNumber)
        {
            return await _context.VendorCustomerMaps
                .AsNoTracking()
                .Where(v => v.CustomerNumber == customerNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<VendorCustomerMap>> GetAllAsync()
        {
            return await _context.VendorCustomerMaps
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(VendorCustomerMap vendorCustomerMap, CancellationToken cancellationToken = default)
        {
            await _context.VendorCustomerMaps.AddAsync(vendorCustomerMap, cancellationToken);
        }

        public void Update(VendorCustomerMap vendorCustomerMap)
        {
            _context.VendorCustomerMaps.Update(vendorCustomerMap);
        }

        public void Remove(VendorCustomerMap vendorCustomerMap)
        {
            _context.VendorCustomerMaps.Remove(vendorCustomerMap);
        }
    }
}
