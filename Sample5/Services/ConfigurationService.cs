using Newtonsoft.Json;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using System.Linq;

namespace Sample5.Services
{
    [IsService("config", "/services/config", new Method[]{ Method.Get })]
    [GenerateClientScript("config_service_client")]
    public class ConfigurationService
    {
        [Endpoint]
        public void User(IEndpointRequest request)
        {
            var userConfig = new UserConfig();

            var identification = request.OwinContext.GetFeature<IIdentification>();

            if (identification != null)
            {
                userConfig.IsLoggedIn = !identification.IsAnonymous;

                var usernameClaim = identification.Claims.FirstOrDefault(claim => claim.Name == ClaimNames.Username);
                if (usernameClaim != null)
                {
                    userConfig.Email = usernameClaim.Value;
                    userConfig.IsEmailVerified = usernameClaim.Status == ClaimStatus.Verified;
                }
            }

            request.Success(userConfig);
        }

        private class UserConfig
        {
            [JsonProperty("isLoggedIn")]
            public bool IsLoggedIn { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("isEmailVerified")]
            public bool IsEmailVerified { get; set; }
        }
    }
}