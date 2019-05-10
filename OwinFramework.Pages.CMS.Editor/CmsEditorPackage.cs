using OwinFramework.Pages.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.CMS.Editor.Services;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.CMS.Editor
{
    public class CmsEditorPackage: IPackage
    {
        string IPackage.NamespaceName { get; set; }
        IModule IPackage.Module { get; set; }
        ElementType INamed.ElementType { get { return ElementType.Package; } }
        string INamed.Name { get; set; }

        IPackage IPackage.Build(IFluentBuilder fluentBuilder)
        {
            fluentBuilder.BuildUpService(null, typeof(LiveUpdateService))
                .Route("/cms/live-update/", new []{ Method.Get }, 0)
                .Build();

            return this;
        }
    }
}
