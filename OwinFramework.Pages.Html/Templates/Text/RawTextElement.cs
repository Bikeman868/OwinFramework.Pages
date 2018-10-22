namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class RawTextElement : Element, ITextElement
    {
        public string Text { get; set; }

        public RawTextElement()
        {
            ElementType = ElementTypes.RawText;
        }
    }
}