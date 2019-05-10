using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Runtime;

namespace OwinFramework.Pages.CMS.Editor.Assets
{
    internal class AssetsService: Service
    {
        private readonly string _javaScript;
        private readonly string _css;

        public AssetsService(
            IServiceDependenciesFactory dependencies,
            IEnumerable<string> javaScript, 
            IEnumerable<string> css)
            :base(dependencies)
        {
            _javaScript = javaScript == null ? String.Empty : String.Join("\n", javaScript);
            _css = css == null ? String.Empty : String.Join("\n", css);
        }

        [Endpoint(ResponseSerializer = typeof(ScriptSerializer))]
        public void Script(IEndpointRequest request)
        {
            request.Success(_javaScript);
        }

        [Endpoint(ResponseSerializer = typeof(StyleSerializer))]
        public void Style(IEndpointRequest request)
        {
            request.Success(_css);
        }

    }
}