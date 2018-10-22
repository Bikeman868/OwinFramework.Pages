namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class AnchorElement : Element, ILinkElement
    {
        public LinkTypes LinkType { get; set; }
        public string LinkAddress { get; set; }
        public string AltText { get; set; }

        public AnchorElement()
        {
            ElementType = ElementTypes.Link;
            LinkType = LinkTypes.Reference;
        }
    }
}