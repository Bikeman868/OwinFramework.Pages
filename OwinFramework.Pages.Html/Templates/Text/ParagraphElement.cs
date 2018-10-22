namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class ParagraphElement : FormattedElement, IBreakElement
    {
        public BreakTypes BreakType { get; set; }

        public ParagraphElement()
        {
            ElementType = ElementTypes.Paragraph;
            BreakType = BreakTypes.ParapgraphBreak;
        }
    }
}