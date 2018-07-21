using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// All classes that contribute to response rendering inherit from this interface
    /// </summary>
    public interface IElement: IDeployable, IPackagable, INamed
    {
        /// <summary>
        /// Returns the type of element. This is used to make the fully qualified
        /// element name unique
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// Gets debuging information for this element
        /// </summary>
        /// <returns></returns>
        DebugInfo GetDebugInfo();

        /// <summary>
        /// This is called after name resolution. It is called once for each page where the
        /// element appears
        /// </summary>
        void Initialize(IInitializationData initializationData);

        /// <summary>
        /// Returns a disposable enumerator for the children of this element. By calling
        /// this for each of the returned elements recusrively it is possible to traverse
        /// the entire element tree.
        /// </summary>
        IEnumerator<IElement> GetChildren();

        /// <summary>
        /// Returns the data consumer for this element. The data consumer defines the
        /// runtime data needs of this element
        /// </summary>
        IDataConsumer GetDataConsumer();

        /// <summary>
        /// This is where the element is responsible for outputting its dynamic css rules.
        /// Dynamic assets are defined as assets whose names are randomly
        /// generated at runtime and are therefore different on each web server.
        /// These types of asset are always rendered into the page.
        /// The recommended approach is to generate these names using the INameManager
        /// in the constructor of your element and keep using the same names
        /// for the lifetime of the instance.
        /// This method might be called in the context of rendering a page, or it might
        /// be called once for the page and the output cached for reuse on all subsequent
        /// pages.
        /// </summary>
        /// <param name="writer">The text writer to srite them to</param>
        /// <param name="includeChildren">When true recursively traverses the element
        /// tree below this one writing the dynamic CSS for all the descendants</param>
        IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren = true);

        /// <summary>
        /// This is where the element is responsible for outputting its dynamic Javascript.
        /// Dynamic assets are defined as assets whose names are randomly
        /// generated at runtime and are therefore different on each web server.
        /// These types of asset are always rendered into the page.
        /// The recommended approach is to generate these names using the INameManager
        /// in the constructor of your element and keep using the same names
        /// for the lifetime of the instance.
        /// This method might be called in the context of rendering a page, or it might
        /// be called once for the page and the output cached for reuse on all subsequent
        /// pages.
        /// </summary>
        /// <param name="writer">The text writer to srite them to</param>
        /// <param name="includeChildren">When true recursively traverses the element
        /// tree below this one writing the dynamic assets for all the descendants</param>
        IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren = true);

        /// <summary>
        /// This is where the element gets an opportunity to write JavaScript into the page
        /// after all of the html has been written. This Javascript will run then the
        /// page loads.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="includeChildren">When true recursively traverses the element
        /// tree below this one writing the initialization script for all the descendants</param>
        IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren = true);

        /// <summary>
        /// This method is called during page rendering and is this elements opportunity
        /// to output the page title.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="includeChildren">When true recursively traverses the element
        /// tree below this one writing the page title for all the descendants</param>
        IWriteResult WriteTitle(IRenderContext context, bool includeChildren = true);

        /// <summary>
        /// This method is called during page rendering and is this elements opportunity
        /// to output html into the head part of the page. This can be used to write page
        /// style sheet references, canonical links etc.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="includeChildren">When true recursively traverses the element
        /// tree below this one writing the page head for all the descendants</param>
        IWriteResult WriteHead(IRenderContext context, bool includeChildren = true);

        /// <summary>
        /// This method is called during page rendering and is this elements opportunity
        /// to output html into the page at the place where this element is on the page
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="includeChildren">When true recursively traverses the element
        /// tree below this one writing the html for all the descendants</param>
        IWriteResult WriteHtml(IRenderContext context, bool includeChildren = true);
    }

    /// <summary>
    /// Provides extension methods for the IElement interface
    /// </summary>
    public static class ElementExtensions
    {
        /// <summary>
        /// Returns the fully qualified name of the element
        /// </summary>
        public static string FullyQualifiedName(this IElement element)
        {
            if (element == null) 
                return null;

            if (element.Package == null || string.IsNullOrEmpty(element.Package.NamespaceName)) 
                return element.Name;

            return (element.Package.NamespaceName + ":" + element.Name);
        }
    }
}
