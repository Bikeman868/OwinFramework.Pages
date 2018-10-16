﻿namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// Constructs ITemplate instances
    /// </summary>
    public interface ITemplateFactory
    {
        /// <summary>
        /// Constructs and initializes a template definition. The template
        /// definition can define the template fluently then construct an
        /// ITemplate instance.
        /// </summary>
        ITemplateDefinition Create();
    }
}
