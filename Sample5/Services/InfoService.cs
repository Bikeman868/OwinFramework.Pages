using Newtonsoft.Json;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using System.Linq;

namespace Sample5.Services
{
    [IsService("info", "/services/info", new Method[]{ Method.Get })]
    [PartOf("application_package")]
    [DeployedAs("navigation_module")]
    [GenerateClientScript("config_service_client")]
    public class InfoService
    {
        [Endpoint]
        public void GetUserInfo(IEndpointRequest request)
        {
            var userInfo = new UserInfo();

            var identification = request.OwinContext.GetFeature<IIdentification>();

            if (identification != null)
            {
                userInfo.IsLoggedIn = !identification.IsAnonymous;

                var usernameClaim = identification.Claims.FirstOrDefault(claim => claim.Name == ClaimNames.Username);
                if (usernameClaim != null)
                {
                    userInfo.Email = usernameClaim.Value;
                    userInfo.IsEmailVerified = usernameClaim.Status == ClaimStatus.Verified;
                }
            }

            request.Success(userInfo);
        }

        private class UserInfo
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