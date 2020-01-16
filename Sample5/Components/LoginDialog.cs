using System.Linq;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;

namespace Sample5.Components
{
    [IsComponent("login_dialog")]
    [NeedsComponent("libraries:vue")]
    [PartOf("application_package")]
    [DeployedAs(AssetDeployment.PerModule, "navigation_module")]
    [Description("Writes the login template to the page but adds some dynamically generated JavaScript that identifies wether the user is logged in or not")]
    public class LoginDialog : Component
    {
        public LoginDialog(IComponentDependenciesFactory dependencies) : base(dependencies)
        {
            PageAreas = new[] 
            { 
                PageArea.Body, PageArea.Initialization
            };
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            switch (pageArea)
            {
                case PageArea.Body:
                    {
                        var template = Dependencies.NameManager.ResolveTemplate("/widget/login");
                        template?.WritePageArea(context, pageArea);
                        break;
                    }
                case PageArea.Initialization:
                    {
                        var isLoggedIn = "false";
                        var email = string.Empty;
                        var isEmailVerified = "false";

                        var identification = context.OwinContext.GetFeature<IIdentification>();
                        if (identification != null)
                        {
                            if (!identification.IsAnonymous) isLoggedIn = "true";
                            
                            var usernameClaim = identification.Claims.FirstOrDefault(claim => claim.Name == ClaimNames.Username);
                            if (usernameClaim != null)
                            {
                                email = usernameClaim.Value;
                                if (usernameClaim.Status == ClaimStatus.Verified)
                                    isEmailVerified = "true";
                            }
                        }

                        var template = Dependencies.NameManager.ResolveTemplate("/widget/login");
                        if (template != null)
                        {
                            context.Html.WriteScriptOpen();
                            context.Html.WriteLine();

                            context.Html.WriteLine("new Vue({");
                            context.Html.WriteLine("  el: \"#login\",");
                            context.Html.WriteLine("  data: {");
                            context.Html.WriteLine("    isLoggedIn: " + isLoggedIn + ",");
                            context.Html.WriteLine("    email: \"" + email + "\",");
                            context.Html.WriteLine("    isEmailVerified: " + isEmailVerified + ",");

                            template.WritePageArea(context, pageArea);

                            context.Html.WriteLine("});");

                            context.Html.WriteScriptClose();
                            context.Html.WriteLine();
                        }
                    }
                    break;
            }

            return base.WritePageArea(context, pageArea);
        }
    }
}