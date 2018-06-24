using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to specify the data context that
    /// is required for this element. You don not have to declare this with
    /// an attribute, the data binding system will search for a data context
    /// automatically, but this allows you to attach the data to a node higher
    /// up the element tree
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NeedsDataAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines some data
        /// need of the element
        /// </summary>
        /// <param name="dataProviderName">The name of a data provider to run before
        /// rendering this element. If multiple elements need the same data profiver
        /// then it will be run only once for each request</param>
        public NeedsDataAttribute(string dataProviderName)
        {
            DataProviderName = dataProviderName;
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines some data
        /// need of the element
        /// </summary>
        /// <param name="dataType">The type of data needed by this element</param>
        /// <param name="scope">Optional scope specifier. When not specified data
        /// from the closest parent region that has this data will be used regardless
        /// of the scope. When scope is specified the closest parent region with
        /// this type and scope will be used.</param>
        public NeedsDataAttribute(Type dataType, string scope = null)
        {
            DataType = dataType;
            Scope = scope;
        }

        /// <summary>
        /// The name of the required data context
        /// </summary>
        public string DataProviderName { get; set; }

        /// <summary>
        /// The type of data required
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Selects a specific scope to get the data from
        /// </summary>
        public string Scope { get; set; }
    }
}
