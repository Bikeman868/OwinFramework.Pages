namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines an engine that knows how to build centain types of element
    /// </summary>
    public interface IBuildEngine
    {
        /// <summary>
        /// Installs the build engine into an element registrar. This element
        /// registrar will use the capabilities of this build engine for
        /// elements that are registered with it.
        /// </summary>
        /// <param name="builder">The builder to install this engine into</param>
        void Install(IFluentBuilder builder);
    }
}
