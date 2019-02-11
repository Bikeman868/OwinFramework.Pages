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
using System.Security.Cryptography;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// Implements ITemplateLoader by loading and parsing templates 
    /// stored in the file system
    /// </summary>
    public class FileSystemLoader: TemplateLoader
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly INameManager _nameManager;

        public DirectoryInfo TemplateDirectory { get; set; }

        private Thread _reloadThread;
        private IList<TemplateInfo> _reloadList;

        private class TemplateInfo
        {
            public string FileName;
            public string TemplatePath;
            public ITemplateParser Parser;
            public byte[] Checksum;
        }

        public FileSystemLoader(
            IHostingEnvironment hostingEnvironment,
            INameManager nameManager)
        {
            _hostingEnvironment = hostingEnvironment;
            _nameManager = nameManager;
        }

        public override void Load(
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
                    LoadFile(file.FullName, parser, templatePath);
                }
            }
        }

        /// <summary>
        /// Loads a single file and returns the template
        /// </summary>
        /// <param name="fileName">The file to load</param>
        /// <param name="parser">The parser to use to parse the template file</param>
        /// <param name="templatePath">Optional template path registers the template 
        /// with the Name Manager. Also causes the template to be periodically
        /// reloaded</param>
        /// <returns>The template that was loaded</returns>
        public ITemplate LoadFile(string fileName, ITemplateParser parser, string templatePath = null)
        {
            Encoding encoding;
            var buffer = LoadFileContents(fileName, out encoding);
            var template = parser.Parse(buffer, encoding, Package);

            if (!string.IsNullOrEmpty(templatePath))
            {
                _nameManager.Register(template, templatePath);

                Reload(new TemplateInfo
                    {
                        FileName = fileName,
                        Parser = parser,
                        TemplatePath = templatePath,
                        Checksum = CalculateChecksum(buffer)
                    });
            }

            template.IsStatic = !ReloadInterval.HasValue;

            return template;
        }

        private byte[] LoadFileContents(string fileName, out Encoding encoding)
        {
            var file = new FileInfo(fileName);

            if (!file.Exists)
                throw new TemplateLoaderException("Template file does not exist '" + fileName + "'");

            if (file.Length > 500 * 1024)
                throw new TemplateLoaderException("Template file '" + fileName + "'is too large. Maximum size is 500KB.");

            var buffer = new byte[file.Length];
            using (var stream = file.OpenRead())
            {
                var offset = 0;
                while (offset < buffer.Length)
                    offset += stream.Read(buffer, offset, buffer.Length - offset);
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
                                    Encoding encoding;
                                    var buffer = LoadFileContents(templateInfo.FileName, out encoding);
                                    var checksum = CalculateChecksum(buffer);

                                    if (checksum.Length != templateInfo.Checksum.Length)
                                    {
                                        var template = templateInfo.Parser.Parse(buffer, encoding, Package);
                                        _nameManager.Register(template, templateInfo.TemplatePath);
                                        templateInfo.Checksum = checksum;
                                    }
                                    else
                                    {
                                        for (var j = 0; j < checksum.Length; j++)
                                        {
                                            if (checksum[j] != templateInfo.Checksum[j])
                                            {
                                                var template = templateInfo.Parser.Parse(buffer, encoding, Package);
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
                                    var message = "Failed to load template " + templateInfo.FileName + ". " + ex.Message;
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
