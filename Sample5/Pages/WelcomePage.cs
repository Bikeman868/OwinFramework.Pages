using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sample5.Pages
{
    [IsPage("welcome_page", "/welcome")]
    [Route("/welcome", Method.Get, Priority = 100)]
    [PageTitle("Welcome")]
    [ZoneTemplate("body_zone", "/page/welcome-page")]
    public class WelcomePage : MasterPage { }
}