namespace OwinFramework.Pages.Html.Templates.Text
{
    public interface IDocument: IDocumentElement
    {
        /// <summary>
        /// The mime type of the original text that was parsed to produce 
        /// this document.
        /// </summary>
        string MimeType { get; }

        /// <summary>
        /// This is a measure of how well the document conforms to the
        /// format specification as a value between 0 and 1. This is
        /// used to identify the likely format of a document with
        /// unknown format.
        /// </summary>
        float ConformanceLevel { get; }
    }
}
