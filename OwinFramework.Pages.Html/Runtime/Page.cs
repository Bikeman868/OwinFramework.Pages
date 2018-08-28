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
using OwinFramework.Pages.Html.Elements;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public class Page: Element, IPage, IDataScopeProvider, IDataConsumer
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

        private Dictionary<string, IElement> _regions;

        private readonly IPageDependenciesFactory _dependencies;
        private readonly IDataScopeProvider _dataScopeProvider;
        private readonly IDataConsumer _dataConsumer;
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
            _dataScopeProvider = dependencies.DataScopeProviderFactory.Create();
            _dataConsumer = dependencies.DataConsumerFactory.Create();
        }

        #region Page one-time initialization

        public virtual void Initialize()
        {
            var data = new PageData(AssetDeployment, this);

            if (AssetDeployment == AssetDeployment.Inherit)
            {
                data.AssetDeployment = Module == null || Module.AssetDeployment == AssetDeployment.Inherit
                    ? AssetDeployment.PerWebsite 
                    : Module.AssetDeployment;
            }


            var elementDependencies = new PageElementDependencies
            {
                DictionaryFactory = _dependencies.DictionaryFactory
            };

            if (Layout != null)
            {
                var regionElements = new List<Tuple<string, IRegion, IElement>>();
                foreach(var regionName in Layout.GetRegionNames())
                {
                    var region = Layout.GetRegion(regionName);
                    var element = _regions.ContainsKey(regionName)
                        ? _regions[regionName]
                        : Layout.GetRegion(regionName);
                    regionElements.Add(new Tuple<string, IRegion, IElement>(regionName, region, element));
                }
                _layout = new PageLayout(elementDependencies, null, Layout, regionElements, data);
            }

            _pageComponents = _components == null
                ? new PageComponent[0]
                : _components.Select(c => new PageComponent(elementDependencies, null, c, data)).ToArray();

            _referencedModules = new List<IModule>();
            var styles = _dependencies.CssWriterFactory.Create();
            var functions = _dependencies.JavascriptWriterFactory.Create();

#if TRACE
            System.Diagnostics.Trace.WriteLine("Page " + Name + " asset deployment");
#endif
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
                        deployment = element.AssetDeployment.ToString().ToLower();
                        break;
                }

#if TRACE
                System.Diagnostics.Trace.WriteLine("   " + name + " deployed to " + deployment);
#endif
            }

            _inPageCssLines = styles.ToLines();
            _inPageScriptLines = functions.ToLines();
        }

        private class PageData: IPageData
        {
            private class State
            {
                public AssetDeployment AssetDeployment;
                public IDataScopeProvider ScopeProvider;
                public string MessagePrefix;

                public State Clone()
                {
                    return new State
                    {
                        AssetDeployment = AssetDeployment,
                        ScopeProvider = ScopeProvider,
                        MessagePrefix = MessagePrefix + "  "
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

            public PageData(
                AssetDeployment assetDeployment, 
                Page page)
            {
                AssetDeployment = assetDeployment;
                _page = page;

                _page._dataScopeProvider.Initialize(null);
                _currentState.MessagePrefix = "Init " + page.Name + ": ";
                _currentState.ScopeProvider = page._dataScopeProvider;
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
                Log("Adding " + scopeProvider);
                scopeProvider.Initialize(_currentState.ScopeProvider);
                _currentState.ScopeProvider = scopeProvider; 
            }

            public AssetDeployment AssetDeployment
            {
                get { return _currentState.AssetDeployment; }
                set { _currentState.AssetDeployment = value; }
            }

            public IDataScopeProvider ScopeProvider
            {
                get { return _currentState.ScopeProvider; }
                set { _currentState.ScopeProvider = value; }
            }

            public IPageData NeedsComponent(IComponent component)
            {
                Log("Needs " + component);
                _page.AddComponent(component);
                return this;
            }

            public void HasElement(
                IElement element, 
                AssetDeployment assetDeployment, 
                IModule module)
            {
                Log("Has " + element);
                Elements.Add(new ElementRegistration
                    {
                        Element = element,
                        AssetDeployment = assetDeployment,
                        Module = module
                    });
            }

            public void Log(string message)
            {
#if TRACE
                if (string.IsNullOrEmpty(message)) return;
                System.Diagnostics.Trace.WriteLine(_currentState.MessagePrefix + message);
#endif
            }
        }

        #endregion

        public void PopulateRegion(string regionName, IElement element)
        {
            if (_regions == null)
                _regions = new Dictionary<string, IElement>(StringComparer.OrdinalIgnoreCase);
            _regions[regionName] = element;
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

#if TRACE
            context.Trace(() => "Adding static data to the render context");
            context.TraceIndent();
#endif
            _dataScopeProvider.SetupDataContext(context);
#if TRACE
            context.TraceOutdent();
#endif

            var writeResult = WriteResult.Continue();
            try
            {
                writeResult.Add(WritePage(context));
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

        private IWriteResult WritePage(IRenderContext context)
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

            html.Write("<title>");
            result.Add(WritePageArea(context, _dataScopeProvider, PageArea.Title));
            html.WriteLine("</title>");

            result.Add(WritePageArea(context, _dataScopeProvider, PageArea.Head));
            result.Add(WritePageArea(context, _dataScopeProvider, PageArea.Styles));
            result.Add(WritePageArea(context, _dataScopeProvider, PageArea.Scripts));

            html.WriteCloseTag("head");

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

            result.Add(WritePageArea(context, _dataScopeProvider, PageArea.Body));
            result.Add(WritePageArea(context, _dataScopeProvider, PageArea.Initialization));

            html.WriteCloseTag("body");

            return result;
        }

        #endregion

        #region Overriding base class methods for rendering Html

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            var result = WriteResult.Continue();

            //if (!ReferenceEquals(_pageComponents, null))
            //{
            //    for (var i = 0; i < _pageComponents.Length; i++)
            //    {
            //        var pageComponent = _pageComponents[i];
            //        result.Add(pageComponent.Element.WriteStaticCss(writer));
            //    }
            //}

            //if (!ReferenceEquals(_layout, null))
            //    result.Add(_layout.Element.WriteStaticCss(writer));

            return result;
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            var result = WriteResult.Continue();

            //if (!ReferenceEquals(_pageComponents, null))
            //{
            //    for (var i = 0; i < _pageComponents.Length; i++)
            //    {
            //        var pageComponent = _pageComponents[i];
            //        result.Add(pageComponent.Element.WriteStaticJavascript(writer));
            //    }
            //}

            //if (!ReferenceEquals(_layout, null))
            //    result.Add(_layout.Element.WriteStaticJavascript(writer));

            return result;
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
            IDataContextBuilder dataContextBuilder,
            PageArea pageArea)
        {
            var writeResult = WriteResult.Continue();

#if TRACE
            renderContext.Trace(() => "Writing the page " + Enum.GetName(typeof(PageArea), pageArea));
            renderContext.TraceIndent();

            try
            {
#endif
                if (pageArea == PageArea.Title && !ReferenceEquals(TitleFunc, null))
                {
                    renderContext.Html.Write(TitleFunc(renderContext));
                    return writeResult;
                }

                switch (pageArea)
                {
                    case PageArea.Styles:
                        writeResult.Add(WriteStylesArea(renderContext, dataContextBuilder));
                        break;
                    case PageArea.Scripts:
                        writeResult.Add(WriteScriptsArea(renderContext, dataContextBuilder));
                        break;
                    case PageArea.Head:
                        writeResult.Add(WriteHeadArea(renderContext, dataContextBuilder));
                        break;
                    case PageArea.Body:
                        writeResult.Add(WriteBodyArea(renderContext, dataContextBuilder));
                        break;
                    case PageArea.Initialization:
                        writeResult.Add(WriteInitializationArea(renderContext, dataContextBuilder));
                        break;
                }

                if (!ReferenceEquals(_pageComponents, null))
                {
                    for (var i = 0; i < _pageComponents.Length; i++)
                    {
                        var pageComponent = _pageComponents[i];
                        if (writeResult.Add(pageComponent.WritePageArea(renderContext, dataContextBuilder, pageArea)).IsComplete)
                            return writeResult;
                    }
                }

                if (!ReferenceEquals(_layout, null))
                    writeResult.Add(_layout.WritePageArea(renderContext, dataContextBuilder, pageArea));
#if TRACE
            }
            finally
            {
                renderContext.TraceOutdent();
            }
#endif
            return writeResult;
        }

        public virtual IWriteResult WriteStylesArea(
            IRenderContext renderContext,
            IDataContextBuilder dataContextBuilder)
        {
            var html = renderContext.Html;

            if (_inPageCssLines != null && _inPageCssLines.Count > 0)
            {
                if (renderContext.IncludeComments)
                    html.WriteComment("static in-page styles");

                html.WriteOpenTag("style");

                foreach (var line in _inPageCssLines)
                    html.WriteLine(line);

                html.WriteCloseTag("style");
            }

            using (var cssWriter = _dependencies.CssWriterFactory.Create())
            {
                var writeResult = WriteResult.Continue();

                if (!ReferenceEquals(_pageComponents, null))
                {
                    for (var i = 0; i < _pageComponents.Length; i++)
                    {
                        var pageComponent = _pageComponents[i];
                        writeResult.Add(pageComponent.WriteStyles(cssWriter));
                    }
                }

                if (!ReferenceEquals(_layout, null))
                    writeResult.Add(_layout.WriteStyles(cssWriter));

                writeResult.Wait();

                if (cssWriter.HasContent)
                {
                    if (renderContext.IncludeComments)
                        html.WriteComment("dynamic styles");

                    html.WriteOpenTag("style");
                    cssWriter.ToHtml(html);
                    html.WriteCloseTag("style");
                }
            }

            return WriteResult.Continue();
        }

        public virtual IWriteResult WriteScriptsArea(
            IRenderContext renderContext,
            IDataContextBuilder dataContextBuilder)
        {
            var html = renderContext.Html;

            if (_inPageScriptLines != null && _inPageScriptLines.Count > 0)
            {
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

                if (!ReferenceEquals(_pageComponents, null))
                {
                    for (var i = 0; i < _pageComponents.Length; i++)
                    {
                        var pageComponent = _pageComponents[i];
                        writeResult.Add(pageComponent.WriteScripts(javascriptWriter));
                    }
                }

                if (!ReferenceEquals(_layout, null))
                    writeResult.Add(_layout.WriteScripts(javascriptWriter));

                writeResult.Wait();

                if (javascriptWriter.HasContent)
                {
                    if (renderContext.IncludeComments)
                        html.WriteComment("dynamic javascript");

                    html.WriteScriptOpen();
                    javascriptWriter.ToHtml(html);
                    html.WriteScriptClose();
                }
            }

            return WriteResult.Continue();
        }

        public virtual IWriteResult WriteHeadArea(
            IRenderContext context,
            IDataContextBuilder dataContextBuilder)
        {
            var html = context.Html;

            var websiteStylesUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Style);
            if (websiteStylesUrl != null)
            {
#if TRACE
                context.Trace(() => "Writing link to website css " + websiteStylesUrl);
#endif
                html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", websiteStylesUrl.ToString());
                html.WriteLine();
            }

            if (_referencedModules != null)
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
                        html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", moduleStylesUrl.ToString());
                        html.WriteLine();
                    }
                }
                context.TraceOutdent();
            }

            var pageStylesUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Style);
            if (pageStylesUrl != null)
            {
#if TRACE
                context.Trace(() => "Writing links to page specific css " + pageStylesUrl);
#endif
                html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", pageStylesUrl.ToString());
                html.WriteLine();
            }

            var websiteScriptUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Script);
            if (websiteScriptUrl != null)
            {
#if TRACE
                context.Trace(() => "Writing link to website JavaScript " + websiteScriptUrl);
#endif
                html.WriteElement("script", "type", "text/javascript", "src", websiteScriptUrl.ToString());
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
                        html.WriteElement("script", null, "type", "text/javascript", "src", moduleScriptUrl.ToString());
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
                html.WriteElement("script", null, "type", "text/javascript", "src", pageScriptUrl.ToString());
                html.WriteLine();
            }

            return WriteResult.Continue();
        }

        public virtual IWriteResult WriteBodyArea(
            IRenderContext renderContext,
            IDataContextBuilder dataContextBuilder)
        {
            return WriteResult.Continue();
        }

        public virtual IWriteResult WriteInitializationArea(
            IRenderContext renderContext,
            IDataContextBuilder dataContextBuilder)
        {
            return WriteResult.Continue();
        }

        #endregion

        #region Debug info

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            _dataScopeProvider.ElementName = "Page " + Name;

            var debugPage = debugInfo as DebugPage ?? new DebugPage();

            debugPage.Layout = Layout == null ? null : (DebugLayout)Layout.GetDebugInfo();
            debugPage.Scope = _dataScopeProvider.GetDebugInfo(0, -1);
            debugPage.RequiredPermission = RequiredPermission;

            return base.PopulateDebugInfo(debugPage);
        }

        public override string ToString()
        {
            if (ReferenceEquals(Layout, null))
                return "page with no layout";
            return "page with '" + Layout + "' layout";
        }

        #endregion

        #region IDataScopeProvider Mixin

        int IDataScopeProvider.Id { get { return _dataScopeProvider.Id; } }

        string IDataScopeProvider.ElementName
        {
            get { return _dataScopeProvider.ElementName; }
            set { _dataScopeProvider.ElementName = value; }
        }

        DebugDataScopeProvider IDataScopeProvider.GetDebugInfo(int parentDepth, int childDepth)
        {
            return _dataScopeProvider.GetDebugInfo(parentDepth, childDepth);
        }

        void IDataScopeProvider.SetupDataContext(IRenderContext renderContext)
        {
            _dataScopeProvider.SetupDataContext(renderContext);
        }

        IDataScopeProvider IDataScopeProvider.CreateInstance()
        {
            return _dataScopeProvider.CreateInstance();
        }

        IDataScopeProvider IDataScopeProvider.Parent
        {
            get { return _dataScopeProvider.Parent; }
        }

        void IDataScopeProvider.AddChild(IDataScopeProvider child)
        {
            _dataScopeProvider.AddChild(child);
        }

        void IDataScopeProvider.Initialize(IDataScopeProvider parent)
        {
            _dataScopeProvider.Initialize(parent);
        }

        void IDataScopeProvider.AddScope(Type type, string scopeName)
        {
            _dataScopeProvider.AddScope(type, scopeName);
        }

        void IDataScopeProvider.BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            _dataScopeProvider.BuildDataContextTree(renderContext, parentDataContext);
        }

        IDataSupply IDataScopeProvider.AddSupplier(IDataSupplier supplier, IDataDependency dependency)
        {
            return _dataScopeProvider.AddSupplier(supplier, dependency);
        }

        void IDataScopeProvider.AddSupply(IDataSupply supply)
        {
            _dataScopeProvider.AddSupply(supply);
        }

        IDataContext IDataScopeProvider.SetDataContext(IRenderContext renderContext)
        {
            return _dataScopeProvider.SetDataContext(renderContext);
        }

        #endregion

        #region IDataContextBuilder Mixin

        bool IDataContextBuilder.IsInScope(IDataDependency dependency)
        {
            return _dataScopeProvider.IsInScope(dependency);
        }

        void IDataContextBuilder.AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            _dataScopeProvider.AddMissingData(renderContext, missingDependency);
        }

        IDataSupply IDataContextBuilder.AddDependency(IDataDependency dependency)
        {
            return _dataScopeProvider.AddDependency(dependency);
        }

        IList<IDataSupply> IDataContextBuilder.AddConsumer(IDataConsumer consumer)
        {
            return _dataScopeProvider.AddConsumer(consumer);
        }

        #endregion

        #region IDataConsumer Mixin

        void IDataConsumer.HasDependency(IDataSupply dataSupply)
        {
            _dataConsumer.HasDependency(dataSupply);
        }

        IList<IDataSupply> IDataConsumer.AddDependenciesToScopeProvider(IDataScopeProvider dataScope)
        {
            return _dataConsumer.AddDependenciesToScopeProvider(dataScope);
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

        void IDataConsumer.HasDependency(IDataProvider dataProvider, IDataDependency dependency)
        {
            _dataConsumer.HasDependency(dataProvider, dependency);
        }

        DebugDataConsumer IDataConsumer.GetDataConsumerDebugInfo()
        {
            return _dataConsumer.GetDataConsumerDebugInfo();
        }

        #endregion
    }
}
