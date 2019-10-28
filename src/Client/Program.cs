using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Discover endpoints from metadata
            var identityServerClient = new HttpClient();
            var disco = await identityServerClient.GetDiscoveryDocumentAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return 1;
            }
            // Request token
            var tokenResponse = await identityServerClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return 2;
            }
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");
            // Call API
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);
            var apiResponse = await apiClient.GetAsync("http://localhost:5002/identity");
            if (!apiResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(apiResponse.StatusCode);
                return 3;
            }
            var content = await apiResponse.Content.ReadAsStringAsync();
            Console.WriteLine(JArray.Parse(content));
            return 0; 
        }
    }
}
