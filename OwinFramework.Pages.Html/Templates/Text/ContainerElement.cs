using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class ContainerElement : Element, IConfigurableElement, IContainerElement
    {
        public IDictionary<string, string> Attributes { get; set; }
        public ContainerTypes ContainerType { get; set; }
        public object ChildLayout { get; set; }

        public ContainerElement()
        {
            ElementType = ElementTypes.Container;
        }
    }
}