namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class BreakElement : Element, IBreakElement
    {
        public BreakTypes BreakType { get; set; }

        public BreakElement()
        {
            ElementType = ElementTypes.Break;
            BreakType = BreakTypes.LineBreak;
        }
    }
}