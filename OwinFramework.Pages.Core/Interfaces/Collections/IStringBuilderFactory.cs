namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Generates disposable string builders that are returned to a pool when
    /// they are disposed and reused to avoid thrashing the garbage collector 
    /// </summary>
    public interface IStringBuilderFactory
    {
        /// <summary>
        /// Creates a new disposable/reusable string builder. You must dispose of
        /// the string builder when you are finished with it so that it can be reused
        /// </summary>
        IStringBuilder Create();

        /// <summary>
        /// Creates a new disposable/reusable string builder. You must dispose of
        /// the string builder when you are finished with it so that it can be reused
        /// </summary>
        /// <param name="capacity">Pass a value that is bigger than most of the 
        /// strings you will build</param>
        IStringBuilder Create(long capacity);

        /// <summary>
        /// Creates a new disposable/reusable string builder. You must dispose of
        /// the string builder when you are finished with it so that it can be reused
        /// </summary>
        /// <param name="data">The string data to initialize the string builder with</param>
        IStringBuilder Create(string data);
    }
}
