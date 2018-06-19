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

        private readonly IPageDependenciesFactory _dependencies;
        private IList<IComponent> _components;
        private string _bodyStyleName;
        private List<string> _inPageCssLines;
        private List<string> _inPageScriptLines;
        private List<IModule> _referencedModules;

        protected Page(IPageDependenciesFactory dependencies)
        {
            _dependencies = dependencies;
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

            _referencedModules = new List<IModule>();
            var styles = new AssetWriter();
            var functions = new Dictionary<string, AssetWriter>(StringComparer.InvariantCultureIgnoreCase);

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

                        element.Element.WriteStaticAssets(AssetType.Style, styles);

                        var ns = string.Empty;
                        if (element.Element.Package != null) ns = element.Element.Package.NamespaceName;

                        AssetWriter functionWriter;
                        if (!functions.TryGetValue(ns, out functionWriter))
                        {
                            functionWriter = new AssetWriter();
                            functions[ns] = functionWriter;
                        }

                        element.Element.WriteStaticAssets(AssetType.Script, functionWriter);

                        break;
                    default:
                        deployment = element.AssetDeployment.ToString();
                        break;
                }

                System.Diagnostics.Trace.WriteLine("   " + name + " deployed to " + deployment);
            }

            _inPageCssLines = styles.GetLines();

            _inPageScriptLines = new List<string>();
            foreach(var namespaceName in functions.Keys)
            {
                var lines = functions[namespaceName].GetLines();
                if (lines == null || lines.Count <= 0) continue;

                _inPageScriptLines.Add("var owinFramework = (window.owinFramework = window.owinFramework || {});");
                _inPageScriptLines.Add("owinFramework." + namespaceName + " = function () {");

                foreach (var line in lines)
                    _inPageScriptLines.Add("   " + line);

                _inPageScriptLines.Add("}();");
            }
        }

        private class AssetWriter: IHtmlWriter
        {
            public bool Indented { get; set; }
            public bool IncludeComments { get; set; }
            public int IndentLevel { get; set; }

            private List<string> _lines = new List<string>();

            public List<string> GetLines()
            {
                return _lines;
            }

            public System.IO.TextWriter GetTextWriter()
            {
                throw new NotImplementedException();
            }

            public void ToResponse(IOwinContext context)
            {
                throw new NotImplementedException();
            }

            public Task ToResponseAsync(IOwinContext context)
            {
                throw new NotImplementedException();
            }

            public void ToStringBuilder(Core.Interfaces.Collections.IStringBuilder stringBuilder)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter CreateInsertionPoint()
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter Write(char c)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter Write(string s)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter Write<T>(T s)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteLine()
            {
                _lines.Add(string.Empty);
                return this;
            }

            public IHtmlWriter WriteLine(string s)
            {
                _lines.Add(s);
                return this;
            }

            public IHtmlWriter WriteLine<T>(T s)
            {
                _lines.Add(s.ToString());
                return this;
            }

            public IHtmlWriter WriteDocumentStart(string language)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteDocumentEnd()
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteOpenTag(string tag, bool selfClosing, params string[] attributePairs)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteOpenTag(string tag, params string[] attributePairs)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteCloseTag(string tag)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteElement(string tag, string content, params string[] attributePairs)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteUnclosedElement(string tag, params string[] attributePairs)
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteComment(string comment, CommentStyle commentStyle = CommentStyle.Xml)
            {
                switch (commentStyle)
                {
                    case CommentStyle.Xml:
                        _lines.Add("<!-- " + comment + " -->");
                        break;
                    case CommentStyle.MultiLineC:
                        _lines.Add("/* " + comment + " */");
                        break;
                    default:
                        _lines.Add("// " + comment);
                        break;
                }
                return this;
            }

            public IHtmlWriter WriteScriptOpen(string type = "text/javascript")
            {
                throw new NotImplementedException();
            }

            public IHtmlWriter WriteScriptClose()
            {
                throw new NotImplementedException();
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
            var dependencies = _dependencies.Create(owinContext);
            var context = dependencies.RenderContext;
            var html = context.Html;
            var data = dependencies.DataContext;

            owinContext.Response.ContentType = "text/html";

            var writeResult = WriteResult.Continue();
            try
            {
                html.WriteDocumentStart(context.Language);

                WritePageHead(context, data, html, writeResult);
                WritePageBody(context, data, html, writeResult);
                WriteInitializationScript(context, data, true);

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

        public override IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer, bool includeChildren)
        {
            var writeResult = WriteResult.Continue();

            if (assetType == AssetType.Style && !string.IsNullOrEmpty(BodyStyle))
            {
                _dependencies.NameManager.EnsureAssetName(this, ref _bodyStyleName);
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
                renderContext.Html.WriteElement("script", "type", "text/javascript", "src", websiteScriptUrl.ToString());

            foreach (var module in _referencedModules)
            {
                var moduleScriptUrl = _dependencies.AssetManager.GetModuleAssetUrl(module, AssetType.Script);
                if (moduleScriptUrl != null)
                    renderContext.Html.WriteElement("script", "type", "text/javascript", "src", moduleScriptUrl.ToString());
            }

            var pageScriptUrl = _dependencies.AssetManager.GetPageAssetUrl(this, AssetType.Script);
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

            if (_inPageCssLines.Count > 0)
            {
                html.WriteOpenTag("style");
                foreach(var line in _inPageCssLines)
                    html.WriteLine(line);
                html.WriteCloseTag("style");
            }

            if (_inPageScriptLines.Count > 0)
            {
                html.WriteScriptOpen();
                foreach (var line in _inPageScriptLines)
                    html.WriteLine(line);
                html.WriteScriptClose();
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

            writeResult.Add(WriteHtml(context, data, true));
            html.WriteCloseTag("body");
        }

        #endregion
    }
}
