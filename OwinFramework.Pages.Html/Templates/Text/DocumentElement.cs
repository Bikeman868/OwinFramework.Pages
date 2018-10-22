namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class DocumentElement : Element, IDocument
    {
        /// <summary>
        /// The mime type of the original text that was parsed to produce 
        /// this document.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// This is a measure of how well the document conforms to the
        /// format specification as a value between 0 and 1. This is
        /// used to identify the likely format of a document with
        /// unknown format.
        /// </summary>
        public float ConformanceLevel { get; set; }

        public DocumentElement()
        {
            ElementType = ElementTypes.Document;
        }

        public void Prepare()
        {
            FixupChildren(this);
        }

        private void FixupChildren(IDocumentElement element)
        {
            if (element.Children != null)
            {
                foreach (var child in element.Children)
                {
                    child.Parent = element;
                    FixupChildren(child);
                }
            }
        }
    }
}