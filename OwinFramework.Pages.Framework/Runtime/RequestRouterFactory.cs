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
        private readonly IUserSegmenter _userSegmenter;

        public RequestRouterFactory(IUserSegmenter userSegmenter)
        {
            _userSegmenter = userSegmenter;
        }

        public IRequestRouter Create()
        {
            return new RequestRouter(_userSegmenter);
        }
    }
}
