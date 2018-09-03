using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Elements implement this interface when they can have dependencies
    /// on components that must be added to any page containing this element
    /// </summary>
    public interface ILibraryConsumer
    {
        /// <summary>
        /// Adda a dependent component
        /// </summary>
        void NeedsComponent(IComponent component);

        /// <summary>
        /// Gets a list of the component dependencies
        /// </summary>
        List<IComponent> GetDependentComponents();
    }
}
