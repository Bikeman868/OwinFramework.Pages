using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Core.BaseClasses
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public abstract class Page: IPage
    {
        /// <summary>
        /// A uniqie name for this page within the package
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Optional package that this page belongs to
        /// </summary>
        public virtual IPackage Package { get; set; }

        /// <summary>
        /// Returns the name of the permission that the user must have to view this page
        /// </summary>
        public virtual string RequiredPermission { get { return null; } }

        /// <summary>
        /// Return false if anonymouse users are not permitted to view this page
        /// </summary>
        public virtual bool AllowAnonymous { get{return true; } }

        /// <summary>
        /// Return a custom authentication check
        /// </summary>
        public virtual Func<IOwinContext, bool> AuthenticationFunc { get { return null; } }

        /// <summary>
        /// You must override this in derrived classes to define how the page produced the
        /// response.
        /// </summary>
        public abstract Task Run(IOwinContext context);

    }
}
