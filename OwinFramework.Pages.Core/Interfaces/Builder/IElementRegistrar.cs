using System;
using System.Reflection;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// This class is responsible for finding components, regions, layouts etc
    /// and registering them with the routing engine so that they receive requests
    /// </summary>
    public interface IElementRegistrar
    {
        /// <summary>
        /// Registers all components, layouts, regions etc defined in the package
        /// </summary>
        void Register(IPackage package);

        /// <summary>
        /// Searches within the given assembly for all eleemnts and
        /// registers them.
        /// </summary>
        /// <param name="assembly">The assembly to search for eleemnts</param>
        void Register(Assembly assembly);

        /// <summary>
        /// Registeres a class as an element if it is one, does nothing
        /// if this is not an element
        /// </summary>
        /// <param name="type">The element type to register</param>
        void Register(Type type);
    }

}
