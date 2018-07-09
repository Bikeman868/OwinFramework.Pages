using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Implemented by elements that can assets deployed in modules
    /// </summary>
    public interface IDeployable
    {
        /// <summary>
        /// Optional module that this elements assets are deployed to
        /// </summary>
        IModule Module { get; set; }

        /// <summary>
        /// Defines how the assets are deployed for this element
        /// </summary>
        AssetDeployment AssetDeployment { get; set; }

        /// <summary>
        /// This is where the element is responsible for outputting its static CSS classes.
        /// Static classes are defined as styles whose names are fixed at design time
        /// and can therefore be written to a file and served statically. The
        /// name will be prefixed with a namespace from the package it belomgs to.
        /// This method can be called during page rendering to write the assets
        /// into the page, it could be called once at startup to create an asset
        /// file, or it could be called during deployment to produce an asset
        /// file that is deployed and served statically
        /// </summary>
        /// <param name="writer">The text writer to srite them to</param>
        /// <rereturns>An object indicating how/when the write was completed.
        /// You can return null if the write completed normally and synchronously</rereturns>
        IWriteResult WriteStaticCss(ICssWriter writer);

        /// <summary>
        /// This is where the element is responsible for outputting its static Javascript functions.
        /// Static functions are defined as functions whose names are fixed at design time
        /// and can therefore be written to a file and served statically. The
        /// name will be prefixed with a namespace from the package it belomgs to.
        /// This method can be called during page rendering to write the assets
        /// into the page, it could be called once at startup to create an asset
        /// file, or it could be called during deployment to produce an asset
        /// file that is deployed and served statically
        /// </summary>
        /// <param name="writer">The text writer to srite them to</param>
        /// <rereturns>An object indicating how/when the write was completed.
        /// You can return null if the write completed normally and synchronously</rereturns>
        IWriteResult WriteStaticJavascript(IJavascriptWriter writer);

    }
}
