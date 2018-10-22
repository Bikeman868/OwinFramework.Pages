namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// When documents contain a break in the text flow, this indicates what
    /// kind of break it is
    /// </summary>
    public enum BreakTypes 
    { 
        /// <summary>
        /// Text continues on the next line
        /// </summary>
        LineBreak, 

        /// <summary>
        /// Text continues in a new parapgraph
        /// </summary>
        ParapgraphBreak,

        /// <summary>
        /// A horizontal line breaks the text
        /// </summary>
        HorizontalRule
    }
}