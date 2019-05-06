using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.CMS.Runtime.Data;
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

            _regions = new []
            {
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "header",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "footer",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "customer_list",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "order_list",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _layouts = new []
            {
                new LayoutRecord 
                {
                    ElementId = elementId++,
                    Name = "page",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new LayoutRecord 
                {
                    ElementId = elementId++,
                    Name = "header",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new LayoutRecord 
                {
                    ElementId = elementId++,
                    Name = "footer",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
            };



            _dataScopes = new DataScopeRecord[0];

            _dataTypes = new []
            {
                new DataTypeRecord
                {
                    DataTypeId = dataTypeId++,
                    DisplayName = "Customer",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new DataTypeRecord
                {
                    DataTypeId = dataTypeId++,
                    DisplayName = "Order",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _dataTypeVersions = new []
            {
                new DataTypeVersionRecord
                {
                    DataTypeId = _dataTypes[0].DataTypeId,
                    DataTypeVersionId = dataTypeVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    AssemblyName = typeof(CustomerViewModel).Assembly.FullName,
                    TypeName = typeof(CustomerViewModel).FullName,
                    Type = typeof(CustomerViewModel)
                },
                new DataTypeVersionRecord
                {
                    DataTypeId = _dataTypes[1].DataTypeId,
                    DataTypeVersionId = dataTypeVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    AssemblyName = typeof(OrderViewModel).Assembly.FullName,
                    TypeName = typeof(OrderViewModel).FullName,
                    Type = typeof(OrderViewModel)
                },
            };



            _regionVersions = new []
            {
                // header region
                new RegionVersionRecord
                {
                    ElementId = _regions[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    LayoutName = "layouts:col_2_left_fixed"
                },
                // footer region
                new RegionVersionRecord
                {
                    ElementId = _regions[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    LayoutName = "layouts:full_page"
                },
                // customer_list region
                new RegionVersionRecord
                {
                    ElementId = _regions[2].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    RegionTemplates = new []
                    {
                        new RegionTemplateRecord{PageArea = PageArea.Body, TemplatePath = "/customer"}
                    },
                    RepeatDataTypeId = _dataTypes[0].DataTypeId
                },
                // order_list region
                new RegionVersionRecord
                {
                    ElementId = _regions[3].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    RegionTemplates = new []
                    {
                        new RegionTemplateRecord{PageArea = PageArea.Body, TemplatePath = "/order"}
                    },
                    RepeatDataTypeId = _dataTypes[1].DataTypeId
                },
            };

            _layoutVersions = new []
            {
                // page layout
                new LayoutVersionRecord {
                    ElementId = _layouts[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "header,main,footer",
                    LayoutRegions = new []
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
                // header layout
                new LayoutVersionRecord {
                    ElementId = _layouts[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "title,menu",
                    LayoutRegions = new []
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "title",
                            ContentType = "html",
                            ContentName = "website-title",
                            ContentValue = "<h1>CMS Example</h1>"
                        },
                        new LayoutRegionRecord
                        {
                            RegionName = "menu",
                            ContentType = "region",
                            ContentName = "menus:desktop_menu"
                        }
                    },
                    Components = new []
                    {
                        new ElementComponentRecord
                        {
                            ComponentName = "menus:menuStyle1"
                        }
                    }
                },
                // footer layout
                new LayoutVersionRecord {
                    ElementId = _layouts[2].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "footer",
                    LayoutRegions = new []
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



            _pages = new []
            {
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "customers",
                    Description = "Displays a list of customers"
                },
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "orders",
                    Description = "Displays a list of orders"
                }
            };

            _pageVersions = new []
            {
                new PageVersionRecord
                {
                    ElementId = _pages[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    LayoutId = _layouts[0].ElementId,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    Title = "Customers",
                    Routes = new List<PageRouteRecord>
                    {
                        new PageRouteRecord
                        {
                            Path = "/customers",
                            Priority = 200
                        }
                    },
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "main",
                            RegionId = _regions[2].ElementId
                        }
                    }
                },
                new PageVersionRecord
                {
                    ElementId = _pages[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    LayoutId = _layouts[0].ElementId,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    Title = "Orders",
                    Routes = new List<PageRouteRecord>
                    {
                        new PageRouteRecord
                        {
                            Path = "/orders",
                            Priority = 200
                        }
                    },
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "main",
                            RegionId = _regions[3].ElementId
                        }
                    }
                }
            };

            _properties = new ElementPropertyRecord[0];

            _websiteVersions = new []
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
                .ToArray();

            _websiteVersionLayouts = _layoutVersions
                .Select(lv => 
                    new WebsiteVersionLayoutRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        LayoutId = lv.ElementId,
                        LayoutVersionId = lv.ElementVersionId
                    })
                .ToArray();

            _websiteVersionRegions = _regionVersions
                .Select(rv => 
                    new WebsiteVersionRegionRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        RegionId = rv.ElementId,
                        RegionVersionId = rv.ElementVersionId
                    })
                .ToArray();

            _websiteVersionDataTypes = _dataTypeVersions
                .Select(v => 
                    new WebsiteVersionDataTypeRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        DataTypeVersionId = v.DataTypeVersionId,
                        DataTypeId = v.DataTypeId
                    })
                .ToArray();
        }
    }
}
