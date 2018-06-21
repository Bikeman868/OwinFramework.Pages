using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a class to define it as a context provider for a
    /// specific type of data. It must implement the IDataContextProvider interface.
    /// You can add multiple attributes if the context provider can provide more than
    /// one kind of data
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class IsDataProviderAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies marks this class as a context provider
        /// </summary>
        /// <param name="type">The type of data added to the data context by this provider. Should be either an
        /// interface type or IList of an interface type</param>
        /// <param name="scope">Indicates the areas of the website where this context handler should be used</param>
        public IsDataProviderAttribute(Type type, string scope = null)
        {
            Type = type;
            Scope = scope;
        }

        /// <summary>
        /// Constructs and initializes an attribute that specifies marks this class as a context provider
        /// </summary>
        /// <param name="name">Identifies this as a named data provider. Other controls can request
        /// this specific data provider by name rather than resolving the data type and scope</param>
        public IsDataProviderAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// You can optionally name your context handlers and refer to them by
        /// name when requesting content
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of data added to the data context. This should usually be
        /// an interface type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Optionally restricts this data provider to a named scope. This will
        /// be used to resolve which context provider to use when there are multiple
        /// providers of the same type of data
        /// </summary>
        public string Scope { get; set; }
    }
}
