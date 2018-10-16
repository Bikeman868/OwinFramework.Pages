using System.Reflection;

namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// You can add classes to your application that implement this
    /// interface, then use them to format data for presentation.
    /// This generally involves a lot less work than defining
    /// view models for each presentation of each type of data.
    /// </summary>
    public interface IDataFieldFormatter
    {
        /// <summary>
        /// Formats a property value into valid Html. Note that this is
        /// called concurrently on many threads for different requests.
        /// </summary>
        /// <param name="property">The property info is provided here so that
        /// your application code can see the property name, type and any
        /// custom attributes attached. In theory this would allow you to 
        /// write one formatter for your entire application but I do not 
        /// recommend it.</param>
        /// <param name="value">The value of the property to format</param>
        /// <returns>Html to write when the template is rendered</returns>
        string Format(PropertyInfo property, object value);
    }
}
