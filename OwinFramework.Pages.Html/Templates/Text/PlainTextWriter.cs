namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class PlainTextWriter
    {
        private readonly TextWriter _textWriter;

        public PlainTextWriter(System.IO.TextWriter writer)
        {
            var characterStream = new PlainTextCharacterStream(writer);
            _textWriter = new TextWriter(characterStream);
        }

        public bool Write(IDocumentElement element)
        {
            switch (element.ElementType)
            {
                case ElementTypes.Document:
                case ElementTypes.Container:
                    return WriteContainer(element);
                case ElementTypes.Break:
                    return WriteBreak(element);
                case ElementTypes.Heading:
                    return WriteHeading(element);
                case ElementTypes.InlineText:
                    return WriteInlineText(element);
                case ElementTypes.Link:
                    return WriteLink(element);
                case ElementTypes.Paragraph:
                    return WriteParagraph(element);
            }

            var textElement = element as ITextElement;
            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
                _textWriter.Write(textElement.Text);

            return true;
        }

        private bool WriteBreak(IDocumentElement element)
        {
            _textWriter.EnsureBlankLine();
            return true;
        }

        private bool WriteContainer(IDocumentElement element)
        {
            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            return true;
        }

        private bool WriteHeading(IDocumentElement element)
        {
            _textWriter.EnsureBlankLine();

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            _textWriter.EnsureNewLine();

            return true;
        }

        private bool WriteInlineText(IDocumentElement element)
        {
            var textElement = element as ITextElement;

            if (textElement != null)
                _textWriter.Write(textElement.Text);

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            return true;
        }

        private bool WriteLink(IDocumentElement element)
        {
            var linkElement = element as ILinkElement;
            if (linkElement == null) return false;

            _textWriter.Write(linkElement.LinkAddress);

            return true;
        }

        private bool WriteParagraph(IDocumentElement element)
        {
            var textElement = element as ITextElement;
            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
                _textWriter.Write(textElement.Text);

            if (element.Children != null)
            {
                foreach (var child in element.Children)
                    Write(child);
            }

            return true;
        }
    }
}
