using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OmniTrxnService.Domain;
using OmniTrxnService.Domain.Entities;
using OmniTrxnService.Domain.Enums;

namespace OmniTrxnService.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OmniTrxnDbContext>();

            // Ensure database is created (or use migrations)
            await context.Database.MigrateAsync();

            // Seed Customer if not exists
            if (!await context.Customers.AnyAsync(c => c.CustomerNumber == "cust42158"))
            {
                var customer = new Customer
                {
                    CustomerNumber = "cust42158",
                    IdNumber = "9505125001080",
                    FirstName = "Jeremy",
                    LastName = "Clarkson",
                    ContactNumber = "0765436785",
                    EmailAddress = "jclarkson@mail.co.za"
                };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();

                // Add vendor mappings
                context.VendorCustomerMaps.AddRange(
                    new VendorCustomerMap { CustomerNumber = customer.CustomerNumber, VendorCustomerNumber = "cus_ozow_00932", VendorName = VendorName.Ozow, CustomerId = customer.Id },
                    new VendorCustomerMap { CustomerNumber = customer.CustomerNumber, VendorCustomerNumber = "cust-acct-908", VendorName = VendorName.Fnb, CustomerId = customer.Id }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
