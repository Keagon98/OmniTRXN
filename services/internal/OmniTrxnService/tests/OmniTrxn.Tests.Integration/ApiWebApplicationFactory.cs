using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OmniTrxn.Tests.Integration
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;
        private readonly WireMockServer _gatewayMock;
        private string _dbConnectionString = string.Empty;

        public ApiWebApplicationFactory()
        {
            _gatewayMock = WireMockServer.Start();
            ConfigureGatewayMock();

            Environment.SetEnvironmentVariable("ApiGateway__BaseUrl", _gatewayMock.Urls[0]);

            _dbContainer = new MsSqlBuilder()
               .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
               .WithPassword("admin123")
               .Build();
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            _dbConnectionString = _dbContainer.GetConnectionString();
        }

        public async Task DisposeAsync()
        {
            _gatewayMock?.Stop();
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
                    ["ApiGateway:BaseUrl"] = _gatewayMock.Urls[0],
                    ["Polling:IntervalMinutes"] = "100000"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHostedService>();
            });
        }

        private void ConfigureGatewayMock()
        {
            // Mock Ozow REST endpoint
            _gatewayMock
                .Given(Request.Create()
                    .WithPath("/api/v1/Transactions/cus_ozow_00932")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(OzowJson));

            // Mock FNB SOAP endpoint
            _gatewayMock
                .Given(Request.Create()
                    .WithPath("/soap/fnb/transactions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml")
                    .WithBody(FnbSoapXml));
        }

        #region Sample Responses
        private const string OzowJson = @"{
            ""merchandId"": ""101"",
            ""customerId"": ""cus_ozow_00932"",
            ""customerEmail"": ""jclarkson@mail.co.za"",
            ""transactions"": [
                {
                    ""transactionId"": ""local_txn_1001"",
                    ""ozowPaymentId"": ""ozow_20260825_ABC123"",
                    ""merchantRef"": ""INV-2026-0001"",
                    ""date"": ""2026-08-25T11:12:34+02:00"",
                    ""status"": ""success"",
                    ""type"": ""payment"",
                    ""direction"": ""inbound"",
                    ""amount"": 1250.00,
                    ""currency"": ""ZAR"",
                    ""category"": ""Groceries""
                },
                {
                    ""transactionId"": ""local_txn_1002_refund"",
                    ""ozowPaymentId"": ""ozow_20260712_DEF456_refund"",
                    ""merchantRef"": ""INV-2026-0002"",
                    ""date"": ""2026-08-01T12:00:00+02:00"",
                    ""status"": ""refunded"",
                    ""type"": ""refund"",
                    ""direction"": ""outbound"",
                    ""amount"": 500.00,
                    ""currency"": ""ZAR"",
                    ""category"": ""Dining""
                }
            ]
        }";

        private const string FnbSoapXml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
            <SOAP-ENV:Header/>
            <SOAP-ENV:Body>
                <ns2:getCustomerTransactionsResponse xmlns:ns2=""http://transactions.fnb.soapservice.co.za"">
                    <ns2:statementId>STMT-ACCT-1111-20260825-02</ns2:statementId>
                    <ns2:createdDateTime>2026-08-25T11:30:00Z</ns2:createdDateTime>
                    <ns2:account>
                        <ns2:accountId>cust-acct-908</ns2:accountId>
                        <ns2:masked>****2311</ns2:masked>
                        <ns2:name>Jeremy Clarkson</ns2:name>
                        <ns2:currency>ZAR</ns2:currency>
                        <ns2:availableBalance>10250.75</ns2:availableBalance>
                    </ns2:account>
                    <ns2:transactions>
                        <ns2:transaction>
                            <ns2:txId>txn-uuid-3001</ns2:txId>
                            <ns2:bookingDate>2026-08-01</ns2:bookingDate>
                            <ns2:valueDate>2026-08-01</ns2:valueDate>
                            <ns2:amount>-450.75</ns2:amount>
                            <ns2:currency>ZAR</ns2:currency>
                            <ns2:creditDebit>DEBIT</ns2:creditDebit>
                            <ns2:merchantName>Shoprite</ns2:merchantName>
                            <ns2:category>Groceries</ns2:category>
                            <ns2:mcc>5411</ns2:mcc>
                            <ns2:remittance>Weekly groceries</ns2:remittance>
                            <ns2:bankReference>FNB-BANKTX-20260801-3001</ns2:bankReference>
                        </ns2:transaction>
                        <ns2:transaction>
                            <ns2:txId>txn-uuid-3011</ns2:txId>
                            <ns2:bookingDate>2026-08-22</ns2:bookingDate>
                            <ns2:valueDate>2026-08-22</ns2:valueDate>
                            <ns2:amount>8000.00</ns2:amount>
                            <ns2:currency>ZAR</ns2:currency>
                            <ns2:creditDebit>CREDIT</ns2:creditDebit>
                            <ns2:merchantName>Employer Pty Ltd</ns2:merchantName>
                            <ns2:category>Salary/Income</ns2:category>
                            <ns2:mcc>0000</ns2:mcc>
                            <ns2:remittance>Salary Aug 2026</ns2:remittance>
                            <ns2:bankReference>FNB-BANKTX-20260822-2003</ns2:bankReference>
                        </ns2:transaction>
                    </ns2:transactions>
                </ns2:getCustomerTransactionsResponse>
            </SOAP-ENV:Body>
        </SOAP-ENV:Envelope>";
        #endregion
    }
}
