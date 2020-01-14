using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public class Page : Element, IPage, IDataScopeRules, IDataConsumer, IDataContextBuilder
    {
        /// <summary>
        /// Gets and sets the name of the permission that the user must have to view this page
        /// </summary>
        public virtual string RequiredPermission { get; set; }

        /// <summary>
        /// Gets and sets the optional path to a protected resource that is used to 
        /// further qualify the RequiredPermission
        /// </summary>
        public virtual string SecureResource { get; set; }

        /// <summary>
        /// Return false if anonymouse users are not permitted to view this page
        /// </summary>
        public virtual bool AllowAnonymous { get; set; }

        /// <summary>
        /// Return a custom authentication check
        /// </summary>
        public virtual Func<IOwinContext, bool> AuthenticationFunc { get; set; }

        /// <summary>
        /// Calculates the page page title
        /// </summary>
        public Func<IRenderContext, string> TitleFunc { get; set; }

        /// <summary>
        /// Calculates the page canonical Url
        /// </summary>
        public virtual Func<IRenderContext, string> CanonicalUrlFunc { get; set; }

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
        /// The category name to pass to the output cache to define the caching rules for this page
        /// </summary>
        public string CacheCategory { get; set; }

        /// <summary>
        /// The importance of caching this page, inverse of the cost of producing it
        /// </summary>
        public CachePriority CachePriority { get; set; }

        /// <summary>
        /// Sets the element type to Page
        /// </summary>
        public override ElementType ElementType { get { return ElementType.Page; } }

        private Dictionary<string, IElement> _layoutZones;

        private readonly IPageDependenciesFactory _dependencies;
        private readonly IDataScopeRules _dataScopeRules;
        private readonly IDataConsumer _dataConsumer;
        private IDataContextBuilder _dataContextBuilder;
        private IList<IComponent> _components;
        private string _bodyStyleName;
        private IList<string> _inPageCssLines;
        private IList<string> _inPageScriptLines;
        private IList<IModule> _referencedModules;
        private PageLayout _layout;
        private PageComponent[] _pageComponents; 

        public Page(IPageDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all pages in all applications that use
            // this framework!!

            _dependencies = dependencies;
            _dataScopeRules = dependencies.DataScopeProviderFactory.Create();
            _dataConsumer = dependencies.DataConsumerFactory.Create();

            AllowAnonymous = true;
            CachePriority = CachePriority.Never;
        }

        #region Page one-time initialization

        public virtual void Initialize()
        {
            var data = new PageData(_dependencies, this);

            var elementDependencies = new PageElementDependencies
            {
                DictionaryFactory = _dependencies.DictionaryFactory,
                DataDependencyFactory = _dependencies.DataDependencyFactory
            };

            if (Layout != null)
            {
                var regionElements = new List<Tuple<string, IRegion, IElement>>();
                foreach(var zoneName in Layout.GetZoneNames())
                {
                    var element = _layoutZones != null && _layoutZones.ContainsKey(zoneName)
                        ? _layoutZones[zoneName]
                        : Layout.GetElement(zoneName);
                    var region = element as IRegion;

                    if (region == null)
                        region = Layout.GetRegion(zoneName);
                    else
                        element = null;

                    regionElements.Add(new Tuple<string, IRegion, IElement>(zoneName, region, element));
                }
                _layout = new PageLayout(elementDependencies, null, Layout, regionElements, data);
            }

            if (_components == null)
            {
                _pageComponents = new PageComponent[0];
            }
            else
            {
                _pageComponents = _components
                    .ToList()
                    .Select(c => new PageComponent(elementDependencies, null, c, data))
                    .ToArray();

                foreach (var component in _components)
                {
                    data.BeginAddElement(component, null);
                    data.EndAddElement(component);
                }
            }

            _dataContextBuilder = data.RootDataContextBuilder;
            _dataContextBuilder.ResolveSupplies();

            _referencedModules = new List<IModule>();
            var styles = _dependencies.CssWriterFactory.Create();
            var functions = _dependencies.JavascriptWriterFactory.Create();

#if TRACE
            System.Diagnostics.Trace.WriteLine("Page '" + Name + "' asset deployment");
#endif
            var elements = new HashSet<string>();

            foreach (var element in data.Elements)
            {
                var name = string.IsNullOrEmpty(element.Element.Name)
                    ? element.Element.GetType().Name
                    : element.Element.Name;

                var elementUniqueName = element.Element.ElementType.ToString() + ":" + name;
                if (!elements.Add(elementUniqueName))
                    continue;

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
                        deployment = element.AssetDeployment.ToString().ToLower();
                        break;
                }

#if DEBUG
                System.Diagnostics.Trace.WriteLine("   " + name + " deployed to " + deployment);
#endif
            }

            _inPageCssLines = styles.ToLines();
            _inPageScriptLines = functions.ToLines();
        }

        #endregion

        public void PopulateRegion(string zoneName, IElement element)
        {
            if (_layoutZones == null)
                _layoutZones = new Dictionary<string, IElement>(StringComparer.OrdinalIgnoreCase);
            _layoutZones[zoneName] = element;
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
        public virtual Task Run(IOwinContext owinContext, Action<IOwinContext, Func<string>> trace)
        {
            var dependencies = _dependencies.Create(owinContext, trace);
            var context = dependencies.RenderContext;
            var html = context.Html;
            owinContext.Response.ContentType = "text/html";

            trace(owinContext, () => "Request is for a Pages Framework webpage");

#if TRACE
            context.Trace(() => "Adding static data to the render context");
            context.TraceIndent();
#endif
            _dataContextBuilder.SetupDataContext(context);
#if TRACE
            context.TraceOutdent();
#endif

            var writeResult = WriteResult.Continue();
            try
            {
                writeResult.Add(WritePage(context, trace));
            }
            catch (Exception ex)
            {
#if TRACE
                context.Trace(e => "Exception thrown " + e.Message, ex);
#endif
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

        #region Writing page html structure

        private IWriteResult WritePage(IRenderContext context, Action<IOwinContext, Func<string>> trace)
        {
            var result = WriteResult.Continue();
            var html = context.Html;

#if TRACE
            context.Trace(() => "Writing page HTML");
            context.TraceIndent();
#endif
            html.WriteDocumentStart(context.Language);

            result.Add(WritePageHead(context));
            result.Add(WritePageBody(context));

            html.WriteDocumentEnd();
#if TRACE
            context.TraceOutdent();
#endif

            return result;
        }

        private IWriteResult WritePageHead(IRenderContext context)
        {
            var result = WriteResult.Continue();
            var html = context.Html;

            html.WriteOpenTag("head");
            html.WriteLine();

            html.WriteOpenTag("title");
            result.Add(WritePageArea(context, PageArea.Title));
            html.WriteCloseTag("title");
            html.WriteLine();

            result.Add(WritePageArea(context, PageArea.Head));
            result.Add(WritePageArea(context, PageArea.Styles));
            result.Add(WritePageArea(context, PageArea.Scripts));

            html.WriteCloseTag("head");
            html.WriteLine();

            return result;
        }

        private IWriteResult WritePageBody(IRenderContext context)
        {
            var result = WriteResult.Continue();
            var html = context.Html;

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
            html.WriteLine();

            result.Add(WritePageArea(context, PageArea.Body));
            result.Add(WritePageArea(context, PageArea.Initialization));

            html.WriteCloseTag("body");
            html.WriteLine();

            return result;
        }

        #endregion

        #region Overriding base class methods for rendering Html

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            var writeResult = WriteResult.Continue();

            if (!ReferenceEquals(_pageComponents, null))
            {
                for (var i = 0; i < _pageComponents.Length; i++)
                {
                    var pageComponent = _pageComponents[i];
                    if (writeResult.Add(pageComponent.WriteStaticCss(writer)).IsComplete)
                        return writeResult;
                }
            }

            if (!ReferenceEquals(_layout, null))
                writeResult.Add(_layout.WriteStaticCss(writer));

            return writeResult;
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            var writeResult = WriteResult.Continue();

            if (!ReferenceEquals(_pageComponents, null))
            {
                for (var i = 0; i < _pageComponents.Length; i++)
                {
                    var pageComponent = _pageComponents[i];
                    if (writeResult.Add(pageComponent.WriteStaticJavascript(writer)).IsComplete)
                        return writeResult;
                }
            }

            if (!ReferenceEquals(_layout, null))
                writeResult.Add(_layout.WriteStaticJavascript(writer));

            return writeResult;
        }

        public override IWriteResult WriteInPageStyles(
            ICssWriter writer,
            Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            var writeResult = WriteResult.Continue();

            if (!string.IsNullOrEmpty(BodyStyle))
            {
                _dependencies.NameManager.EnsureAssetName(this, ref _bodyStyleName);
                writer.WriteRule("." + _bodyStyleName, BodyStyle);
            }

            if (!ReferenceEquals(_pageComponents, null))
            {
                for (var i = 0; i < _pageComponents.Length; i++)
                {
                    var pageComponent = _pageComponents[i];
                    if (writeResult.Add(pageComponent.WriteStyles(writer)).IsComplete)
                        return writeResult;
                }
            }

            if (!ReferenceEquals(_layout, null))
                writeResult.Add(_layout.WriteStyles(writer));

            return writeResult;
        }

        public override IWriteResult WriteInPageFunctions(
            IJavascriptWriter writer,
            Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            var writeResult = WriteResult.Continue();

            if (!ReferenceEquals(_pageComponents, null))
            {
                for (var i = 0; i < _pageComponents.Length; i++)
                {
                    var pageComponent = _pageComponents[i];
                    if (writeResult.Add(pageComponent.WriteScripts(writer)).IsComplete)
                        return writeResult;
                }
            }

            if (!ReferenceEquals(_layout, null))
                writeResult.Add(_layout.WriteScripts(writer));

            return writeResult;
        }

        #endregion

        #region Page specific html rendering methods

        public virtual IWriteResult WritePageArea(
            IRenderContext renderContext,
            PageArea pageArea)
        {
            var writeResult = WriteResult.Continue();

#if TRACE
            renderContext.Trace(() => "Writing the page " + Enum.GetName(typeof(PageArea), pageArea));
            renderContext.TraceIndent();

            try
            {
#endif
                if (pageArea == PageArea.Title)
                {
                    if (ReferenceEquals(TitleFunc, null))
                    {
                        if (_layout != null)
                            writeResult.Add(_layout.WritePageArea(renderContext, PageArea.Title));
                    }
                    else
                    {
                        renderContext.Html.Write(TitleFunc(renderContext));
                        return writeResult;
                    }
                }

                switch (pageArea)
                {
                    case PageArea.Head:
                        writeResult.Add(WriteHeadArea(renderContext));
                        break;
                    case PageArea.Styles:
                        writeResult.Add(WriteStylesArea(renderContext));
                        break;
                    case PageArea.Scripts:
                        writeResult.Add(WriteScriptsArea(renderContext));
                        break;
                    case PageArea.Body:
                        writeResult.Add(WriteBodyArea(renderContext));
                        break;
                    case PageArea.Initialization:
                        writeResult.Add(WriteInitializationArea(renderContext));
                        break;
                }

                if (!ReferenceEquals(_pageComponents, null))
                {
                    for (var i = 0; i < _pageComponents.Length; i++)
                    {
                        var pageComponent = _pageComponents[i];
                        if (writeResult.Add(pageComponent.WritePageArea(renderContext, pageArea)).IsComplete)
                            return writeResult;
                    }
                }
#if TRACE
            }
            finally
            {
                renderContext.TraceOutdent();
            }
#endif
            return writeResult;
        }

        public virtual IWriteResult WriteStylesArea(IRenderContext renderContext)
        {
#if TRACE
            renderContext.Trace(() => "Writing styles");
            renderContext.TraceIndent();
#endif
            var html = renderContext.Html;

            if (_inPageCssLines == null || _inPageCssLines.Count == 0)
            {
#if TRACE
                renderContext.Trace(() => "page has no static CSS");
#endif
            }
            else
            {
#if TRACE
                renderContext.Trace(() => _inPageCssLines.Count + " lines of static in-page styles");
#endif
                if (renderContext.IncludeComments)
                    html.WriteComment("static in-page styles");

                html.WriteOpenTag("style");
                html.WriteLine();

                foreach (var line in _inPageCssLines)
                    html.WriteLine(line);

                html.WriteCloseTag("style");
                html.WriteLine();
            }

            using (var cssWriter = _dependencies.CssWriterFactory.Create())
            {
                var writeResult = WriteResult.Continue();

                if (!ReferenceEquals(_pageComponents, null))
                {
#if TRACE
                    renderContext.Trace(() => _pageComponents.Length + " non-visual components");
#endif
                    for (var i = 0; i < _pageComponents.Length; i++)
                    {
                        var pageComponent = _pageComponents[i];
                        writeResult.Add(pageComponent.WriteStyles(cssWriter));
                    }
                }

                if (!ReferenceEquals(_layout, null))
                {
#if TRACE
                    renderContext.Trace(() => "layout styles");
#endif
                    writeResult.Add(_layout.WriteStyles(cssWriter));
                }

                writeResult.Wait();

                if (cssWriter.HasContent)
                {
#if TRACE
                    renderContext.Trace(() => "page has dynamic styles");
#endif
                    if (renderContext.IncludeComments)
                        html.WriteComment("dynamic styles");

                    html.WriteOpenTag("style");
                    html.WriteLine();

                    cssWriter.ToHtml(html);

                    html.WriteCloseTag("style");
                    html.WriteLine();
                }
                else
                {
#if TRACE
                    renderContext.Trace(() => "page does not have any dynamic styles");
#endif
                }
            }

#if TRACE
            renderContext.TraceOutdent();
#endif

            if (!ReferenceEquals(_layout, null))
                return _layout.WritePageArea(renderContext, PageArea.Styles);

            return WriteResult.Continue();
        }


        /// <summary>
        /// Override this method to take over how the styles part of 
        /// the page head is written
        /// </summary>
        public virtual IWriteResult WriteScriptsArea(IRenderContext renderContext)
        {
#if TRACE
            renderContext.Trace(() => "Writing JavaScript functions");
            renderContext.TraceIndent();
#endif
            var html = renderContext.Html;

            if (_inPageScriptLines == null || _inPageScriptLines.Count == 0)
            {
#if TRACE
                renderContext.Trace(() => "page has no static JavaScript");
#endif
            }
            else
            {
#if TRACE
                renderContext.Trace(() => "page has " + _inPageScriptLines.Count + " lines of static JavaScript");
#endif
                if (renderContext.IncludeComments)
                    html.WriteComment("static in-page javascript");

                html.WriteScriptOpen();

                foreach (var line in _inPageScriptLines)
                    html.WriteLine(line);

                html.WriteScriptClose();
            }

            using (var javascriptWriter = _dependencies.JavascriptWriterFactory.Create())
            {
                var writeResult = WriteResult.Continue();

                if (!ReferenceEquals(_pageComponents, null) && _pageComponents.Length > 0)
                {
#if TRACE
                    renderContext.Trace(() => _pageComponents.Length + " non-visual components");
#endif
                    for (var i = 0; i < _pageComponents.Length; i++)
                    {
                        var pageComponent = _pageComponents[i];
                        writeResult.Add(pageComponent.WriteScripts(javascriptWriter));
                    }
                }

                if (!ReferenceEquals(_layout, null))
                {
#if TRACE
                    renderContext.Trace(() => "layout JavaScript");
#endif
                    writeResult.Add(_layout.WriteScripts(javascriptWriter));
                }

                writeResult.Wait();

                if (javascriptWriter.HasContent)
                {
#if TRACE
                    renderContext.Trace(() => "page has dynamic JavaScript");
#endif
                    if (renderContext.IncludeComments)
                        html.WriteComment("dynamic javascript");

                    html.WriteScriptOpen();
                    javascriptWriter.ToHtml(html);
                    html.WriteScriptClose();
                }
                else
                {
#if TRACE
                    renderContext.Trace(() => "page does not have any dynamic JavaScript");
#endif
                }
            }

#if TRACE
            renderContext.TraceOutdent();
#endif

            if (!ReferenceEquals(_layout, null))
                return _layout.WritePageArea(renderContext, PageArea.Scripts);

            return WriteResult.Continue();
        }

        /// <summary>
        /// Override this method to take over how the head part of 
        /// the page is written
        /// </summary>
        public virtual IWriteResult WriteHeadArea(IRenderContext context)
        {
            var html = context.Html;

            if (CanonicalUrlFunc != null)
            {
                html.WriteUnclosedElement(
                    "link", 
                    "rel", "canonical", 
                    "href", CanonicalUrlFunc(context));
                html.WriteLine();
            }

            var websiteStylesUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Style);
            if (websiteStylesUrl != null)
            {
#if TRACE
                context.Trace(() => "Writing link to website css " + websiteStylesUrl);
#endif
                html.WriteUnclosedElement(
                    "link", 
                    "rel", "stylesheet", 
                    "type", "text/css", 
                    "href", websiteStylesUrl.ToString());
                html.WriteLine();
            }

            if (_referencedModules != null && _referencedModules.Count > 0)
            {
#if TRACE
                context.Trace(() => "Writing links to css for referenced modules");
                context.TraceIndent();
#endif
                foreach (var module in _referencedModules)
                {
                    var moduleStylesUrl = _dependencies.AssetManager.GetModuleAssetUrl(module, AssetType.Style);
                    if (moduleStylesUrl != null)
                    {
#if TRACE
                        context.Trace(() => "Writing link to css for module " + moduleStylesUrl);
#endif
                        html.WriteUnclosedElement(
                            "link", 
                            "rel", "stylesheet", 
                            "type", "text/css", 
                            "href", moduleStylesUrl.ToString());
                        html.WriteLine();
                    }
                }
#if TRACE
                context.TraceOutdent();
#endif
            }

            var pageStylesUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Style);
            if (pageStylesUrl != null)
            {
#if TRACE
                context.Trace(() => "Writing links to page specific css " + pageStylesUrl);
#endif
                html.WriteUnclosedElement(
                    "link", 
                    "rel", "stylesheet", 
                    "type", "text/css", 
                    "href", pageStylesUrl.ToString());
                html.WriteLine();
            }

            var websiteScriptUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Script);
            if (websiteScriptUrl != null)
            {
#if TRACE
                context.Trace(() => "Writing link to website JavaScript " + websiteScriptUrl);
#endif
                html.WriteElement(
                    "script", null,
                    "type", "text/javascript", 
                    "src", websiteScriptUrl.ToString());
                html.WriteLine();
            }

            if (_referencedModules != null && _referencedModules.Count > 0)
            {
#if TRACE
                context.Trace(() => "Writing links to JavaScript for referenced modules");
                context.TraceIndent();
#endif

                foreach (var module in _referencedModules)
                {
                    var moduleScriptUrl = _dependencies.AssetManager.GetModuleAssetUrl(module, AssetType.Script);
                    if (moduleScriptUrl != null)
                    {
#if TRACE
                        context.Trace(() => "Writing link to JavaScript for module " + moduleScriptUrl);
#endif
                        html.WriteElement(
                            "script", null, 
                            "type", "text/javascript", 
                            "src", moduleScriptUrl.ToString());
                        html.WriteLine();
                    }
                }

#if TRACE
                context.TraceOutdent();
#endif
            }

            var pageScriptUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Script);
            if (pageScriptUrl != null)
            {
#if TRACE
                context.Trace(() => "Writing link to page specific JavaScript " + pageScriptUrl);
#endif
                html.WriteElement(
                    "script", null, 
                    "type", "text/javascript", 
                    "src", pageScriptUrl.ToString());
                html.WriteLine();
            }

            if (!ReferenceEquals(_layout, null))
                return _layout.WritePageArea(context, PageArea.Head);

            return WriteResult.Continue();
        }

        /// <summary>
        /// Override this method to replace the body of the page with custom Html
        /// </summary>
        public virtual IWriteResult WriteBodyArea(IRenderContext renderContext)
        {
            if (!ReferenceEquals(_layout, null))
                return _layout.WritePageArea(renderContext, PageArea.Body);

            return WriteResult.Continue();
        }

        /// <summary>
        /// Override this method to replace the JavaScript initialization area at the 
        /// bottom of the page with your custom implementation
        /// </summary>
        public virtual IWriteResult WriteInitializationArea(IRenderContext renderContext)
        {
            if (!ReferenceEquals(_layout, null))
                return _layout.WritePageArea(renderContext, PageArea.Initialization);

            return WriteResult.Continue();
        }

        #endregion

        #region Debug info

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            _dataScopeRules.ElementName = "Page " + Name;

            if (typeof(T).IsAssignableFrom(typeof(DebugPage)))
            {
                var debugPage = debugInfo as DebugPage ?? new DebugPage();

                debugPage.RequiredPermission = RequiredPermission;
                debugPage.DataContext = _dataContextBuilder.GetDebugInfo<DebugDataScopeRules>(0, -1);
                debugPage.Scope = _dataScopeRules.GetDebugInfo<DebugDataScopeRules>(0, -1);

                if (childDepth != 0)
                    debugPage.Layout = _layout.GetDebugInfo<DebugLayout>(0, childDepth - 1);

                return base.PopulateDebugInfo<T>(debugPage, parentDepth, childDepth);
            }

            if (typeof(T).IsAssignableFrom(typeof(DebugDataScopeRules)))
            {
                var debugDataScopeRules = _dataScopeRules.GetDebugInfo<T>(parentDepth, childDepth);
                return base.PopulateDebugInfo<T>(debugDataScopeRules, parentDepth, childDepth);
            }

            if (typeof(T).IsAssignableFrom(typeof(DebugDataConsumer)))
            {
                var debugDataConsumer = _dataConsumer.GetDebugInfo<T>(parentDepth, childDepth);
                return base.PopulateDebugInfo<T>(debugDataConsumer, parentDepth, childDepth);
            }

            return base.PopulateDebugInfo<T>(debugInfo, parentDepth, childDepth);
        }

        public override string ToString()
        {
            if (ReferenceEquals(Layout, null))
                return "page with no layout";
            return "page with '" + Layout + "' layout";
        }

        #endregion

        #region IDataScopeRules Mixin

        string IDataScopeRules.ElementName
        {
            get { return _dataScopeRules.ElementName; }
            set { _dataScopeRules.ElementName = value; }
        }

        void IDataScopeRules.AddScope(Type type, string scopeName)
        {
            _dataScopeRules.AddScope(type, scopeName);
        }

        void IDataScopeRules.AddSupplier(IDataSupplier supplier, IDataDependency dependencyToSupply)
        {
            _dataScopeRules.AddSupplier(supplier, dependencyToSupply);
        }

        void IDataScopeRules.AddSupply(IDataSupply supply)
        {
            _dataScopeRules.AddSupply(supply);
        }

        IList<IDataScope> IDataScopeRules.DataScopes
        {
            get { return _dataScopeRules.DataScopes; }
        }

        IList<Tuple<IDataSupplier, IDataDependency>> IDataScopeRules.SuppliedDependencies
        {
            get { return _dataScopeRules.SuppliedDependencies; }
        }

        IList<IDataSupply> IDataScopeRules.DataSupplies
        {
            get { return _dataScopeRules.DataSupplies; }
        }

        #endregion

        #region IDataContextBuilder Mixin

        int IDataContextBuilder.Id { get { return _dataContextBuilder.Id; } }

        bool IDataContextBuilder.IsInScope(IDataDependency dependency)
        {
            return _dataContextBuilder.IsInScope(dependency);
        }

        void IDataContextBuilder.AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            _dataContextBuilder.AddMissingData(renderContext, missingDependency);
        }

        void IDataContextBuilder.AddConsumer(IDataConsumer consumer)
        {
            _dataContextBuilder.AddConsumer(consumer);
        }

        IDataContextBuilder IDataContextBuilder.AddChild(IDataScopeRules dataScopeRules)
        {
            return _dataContextBuilder.AddChild(dataScopeRules);
        }

        void IDataContextBuilder.SetupDataContext(IRenderContext renderContext)
        {
            _dataContextBuilder.SetupDataContext(renderContext);
        }

        void IDataContextBuilder.ResolveSupplies()
        {
            _dataContextBuilder.ResolveSupplies();
        }

        #endregion

        #region IDataConsumer Mixin

        void IDataConsumer.HasDependency(IDataSupply dataSupply)
        {
            _dataConsumer.HasDependency(dataSupply);
        }

        void IDataConsumer.HasDependency<T>(string scopeName)
        {
            _dataConsumer.HasDependency<T>(scopeName);
        }

        void IDataConsumer.HasDependency(Type dataType, string scopeName)
        {
            _dataConsumer.HasDependency(dataType, scopeName);
        }

        void IDataConsumer.CanUseData<T>(string scopeName)
        {
            _dataConsumer.CanUseData<T>(scopeName);
        }

        void IDataConsumer.CanUseData(Type dataType, string scopeName)
        {
            _dataConsumer.CanUseData(dataType, scopeName);
        }

        void IDataConsumer.HasDependency(IDataSupplier dataSupplier, IDataDependency dependency)
        {
            _dataConsumer.HasDependency(dataSupplier, dependency);
        }

        IDataConsumerNeeds IDataConsumer.GetConsumerNeeds()
        {
            return _dataConsumer.GetConsumerNeeds();
        }

        #endregion
    }
}
