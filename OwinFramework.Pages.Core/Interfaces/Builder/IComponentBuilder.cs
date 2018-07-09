using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the component builder to construct components using a fluent syntax
    /// </summary>
    public interface IComponentBuilder
    {
        /// <summary>
        /// Starts building a new layout or configuring an existing layout
        /// </summary>
        /// <param name="componentInstance">Pass an instance of the Component class to 
        /// configure an instance of a derrived class or pass null to construct an
        /// instance of the Component class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the component</param>
        /// <param name="package">Optional package adds a namespace to this component</param>
        IComponentDefinition BuildUpComponent(object componentInstance, Type declaringType = null, IPackage package = null);
    }
}
