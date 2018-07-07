using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a data provider to specify additional data that
    /// this data provider can provide
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SuppliesDataAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines data supplied
        /// by this data provider
        /// </summary>
        public SuppliesDataAttribute(Type dataType, string scope = null)
        {
            DataType = dataType;
            Scope = scope;
        }

        /// <summary>
        /// The type of data supplied
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Selects a specific scope to put the data into
        /// </summary>
        public string Scope { get; set; }
    }
}
