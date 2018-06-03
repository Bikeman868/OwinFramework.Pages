using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.RequestFilters
{
    /// <summary>
    /// A request filter that matches all requests for the specified path. 
    /// The path can contain wildcards
    /// </summary>
    public class FilterByPath: IRequestFilter
    {
        private readonly Func<PathString, bool> _matchFunc;
        private readonly string _description;

        /// <summary>
        /// Constructs a filter that will accept any request with a matching http method
        /// </summary>
        /// <param name="path">The path to match to the request. Use * for wildcards. 
        /// End with ** to match all sub-paths</param>
        public FilterByPath(string path)
        {
            path = path.Trim().ToLower();

            if (string.IsNullOrEmpty(path))
            {
                _matchFunc = p => false;
                _description = "None";
                return;
            }

            while (path.StartsWith("/"))
                path = path.Substring(1);

            if (path.Length == 0)
            {
                // Matches the root of the website (home page)
                _matchFunc = p => !p.HasValue || p.Value == "/";
                _description = "/";
                return;
            }

            if (path.IndexOf("*", StringComparison.OrdinalIgnoreCase) < 0)
            {
                // Matches the path exactly with no widecards
                var pathString = new PathString("/" + path);
                _matchFunc = p => p.HasValue && p.Equals(pathString);
                _description = pathString.ToString();
                return;
            }
        
            var elements = path.ToLower().Split('/');
            var matchSubPaths = elements[elements.Length - 1] == "**";

            if (matchSubPaths && elements.Length == 1)
            {
                _matchFunc = p => true;
                _description = "All paths";
            }

            if (matchSubPaths && elements.All(e => e != "*"))
            {
                var pathString = new PathString("/" + string.Join("/", elements.Take(elements.Length - 1)));
                _matchFunc = p => p.HasValue && p.StartsWithSegments(pathString);
                _description = pathString + "/**";
                return;
            }

            var funcs = new List<Func<string, bool>>();

            foreach (var element in elements)
            {
                var localElement = element;
                if (element.IndexOf('*') < 0)
                {
                    funcs.Add(e => e == localElement);
                }
                else if (element.All(c => c == '*'))
                {
                    funcs.Add(e => true);
                }
                else
                {
                    var regularExpression = element
                        .Replace(".", "\\.")
                        .Replace("*", ".*");
                    var expression = new Regex(regularExpression);
                    funcs.Add(expression.IsMatch);
                }
            }

            _matchFunc = p =>
                {
                    if (!p.HasValue) 
                        return false;

                    var pathElements = p.Value.ToLower().Split('/');

                    // Since PathString always starts with / the first element is always blank
                    var pathElementCount = pathElements.Length - 1;

                    if (pathElementCount < funcs.Count)
                        return false;

                    if (!matchSubPaths && pathElementCount > funcs.Count)
                        return false;

                    for (var i = 0; i < funcs.Count; i++)
                    {
                        if (!funcs[i](pathElements[i + 1]))
                            return false;
                    }
                    return true;
                };
            _description = "/" + path;
        }

        string IRequestFilter.Description
        {
            get { return _description; }
        }

        bool IRequestFilter.IsMatch(IOwinContext context)
        {
            return _matchFunc(context.Request.Path);
        }
    }
}
