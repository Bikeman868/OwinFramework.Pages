using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This calss is only used by template parsing engines to construct
    /// templates that can be registered with the template library
    /// </summary>
    public class Template : ITemplate
    {
        string INamed.Name { get; set; }
        IPackage IPackagable.Package { get; set; }

        ElementType INamed.ElementType { get { return ElementType.Template; } }

        private readonly PageArea[] _pageAreas = { PageArea.Body };

        private Func<IRenderContext, IWriteResult>[] _visualElements;

        public void Add(IEnumerable<Func<IRenderContext, IWriteResult>> visualElements)
        {
            if (_visualElements == null)
                _visualElements = visualElements.ToArray();
            else
                _visualElements = _visualElements.Concat(visualElements).ToArray();
        }

        IWriteResult ITemplate.WritePageArea(IRenderContext context, PageArea pageArea)
        {
            var result = WriteResult.Continue();

            if (pageArea == PageArea.Body && _visualElements != null)
            {
                for (var i = 0; i < _visualElements.Length; i++)
                {
                    result = result.Add(_visualElements[i](context));
                }
            }

            return result;
        }

        IEnumerable<PageArea> IPageWriter.GetPageAreas()
        {
            return _pageAreas;
        }

        IWriteResult IPageWriter.WriteInPageStyles(ICssWriter writer, Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            return WriteResult.Continue();
        }

        IWriteResult IPageWriter.WriteInPageFunctions(IJavascriptWriter writer, Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            return WriteResult.Continue();
        }
    }
}
