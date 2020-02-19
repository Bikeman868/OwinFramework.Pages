using System.Reflection;

namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// Classes that implement this intraface are used to evaluate 
    /// if property values are considered true for the purpose of
    /// conditionally including template content
    /// </summary>
    public interface ITruthyEvaluator
    {
        /// <summary>
        /// Tests a property to see if it is considered truthy
        /// </summary>
        /// <param name="property">The property info is provided here so that
        /// your application code can see the property name, type and any
        /// custom attributes attached</param>
        /// <param name="value">The value of the property to evaluate</param>
        /// <returns>True if this property is truthy</returns>
        bool IsTruthy(PropertyInfo property, object value);
    }
}
