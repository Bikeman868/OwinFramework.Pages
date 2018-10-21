using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        public PathString RootPath { get; set; }
        public IPackage Package { get; set; }
        public TimeSpan? ReloadInterval { get; set; }

        public DirectoryInfo TemplateDirectory { get; set; }

        public FileSystemLoader(
            IHostingEnvironment hostingEnvironment,
            INameManager nameManager)
        {
            _hostingEnvironment = hostingEnvironment;
            _nameManager = nameManager;
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
                    Trace.WriteLine("Parsing file '" + file.FullName + "' with '" + parser + "'");
                    var template = LoadFile(file, parser);

                    var templatePath = mapPath(path);

                    Trace.WriteLine("Registering template '" + templatePath + "' loaded from '" + file.FullName + "'");
                    _nameManager.Register(template, templatePath);
                }
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

        public ITemplate LoadFile(FileInfo file, ITemplateParser parser)
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
            foreach (var encodingInfo in Encoding.GetEncodings())
            {
                encoding = encodingInfo.GetEncoding();
                var preamble = encoding.GetPreamble();

                if (preamble == null || preamble.Length == 0 || buffer.Length < preamble.Length)
                {
                    encoding = null;
                }
                else
                {
                    for (var i = 0; i < preamble.Length; i++)
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
                var preamble = encoding.GetPreamble();
                if (preamble != null && preamble.Length > 0)
                {
                    var tempBuffer = new byte[buffer.Length - preamble.Length];
                    Buffer.BlockCopy(buffer, preamble.Length, tempBuffer, 0, tempBuffer.Length);
                    buffer = tempBuffer;
                }
            }

            return parser.Parse(buffer, encoding, Package);
        }
    }
}
