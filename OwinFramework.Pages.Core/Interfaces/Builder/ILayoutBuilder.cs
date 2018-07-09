using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the layout builder to construct layouts using a fluent syntax
    /// </summary>
    public interface ILayoutBuilder
    {
        /// <summary>
        /// Starts building a new layout or configuring an existing layout
        /// </summary>
        /// <param name="layoutInstance">Pass an instance of the Layout class to 
        /// configure an instance of a derrived class or pass null to construct an
        /// instance of the Layout class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the layout</param>
        /// <param name="package">Optional package adds a namespace to this layout</param>
        ILayoutDefinition BuildUpLayout(object layoutInstance = null, Type declaringType = null, IPackage package = null);
    }
}
