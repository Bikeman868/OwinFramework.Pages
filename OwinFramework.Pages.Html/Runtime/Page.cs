using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
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
        public virtual string RequiredPermission { get { return null; } set { } }

        /// <summary>
        /// Return false if anonymouse users are not permitted to view this page
        /// </summary>
        public virtual bool AllowAnonymous { get { return true; } set { } }

        /// <summary>
        /// Return a custom authentication check
        /// </summary>
        public virtual Func<IOwinContext, bool> AuthenticationFunc { get { return null; } }

        /// <summary>
        /// Calculates the page page title
        /// </summary>
        public Func<IRenderContext, string> TitleFunc { get; set; }

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

        private readonly IPageDependenciesFactory _dependencies;
        private readonly IDataScopeProvider _dataScopeProvider;
        private IList<IComponent> _components;
        private string _bodyStyleName;
        private IList<string> _inPageCssLines;
        private IList<string> _inPageScriptLines;
        private IList<IModule> _referencedModules;

        protected Page(IPageDependenciesFactory dependencies)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all pages in all applications that use
            // this framework!!

            _dependencies = dependencies;
            _dataScopeProvider = dependencies.DataScopeProviderFactory.Create(null);
        }

        #region Page one-time initialization

        public virtual void Initialize()
        {
            var data = new Initializationdata(AssetDeployment, this);

            if (AssetDeployment == AssetDeployment.Inherit)
            {
                data.AssetDeployment = Module == null || Module.AssetDeployment == AssetDeployment.Inherit
                    ? AssetDeployment.PerWebsite 
                    : Module.AssetDeployment;
            }

            if (Layout != null)
                Layout.Initialize(data);

            if (_components != null)
            {
                var skip = 0;
                do
                {
                    var newComponents = _components.Skip(skip).ToList();
                    foreach (var component in newComponents)
                        component.Initialize(data);
                    skip += newComponents.Count;
                } while (_components.Count > skip);
            }

            _referencedModules = new List<IModule>();
            var styles = _dependencies.CssWriterFactory.Create();
            var functions = _dependencies.JavascriptWriterFactory.Create();

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
                        _dependencies.AssetManager.AddWebsiteAssets(element.Element);
                        break;
                    case AssetDeployment.PerModule:
                        deployment = element.Module.Name + " module";
                        _dependencies.AssetManager.AddModuleAssets(element.Element, element.Module);
                        if (_referencedModules.All(m => m.Name != element.Module.Name))
                            _referencedModules.Add(element.Module);
                        break;
                    case AssetDeployment.PerPage:
                        deployment = "page assets";
                        _dependencies.AssetManager.AddPageAssets(element.Element, this);
                        break;
                    case AssetDeployment.InPage:
                        deployment = "page head";
                        element.Element.WriteStaticCss(styles);
                        element.Element.WriteStaticJavascript(functions);
                        break;
                    default:
                        deployment = element.AssetDeployment.ToString();
                        break;
                }

                System.Diagnostics.Trace.WriteLine("   " + name + " deployed to " + deployment);
            }

            _inPageCssLines = styles.ToLines();
            _inPageScriptLines = functions.ToLines();
        }

        private class Initializationdata: IInitializationData
        {
            private class State
            {
                public AssetDeployment AssetDeployment;
                public IDataScopeProvider ScopeProvider;

                public State Clone()
                {
                    return new State
                    {
                        AssetDeployment = AssetDeployment,
                        ScopeProvider = ScopeProvider
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
            private readonly Page _page;
            private State _currentState = new State();

            public Initializationdata(
                AssetDeployment assetDeployment, 
                Page page)
            {
                AssetDeployment = assetDeployment;
                _page = page;
                _currentState.ScopeProvider = _page._dataScopeProvider;
            }

            public void Push()
            {
                _stateStack.Push(_currentState);
                _currentState = _currentState.Clone();
            }

            public void Pop()
            {
                _currentState = _stateStack.Pop();
            }

            public void AddScope(IDataScopeProvider scopeProvider) 
            { 
                _currentState.ScopeProvider = scopeProvider; 
            }

            public AssetDeployment AssetDeployment
            {
                get { return _currentState.AssetDeployment; }
                set { _currentState.AssetDeployment = value; }
            }

            public IInitializationData NeedsComponent(IComponent component)
            {
                _page.AddComponent(component);
                return this;
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
            if (_components == null)
                return Layout.AsEnumerable<IElement>().GetEnumerator();

            return _components.Concat(Layout.AsEnumerable<IElement>()).GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds a non-visual component to the page. These components can write to the
        /// page header, include javscript libraries, write canonical links etc
        /// </summary>
        /// <param name="component">The component to add</param>
        public void AddComponent(IComponent component)
        {
            if (_components == null)
                _components = new List<IComponent>();

            if (_components.Any(c => string.Equals(c.Name, component.Name, StringComparison.OrdinalIgnoreCase)))
                return;

            _components.Add(component);
        }

        public override void NeedsComponent(IComponent component)
        {
            AddComponent(component);
        }

        /// <summary>
        /// Override this method to completely takeover how the page is produced
        /// </summary>
        public virtual Task Run(IOwinContext owinContext)
        {
            var dependencies = _dependencies.Create(owinContext);
            var context = dependencies.RenderContext;
            var html = context.Html;

            owinContext.Response.ContentType = "text/html";

            _dataScopeProvider.SetupDataContext(context);

            var writeResult = WriteResult.Continue();
            try
            {
                html.WriteDocumentStart(context.Language);

                WritePageHead(context, html, writeResult);
                WritePageBody(context, html, writeResult);
                WriteInitializationScript(context, true);

                html.WriteDocumentEnd();
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

        #region Outputting Html

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            var writeResult = WriteResult.Continue();

            return writeResult;
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            var writeResult = WriteResult.Continue();

            return writeResult;
        }

        public override IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (!string.IsNullOrEmpty(BodyStyle))
            {
                _dependencies.NameManager.EnsureAssetName(this, ref _bodyStyleName);
                writer.WriteRule("." + _bodyStyleName, BodyStyle);
            }

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    if (writeResult.Add(component.WriteDynamicCss(writer)).IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteDynamicCss(writer));

            return writeResult;
        }

        public override IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    if (writeResult.Add(component.WriteDynamicJavascript(writer)).IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteDynamicJavascript(writer));

            return writeResult;
        }

        public override IWriteResult WriteInitializationScript(IRenderContext renderContext, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteInitializationScript(renderContext));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteInitializationScript(renderContext));

            return writeResult;
        }

        public override IWriteResult WriteTitle(IRenderContext renderContext, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (TitleFunc != null)
            {
                renderContext.Html.WriteLine(TitleFunc(renderContext));
                return writeResult;
            }

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteTitle(renderContext));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteTitle(renderContext));

            return writeResult;
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            var websiteStylesUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Style);
            if (websiteStylesUrl != null)
            {
                renderContext.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", websiteStylesUrl.ToString());
                renderContext.Html.WriteLine();
            }

            foreach(var module in _referencedModules)
            {
                var moduleStylesUrl = _dependencies.AssetManager.GetModuleAssetUrl(module, AssetType.Style);
                if (moduleStylesUrl != null)
                {
                    renderContext.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", moduleStylesUrl.ToString());
                    renderContext.Html.WriteLine();
                }
            }

            var pageStylesUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Style);
            if (pageStylesUrl != null)
            {
                renderContext.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", pageStylesUrl.ToString());
                renderContext.Html.WriteLine();
            }

            var websiteScriptUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Script);
            if (websiteScriptUrl != null)
            {
                renderContext.Html.WriteElement("script", "type", "text/javascript", "src", websiteScriptUrl.ToString());
                renderContext.Html.WriteLine();
            }

            foreach (var module in _referencedModules)
            {
                var moduleScriptUrl = _dependencies.AssetManager.GetModuleAssetUrl(module, AssetType.Script);
                if (moduleScriptUrl != null)
                {
                    renderContext.Html.WriteElement("script", null, "type", "text/javascript", "src", moduleScriptUrl.ToString());
                    renderContext.Html.WriteLine();
                }
            }

            var pageScriptUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Script);
            if (pageScriptUrl != null)
            {
                renderContext.Html.WriteElement("script", null, "type", "text/javascript", "src", pageScriptUrl.ToString());
                renderContext.Html.WriteLine();
            }

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteHead(renderContext));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteHead(renderContext));

            return writeResult;
        }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return Layout == null ? WriteResult.Continue() : Layout.WriteHtml(context);
        }

        #endregion

        #region Private methods

        private void WritePageHead(IRenderContext context, IHtmlWriter html, IWriteResult writeResult)
        {
            html.WriteOpenTag("head");

            html.WriteOpenTag("title");
            writeResult.Add(WriteTitle(context, true));
            html.WriteCloseTag("title");

            writeResult.Add(WriteHead(context, true));

            if (_inPageCssLines != null && _inPageCssLines.Count > 0)
            {
                if (context.IncludeComments)
                    html.WriteComment("static in-page styles");

                html.WriteOpenTag("style");

                foreach(var line in _inPageCssLines)
                    html.WriteLine(line);
                
                html.WriteCloseTag("style");
            }

            if (_inPageScriptLines != null && _inPageScriptLines.Count > 0)
            {
                if (context.IncludeComments)
                    html.WriteComment("static in-page javascript");

                html.WriteScriptOpen();
                foreach (var line in _inPageScriptLines)
                    html.WriteLine(line);
                html.WriteScriptClose();
            }

            using (var cssWriter = _dependencies.CssWriterFactory.Create())
            {
                var result = WriteDynamicCss(cssWriter, true);
                result.Wait();
                if (cssWriter.HasContent)
                {
                    if (context.IncludeComments)
                        html.WriteComment("dynamic styles");

                    html.WriteOpenTag("style");
                    cssWriter.ToHtml(context.Html);
                    html.WriteCloseTag("style");
                }
            }

            using (var javascriptWriter = _dependencies.JavascriptWriterFactory.Create())
            {
                var result = WriteDynamicJavascript(javascriptWriter, true);
                result.Wait();
                if (javascriptWriter.HasContent)
                {
                    if (context.IncludeComments)
                        html.WriteComment("dynamic javascript");

                    html.WriteScriptOpen();
                    javascriptWriter.ToHtml(context.Html);
                    html.WriteScriptClose();
                }
            }
            
            html.WriteCloseTag("head");
        }

        private void WritePageBody(IRenderContext context, IHtmlWriter html, IWriteResult writeResult)
        {
            var bodyClassNames = BodyClassNames;
            if (!string.IsNullOrEmpty(BodyStyle))
            {
                _dependencies.NameManager.EnsureAssetName(this, ref _bodyStyleName);

                if (string.IsNullOrEmpty(bodyClassNames))
                    bodyClassNames = _bodyStyleName;
                else
                    bodyClassNames += " " + _bodyStyleName;
            }

            if (string.IsNullOrEmpty(bodyClassNames))
                html.WriteOpenTag("body");
            else
                html.WriteOpenTag("body", "class", bodyClassNames);

            writeResult.Add(WriteHtml(context, true));
            html.WriteCloseTag("body");
        }

        #endregion

        public DebugPage GetDebugInfo()
        {
            return new DebugPage
            {
                Name = Name,
                Instance = this,
                Layout = Layout == null ? null : Layout.GetDebugInfo()
            };
        }
    }
}
