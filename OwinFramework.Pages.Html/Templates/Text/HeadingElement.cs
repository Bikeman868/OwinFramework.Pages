namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class HeadingElement : Element, INestedElement
    {
        public int Level { get; set; }

        public HeadingElement()
        {
            ElementType = ElementTypes.Heading;
        }
    }
}