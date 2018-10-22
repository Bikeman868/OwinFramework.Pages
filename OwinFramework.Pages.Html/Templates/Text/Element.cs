using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class Element: IDocumentElement
    {
        public ElementTypes ElementType { get; set; }
        public IDocumentElement Parent { get; set; }
        public IList<IDocumentElement> Children { get; set; }
        public string Name { get; set; }
        public bool SuppressOutput { get; set; }

        public Element()
        {
            ElementType = ElementTypes.Document;
        }
    }
}