using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Managers
{
    internal class TemplateManager : ITemplateManager
    {
        void ITemplateManager.Register(ITemplate template, string templatePath, params string[] locales)
        {
        }

        ITemplate ITemplateManager.Get(IRenderContext renderContext, string templatePath)
        {
            return new DummyTemplate();
        }

        private class DummyTemplate: ITemplate
        {
            public string Name { get; set; }
            public IPackage Package { get; set; }
            public ElementType ElementType { get { return ElementType.Template; } }

            public IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
            {
                if (pageArea == PageArea.Body)
                    context.Html.WriteElementLine("p", "Dummy template");

                return new WriteResult();
            }

            public IEnumerable<PageArea> GetPageAreas()
            {
                return new[] { PageArea.Body };
            }

            public IWriteResult WriteInPageStyles(ICssWriter writer, Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
            {
                return new WriteResult();
            }

            public IWriteResult WriteInPageFunctions(IJavascriptWriter writer, Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter)
            {
                return new WriteResult();
            }
        }

        private class WriteResult: IWriteResult
        {
            public bool IsComplete{get { return true; }}

            public IWriteResult Add(IWriteResult priorWriteResult)
            {
                return priorWriteResult;
            }

            public void Wait(bool cancel)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
