namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// Represents a class that performs some processing on a document
    /// as it is parsed.
    /// </summary>
    public interface IDocumentProcessor
    {
        /// <summary>
        /// This method will get called when a new element is discovered
        /// in the source document. At this point the child elements will
        /// not be available.
        /// </summary>
        bool BeginProcessElement(IDocumentElement element);

        /// <summary>
        /// This method will get called when the end of an element is detected
        /// in the source document. At this point the child elements will
        /// be available and will all have been processed.
        /// </summary>
        bool EndProcessElement(IDocumentElement element);
    }
}