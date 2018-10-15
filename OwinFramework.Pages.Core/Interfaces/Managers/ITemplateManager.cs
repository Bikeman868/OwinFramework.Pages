using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Managers
{
    /// <summary>
    /// This is a central repository for templates. Templates are produced by
    /// parsing a text document that describes how to render a fragment of Html.
    /// There can be many template parsers registered in the application that
    /// use different syntax to define the template. These syntax differences
    /// are transparent to the application which just sees the ITemplateManager
    /// interface.
    /// </summary>
    public interface ITemplateManager
    {
        /// <summary>
        /// Adds a template to the template manager so that it can be used by
        /// the application.
        /// </summary>
        /// <param name="template">The template to add</param>
        /// <param name="templatePath">A / separated path to the template. Typically
        /// this is the relative path to the template file, but any other scheme
        /// will also work. The only restriction is that the template path
        /// must be uniqie to the template. The template path is how the application
        /// will refer to the template.
        /// The template path can contain a single * in any path segment in which case
        /// it will match requests for templates with any value in that path element.
        /// The template path can end with ** in which case it matches all sub-paths</param>
        /// <param name="locales">A list of the locales that this template is suitable
        /// for. Pass null or an empty list if this is the only version of this
        /// template, and it should be used for all locales. When registering multiple
        /// templates with the same path and different locales (different language
        /// versions of the same template) there must be no overlap between the locales.</param>
        void Register(ITemplate template, string templatePath, params string[] locales);

        /// <summary>
        /// Finds a template to display
        /// </summary>
        /// <param name="renderContext">The render context to find a template for</param>
        /// <param name="templatePath">The / separated path to the template. This is
        /// usually the relative path to the template file, but this depends on the
        /// implementation of the template parsing engine.</param>
        ITemplate Get(IRenderContext renderContext, string templatePath);
    }
}
