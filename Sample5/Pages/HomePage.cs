using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sample5.Pages
{
    [IsPage("home_page", "/")]
    [Route("/home", Method.Get, Priority = 100)]
    [Route("/", Method.Get, Priority = 100)]
    [PageTitle("Sample5")]
    [ZoneTemplate("body_zone", "/page/home-page")]
    public class HomePage : MasterPage { }
}