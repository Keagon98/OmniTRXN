using OmniTrxnService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniTrxnService.Application.Common.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByCustomerNumberAsync(string customerNumber);
        Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
        void Update(Customer customer);
        void Remove(Customer customer);
    }
}
