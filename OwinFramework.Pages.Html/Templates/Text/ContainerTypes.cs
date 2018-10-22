namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// Defines how a container should arrange and format the
    /// elements within it
    /// </summary>
    public enum ContainerTypes 
    { 
        /// <summary>
        /// Used to group elements with common style. Does not imply any specific formatting
        /// </summary>
        Division,

        /// <summary>
        /// No additional formatting is applied to list items
        /// </summary>
        BareList, 

        /// <summary>
        /// List items should be prefixed by consecutive numbers
        /// </summary>
        NumberedList,
 
        /// <summary>
        /// Each list item should be prefixed by a bullet symbol
        /// </summary>
        BulletList,

        /// <summary>
        /// Paragraphs are indented and styled to indicate that this is
        /// a quotation from some other source
        /// </summary>
        BlockQuote,

        /// <summary>
        /// Disables line wrapping and uses a fixed width font and preserves
        /// whitespace so that spaces can be used to align text vertically
        /// </summary>
        PreFormatted,

        /// <summary>
        /// Rows of equal width columns. Rows can be different heights
        /// These containers can only contain HeaderRow, FooterRow and
        /// DataRow type containers
        /// </summary>
        Table,

        /// <summary>
        /// Container for table header rows
        /// </summary>
        TableHeader,

        /// <summary>
        /// Container for data rows in the table
        /// </summary>
        TableBody,

        /// <summary>
        /// Container for data rows in the table
        /// </summary>
        TableFooter,

        /// <summary>
        /// This is a table header row. These rows should appear before any data rows
        /// These containers can only contain TableDataCell type containers
        /// </summary>
        TableHeaderRow, 

        /// <summary>
        /// This is a table footer row. These rows should appear after any data rows
        /// These containers can only contain TableDataCell type containers
        /// </summary>
        TableFooterRow,
 
        /// <summary>
        /// This is a table row that contains the data in the middle of the table
        /// These containers can only contain TableDataCell type containers
        /// </summary>
        TableDataRow,

        /// <summary>
        /// This is a table row/column intersection. These containers can contain
        /// any other elements including other tables
        /// </summary>
        TableDataCell,
    }
}