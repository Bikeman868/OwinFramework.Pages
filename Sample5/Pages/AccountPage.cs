using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sample5.Pages
{
    [IsPage("account_page", "/account")]
    [Route("/account/*", Method.Get, Priority = 100)]
    [PageTitle("Account management")]
    [ZoneComponent("body_zone", "templates:from_url")]
    public class AccountPage : MasterPage { }
}