using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Owin;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// Implements ITemplateLoader by loading and parsing templates 
    /// retrieved from the Internet
    /// </summary>
    public class UriLoader: ITemplateLoader
    {
        private readonly INameManager _nameManager;

        public PathString RootPath { get; set; }
        public IPackage Package { get; set; }
        public TimeSpan? ReloadInterval { get; set; }

        public Uri BaseUri { get; set; }

        private Thread _reloadThread;
        private IList<TemplateInfo> _reloadList;

        private class TemplateInfo
        {
            public Uri Uri;
            public string TemplatePath;
            public ITemplateParser Parser;
        }

        public UriLoader(
            INameManager nameManager)
        {
            _nameManager = nameManager;
        }

        public void Load(
            ITemplateParser parser, 
            Func<PathString, bool> predicate = null, 
            Func<PathString, string> mapPath = null,
            bool includeSubPaths = true)
        {
            throw new NotImplementedException(
                "Enumerating files from a URI is not implemented yet. "+
                "Please use the LoadUri() method instead.");
        }

        /// <summary>
        /// Loads a template from a URI
        /// </summary>
        /// <param name="uri">The URI to load from</param>
        /// <param name="parser">The parser to use to parse the template</param>
        /// <param name="templatePath">Optional path to register the template at
        /// with the name manager</param>
        /// <returns>The template that was loaded</returns>
        public ITemplate LoadUri(Uri uri, ITemplateParser parser, string templatePath)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            if (parser == null)
                throw new ArgumentNullException("parser");

            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("The Uri must be absolute", "uri");

            var template = LoadAndParseUri(uri, parser);

            if (!string.IsNullOrEmpty(templatePath))
            {
                _nameManager.Register(template, templatePath);

                Reload(new TemplateInfo
                {
                    Uri = uri,
                    Parser = parser,
                    TemplatePath = templatePath
                });
            }

            return template;
        }

        private ITemplate LoadAndParseUri(Uri uri, ITemplateParser parser)
        {
            byte[] templateData;
            using (var webClient = new WebClient())
            {
                templateData = webClient.DownloadData(uri);
            }

            return parser.Parse(templateData, null, Package);
        }

        private void Reload(TemplateInfo info)
        {
            if (!ReloadInterval.HasValue)
                return;

            if (_reloadList == null)
                _reloadList = new List<TemplateInfo> { info };
            else
                lock (_reloadList) _reloadList.Add(info);

            if (_reloadThread == null)
            {
                _reloadThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var interval = ReloadInterval;
                            if (!interval.HasValue)
                            {
                                Thread.Sleep(1000);
                                continue;
                            }

                            Thread.Sleep(interval.Value);

                            int count;
                            lock (_reloadList) count = _reloadList.Count;

                            Exception exception = null;

                            for (var i = 0; i < count; i++)
                            {
                                TemplateInfo templateInfo;
                                lock (_reloadList)
                                    templateInfo = _reloadList[i];

                                try
                                {
                                    var template = LoadAndParseUri(templateInfo.Uri, templateInfo.Parser);
                                    _nameManager.Register(template, templateInfo.TemplatePath);
                                }
                                catch (ThreadAbortException)
                                {
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    var message = "Failed to load template " + templateInfo.Uri + ". " + ex.Message;
                                    exception = new Exception(message, ex);
                                    Trace.WriteLine(message);
                                }
                            }

                            if (exception != null) throw exception;
                        }
                        catch (ThreadAbortException)
                        {
                            return;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Excepton thrown reloading templates. " + ex.Message);
                            Thread.Sleep(1000);
                        }
                    }
                })
                {
                    Name = "Template uri reload",
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal
                };
                _reloadThread.Start();
            }
        }

    }
}
