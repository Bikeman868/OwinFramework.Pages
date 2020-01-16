using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sample5.Pages
{
    [IsPage("form_id_page", "/formid")]
    [Route("/formid/**", Method.Get, Priority = 100)]
    [PageTitle("Account management")]
    [ZoneTemplate("body_zone", "/page/form-id-page")]
    public class FormIdPage : MasterPage { }
}