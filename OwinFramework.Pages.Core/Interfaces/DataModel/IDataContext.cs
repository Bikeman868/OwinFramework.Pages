using System;
using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// An instance of this type is constructed and passed to the
    /// element rendering pipeline when a data binding context is
    /// established. When there is no data binding the overhead of
    /// creating a data context is avoided.
    /// </summary>
    public interface IDataContext: IDisposable
    {
        /// <summary>
        /// Retrieves debugging information from this data context
        /// </summary>
        DebugDataContext GetDebugInfo();

        /// <summary>
        /// Gets and sets the current scope provider. This will only
        /// be used in the case where a component tries to retrieve
        /// data from the data context at runtime that it did not declare
        /// as a dependency. In this case the dependency will be
        /// learned at runtime and the data scope provider will
        /// supply this type of data during initialization on future
        /// requests.
        /// </summary>
        IDataScopeProvider Scope { get; set; }

        /// <summary>
        /// Stores strongly typed data into the data context
        /// </summary>
        /// <typeparam name="T">The type of data to store</typeparam>
        /// <param name="value">The value to store</param>
        /// <param name="scopeName">Optionally allows you to set the
        /// data that will be returned when a specific scope is requested.
        /// This is not normally required because the data context will
        /// defer to its parent and the parents are organized
        /// heirachically by scope already</param>
        /// <param name="level">Use this parameter to write into an ancestor
        /// of the current data context. 0 means this instance, 1 means the
        /// parent context, 2 means the grandparent etc</param>
        void Set<T>(T value, string scopeName = null, int level = 0);

        /// <summary>
        /// Stores data in the data context
        /// </summary>
        /// <param name="type">The type to store store the data as</param>
        /// <param name="value">The data to store</param>
        /// <param name="scopeName">Optionally allows you to set the
        /// data that will be returned when a specific scope is requested.
        /// This is not normally required because the data context will
        /// defer to its parent and the parents are organized
        /// heirachically by scope already</param>
        /// <param name="level">Use this parameter to write into an ancestor
        /// of the current data context. 0 means this instance, 1 means the
        /// parent context, 2 means the grandparent etc</param>
        void Set(Type type, object value, string scopeName = null, int level = 0);

        /// <summary>
        /// Retrieves strongly typed data from the data context
        /// </summary>
        /// <typeparam name="T">The type of data to get</typeparam>
        /// <param name="scopeName">By default data is returned from the
        /// current scope. This is usually the desired behaviour because
        /// it allows you to reuse the same component in different contexts
        /// and have it bind to whatever is in scope. This parameter is
        /// provided to override this default behaviour and request data
        /// that is not in scope for the current operation. The data must
        /// be in scope somewhere in the ancestors</param>
        /// <param name="required">Pass true if this data is essential to be
        /// able to continue. If the data context does not already have this
        /// data then it will try to find a contxt handler that can provide
        /// it, and run this context handler first. If no suitable context
        /// handlers are available then an exception is thrown.</param>
        T Get<T>(string scopeName = null, bool required = true);

        /// <summary>
        /// Retrieves strongly typed data from the data context
        /// </summary>
        /// <param name="type">The type of data to retrieve</param>
        /// <param name="scopeName">By default data is returned from the
        /// current scope. This is usually the desired behaviour because
        /// it allows you to reuse the same component in different contexts
        /// and have it bind to whatever is in scope. This parameter is
        /// provided to override this default behaviour and request data
        /// that is not in scope for the current operation. The data must
        /// be in scope somewhere in the ancestors</param>
        /// <param name="required">Pass true if this data is essential to be
        /// able to continue. If the data context does not already have this
        /// data then it will try to find a contxt handler that can provide
        /// it, and run this context handler first. If no suitable context
        /// handlers are available then an exception is thrown.</param>
        object Get(Type type, string scopeName = null, bool required = true);

        /// <summary>
        /// Constructs a new data context that inherits from the current one.
        /// All of the values from the parent can be read using the child context.
        /// When setting values they apply to the child context only by default
        /// and the original parent context is not affected.
        /// It is important NOT to dispose the parent before disposing of the child
        /// </summary>
        /// <param name="scopeProvider">Attaching a scope provider to the new
        /// data context establishes this context as a new data scope</param>
        IDataContext CreateChild(IDataScopeProvider scopeProvider = null);
    }
}
