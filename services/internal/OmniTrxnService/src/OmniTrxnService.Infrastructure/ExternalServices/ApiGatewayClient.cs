using System.Text;

namespace OmniTrxnService.Infrastructure.ExternalServices
{
    public class ApiGatewayClient
    {
        private readonly HttpClient _httpClient;
        public ApiGatewayClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAsync(string relativeUrl, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync(relativeUrl, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }

        public async Task<string> PostSoapAsync(string relativeUrl, string soapEnvelope, CancellationToken ct = default)
        {
            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            var response = await _httpClient.PostAsync(relativeUrl, content, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }
    }
}
