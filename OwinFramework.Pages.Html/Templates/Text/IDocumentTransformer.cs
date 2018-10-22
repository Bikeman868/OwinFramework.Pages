using System;

namespace OwinFramework.Pages.Html.Templates.Text
{
    public interface IDocumentTransformer
    {
        /// <summary>
        /// Analyses textual content and tries to figure out what format it is in.
        /// This method only does very basic checks to determine the document format,
        /// the better method is to pass the document through each format parser and
        /// see which one found the best conformance to the format specification.
        /// </summary>
        /// <param name="content">The content to analyse</param>
        /// <returns>The mime type of the content or null if the format can't
        /// easily be determined</returns>
        string DetermineMimeType(string content);

        /// <summary>
        /// Parses a document with a known Mime type and makes a callback for each element
        /// of the document. Note that headings will only be adjusted after the whole document
        /// is parsed. The adjusted headings are therefore only available in the onEndProcessElement
        /// event for the document root node.
        /// </summary>
        /// <param name="mimeType">The mime type of the document which determines
        /// how the document will be parsed. You must pass a supported mime type
        /// or an exception will be thrown</param>
        /// <param name="documentContent">The document</param>
        /// <param name="onBeginProcessElement">This function is executed every time a new
        /// element is discovered in the document. At this point the element is
        /// not fully parsed and the children will not be populated</param>
        /// <param name="onEndProcessElement">This function is executed every time
        /// the end of an element is encountered. At this point all of the children
        /// will be populated</param>
        void ParseDocument(
            string mimeType, 
            string documentContent, 
            Func<IDocumentElement, bool> onBeginProcessElement,
            Func<IDocumentElement, bool> onEndProcessElement);

        /// <summary>
        /// Parses a document with known mime type and pushes the 
        /// document elements through a document processor. The
        /// document processor will typically write the document out
        /// in some other format, for example parsing html and writing
        /// out Markdown
        /// </summary>
        /// <param name="mimeType">The mime type of the document which determines
        /// how the document will be parsed. You must pass a supported mime type
        /// or an exception will be thrown</param>
        /// <param name="documentContent">The document</param>
        /// <param name="documentProcessor">An instance that will process
        /// the elements of the document as they are parsed</param>
        void ParseDocument(
            string mimeType,
            string documentContent,
            IDocumentProcessor documentProcessor);

        /// <summary>
        /// Parses a document with known mime type.
        /// </summary>
        /// <param name="mimeType">The mime type of the document which determines
        /// how the document will be parsed. If you pass null or empty strin then
        /// the document will be parsed by each of the parsers and the one that
        /// has the closest match to the format specification will  used.</param>
        /// <param name="documentContent">The document</param>
        /// <returns>The root element of the document that was parsed</returns>
        IDocument ParseDocument(
            string mimeType,
            string documentContent);

        /// <summary>
        /// Writes out a document in the specified mime type
        /// </summary>
        /// <param name="mimeType">The mime type of the document to write</param>
        /// <param name="rootElement">The root element of the document to write. This
        /// can be obtained by parsing a document from some other format</param>
        /// <param name="writer">Specifies where to write the document to</param>
        /// <param name="fragment">Pass true to emit a document fragment that can be
        /// inserted into the middle of another document. Pass false to write
        /// a complete stand-alone document.</param>
        void FormatDocument(
            string mimeType, 
            System.IO.TextWriter writer,
            IDocumentElement rootElement, 
            bool fragment = true);

        /// <summary>
        /// Adjusts the headings in the document to have a minimum heading level.
        /// Generally you want to do this after parsing a source document and before
        /// writing this document out in another format.
        /// </summary>
        /// <param name="rootElement">This element and all of its descendants will
        /// have thier headings normalized to the specified minimum level</param>
        /// <param name="minimumHeadingLevel">Adjusts headings in the document so
        /// that there are no higher level headings. For example if you pass a value
        /// of 3 then write the document out at html then the html will contain
        /// h3, h4, h5 etc but no h1 or h2 headings</param>
        void AdjustHeadings(IDocumentElement rootElement, int minimumHeadingLevel);

        /// <summary>
        /// Removes attributes from document elements that don't match the
        /// specified filter. For example you can remove all attributes
        /// except for id, or you can remove all class attributes etc.
        /// </summary>
        /// <param name="rootElement">Applies to this element and all of its
        /// descendants</param>
        /// <param name="attributeFilter">This function is passed an element,
        /// the name and the value of the attribute on that element and the
        /// function should return true to keep this attribute and flase to
        /// remove it.</param>
        void CleanAttributes(
            IDocumentElement rootElement, 
            Func<IDocumentElement, string, string, bool> attributeFilter);

        /// <summary>
        /// Removes tags from document that don't match the specified filter.
        /// For example you can remove all iframes from the document.
        /// </summary>
        /// <param name="rootElement">Applies all descendants of this element</param>
        /// <param name="elementFilter">This function is passed an element and the
        /// function should return true to keep this element and flase to
        /// remove it.</param>
        void CleanElements(
            IDocumentElement rootElement,
            Func<IDocumentElement, bool> elementFilter);

        /// <summary>
        /// Processes a document applying specific cleaning rules
        /// </summary>
        /// <param name="rootElement">Applies to this element and all of its
        /// descendants</param>
        /// <param name="actions">The actions to perform on the elements</param>
        void CleanDocument(IDocumentElement rootElement, DocumentCleaning actions);
    }

    [Flags]
    public enum DocumentCleaning
    {
        None = 0,

        /// <summary>
        /// Converts raw text + &lt;br> into &lt;p> tags. Also collapses multiple
        /// consecutive &lt;br> tags.
        /// </summary>
        MakeParagraphs = 1,

        /// <summary>
        /// Removes empty paragraphs and empty containers (divs, tables etc).
        /// </summary>
        RemoveBlankLines = 2
    }
}