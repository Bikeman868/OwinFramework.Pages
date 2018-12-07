using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    /// <summary>
    /// Concrete implementation of IRequestRouterFactory
    /// </summary>
    public class RequestRouterFactory : IRequestRouterFactory
    {
        public IRequestRouter Create()
        {
            return new RequestRouter();
        }
    }
}
