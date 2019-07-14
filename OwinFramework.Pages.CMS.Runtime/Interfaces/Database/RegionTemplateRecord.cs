using Newtonsoft.Json;
using OwinFramework.Pages.Core.Enums;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class RegionTemplateRecord: IDataContract<RegionTemplateRecord>
    {
        /// <summary>
        /// The region version that this template should be rendered into
        /// </summary>
        [Mapping("regionVersionId")]
        [JsonProperty("regionVersionId")]
        public long RegionVersionId { get; set; }

        /// <summary>
        /// The path of the template to render
        /// </summary>
        [Mapping("templatePath")]
        [JsonProperty("templatePath")]
        public string TemplatePath { get; set; }

        /// <summary>
        /// The area of the page where this template should be rendered
        /// </summary>
        [Mapping("pageArea")]
        [JsonProperty("pageArea")]
        public PageArea PageArea { get; set; }

        public void AddMappings(ITypeDefinition<RegionTemplateRecord> typeDefinition, string dataSetName)
        {
            typeDefinition.AddField("regionVersionId", r => r.RegionVersionId, 0);
            typeDefinition.AddField("templatePath", r => r.TemplatePath, "/blankRegion");
            typeDefinition.AddField("pageArea", r => r.PageArea, PageArea.Body);
        }

        public void SetCalculated(IDataReader dataReader, string dataSetName)
        {
        }
    }
}
