namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines a factory that returns resuable arrays.
    /// </summary>
    public interface IArrayFactory
    {
        /// <summary>
        /// Constructs and returns a fixed length array
        /// </summary>
        /// <param name="size">The number of elements in the array</param>
        IArray<T> CreateExact<T>(long size);

        /// <summary>
        /// Constructs and returns an array that will expand
        /// automatically to accomodate writes to higher elements
        /// than exist within the array
        /// </summary>
        /// <param name="size">The starting size of the array. Enlarging the
        /// array is expensive so set this to a value cose to what you expect 
        /// the final size of the array to be</param>
        IArray<T> CreateAtLeast<T>(long size);
    }
}
