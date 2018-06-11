using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to specify the name of a data context that
    /// is required for this page
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NeedsDataAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines some data
        /// need of the element
        /// </summary>
        /// <param name="dataContextName">The name of a data context that is
        /// required by this element</param>
        public NeedsDataAttribute(string dataContextName)
        {
            DataContextName = dataContextName;
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines some data
        /// need of the element
        /// </summary>
        /// <param name="dataType">The type of data needed by this element</param>
        public NeedsDataAttribute(Type dataType)
        {
            DataType = dataType;
        }

        /// <summary>
        /// The name of the required data context
        /// </summary>
        public string DataContextName { get; set; }

        /// <summary>
        /// The type of data required
        /// </summary>
        public Type DataType { get; set; }
    }
}
