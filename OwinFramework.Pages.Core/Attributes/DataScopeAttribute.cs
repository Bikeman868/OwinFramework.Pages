using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// This can be attached to a Page or a zone to define how a particular type
    /// of data will be retrieved for this element and its children.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DataScopeAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines a new data scope.
        /// This forces retrieval of the data again but from a different source. For
        /// example you might have IUser in the context of the page, but if the
        /// user is looking at another user's profile page there will be a part of
        /// the page that is in context of IUser for the profile being viewed. These
        /// two use cases for the same data type are what scopes are for.
        /// </summary>
        /// <param name="dataType">The type of data that is being scoped</param>
        /// <param name="scope">Specifies the scope name to use to find the
        /// appropriate data provider. Other elements can also specifically
        /// depend on this scope of they want to bind to data from higher up
        /// the element tree</param>
        public DataScopeAttribute(Type dataType, string scope)
        {
            DataType = dataType;
            Scope = scope;
        }

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
