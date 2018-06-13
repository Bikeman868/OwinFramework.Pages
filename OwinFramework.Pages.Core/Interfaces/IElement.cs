﻿using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// All classes that contribute to response rendering inherit from this interface
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// The unique name of this element for its type within the package
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Optional package - not all elements have to be packaged
        /// </summary>
        IPackage Package { get; set; }

        /// <summary>
        /// Defines how the assets are deployed for this element
        /// </summary>
        AssetDeployment AssetDeployment { get; set; }

        /// <summary>
        /// This is called once only after name resolution
        /// </summary>
        void Initialize();

        /// <summary>
        /// Returns a disposable enumerator for the children of this element. By calling
        /// this for each of the returned elements recusrively it is possible to traverse
        /// the entire element tree.
        /// </summary>
        IEnumerator<IElement> GetChildren();

        /// <summary>
        /// This is where the element is responsible for outputting its static assets.
        /// Static assets are defined as assets whose names are fixed at design time
        /// and can therefore be written to a file and served statically. The
        /// name will be prefixed with a namespace from the package it belomgs to.
        /// This method can be called during page rendering to write the assets
        /// into the page, it could be called once at startup to create an asset
        /// file, or it could be called during deployment to produce an asset
        /// file that is deployed and served statically
        /// </summary>
        /// <param name="assetType">The type of asset to write</param>
        /// <param name="writer">The text writer to srite them to</param>
        /// <rereturns>An object indicating how/when the write was completed.
        /// You can return null if the write completed normally and synchronously</rereturns>
        IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer);

        /// <summary>
        /// This is where the element is responsible for outputting its dynamic assets.
        /// Dynamic assets are defined as assets whose names are randomly
        /// generated at runtime and are therefore different on each web server.
        /// These types of asset are always rendered into the page.
        /// The recommended approach is to generate these names using the INameManager
        /// in the constructor of your element and keep using the same names
        /// for the lifetime of the instance.
        /// This method might be called in the context of rendering a page, or it might
        /// be called once for the page and the output cached for reuse on all subsequent
        /// pages.
        /// </summary>
        /// <param name="assetType">The type of asset to write</param>
        /// <param name="writer">The text writer to srite them to</param>
        IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer);

        /// <summary>
        /// This is where the element gets an opportunity to write JavaScript into the page
        /// after all of the html has been written. This Javascript will run then the
        /// page loads.
        /// </summary>
        /// <param name="renderContext">The page rendering context for reference</param>
        /// <param name="dataContext">The data binding context or null if none has been established</param>
        /// <rereturns>An object indicating how/when the write was completed.
        /// You can return null if the write completed normally and synchronously</rereturns>
        IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext);

        /// <summary>
        /// This method is called during page rendering and is this elements opportunity
        /// to output the page title.
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="dataContext">The data binding context or null if none has been established</param>
        /// <rereturns>An object indicating how/when the write was completed.
        /// You can return null if the write completed normally and synchronously</rereturns>
        IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext);

        /// <summary>
        /// This method is called during page rendering and is this elements opportunity
        /// to output html into the head part of the page. This can be used to write page
        /// style sheet references, canonical links etc.
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="dataContext">The data binding context or null if none has been established</param>
        /// <rereturns>An object indicating how/when the write was completed.
        /// You can return null if the write completed normally and synchronously</rereturns>
        IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext);

        /// <summary>
        /// This method is called during page rendering and is this elements opportunity
        /// to output html into the page at the place where this element is on the page
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="dataContext">The data binding context or null if none has been established</param>
        /// <rereturns>An object indicating how/when the write was completed.
        /// You can return null if the write completed normally and synchronously</rereturns>
        IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext);
    }
}
