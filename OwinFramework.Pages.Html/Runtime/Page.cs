using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public abstract class Page: Element, IPage
    {
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
        /// Calculates the page page title
        /// </summary>
        public Func<IRenderContext, IDataContext, string> TitleFunc { get; set; }

        /// <summary>
        /// Defines the layout of this page
        /// </summary>
        public ILayout Layout { get; set; }

        /// <summary>
        /// The names of the css class to attach to the body element
        /// </summary>
        public string BodyClassNames { get; set; }

        /// <summary>
        /// The names of the css class to attach to the body element
        /// </summary>
        public string BodyStyle { get; set; }

        /// <summary>
        /// Sets the element type to Page
        /// </summary>
        public override ElementType ElementType { get { return ElementType.Page; } }

        private readonly IPageDependenciesFactory _dependenciesFactory;
        private IList<IComponent> _components;
        private string _bodyStyleName;
        private string _inPageCsss;
        private string _inPageScript;
        private List<IModule> _referencedModules;

        protected Page(IPageDependenciesFactory dependenciesFactory)
        {
            _dependenciesFactory = dependenciesFactory;
        }

        public virtual void Initialize()
        {
            var data = new Initializationdata
            { 
                AssetDeployment = AssetDeployment
            };

            if (AssetDeployment == AssetDeployment.Inherit)
            {
                data.AssetDeployment = Module == null || Module.AssetDeployment == AssetDeployment.Inherit
                    ? AssetDeployment.PerWebsite 
                    : Module.AssetDeployment;
            }

            if (Layout != null) 
                Layout.Initialize(data);

            // TODO: Group elements by module and deployment method
            // TODO: Create in-page static asset html

            _inPageCsss = "p { font-weight: normal; }";
            _inPageScript = "var i = 0;";
            _referencedModules = new List<IModule>();

            System.Diagnostics.Trace.WriteLine("Page " + Name);
            foreach (var element in data.Elements)
            {
                var name = string.IsNullOrEmpty(element.Element.Name)
                    ? element.Element.GetType().Name
                    : element.Element.Name;

                string deployment;
                switch (element.AssetDeployment)
                {
                    case AssetDeployment.PerWebsite:
                        deployment = "website assets";
                        _dependenciesFactory.AssetManager.AddWebsiteAssets(element.Element);
                        break;
                    case AssetDeployment.PerModule:
                        deployment = element.Module.Name + " module";
                        _dependenciesFactory.AssetManager.AddModuleAssets(element.Element, element.Module);
                        if (_referencedModules.All(m => m.Name != element.Module.Name))
                            _referencedModules.Add(element.Module);
                        break;
                    case AssetDeployment.PerPage:
                        deployment = "page assets";
                        _dependenciesFactory.AssetManager.AddPageAssets(element.Element, this);
                        break;
                    case AssetDeployment.InPage:
                        deployment = "page head";
                        break;
                    default:
                        deployment = element.AssetDeployment.ToString();
                        break;
                }

                System.Diagnostics.Trace.WriteLine("   " + name + " deployed to " + deployment);
            }
        }

        private class Initializationdata: IInitializationData
        {
            private class State
            {
                public AssetDeployment AssetDeployment;

                public State Clone()
                {
                    return new State
                    {
                        AssetDeployment = AssetDeployment
                    };
                }
            }

            public class ElementRegistration
            {
                public IElement Element;
                public AssetDeployment AssetDeployment;
                public IModule Module;
            }

            public readonly List<ElementRegistration> Elements = new List<ElementRegistration>();

            private readonly Stack<State> _stateStack = new Stack<State>();
            private State _currentState = new State();

            public void Push()
            {
                _stateStack.Push(_currentState);
                _currentState = _currentState.Clone();
            }

            public void Pop()
            {
                _currentState = _stateStack.Pop();
            }

            public AssetDeployment AssetDeployment
            {
                get { return _currentState.AssetDeployment; }
                set { _currentState.AssetDeployment = value; }
            }

            public void HasElement(
                IElement element, 
                AssetDeployment assetDeployment, 
                IModule module)
            {
                Elements.Add(new ElementRegistration
                    {
                        Element = element,
                        AssetDeployment = assetDeployment,
                        Module = module
                    });
            }
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Layout.AsEnumerable<IElement>().GetEnumerator();
        }

        /// <summary>
        /// Adds a non-visual component to the page. These components can write to the
        /// page header, include javscript libraries, write canonical links etc
        /// </summary>
        /// <param name="component">The component to add</param>
        public void AddComponent(IComponent component)
        {
            if (_components == null)
                _components = new List<IComponent>();

            _components.Add(component);
        }

        public override IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            var writeResult = WriteResult.Continue();

            return writeResult;
        }

        /// <summary>
        /// Override this method to completely takeover how the page is produced
        /// </summary>
        public virtual Task Run(IOwinContext owinContext)
        {
            var dependencies = _dependenciesFactory.Create(owinContext);
            var context = dependencies.RenderContext;
            var html = context.Html;
            var data = dependencies.DataContext;

            owinContext.Response.ContentType = "text/html";

            var writeResult = WriteResult.Continue();
            try
            {
                html.WriteLine("<!doctype html>");
                html.WriteOpenTag("html", "itemtype", "http://schema.org/WebPage", "lang", context.Language);

                WritePageHead(context, data, html, writeResult);
                WritePageBody(context, data, html, writeResult);
                WriteInitializationScript(context, data, true);

                html.WriteCloseTag("html");
            }
            catch
            {
                writeResult.Wait(true);
                writeResult.Dispose();
                dependencies.Dispose();

                throw;
            }

            return Task.Factory.StartNew(() =>
                {
                    writeResult.Wait();
                    writeResult.Dispose();
                    html.ToResponse(owinContext);
                    dependencies.Dispose();
                });
        }

        public override IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (assetType == AssetType.Style && !string.IsNullOrEmpty(BodyStyle))
            {
                _dependenciesFactory.NameManager.EnsureAssetName(this, ref _bodyStyleName);
                writer.WriteElement("style", "." + _bodyStyleName + " { " + BodyStyle + " }");
                writer.WriteLine();
            }

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteDynamicAssets(assetType, writer));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteDynamicAssets(assetType, writer));

            return writeResult;
        }

        public override IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteInitializationScript(renderContext, dataContext));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteInitializationScript(renderContext, dataContext));

            return writeResult;
        }

        public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (TitleFunc != null)
            {
                renderContext.Html.WriteLine(TitleFunc(renderContext, dataContext));
                return writeResult;
            }

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteTitle(renderContext, dataContext));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteTitle(renderContext, dataContext));

            return writeResult;
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            var websiteStylesUrl = _dependenciesFactory.AssetManager.GetWebsiteAssetUrl(AssetType.Style);
            if (websiteStylesUrl != null)
            {
                renderContext.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", websiteStylesUrl.ToString());
                renderContext.Html.WriteLine();
            }

            foreach(var module in _referencedModules)
            {
                var moduleStylesUrl = _dependenciesFactory.AssetManager.GetModuleAssetUrl(module, AssetType.Style);
                if (moduleStylesUrl != null)
                {
                    renderContext.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", moduleStylesUrl.ToString());
                    renderContext.Html.WriteLine();
                }
            }

            var pageStylesUrl = _dependenciesFactory.AssetManager.GetPageAssetUrl(this, AssetType.Style);
            if (pageStylesUrl != null)
            {
                renderContext.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", pageStylesUrl.ToString());
                renderContext.Html.WriteLine();
            }

            var websiteScriptUrl = _dependenciesFactory.AssetManager.GetWebsiteAssetUrl(AssetType.Script);
            if (websiteScriptUrl != null)
                renderContext.Html.WriteElement("script", "type", "text/javascript", "src", websiteScriptUrl.ToString());

            foreach (var module in _referencedModules)
            {
                var moduleScriptUrl = _dependenciesFactory.AssetManager.GetModuleAssetUrl(module, AssetType.Script);
                if (moduleScriptUrl != null)
                    renderContext.Html.WriteElement("script", "type", "text/javascript", "src", moduleScriptUrl.ToString());
            }

            var pageScriptUrl = _dependenciesFactory.AssetManager.GetPageAssetUrl(this, AssetType.Script);
            if (pageScriptUrl != null)
                renderContext.Html.WriteElement("script", "type", "text/javascript", "src", pageScriptUrl.ToString());

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteHead(renderContext, dataContext));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteHead(renderContext, dataContext));

            return writeResult;
        }

        public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            return Layout == null ? WriteResult.Continue() : Layout.WriteHtml(renderContext, dataContext);
        }

        #region Private methods

        private void WritePageHead(IRenderContext context, IDataContext data, IHtmlWriter html, IWriteResult writeResult)
        {
            html.WriteOpenTag("head");

            html.WriteOpenTag("title");
            writeResult.Add(WriteTitle(context, data, true));
            html.WriteCloseTag("title");

            writeResult.Add(WriteHead(context, data, true));

            if (!string.IsNullOrEmpty(_inPageCsss))
            {
                html.WriteOpenTag("style");
                html.WriteLine(_inPageCsss);
                html.WriteCloseTag("style");
            }

            if (!string.IsNullOrEmpty(_inPageScript))
            {
                html.WriteOpenTag("script", "type", "text/javascript");
                html.WriteLine("//<![CDATA[");
                html.IndentLevel++;
                html.WriteLine(_inPageScript);
                html.IndentLevel--;
                html.WriteLine("//]]>");
                html.WriteCloseTag("script");
            }

            writeResult.Add(WriteDynamicAssets(AssetType.Style, context.Html, true));
            writeResult.Add(WriteDynamicAssets(AssetType.Script, context.Html, true));
            
            html.WriteCloseTag("head");
        }

        private void WritePageBody(IRenderContext context, IDataContext data, IHtmlWriter html, IWriteResult writeResult)
        {
            var bodyClassNames = BodyClassNames;
            if (!string.IsNullOrEmpty(BodyStyle))
            {
                _dependenciesFactory.NameManager.EnsureAssetName(this, ref _bodyStyleName);

                if (string.IsNullOrEmpty(bodyClassNames))
                    bodyClassNames = _bodyStyleName;
                else
                    bodyClassNames += " " + _bodyStyleName;
            }

            if (string.IsNullOrEmpty(bodyClassNames))
                html.WriteOpenTag("body");
            else
                html.WriteOpenTag("body", "class", bodyClassNames);

            writeResult.Add(WriteHtml(context, data, true));
            html.WriteCloseTag("body");
        }

        #endregion
    }
}
