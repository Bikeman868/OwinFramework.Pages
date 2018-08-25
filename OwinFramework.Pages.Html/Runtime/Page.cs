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
            var data = new InitializationData(AssetDeployment, this);

            if (AssetDeployment == AssetDeployment.Inherit)
            {
                data.AssetDeployment = Module == null || Module.AssetDeployment == AssetDeployment.Inherit
                    ? AssetDeployment.PerWebsite 
                    : Module.AssetDeployment;
            }

            if (_regions != null)
            {
                foreach (var region in _regions)
                    Layout.Populate(region.Key, region.Value);
            }

            InitializeDependants(data);
            InitializeChildren(data, AssetDeployment.Inherit);

            _referencedModules = new List<IModule>();
            var styles = _dependencies.CssWriterFactory.Create();
            var functions = _dependencies.JavascriptWriterFactory.Create();

            System.Diagnostics.Trace.WriteLine("Page " + Name + " asset deployment");
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

                System.Diagnostics.Trace.WriteLine("   " + name + " deployed to " + deployment);
            }

            _inPageCssLines = styles.ToLines();
            _inPageScriptLines = functions.ToLines();
        }

        private class InitializationData: IInitializationData
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

            public InitializationData(
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

            public IInitializationData NeedsComponent(IComponent component)
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
                if (string.IsNullOrEmpty(message)) return;
                System.Diagnostics.Trace.WriteLine(_currentState.MessagePrefix + message);
            }
        }

        public override IEnumerator<IElement> GetChildren()
        {
            if (_components == null)
            {
                return ReferenceEquals(Layout, null) 
                    ? null 
                    : Layout.AsEnumerable<IElement>().GetEnumerator();
            }

            return ReferenceEquals(Layout, null)
                ? _components.GetEnumerator()
                : _components.Concat(Layout.AsEnumerable<IElement>()).GetEnumerator();
        }

        #endregion

        public void PopulateRegion(string regionName, IElement element)
        {
            if (_regions == null)
                _regions = new Dictionary<string, IElement>();
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

            context.Trace(() => "Adding static data to the render context");
            context.TraceIndent();
            _dataScopeProvider.SetupDataContext(context);
            context.TraceOutdent();

            var writeResult = WriteResult.Continue();
            try
            {
                context.Trace(() => "Writing document start");
                context.TraceIndent();
                html.WriteDocumentStart(context.Language);
                context.TraceOutdent();

                context.Trace(() => "Writing page head");
                context.TraceIndent();
                WritePageHead(context, html, writeResult);
                context.TraceOutdent();

                context.Trace(() => "Writing page body");
                context.TraceIndent();
                WritePageBody(context, html, writeResult);
                context.TraceOutdent();

                context.Trace(() => "Writing initialization JavaScript");
                context.TraceIndent();
                WriteInitializationScript(context, true);
                context.TraceOutdent();

                context.Trace(() => "Writing document end");
                context.TraceIndent();
                html.WriteDocumentEnd();
                context.TraceOutdent();
            }
            catch (Exception ex)
            {
                context.Trace(e => "Exception thrown " + e.Message, ex);

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

        public override IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteInitializationScript(context));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteInitializationScript(context));

            return writeResult;
        }

        public override IWriteResult WriteTitle(IRenderContext context, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (TitleFunc != null)
            {
                context.Html.Write(TitleFunc(context));
                return writeResult;
            }

            if (_components != null)
            {
                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteTitle(context));

                    if (writeResult.IsComplete)
                        return writeResult;
                }
            }

            if (Layout != null)
                writeResult.Add(Layout.WriteTitle(context));

            return writeResult;
        }

        public override IWriteResult WriteHead(IRenderContext context, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            var websiteStylesUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Style);
            if (websiteStylesUrl != null)
            {
                context.Trace(() => "Writing link to website css " + websiteStylesUrl);
                context.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", websiteStylesUrl.ToString());
                context.Html.WriteLine();
            }

            if (_referencedModules != null)
            {
                context.Trace(() => "Writing links to css for referenced modules");
                context.TraceIndent();
                foreach (var module in _referencedModules)
                {
                    var moduleStylesUrl = _dependencies.AssetManager.GetModuleAssetUrl(module, AssetType.Style);
                    if (moduleStylesUrl != null)
                    {
                        context.Trace(() => "Writing link to css for module " + moduleStylesUrl);
                        context.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", moduleStylesUrl.ToString());
                        context.Html.WriteLine();
                    }
                }
                context.TraceOutdent();
            }

            var pageStylesUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Style);
            if (pageStylesUrl != null)
            {
                context.Trace(() => "Writing links to page specific css " + pageStylesUrl);
                context.Html.WriteUnclosedElement("link", "rel", "stylesheet", "type", "text/css", "href", pageStylesUrl.ToString());
                context.Html.WriteLine();
            }

            var websiteScriptUrl = _dependencies.AssetManager.GetWebsiteAssetUrl(AssetType.Script);
            if (websiteScriptUrl != null)
            {
                context.Trace(() => "Writing link to website JavaScript " + websiteScriptUrl);
                context.Html.WriteElement("script", "type", "text/javascript", "src", websiteScriptUrl.ToString());
                context.Html.WriteLine();
            }

            if (_referencedModules != null && _referencedModules.Count > 0)
            {
                context.Trace(() => "Writing links to JavaScript for referenced modules");
                context.TraceIndent();

                foreach (var module in _referencedModules)
                {
                    var moduleScriptUrl = _dependencies.AssetManager.GetModuleAssetUrl(module, AssetType.Script);
                    if (moduleScriptUrl != null)
                    {
                        context.Trace(() => "Writing link to JavaScript for module " + moduleScriptUrl);
                        context.Html.WriteElement("script", null, "type", "text/javascript", "src", moduleScriptUrl.ToString());
                        context.Html.WriteLine();
                    }
                }

                context.TraceOutdent();
            }

            var pageScriptUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Script);
            if (pageScriptUrl != null)
            {
                context.Trace(() => "Writing link to page specific JavaScript " + pageScriptUrl);
                context.Html.WriteElement("script", null, "type", "text/javascript", "src", pageScriptUrl.ToString());
                context.Html.WriteLine();
            }

            if (_components != null && _components.Count > 0)
            {
                context.Trace(() => "Allowing components to write into the page head");
                context.TraceIndent();

                foreach (var component in _components)
                {
                    writeResult.Add(component.WriteHead(context));

                    if (writeResult.IsComplete)
                        return writeResult;
                }

                context.TraceOutdent();
            }

            if (Layout != null)
            {
                context.Trace(() => "Allowing layout to write into the page head");
                context.TraceIndent();

                writeResult.Add(Layout.WriteHead(context));

                context.TraceOutdent();
            }

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

            html.Write("<title>");
            writeResult.Add(WriteTitle(context, true));
            html.WriteLine("</title>");

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

        bool IDataScopeProvider.IsInScope(IDataDependency dependency)
        {
            return _dataScopeProvider.IsInScope(dependency);
        }

        void IDataScopeProvider.SetupDataContext(IRenderContext renderContext)
        {
            _dataScopeProvider.SetupDataContext(renderContext);
        }

        IDataScopeProvider IDataScopeProvider.CreateInstance()
        {
            return _dataScopeProvider.CreateInstance();
        }

        void IDataScopeProvider.AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            _dataScopeProvider.AddMissingData(renderContext, missingDependency);
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

        IDataSupply IDataScopeProvider.AddDependency(IDataDependency dependency)
        {
            return _dataScopeProvider.AddDependency(dependency);
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

        IList<IDataSupply> IDataScopeProvider.AddConsumer(IDataConsumer consumer)
        {
            return _dataScopeProvider.AddConsumer(consumer);
        }

        IDataContext IDataScopeProvider.SetDataContext(IRenderContext renderContext)
        {
            return _dataScopeProvider.SetDataContext(renderContext);
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

        DebugDataConsumer IDataConsumer.GetDebugInfo()
        {
            return _dataConsumer.GetDebugInfo();
        }

        #endregion
    }
}
