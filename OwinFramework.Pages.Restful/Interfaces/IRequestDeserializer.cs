using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin;

namespace OwinFramework.Pages.Restful.Interfaces
{
    public interface IRequestDeserializer
    {
        /// <summary>
        /// Deserializes the body of the request and returns it
        /// as a specific type
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="context">The request to deserialize</param>
        T Body<T>(IOwinContext context);
    }
}
