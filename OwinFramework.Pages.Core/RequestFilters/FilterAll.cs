using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.RequestFilters
{
    /// <summary>
    /// A request filter that matches all requests. Use this filter to catch
    /// all other requests. Any lower priority routes will never be reached
    /// </summary>
    public class FilterAll: IRequestFilter
    {
        string IRequestFilter.Description
        {
            get { return "All requests"; }
        }

        /// <summary>
        /// Returns true always
        /// </summary>
        public bool IsMatch(IOwinContext context, string absolutePath, string method)
        {
            return true;
        }
    }
}
