using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;
using Sample4.DataProviders;

namespace Sample4.CmsData
{
    internal class StaticData: TestDatabaseReaderBase
    {
        public StaticData()
        {
            const string creator = "urn:user:1";

            var elementId = 1;
            var elementVersionId = 1;
            var propertyId = 1;
            var websiteVersionId = 1;
            var dataScopeId = 1;
            var dataTypeId = 1;
            var dataTypeVersionId = 1;

            _regions = new List<RegionRecord>
            {
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "menu",
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "header",
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "footer",
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _layouts = new List<LayoutRecord>
            {
                new LayoutRecord 
                {
                    ElementId = elementId++,
                    Name = "page",
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new LayoutRecord 
                {
                    ElementId = elementId++,
                    Name = "header",
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new LayoutRecord 
                {
                    ElementId = elementId++,
                    Name = "footer",
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
            };



            _dataScopes = new List<DataScopeRecord>
            {

            };

            _dataTypes = new List<DataTypeRecord>
            {
                new DataTypeRecord
                {
                    DataTypeId = dataTypeId++,
                    DisplayName = "Customer",
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _dataTypeVersions = new List<DataTypeVersionRecord>
            {
                new DataTypeVersionRecord
                {
                    DataTypeId = _dataTypes[0].DataTypeId,
                    DataTypeVersionId = dataTypeVersionId++,
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    AssemblyName = typeof(CustomerViewModel).Assembly.FullName,
                    TypeName = typeof(CustomerViewModel).FullName,
                    Scopes = new long[0]
                }
            };



            _regionVersions = new List<RegionVersionRecord>
            {
                new RegionVersionRecord
                {
                    ElementId = _regions[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    LayoutName = "layouts:col_2_left_fixed"
                },
                new RegionVersionRecord
                {
                    ElementId = _regions[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    RegionTemplates = new List<RegionTemplateRecord>
                    {
                        new RegionTemplateRecord{PageArea = PageArea.Body, TemplatePath = "/customer"}
                    },
                    RepeatDataTypeId = _dataTypes[0].DataTypeId
                },
                new RegionVersionRecord
                {
                    ElementId = _regions[2].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    LayoutName = "layouts:full_page"
                }
            };

            _layoutVersions = new List<LayoutVersionRecord>
            {
                new LayoutVersionRecord {
                    ElementId = _layouts[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "header,main,footer",
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "header",
                            LayoutId = _layouts[1].ElementId
                        },
                        new LayoutRegionRecord
                        {
                            RegionName = "footer",
                            LayoutId = _layouts[2].ElementId
                        }
                    }
                },
                new LayoutVersionRecord {
                    ElementId = _layouts[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "menu",
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "menu",
                            ContentType = "region",
                            ContentName = "menus:desktop_menu"
                        }
                    },
                    Components = new List<ElementComponentRecord>
                    {
                        new ElementComponentRecord
                        {
                            ComponentName = "menus:menuStyle1"
                        }
                    }
                },
                new LayoutVersionRecord {
                    ElementId = _layouts[2].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "footer",
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "footer",
                            ContentType = "html",
                            ContentName = "footer-text",
                            ContentValue = "This is the footer"
                        }
                    }
                }
            };



            _pages = new List<PageRecord>
            {
                new PageRecord 
                {
                    ElementId = elementId++,
                    CraetedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "home"
                }
            };

            _pageVersions = new List<PageVersionRecord>
            {
                new PageVersionRecord
                {
                    ElementId = _pages[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    LayoutId = _layouts[0].ElementId,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    Title = "CMS Example",
                    Routes = new List<PageRouteRecord>
                    {
                        new PageRouteRecord
                        {
                            Path = "/home",
                            Priority = 200
                        }
                    },
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "main",
                            ContentType = "html",
                            ContentName = "cms-page1",
                            ContentValue = "<p>Page content goes here</p>"
                        }
                    }
                }
            };

            _properties = new List<ElementPropertyRecord> 
            { 
            };

            _websiteVersions = new List<WebsiteVersionRecord>
            {
                new WebsiteVersionRecord
                {
                    Id = websiteVersionId,
                    Name = "1.0",
                    Description = "First version",
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    BaseUrl = "http://sample4.localhost/"
                }
            };

            // There is only 1 version of this website so all versions
            // of all elements are included

            _websiteVersionPages = _pageVersions
                .Select(pv => 
                    new WebsiteVersionPageRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        PageId = pv.ElementId,
                        PageVersionId = pv.ElementVersionId
                    
                    })
                .ToList();

            _websiteVersionLayouts = _layoutVersions
                .Select(lv => 
                    new WebsiteVersionLayoutRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        LayoutId = lv.ElementId,
                        LayoutVersionId = lv.ElementVersionId
                    })
                .ToList();

            _websiteVersionRegions = _regionVersions
                .Select(rv => 
                    new WebsiteVersionRegionRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        RegionId = rv.ElementId,
                        RegionVersionId = rv.ElementVersionId
                    })
                .ToList();
        }
    }
}
