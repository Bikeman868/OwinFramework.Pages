namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// Defines a class that retrieves templates from a source (folder
    /// structure, database, web service etc) parses these templates
    /// and adds them to the template manager.
    /// </summary>
    public interface ITemplateLoader
    {
        /// <summary>
        /// Loads templates from the template source and parses them using
        /// the supplied parser then registeres them with the template manager
        /// </summary>
        /// <param name="parser">The parser to use to construct the ITemplate
        /// instance from the template file</param>
        /// <param name="rootPath">Where to map these templates to in the template 
        /// path heirachy. You only need to specify this parameter if you are loading
        /// multiple sets of templates from different sources</param>
        /// <param name="package">Optionally sets the package used for name resolution</param>
        /// <param name="includeSubPaths">Root path only or all sub-paths?</param>
        /// <param name="locales">Pass a list of locales to load. Pass null or
        /// empty list to load all locales</param>
        void Load(ITemplateParser parser, IPackage package = null, string rootPath = "/", bool includeSubPaths = true, params string[] locales);
    }
}
