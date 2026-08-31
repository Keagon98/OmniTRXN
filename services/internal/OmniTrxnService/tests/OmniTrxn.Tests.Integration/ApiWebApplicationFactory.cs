using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.DTOs;
using OmniTrxnService.Domain.Enums;
using Testcontainers.MsSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OmniTrxn.Tests.Integration
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;
        private string _dbConnectionString = string.Empty;

        public ApiWebApplicationFactory()
        {
            _dbContainer = new MsSqlBuilder()
               .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
               .WithPassword("%adMin123$")
               .WithEnvironment("ACCEPT_EULA", "Y")
               .WithEnvironment("MSSQL_PID", "Developer")
               .WithEnvironment("MSSQL_MEMORY_LIMIT_MB", "3072")
               .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
               .Build();
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _dbContainer.StartAsync();
                _dbConnectionString = _dbContainer.GetConnectionString() + ";Encrypt=False;ConnectRetryCount=3;ConnectRetryInterval=10";
            }
            catch (Exception ex)
            {
                var logs = await _dbContainer.GetLogsAsync();
                Console.WriteLine(logs);
                throw;
            }

        }

        public async Task DisposeAsync()
        {
            if (_dbContainer != null)
            {
                await _dbContainer.DisposeAsync();
            }
            base.Dispose();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:OmniTrxnDb"] = _dbConnectionString,
                    ["Polling:IntervalMinutes"] = "100000"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHostedService>();

                services.RemoveAll<IVendorApiClient>();
                services.AddScoped<IVendorApiClient>(sp => new FakeOzowApiClient());
                services.AddScoped<IVendorApiClient>(sp => new FakeFnbSoapClient());

                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("TestScheme")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        }

        private class FakeOzowApiClient : IVendorApiClient
        {
            public VendorName Vendor => VendorName.Ozow;
            public Task<RawVendorResponse> FetchTransactionsAsync(string vendorCustomerId)
            {
                var json = @"{
                    ""transactions"": [
                        {
                            ""transactionId"": ""local_txn_1001"",
                            ""merchantRef"": ""INV-2026-0001"",
                            ""date"": ""2026-08-25T11:12:34+02:00"",
                            ""amount"": 1250.00,
                            ""currency"": ""ZAR"",
                            ""category"": ""Groceries"",
                            ""direction"": ""inbound""
                        },
                        {
                            ""transactionId"": ""local_txn_1002_refund"",
                            ""merchantRef"": ""INV-2026-0002"",
                            ""date"": ""2026-08-01T12:00:00+02:00"",
                            ""amount"": 500.00,
                            ""currency"": ""ZAR"",
                            ""category"": ""Dining"",
                            ""direction"": ""outbound""
                        }
                    ]
                }";
                return Task.FromResult(new RawVendorResponse
                {
                    Content = json,
                    Type = ContentType.Json,
                    Vendor = VendorName.Ozow,
                    VendorCustomerId = vendorCustomerId
                });
            }
        }

        private class FakeFnbSoapClient : IVendorApiClient
        {
            public VendorName Vendor => VendorName.Fnb;
            public Task<RawVendorResponse> FetchTransactionsAsync(string vendorCustomerId)
            {
                var xml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
                    <SOAP-ENV:Body>
                        <ns2:getCustomerTransactionsResponse xmlns:ns2=""http://transactions.fnb.soapservice.co.za"">
                            <ns2:transactions>
                                <ns2:transaction>
                                    <ns2:txId>txn-uuid-3001</ns2:txId>
                                    <ns2:bankReference>FNB-BANKTX-20260801-3001</ns2:bankReference>
                                    <ns2:bookingDate>2026-08-01</ns2:bookingDate>
                                    <ns2:amount>-450.75</ns2:amount>
                                    <ns2:currency>ZAR</ns2:currency>
                                    <ns2:category>Groceries</ns2:category>
                                    <ns2:creditDebit>DEBIT</ns2:creditDebit>
                                </ns2:transaction>
                                <ns2:transaction>
                                    <ns2:txId>txn-uuid-3011</ns2:txId>
                                    <ns2:bankReference>FNB-BANKTX-20260822-2003</ns2:bankReference>
                                    <ns2:bookingDate>2026-08-22</ns2:bookingDate>
                                    <ns2:amount>8000.00</ns2:amount>
                                    <ns2:currency>ZAR</ns2:currency>
                                    <ns2:category>Salary/Income</ns2:category>
                                    <ns2:creditDebit>CREDIT</ns2:creditDebit>
                                </ns2:transaction>
                            </ns2:transactions>
                        </ns2:getCustomerTransactionsResponse>
                    </SOAP-ENV:Body>
                </SOAP-ENV:Envelope>";
                return Task.FromResult(new RawVendorResponse
                {
                    Content = xml,
                    Type = ContentType.Xml,
                    Vendor = VendorName.Fnb,
                    VendorCustomerId = vendorCustomerId
                });
            }
        }
    }
}
