using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    internal class TemplateBuilder : ITemplateBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;
        private readonly IDataConsumerFactory _dataConsumerFactory;

        public TemplateBuilder(
            INameManager nameManager,
            IAssetManager assetManager,
            IDataConsumerFactory dataConsumerFactory)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;
            _dataConsumerFactory = dataConsumerFactory;
        }

        public ITemplateDefinition BuildUpTemplate()
        {
            return new TemplateDefinition(
                _nameManager,
                _assetManager,
                _dataConsumerFactory);
        }
    }
}
