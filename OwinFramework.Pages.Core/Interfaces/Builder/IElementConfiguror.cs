using System;
using OwinFramework.Pages.Core.Attributes;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Knows how to take custom attributes and apply them to 
    /// classes that implement certain interfaces
    /// </summary>
    public interface IElementConfiguror
    {
        /// <summary>
        /// Uses custom attributes from the declaring type
        /// and uses them to configure an element
        /// </summary>
        void Configure(object element,AttributeSet attributes);
    }
}
