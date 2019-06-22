using System;
using System.Linq;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;
using Sample4.ViewModels;

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

            #region Elements

            _components = new[]
            {
                new ComponentRecord
                {
                    ElementId = elementId++,
                    Name = "message",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Description = "Displays a text message as a paragraph"
                }
            };

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
                },
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "title",
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
                    ElementId = elementId++,
                    DisplayName = "Customer",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new DataTypeRecord
                {
                    ElementId = elementId++,
                    DisplayName = "Order",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _properties = new[]
            {
                new ElementPropertyRecord
                {
                    ElementId = _components[0].ElementId,
                    ElementPropertyId = propertyId++,
                    Name = "Message",
                    TypeName = "System.String",
                    Type = typeof(string),
                    DisplayName = "Message",
                    Description = "The message to output onto the page"
                },
                new ElementPropertyRecord
                {
                    ElementId = _components[0].ElementId,
                    ElementPropertyId = propertyId++,
                    Name = "Style",
                    TypeName = "System.String",
                    Type = typeof(string),
                    DisplayName = "Style",
                    Description = "The inline style to apply to the message"
                }
            };

            #endregion

            #region Element versions

            _dataTypeVersions = new []
            {
                new DataTypeVersionRecord
                {
                    ElementId = _dataTypes[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    AssemblyName = typeof(CustomerViewModel).Assembly.FullName,
                    TypeName = typeof(CustomerViewModel).FullName,
                    Type = typeof(CustomerViewModel)
                },
                new DataTypeVersionRecord
                {
                    ElementId = _dataTypes[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    AssemblyName = typeof(OrderViewModel).Assembly.FullName,
                    TypeName = typeof(OrderViewModel).FullName,
                    Type = typeof(OrderViewModel)
                },
            };

            _componentVersions = new[]
            {
                new ComponentVersionRecord
                {
                    ElementId = _components[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1
                }
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
                    ComponentId = _components[0].ElementId
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
                    RepeatDataTypeId = _dataTypes[0].ElementId
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
                    RepeatDataTypeId = _dataTypes[1].ElementId
                },
                // title region
                new RegionVersionRecord
                {
                    ElementId = _regions[4].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    ComponentId = _components[0].ElementId
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
                    ZoneNesting = "header,main,footer",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "header",
                            LayoutId = _layouts[1].ElementId
                        },
                        new LayoutZoneRecord
                        {
                            ZoneName = "footer",
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
                    ZoneNesting = "title,menu",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "title",
                            RegionId = _regions[4].ElementId
                        },
                        new LayoutZoneRecord
                        {
                            ZoneName = "menu",
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
                    ZoneNesting = "footer",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "footer",
                            RegionId = _regions[1].ElementId
                        }
                    }
                }
            };

            _propertyValues = new[]
            {
                new ElementPropertyValueRecord
                {
                    ElementPropertyId = _properties[0].ElementPropertyId,
                    ElementVersionId = _regionVersions[1].ElementVersionId,
                    ValueText = "Copyright Martin Halliday 2018-2019",
                    Value = "Copyright Martin Halliday 2018-2019"
                },
                new ElementPropertyValueRecord
                {
                    ElementPropertyId = _properties[0].ElementPropertyId,
                    ElementVersionId = _regionVersions[4].ElementVersionId,
                    ValueText = "Sample 4",
                    Value = "Sample 4"
                },
                new ElementPropertyValueRecord
                {
                    ElementPropertyId = _properties[1].ElementPropertyId,
                    ElementVersionId = _regionVersions[4].ElementVersionId,
                    ValueText = "font-size: 4vw;padding: 15px 0px 0px 30px;margin: 0;letter-spacing: 1px;font-family: sans-serif;color:whitesmoke;background:gray;",
                    Value = "font-size: 4vw;padding: 15px 0px 0px 30px;margin: 0;letter-spacing: 1px;font-family: sans-serif;color:whitesmoke;background:gray;"
                }
            };

            #endregion

            #region Pages

            _pages = new []
            {
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "master_page",
                    DisplayName = "Master page",
                    Description = "Defines page attributes that are inherited by other pages"
                },
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "customers",
                    DisplayName = "Customer list",
                    Description = "Displays a list of customers"
                },
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "orders",
                    DisplayName = "Order list",
                    Description = "Displays a list of orders"
                },
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "cms_manager",
                    DisplayName = "CMS manager",
                    Description = "Displays the CMS manager UI"
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
                    Title = "Sample 4",
                },
                new PageVersionRecord
                {
                    ElementId = _pages[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    MasterPageId = _pages[0].ElementId,
                    Title = "Customers",
                    Routes = new []
                    {
                        new PageRouteRecord
                        {
                            Path = "/customers",
                            Priority = 200
                        }
                    },
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "main",
                            RegionId = _regions[2].ElementId
                        }
                    }
                },
                new PageVersionRecord
                {
                    ElementId = _pages[2].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    MasterPageId = _pages[0].ElementId,
                    Title = "Orders",
                    Routes = new []
                    {
                        new PageRouteRecord
                        {
                            Path = "/orders",
                            Priority = 200
                        }
                    },
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "main",
                            RegionId = _regions[3].ElementId
                        }
                    }
                },
                new PageVersionRecord
                {
                    ElementId = _pages[3].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    MasterPageId = _pages[0].ElementId,
                    Title = "CMS",
                    Routes = new []
                    {
                        new PageRouteRecord
                        {
                            Path = "/admin/cms",
                            Priority = 200
                        }
                    },
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "main",
                            ContentType = "Region",
                            ContentName = "cmsmanager:manager"
                        }
                    }
                }
            };

            #endregion

            #region Website

            _websiteVersions = new []
            {
                new WebsiteVersionRecord
                {
                    WebsiteVersionId = websiteVersionId,
                    Name = "1.0",
                    Description = "First version",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                }
            };

            _environments = new[]
            {
                new EnvironmentRecord
                {
                    EnvironmentId = 1,
                    Name = "prod",
                    DisplayName = "Production",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    BaseUrl = "http://sample4.localhost/",
                    WebsiteVersionId = websiteVersionId
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

            _websiteVersionComponents = _componentVersions
                .Select(v => 
                    new WebsiteVersionComponentRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        ComponentId = v.ElementId,
                        ComponentVersionId = v.ElementVersionId
                    })
                .ToArray();

            _websiteVersionDataTypes = _dataTypeVersions
                .Select(v => 
                    new WebsiteVersionDataTypeRecord
                    {
                        WebsiteVersionId = websiteVersionId,
                        DataTypeVersionId = v.ElementVersionId,
                        DataTypeId = v.ElementId
                    })
                .ToArray();

            #endregion
        }

    }
}
