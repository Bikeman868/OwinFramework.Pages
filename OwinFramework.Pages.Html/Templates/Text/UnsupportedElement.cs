namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class UnsupportedElement : Element, IConfigurableElement
    {
        public System.Collections.Generic.IDictionary<string, string> Attributes { get; set; }

        public UnsupportedElement()
        {
            ElementType = ElementTypes.Unsupported;
            SuppressOutput = true;
        }
    }
}