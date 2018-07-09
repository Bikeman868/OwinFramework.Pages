using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the page builder to construct pages using a fluent syntax
    /// </summary>
    public interface IPageBuilder
    {
        /// <summary>
        /// Starts building a new page or configuring an existing page
        /// </summary>
        /// <param name="pageInstance">Pass an instance of the Page class to 
        /// configure an instance of a derrived class or pass null to construct an
        /// instance of the Page class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the page</param>
        /// <param name="package">Optional package adds a namespace to this page</param>
        IPageDefinition BuildUpPage(object pageInstance, Type declaringType = null, IPackage package = null);
    }
}
