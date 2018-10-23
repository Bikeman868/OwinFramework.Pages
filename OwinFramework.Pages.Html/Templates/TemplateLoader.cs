using System;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// Base class for classes that load templates
    /// </summary>
    public abstract class TemplateLoader: ITemplateLoader
    {
        private readonly Encoding[] _detectableEncodings;

        public PathString RootPath { get; set; }
        public IPackage Package { get; set; }
        public TimeSpan? ReloadInterval { get; set; }

        public TemplateLoader()
        {
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

        public abstract void Load(
            ITemplateParser parser, 
            Func<PathString, bool> predicate = null, 
            Func<PathString, string> mapPath = null, 
            bool includeSubPaths = true);

        protected byte[] RemovePreamble(byte[] buffer, out Encoding encoding)
        {
            var preambleLength = 0;
            encoding = null;

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

            if (encoding == null)
                return buffer;

            var tempBuffer = new byte[buffer.Length - preambleLength];
            Buffer.BlockCopy(buffer, preambleLength, tempBuffer, 0, tempBuffer.Length);
            return tempBuffer;
        }
    }
}
