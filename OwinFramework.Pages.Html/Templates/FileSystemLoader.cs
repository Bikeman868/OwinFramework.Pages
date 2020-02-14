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
            public string[] FileNames;
            public string TemplatePath;
            public ITemplateParser Parser;
            public byte[][] Checksum;
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

            var files = GetFiles(directory, includeSubPaths)
                .Select(fileInfo => new Tuple<FileInfo, PathString>
                (
                    fileInfo,
                    new PathString(fileInfo.FullName.Substring(directory.FullName.Length).Replace('\\', '/'))
                ))
                .Where(t => predicate(t.Item2))
                .OrderBy(t => t.Item2.Value)
                .ToList();

            List<Tuple<FileInfo, PathString>> fileSet = new List<Tuple<FileInfo, PathString>>();

            Action parse = () =>
            {
                if (fileSet.Count > 0)
                {
                    var templatePath = mapPath(fileSet[0].Item2);
                    Trace.WriteLine(fileSet.Aggregate("Parsing files ", (s, f) => s + "'" + f.Item1.FullName + "' ") + " with '" + parser + "' and registering as '" + templatePath + "'");
                    LoadFileSet(fileSet.Select(f => f.Item1.FullName).ToArray(), parser, templatePath);
                }
                fileSet.Clear();
            };

            foreach(var file in files)
            {
                if (fileSet.Count == 0)
                {
                    fileSet.Add(file);
                }
                else
                {
                    var file1 = Path.GetFileNameWithoutExtension(fileSet[0].Item1.FullName);
                    var file2 = Path.GetFileNameWithoutExtension(file.Item1.FullName);
                    if (string.Equals(file1, file2, StringComparison.OrdinalIgnoreCase))
                    {
                        fileSet.Add(file);
                    }
                    else
                    {
                        parse();
                        fileSet.Add(file);
                    }
                }
            }

            parse();
        }

        /// <summary>
        /// Loads a set of files that comprise the parts of a single template
        /// </summary>
        /// <param name="fileNames">The files to load</param>
        /// <param name="parser">The parser to use to parse the template files</param>
        /// <param name="templatePath">Optional template path registers the template 
        /// with the Name Manager. Also causes the template to be periodically
        /// reloaded</param>
        /// <returns>The template that was loaded</returns>
        public ITemplate LoadFileSet(string[] fileNames, ITemplateParser parser, string templatePath = null)
        {
            var resources = new TemplateResource[fileNames.Length];

            for (var i = 0; i < fileNames.Length; i++)
            {
                resources[i] = new TemplateResource
                {
                    Content = LoadFileContents(fileNames[i], out Encoding encoding)
                };
                resources[i].Encoding = encoding;
                resources[i].ContentType = ContentTypeFromExt(Path.GetExtension(fileNames[i]));
            }

            var template = parser.Parse(resources, Package);

            if (!string.IsNullOrEmpty(templatePath))
            {
                _nameManager.Register(template, templatePath);

                var templateInfo = new TemplateInfo
                {
                    FileNames = fileNames,
                    Parser = parser,
                    TemplatePath = templatePath,
                    Checksum = new byte[fileNames.Length][]
                };

                for (var i = 0; i < fileNames.Length; i++)
                {
                    templateInfo.Checksum[i] = CalculateChecksum(resources[i].Content);
                }

                Reload(templateInfo);
            }

            template.IsStatic = !ReloadInterval.HasValue;

            return template;
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
            var template = parser.Parse(new[] { new TemplateResource { Content = buffer, Encoding = encoding } }, Package);

            if (!string.IsNullOrEmpty(templatePath))
            {
                _nameManager.Register(template, templatePath);

                Reload(new TemplateInfo
                    {
                        FileNames = new[] { fileName },
                        Parser = parser,
                        TemplatePath = templatePath,
                        Checksum = new[] { CalculateChecksum(buffer) }
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
                                    var modified = false;

                                    var resources = new TemplateResource[templateInfo.FileNames.Length];

                                    for (var j = 0; j < templateInfo.FileNames.Length; j++)
                                    {
                                        var fileName = templateInfo.FileNames[j];
                                        resources[j] = new TemplateResource
                                        {
                                            Content = LoadFileContents(fileName, out Encoding encoding),
                                            ContentType = ContentTypeFromExt(Path.GetExtension(fileName))
                                        };
                                        resources[j].Encoding = encoding;

                                        var checksum = CalculateChecksum(resources[j].Content);

                                        if (checksum.Length != templateInfo.Checksum[j].Length)
                                        {
                                            templateInfo.Checksum[j] = checksum;
                                            modified = true;
                                        }
                                        else
                                        {
                                            for (var k = 0; k < checksum.Length; k++)
                                            {
                                                if (checksum[k] != templateInfo.Checksum[j][k])
                                                {
                                                    templateInfo.Checksum[j] = checksum;
                                                    modified = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (modified)
                                    {
                                        var template = templateInfo.Parser.Parse(resources, Package);
                                        _nameManager.Register(template, templateInfo.TemplatePath);
                                    }
                                }
                                catch (ThreadAbortException)
                                {
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    var message = templateInfo.FileNames.Aggregate("Failed to load template files", (m, f) => m + " '" + f + "'") + ". " + ex.Message;
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

        private string ContentTypeFromExt(string ext)
        {
            switch (ext.ToLower())
            {
                case ".js": 
                    return "application/javascript";
                case ".css":
                    return "text/css";
                case ".less":
                    return "text/less";
                case ".html":
                    return "text/html";
                case ".md":
                    return "text/x-markdown";
            }
            return null;
        }
    }
}
