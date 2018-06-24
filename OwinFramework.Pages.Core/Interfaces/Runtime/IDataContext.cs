using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// An instance of this type is constructed and passed to the
    /// element rendering pipeline when a data binding context is
    /// established. When there is no data binding the overhead of
    /// creating a data context is avoided.
    /// </summary>
    public interface IDataContext
    {
        /// <summary>
        /// Gets and sets the current scope. The scope can be used to
        /// determine which data provider will be used to supply missing 
        /// data for the rendering operation. If the Scope is not set then
        /// the parent scope will be returned. Setting a value of null cancels
        /// any override returning the scope to inheriting from the parent 
        /// </summary>
        string Scope { get; set; }

        /// <summary>
        /// If the specified data provider has not been executed in this
        /// context then it is executed to add data to this context
        /// </summary>
        /// <param name="provider">The provider to run</param>
        void Ensure(IDataProvider provider);

        /// <summary>
        /// Makes sure that the data context contains the specified
        /// type of data
        /// </summary>
        /// <param name="type">The type of data required in the context</param>
        void Ensure(Type type);

        /// <summary>
        /// Stores strongly typed data into the data context
        /// </summary>
        /// <typeparam name="T">The type of data to store</typeparam>
        /// <param name="value">The value to store</param>
        /// <param name="name">If you are storing more than one data item 
        /// of the same type, use this name to identify each instance</param>
        /// <param name="level">Use this parameter to write into an ancestor
        /// of the current data context. 0 means this instance, 1 means the
        /// parent context, 2 means the grandparent etc</param>
        void Set<T>(T value, string name = null, int level = 0);

        /// <summary>
        /// Retrieves strongly typed data from the data context
        /// </summary>
        /// <typeparam name="T">The type of data to get</typeparam>
        /// <param name="name">The name can be used where there are multiple
        /// data items with the same type. Not required otherwise</param>
        /// <param name="required">Pass true if this data is essential to be
        /// able to continue. If the data context does not already have this
        /// data then it will try to find a contxt handler that can provide
        /// it, and run this context handler first. If no suitable context
        /// handlers are available then an exception is thrown.</param>
        T Get<T>(string name = null, bool required = true);

        /// <summary>
        /// Retrieves strongly typed data from the data context
        /// </summary>
        /// <param name="type">The type of data to retrieve</param>
        /// <param name="name">The name can be used where there are multiple
        /// data items with the same type. Not required otherwise</param>
        /// <param name="required">Pass true if this data is essential to be
        /// able to continue. If the data context does not already have this
        /// data then it will try to find a contxt handler that can provide
        /// it, and run this context handler first. If no suitable context
        /// handlers are available then an exception is thrown.</param>
        object Get(Type type, string name = null, bool required = true);

        /// <summary>
        /// This is more efficient than calling Get() once for each type.
        /// The data context performs one pass filling in all missing data.
        /// All data is assumed unnamed and required, if this is not the case
        /// then call the Get() method for these types of data
        /// </summary>
        /// <param name="types">A list of the types of data required in
        /// context. If any of these are missing they will be added by
        /// finding suitavle data providers</param>
        IList<object> GetMultiple(IList<Type> types);

        /// <summary>
        /// Stores and retrieves name/value pairs with no strong typing.
        /// </summary>
        /// <param name="name">The case insensitive name to get/set</param>
        string this[string name] { get; set; }

        /// <summary>
        /// Constructs a new data context that inherits from the current one.
        /// All of the values from the parent can be read using the child context.
        /// When setting values they apply to the child context only by default
        /// and the original parent context is not affected.
        /// It is important NOT to dispose the parent before disposing of the child
        /// </summary>
        IDataContext CreateChild();
    }
}
