namespace OwinFramework.Pages.Html.Templates.Text
{
    public enum ElementTypes
    {
        /// <summary>
        /// This is the element type fo the root element that
        /// represents the whole document.
        /// </summary>
        Document,

        /// <summary>
        /// This is a paragraph of text that does not contain
        /// any line breaks. It sould be rendered in HTML using
        /// the <p/> tag. The paragraph element will usually contain
        /// some text or at least one child element. If there is no 
        /// text and no children then this represents a blank line of 
        /// text.
        /// </summary>
        Paragraph,

        /// <summary>
        /// A run of characters that has other elements on either side. 
        /// Usually contained within a paragraph. This maps onto the 
        /// html <span/> tag.
        /// </summary>
        InlineText,

        /// <summary>
        /// This is a heading with a heading level. Translates into <h1/>
        /// <h2/> etc in HTML.
        /// </summary>
        Heading,

        /// <summary>
        /// This is used for markup that does not have any text inside it
        /// such as <br/> <hr/> etc. Elements of this type do not contain
        /// child elements
        /// </summary>
        Break,

        /// <summary>
        /// This is a container for raw text in the input stream. This allows
        /// the children of an element to be a mix of text and markup
        /// </summary>
        RawText,

        /// <summary>
        /// This is a container that arranges it's children according to
        /// the ContainerTypes enum
        /// </summary>
        Container,

        /// <summary>
        /// An element that contains a reference to another resource such
        /// as a stylesheet reference, embedded image or navigation link.
        /// </summary>
        Link,

        /// <summary>
        /// This is an element type that we don't care about for example
        /// comments and scripts
        /// </summary>
        Unsupported
    }
}