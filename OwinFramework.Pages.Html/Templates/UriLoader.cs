using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Templates;
using System.Security.Cryptography;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// Implements ITemplateLoader by loading and parsing templates 
    /// retrieved from the Internet
    /// </summary>
    public class UriLoader: TemplateLoader
    {
        private readonly INameManager _nameManager;

        public Uri BaseUri { get; set; }

        private Thread _reloadThread;
        private IList<TemplateInfo> _reloadList;

        private class TemplateInfo
        {
            public Uri Uri;
            public string TemplatePath;
            public ITemplateParser Parser;
            public byte[] Checksum;
        }

        public UriLoader(
            INameManager nameManager)
        {
            _nameManager = nameManager;
        }

        public override void Load(
            ITemplateParser parser, 
            Func<PathString, bool> predicate = null, 
            Func<PathString, string> mapPath = null,
            bool includeSubPaths = true)
        {
            throw new NotImplementedException(
                "Enumerating templates from a URI is not implemented yet. "+
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

            Encoding encoding;
            var buffer = LoadUriContents(uri, out encoding);
            var template = parser.Parse(new[] { new TemplateResource { Content = buffer, Encoding = encoding } }, Package, Module);

            if (!string.IsNullOrEmpty(templatePath))
            {
                _nameManager.Register(template, templatePath);

                Reload(new TemplateInfo
                {
                    Uri = uri,
                    Parser = parser,
                    TemplatePath = templatePath,
                    Checksum = CalculateChecksum(buffer)
                });
            }

            template.IsStatic = !ReloadInterval.HasValue;
            
            return template;
        }

        private byte[] LoadUriContents(Uri uri, out Encoding encoding)
        {
            byte[] buffer;
            using (var webClient = new WebClient())
            {
                buffer = webClient.DownloadData(uri);
            }

            buffer = RemovePreamble(buffer, out encoding);

            return buffer;
        }

        private byte[] CalculateChecksum(byte[] buffer)
        {
            using (var sha = new SHA1Managed())
            {
                return sha.ComputeHash(buffer);
            }
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
                                    Encoding encoding;
                                    var buffer = LoadUriContents(templateInfo.Uri, out encoding);
                                    var checksum = CalculateChecksum(buffer);

                                    if (checksum.Length != templateInfo.Checksum.Length)
                                    {
                                        var template = templateInfo.Parser.Parse(
                                            new []{ new TemplateResource { Content = buffer, Encoding = encoding } }, 
                                            Package,
                                            Module);
                                        _nameManager.Register(template, templateInfo.TemplatePath);
                                        templateInfo.Checksum = checksum;
                                    }
                                    else
                                    {
                                        for (var j = 0; j < checksum.Length; j++)
                                        {
                                            if (checksum[j] != templateInfo.Checksum[j])
                                            {
                                                var template = templateInfo.Parser.Parse(
                                                    new[] { new TemplateResource { Content = buffer, Encoding = encoding } },
                                                    Package,
                                                    Module);
                                                _nameManager.Register(template, templateInfo.TemplatePath);
                                                templateInfo.Checksum = checksum;
                                                break;
                                            }
                                        }
                                    }
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
