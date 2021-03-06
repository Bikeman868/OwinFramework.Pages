﻿using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A factory for constructing HtmlWriter instancces
    /// </summary>
    public interface IJavascriptWriterFactory
    {
        /// <summary>
        /// Creates and initializes an instance that can write Javascript
        /// </summary>
        /// <param name="format">The Html standards to apply</param>
        /// <param name="indented">Choose readable vs compact</param>
        /// <param name="includeComments">Pass false here to surpress output of comments</param>
        IJavascriptWriter Create(HtmlFormat format = HtmlFormat.Html, bool indented = true, bool includeComments = true);

        /// <summary>
        /// Creates and initializes an instance that can write Javascript
        /// </summary>
        IJavascriptWriter Create(IRenderContext context);
    }
}
