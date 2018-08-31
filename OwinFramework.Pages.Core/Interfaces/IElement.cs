using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// All classes that contribute to response rendering inherit from this interface
    /// </summary>
    public interface IElement : IPageWriter, IDeployable, IPackagable, INamed, IDataConsumer
    {
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
