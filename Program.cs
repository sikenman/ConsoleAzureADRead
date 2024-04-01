using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;

namespace AzureADRead
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Values from app registration
            var clientId = "da956ff6-f52d-40c5-b1ff-c1332380e6fb"; // Azure AD App Registration's Application (client) ID
            var tenantId = "0ac52edc-a605-4545-861a-50db3b521de4"; // Azure AD tenant ID
            var clientSecret = "your-client-sercets";

            string UserID = "e30effdc-b79b-492f-8d6f-0bd2f4bd7f86";     // User1 (belongs to 2 Groups)
            //string UserID = "008c6c9c-eb11-4af7-8747-7d6269c4a5e5";   // User2 (belongs to 3 Groups)

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            try
            {
                // Get User Details
                await Console.Out.WriteLineAsync("\nUser Details:");
                var user = await graphClient.Users[UserID].GetAsync();
                await Console.Out.WriteLineAsync(user.Id + ":" + user.DisplayName);
                string UD = JsonConvert.SerializeObject(user);
                Console.WriteLine(UD);
                
                // Get User Groups where he belongs to
                var result = await graphClient.Users[UserID].MemberOf.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Filter = "startswith(displayname,'Group')";
                    requestConfiguration.QueryParameters.Orderby = ["displayName"];
                    requestConfiguration.QueryParameters.Count = true;
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                });
                //string UD = JsonConvert.SerializeObject(result);
                //Console.WriteLine(UD);

                await Console.Out.WriteLineAsync("User belongs to following Groups:");
                foreach (Group group in result.Value)
                {
                    await Console.Out.WriteLineAsync(group.Id + ": " + group.DisplayName);
                }


                // Applications Groups
                await Console.Out.WriteLineAsync("\nApplication has rights to following Groups:");
                var groups = await graphClient.Groups.GetAsync();
                foreach (var group in groups.Value)
                {
                    await Console.Out.WriteLineAsync(group.Id + ":" + group.DisplayName);
                }
                string GD = JsonConvert.SerializeObject(groups);
                Console.WriteLine(GD);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            await Console.Out.WriteLineAsync("Done!");

        }

        private static void AzureDeviceAuth(string clientId, string tenantId)
        {
            var scopes = new[] { "User.Read" };
        
            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            //var tenantId = "common";
        
            var options = new DeviceCodeCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                ClientId = clientId,
                TenantId = tenantId,
                // Callback function that receives the user prompt
                // Prompt contains the generated device code that user must
                // enter during the auth process in the browser
                DeviceCodeCallback = (code, cancellation) =>
                {
                    Console.WriteLine(code.Message);
                    return Task.FromResult(0);
                },
            };
        }
    }
}
