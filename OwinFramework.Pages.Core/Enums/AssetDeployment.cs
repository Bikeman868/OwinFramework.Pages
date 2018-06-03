namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// When components and regions generate JavaScript and CSS
    /// it must be delivered to the browser. This browser delivery
    /// mechanism can be chosen for each component and each region
    /// or they can inhetit from their parent container.
    /// The default behaviour is to generate one JavaScript and one
    /// CSS for the whole website. Individual components or regions
    /// can override this just for that component or region.
    /// </summary>
    public enum AssetDeployment
    {
        /// <summary>
        /// The default is to inherit from the parent for everything
        /// except modules whos default is to serve one file for each
        /// module.
        /// </summary>
        Inherit = 0, 

        /// <summary>
        /// This option causes CSS and JavaScript to be output into the
        /// head section of the page
        /// </summary>
        InPage,

        /// <summary>
        /// This option causes the CSS and JavaScript to be rendered 
        /// into memory once for each page. The page will fetch the
        /// CSS and JavaScript after the page is retrieved. The CSS and
        /// JavaScript returned will only contain what is referenced
        /// by the components and regions on the paage.
        /// </summary>
        PerPage,

        /// <summary>
        /// This option causes the creation of large CSS and JavaScript
        /// files to be generated and served that contain all of the 
        /// CSS and JavaScript for the module. All pages in the module
        /// will receive the same CSS and JavaScript files
        /// </summary>
        PerModule,

        /// <summary>
        /// With this option only one CSS and JavaScript file is 
        /// generated for the entire website and served to every page
        /// on the website
        /// </summary>
        PerWebsite
    }
}
