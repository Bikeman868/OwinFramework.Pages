using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to specify that the caller must be an identified
    /// user of the system
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresIdentificationAttribute: Attribute
    {
    }
}
