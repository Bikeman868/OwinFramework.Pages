using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Classes that implement this interface can be queried for information
    /// to help debug issues with the application
    /// </summary>
    public interface IDebuggable
    {
        /// <summary>
        /// Gets debuging information for this element
        /// </summary>
        /// <param name="parentDepth">Pass 0 for no parent, 1 for immediate parent, 
        /// 2 for grandparents etc. Pass a negative value to include all ancestors</param>
        /// <param name="childDepth">Pass 0 for no children, 1 for immediate children,
        /// 2 for grandchildren etc. Pass a negative value to include all descendents</param>
        T GetDebugInfo<T>(int parentDepth = 0, int childDepth = 1) where T: DebugInfo;
    }

    /// <summary>
    /// Extension methods that allow any class to implements IDebuggable
    /// </summary>
    public static class DebuggableExtensions
    {
        /// <summary>
        /// Gets debug information from objects that support it
        /// </summary>
        /// <typeparam name="T">The type of debug info expected from this object</typeparam>
        /// <param name="o">The object to extract debug info from</param>
        /// <param name="parentDepth">Pass 0 for no parent, 1 for immediate parent, 
        /// 2 for grandparents etc. Pass a negative value to include all ancestors</param>
        /// <param name="childDepth">Pass 0 for no children, 1 for immediate children,
        /// 2 for grandchildren etc. Pass a negative value to include all descendents</param>
        public static T GetDebugInfo<T>(this object o, int parentDepth = 0, int childDepth = 1) 
            where T : DebugInfo
        {
            var debuggable = o as IDebuggable;
            return ReferenceEquals(debuggable, null) 
                ? null 
                : debuggable.GetDebugInfo<T>(parentDepth, childDepth);
        }

        /// <summary>
        /// Gets debug information from objects that support it
        /// </summary>
        /// <param name="o">The object to extract debug info from</param>
        /// <param name="parentDepth">Pass 0 for no parent, 1 for immediate parent, 
        /// 2 for grandparents etc. Pass a negative value to include all ancestors</param>
        /// <param name="childDepth">Pass 0 for no children, 1 for immediate children,
        /// 2 for grandchildren etc. Pass a negative value to include all descendents</param>
        public static DebugInfo GetDebugInfo(this object o, int parentDepth = 0, int childDepth = 1)
        {
            var debuggable = o as IDebuggable;
            return ReferenceEquals(debuggable, null) ? null : debuggable.GetDebugInfo<DebugInfo>(parentDepth, childDepth);
        }
    }
}
