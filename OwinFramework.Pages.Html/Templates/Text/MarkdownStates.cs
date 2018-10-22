namespace OwinFramework.Pages.Html.Templates.Text
{
    // See https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet

    public enum MarkdownStates 
    { 
        Paragraph,
        ParagraphBreak,
        Heading,
        NumberedList,
        UnorderedList,
        Link,
        Image,
        SourceCode,
        TableHeadings,
        TableRow,
        BlockQuote,
        HorizontalRule
    }
}