using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// Represents an element that was parsed from a formatted document
    /// </summary>
    public interface IDocumentElement
    {
        /// <summary>
        /// The type of document element, see enum values for details
        /// </summary>
        ElementTypes ElementType { get; }

        /// <summary>
        /// The parent of this element. This is null for the root element
        /// of the document, every other element will always have a parent.
        /// </summary>
        IDocumentElement Parent { get; set;  }

        /// <summary>
        /// A list of the child elements. This will be null when a document
        /// element is first identified and will be populated with children
        /// after the element has been fully parsed.
        /// </summary>
        IList<IDocumentElement> Children { get; set;  }

        /// <summary>
        /// The name if the element that was parsed from the original markup. For html
        /// this is the html tag name, for example 'span', 'div', 'p' etc
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// This flag is set on elements that should not normally be included in 
        /// generated documents
        /// </summary>
        bool SuppressOutput { get; set; }
    }
}