using FluentAssertions;
using OmniTrxnService.Infrastructure.Adapters;

namespace OmniTrxn.Tests.Unit.Adapters
{
    public class XmlToJsonAdapterTests
    {
        [Fact]
        public void Convert_WithNamespacedXml_ReturnsJsonWithoutNamespaces()
        {
            // Arrange
            var xml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns2=""http://transactions.fnb.soapservice.co.za"">
                        <SOAP-ENV:Body>
                            <ns2:getCustomerTransactionsResponse>
                                <ns2:transactions>
                                    <ns2:transaction>
                                        <ns2:txId>txn-uuid-3001</ns2:txId>
                                        <ns2:amount>-450.75</ns2:amount>
                                    </ns2:transaction>
                                </ns2:transactions>
                            </ns2:getCustomerTransactionsResponse>
                        </SOAP-ENV:Body>
                    </SOAP-ENV:Envelope>";

            var adapter = new XmlToJsonAdapter();

            // Act
            var json = adapter.Convert(xml);

            // Assert
            json.Should().NotContain("ns2:");
            json.Should().Contain("\"txId\"");
            json.Should().Contain("\"amount\"");
        }
    }
}
