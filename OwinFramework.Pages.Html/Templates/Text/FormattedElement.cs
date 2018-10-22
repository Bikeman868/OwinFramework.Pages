using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class FormattedElement : Element, IConfigurableElement, IStyleElement
    {
        public IDictionary<string, string> Attributes { get; set; }
        public IDictionary<string, string> Styles { get; set; }
        public string ClassNames { get; set; }
    }
}