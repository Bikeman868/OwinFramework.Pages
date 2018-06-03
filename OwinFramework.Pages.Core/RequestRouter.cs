using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core
{
    internal class RequestRouter: IRequestRouter
    {
        IRunable IRequestRouter.Route(IOwinContext context)
        {
            return null;
        }
    }
}
