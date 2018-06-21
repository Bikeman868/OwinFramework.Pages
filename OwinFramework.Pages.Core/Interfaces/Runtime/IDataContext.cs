using Microsoft.Owin;

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
        /// Retrieves strongly types data from the data context
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
