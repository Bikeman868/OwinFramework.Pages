using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class IframeElement : Element, ILinkElement, IStyleElement
    {
        public LinkTypes LinkType { get; set; }
        public string LinkAddress { get; set; }
        public IDictionary<string, string> Styles { get; set; }
        public string ClassNames { get; set; }
        public string AltText { get; set; }

        public IframeElement()
        {
            ElementType = ElementTypes.Link;
            LinkType = LinkTypes.Iframe;
        }
    }
}