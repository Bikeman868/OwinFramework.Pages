using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    /// stored in the file system
    /// </summary>
    public class FileSystemLoader: ITemplateLoader
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly INameManager _nameManager;
        private readonly Encoding[] _detectableEncodings;

        public PathString RootPath { get; set; }
        public IPackage Package { get; set; }
        public TimeSpan? ReloadInterval { get; set; }

        public DirectoryInfo TemplateDirectory { get; set; }

        private Thread _reloadThread;
        private IList<TemplateInfo> _reloadList;

        private class TemplateInfo
        {
            public FileInfo File;
            public string TemplatePath;
            public ITemplateParser Parser;
        }

        public FileSystemLoader(
            IHostingEnvironment hostingEnvironment,
            INameManager nameManager)
        {
            _hostingEnvironment = hostingEnvironment;
            _nameManager = nameManager;
            _detectableEncodings = 
                Encoding.GetEncodings()
                .Where(e => 
                    {
                        var preamble = e.GetEncoding().GetPreamble();
                        return preamble != null && preamble.Length > 0;
                    })
                .Select(e => e.GetEncoding())
                .ToArray();
        }

        public void Load(
            ITemplateParser parser, 
            Func<PathString, bool> predicate = null, 
            Func<PathString, string> mapPath = null,
            bool includeSubPaths = true)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            var directory = TemplateDirectory ?? new DirectoryInfo(_hostingEnvironment.MapPath("~/templates"));

            if (!directory.Exists)
                throw new TemplateLoaderException("The template directory was not found at '" + directory.FullName + "'");

            if (predicate == null)
                predicate = path => true;

            if (mapPath == null)
                mapPath = DefaultMapping;

            foreach(var file in GetFiles(directory, includeSubPaths))
            {
                var path = new PathString(file.FullName.Substring(directory.FullName.Length).Replace('\\', '/'));
                if (predicate(path))
                {
                    var templatePath = mapPath(path);

                    Trace.WriteLine("Parsing file '" + file.FullName + "' with '" + parser + "' and registering as '" + templatePath + "'");
                    LoadFile(file, parser, templatePath);
                }
            }
        }

        /// <summary>
        /// Loads a single file and returns the template
        /// </summary>
        /// <param name="file">The file to load</param>
        /// <param name="parser">The parser to use to parse the template file</param>
        /// <param name="templatePath">Optional template path registers the template 
        /// with the Name Manager. Also causes the template to be periodically
        /// reloaded</param>
        /// <returns>The template that was loaded</returns>
        public ITemplate LoadFile(FileInfo file, ITemplateParser parser, string templatePath = null)
        {
            var template = LoadAndParseFile(file, parser);

            if (!string.IsNullOrEmpty(templatePath))
            {
                _nameManager.Register(template, templatePath);

                Reload(new TemplateInfo
                    {
                        File = file,
                        Parser = parser,
                        TemplatePath = templatePath
                    });
            }

            return template;
        }

        private ITemplate LoadAndParseFile(FileInfo file, ITemplateParser parser)
        {
            if (file.Length > 500 * 1024)
                throw new TemplateLoaderException("Template file is too large. Maximum size is 500KB.");

            var buffer = new byte[file.Length];
            using (var stream = file.OpenRead())
            {
                var offset = 0;
                while (offset < buffer.Length)
                    offset += stream.Read(buffer, offset, buffer.Length - offset);
            }

            Encoding encoding = null;
            var preambleLength = 0;

            for (var encodingIndex = 0; encodingIndex < _detectableEncodings.Length; encodingIndex++)
            {
                encoding = _detectableEncodings[encodingIndex];
                var preamble = encoding.GetPreamble();
                preambleLength = preamble.Length;

                if (buffer.Length < preambleLength)
                {
                    encoding = null;
                }
                else
                {
                    for (var i = 0; i < preambleLength; i++)
                    {
                        if (buffer[i] != preamble[i])
                        {
                            encoding = null;
                            break;
                        }
                    }
                }

                if (encoding != null)
                    break;
            }

            if (encoding != null)
            {
                var tempBuffer = new byte[buffer.Length - preambleLength];
                Buffer.BlockCopy(buffer, preambleLength, tempBuffer, 0, tempBuffer.Length);
                buffer = tempBuffer;
            }

            return parser.Parse(buffer, encoding, Package);
        }

        private void Reload(TemplateInfo info)
        {
            if (!ReloadInterval.HasValue)
                return;

            if (_reloadList == null)
                _reloadList = new List<TemplateInfo> { info };
            else
                lock(_reloadList) _reloadList.Add(info);

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
                                    var template = LoadAndParseFile(templateInfo.File, templateInfo.Parser);
                                    _nameManager.Register(template, templateInfo.TemplatePath);
                                }
                                catch (ThreadAbortException)
                                {
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    var message = "Failed to load template " + templateInfo.File.FullName + ". " + ex.Message;
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
                    Name = "Template file reload",
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal
                };
                _reloadThread.Start();
            }
        }

        private IEnumerable<FileInfo> GetFiles(DirectoryInfo directory, bool includeSubPaths)
        {
            return directory.GetFiles("*.*", includeSubPaths ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        private string DefaultMapping(PathString fileSystemPath)
        {
            var segments = fileSystemPath.Value.ToLower().Split('/');
            var lastSegment = segments[segments.Length - 1];
            var extSeparator = lastSegment.LastIndexOf('.');
            if (extSeparator > 0)
            {
                segments[segments.Length - 1] = lastSegment.Substring(0, extSeparator);
                fileSystemPath = new PathString(string.Join("/", segments));
            }

            if (RootPath.HasValue)
            {
                return RootPath.Add(fileSystemPath).Value;
            }

            return fileSystemPath.Value;
        }

    }
}
