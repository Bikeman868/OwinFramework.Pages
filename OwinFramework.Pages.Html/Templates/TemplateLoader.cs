using System;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// Base class for classes that load templates
    /// </summary>
    public abstract class TemplateLoader: ITemplateLoader
    {
        private readonly Encoding[] _detectableEncodings;
        protected readonly INameManager _nameManager;

        protected Action<string, TemplateResource> _preProcessAction;

        public PathString RootPath { get; private set; }
        public IPackage Package { get; private set; }
        public IModule Module { get; private set; }
        public TimeSpan? ReloadInterval { get; private set; }
        public AssetDeployment AssetDeployment { get; private set; }

        public TemplateLoader(INameManager nameManager)
        {
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

            _preProcessAction = (path, resource) => { };

            AssetDeployment = AssetDeployment.Inherit;
        }

        public ITemplateLoader PartOf(IPackage package)
        {
            Package = package;
            return this;
        }

        public ITemplateLoader PartOf(string packageName)
        {
            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolvePackageNames, 
                nm => 
                {
                    Package = nm.ResolvePackage(packageName);
                });
            return this;
        }

        public ITemplateLoader DeployIn(IModule module)
        {
            Module = module;
            AssetDeployment = AssetDeployment.PerModule;
            return this;
        }

        public ITemplateLoader DeployIn(string moduleName)
        {
            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, 
                nm => 
                {
                    Module = nm.ResolveModule(moduleName);
                });
            AssetDeployment = AssetDeployment.PerModule;
            return this;
        }

        public ITemplateLoader DeployAssetsTo(AssetDeployment assetDeployment)
        {
            AssetDeployment = assetDeployment;
            return this;
        }

        public ITemplateLoader LoadUnder(PathString rootPath)
        {
            RootPath = rootPath;
            return this;
        }

        public abstract ITemplateLoader Load(
            ITemplateParser parser, 
            Func<PathString, bool> predicate = null, 
            Func<PathString, string> mapPath = null, 
            bool includeSubPaths = true);

        public ITemplateLoader ReloadEvery(TimeSpan interval)
        {
            ReloadInterval = interval;
            return this;
        }

        public ITemplateLoader PreProcess(Action<string, TemplateResource> preProcessAction)
        {
            if (preProcessAction == null)
                _preProcessAction = (path, resource) => { };
            else
                _preProcessAction = preProcessAction;
            return this;
        }


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
