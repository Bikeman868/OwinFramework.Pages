using System;
using System.Linq;
using Newtonsoft.Json;
using OwinFramework.Pages.CMS.Manager.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;
using Sample4.ViewModels;

namespace Sample4.CmsData
{
    internal class StaticData: TestDatabaseUpdaterBase, IDatabaseReader
    {
        public StaticData()
        {
            const string creator = "urn:user:1";

            var elementId = 1;
            var elementVersionId = 1;
            var propertyId = 1;
            var dataScopeId = 1;

            #region Elements

            _components = new[]
            {
                new ComponentRecord
                {
                    RecordId = elementId++,
                    Name = "message",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    DisplayName = "Message",
                    Description = "Displays a text message as a paragraph"
                }
            };

            _regions = new []
            {
                new RegionRecord
                {
                    RecordId = elementId++,
                    Name = "header",
                    DisplayName = "Header",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    RecordId = elementId++,
                    Name = "footer",
                    DisplayName = "Footer",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    RecordId = elementId++,
                    Name = "customer_list",
                    DisplayName = "Customer list",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    RecordId = elementId++,
                    Name = "order_list",
                    DisplayName = "Order list",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    RecordId = elementId++,
                    Name = "title",
                    DisplayName = "Title",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _layouts = new []
            {
                new LayoutRecord 
                {
                    RecordId = elementId++,
                    Name = "page",
                    DisplayName = "Page",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new LayoutRecord 
                {
                    RecordId = elementId++,
                    Name = "header",
                    DisplayName = "Page head",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new LayoutRecord 
                {
                    RecordId = elementId++,
                    Name = "footer",
                    DisplayName = "Page footer",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
            };

            _dataScopes = new DataScopeRecord[0];

            _dataTypes = new []
            {
                new DataTypeRecord
                {
                    RecordId = elementId++,
                    DisplayName = "Customer",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                },
                new DataTypeRecord
                {
                    RecordId = elementId++,
                    DisplayName = "Order",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _properties = new[]
            {
                new ElementPropertyRecord
                {
                    ParentRecordId = _components[0].RecordId,
                    RecordId = propertyId++,
                    Name = "Message",
                    TypeName = "System.String",
                    Type = typeof(string),
                    DisplayName = "Message",
                    Description = "The message to output onto the page"
                },
                new ElementPropertyRecord
                {
                    ParentRecordId = _components[0].RecordId,
                    RecordId = propertyId++,
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
                    ParentRecordId = _dataTypes[0].RecordId,
                    RecordId = elementVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    AssemblyName = typeof(CustomerViewModel).Assembly.FullName,
                    TypeName = typeof(CustomerViewModel).FullName,
                    Type = typeof(CustomerViewModel)
                },
                new DataTypeVersionRecord
                {
                    ParentRecordId = _dataTypes[1].RecordId,
                    RecordId = elementVersionId++,
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
                    ParentRecordId = _components[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1
                }
            };

            _regionVersions = new []
            {
                // header region
                new RegionVersionRecord
                {
                    ParentRecordId = _regions[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    LayoutName = "layouts:col_2_left_fixed"
                },
                // footer region
                new RegionVersionRecord
                {
                    ParentRecordId = _regions[1].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    ComponentId = _components[0].RecordId
                },
                // customer_list region
                new RegionVersionRecord
                {
                    ParentRecordId = _regions[2].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    RegionTemplates = new []
                    {
                        new RegionTemplateRecord{PageArea = PageArea.Body, TemplatePath = "/customer"}
                    },
                    RepeatDataTypeId = _dataTypes[0].RecordId
                },
                // order_list region
                new RegionVersionRecord
                {
                    ParentRecordId = _regions[3].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    RegionTemplates = new []
                    {
                        new RegionTemplateRecord{PageArea = PageArea.Body, TemplatePath = "/order"}
                    },
                    RepeatDataTypeId = _dataTypes[1].RecordId
                },
                // title region
                new RegionVersionRecord
                {
                    ParentRecordId = _regions[4].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    ComponentId = _components[0].RecordId
                },
            };

            _layoutVersions = new []
            {
                // page layout
                new LayoutVersionRecord {
                    ParentRecordId = _layouts[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    ZoneNesting = "header,main,footer",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "header",
                            LayoutId = _layouts[1].RecordId
                        },
                        new LayoutZoneRecord
                        {
                            ZoneName = "footer",
                            LayoutId = _layouts[2].RecordId
                        }
                    }
                },

                // header layout
                new LayoutVersionRecord {
                    ParentRecordId = _layouts[1].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    ZoneNesting = "title,menu",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "title",
                            RegionId = _regions[4].RecordId
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
                    ParentRecordId = _layouts[2].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.Inherit,
                    ZoneNesting = "footer",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "footer",
                            RegionId = _regions[1].RecordId
                        }
                    }
                }
            };

            _propertyValues = new[]
            {
                new ElementPropertyValueRecord
                {
                    RecordId = _properties[0].RecordId,
                    ParentRecordId = _regionVersions[1].RecordId,
                    ValueText = "Copyright Martin Halliday 2018-2019",
                    Value = "Copyright Martin Halliday 2018-2019"
                },
                new ElementPropertyValueRecord
                {
                    RecordId = _properties[0].RecordId,
                    ParentRecordId = _regionVersions[4].RecordId,
                    ValueText = "Sample 4",
                    Value = "Sample 4"
                },
                new ElementPropertyValueRecord
                {
                    RecordId = _properties[1].RecordId,
                    ParentRecordId = _regionVersions[4].RecordId,
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
                    RecordId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "root",
                    DisplayName = "Root",
                    Description = "Defines page attributes that are inherited by other pages"
                },
                new PageRecord 
                {
                    RecordId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "customers",
                    DisplayName = "Customer list",
                    Description = "Displays a list of customers"
                },
                new PageRecord 
                {
                    RecordId = elementId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Name = "orders",
                    DisplayName = "Order list",
                    Description = "Displays a list of orders"
                },
                new PageRecord 
                {
                    RecordId = elementId++,
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
                    ParentRecordId = _pages[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    DisplayName = "Version 1",
                    LayoutId = _layouts[0].RecordId,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    Title = "Sample 4",
                },
                new PageVersionRecord
                {
                    ParentRecordId = _pages[1].RecordId,
                    RecordId = elementVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Version = 1,
                    DisplayName = "Version 1",
                    MasterPageId = _pages[0].RecordId,
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
                            RegionId = _regions[2].RecordId
                        }
                    }
                },
                new PageVersionRecord
                {
                    ParentRecordId = _pages[2].RecordId,
                    RecordId = elementVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Version = 1,
                    DisplayName = "Version 1",
                    MasterPageId = _pages[0].RecordId,
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
                            RegionId = _regions[3].RecordId
                        }
                    }
                },
                new PageVersionRecord
                {
                    ParentRecordId = _pages[3].RecordId,
                    RecordId = elementVersionId++,
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    Version = 1,
                    DisplayName = "Version 1",
                    MasterPageId = _pages[0].RecordId,
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
                    RecordId = 1,
                    Name = "v1",
                    DisplayName = "Sprint 224",
                    Description = "First version",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                },
                new WebsiteVersionRecord
                {
                    RecordId = 2,
                    Name = "v2",
                    DisplayName = "Sprint 225",
                    Description = "Second version",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                }
            };

            _environments = new[]
            {
                new EnvironmentRecord
                {
                    RecordId = 1,
                    Name = "prod",
                    DisplayName = "Production",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    BaseUrl = "http://sample4.localhost/",
                    WebsiteVersionId = 1
                },
                new EnvironmentRecord
                {
                    RecordId = 2,
                    Name = "stage",
                    DisplayName = "Staging",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    BaseUrl = "http://staging.sample4.localhost/",
                    WebsiteVersionId = 1
                },
                new EnvironmentRecord
                {
                    RecordId = 3,
                    Name = "uat",
                    DisplayName = "User acceptance",
                    CreatedBy = creator,
                    CreatedWhen = DateTime.UtcNow,
                    BaseUrl = "http://uat.sample4.localhost/",
                    WebsiteVersionId = 2
                }
            };

            _websiteVersionPages = _pageVersions
                .Select(pv => 
                    new WebsiteVersionPageRecord
                    {
                        WebsiteVersionId = 1,
                        PageId = pv.ParentRecordId,
                        PageVersionId = pv.RecordId
                    
                    })
                .ToArray();

            _websiteVersionLayouts = _layoutVersions
                .Select(lv => 
                    new WebsiteVersionLayoutRecord
                    {
                        WebsiteVersionId = 1,
                        LayoutId = lv.ParentRecordId,
                        LayoutVersionId = lv.RecordId
                    })
                .ToArray();

            _websiteVersionRegions = _regionVersions
                .Select(rv => 
                    new WebsiteVersionRegionRecord
                    {
                        WebsiteVersionId = 1,
                        RegionId = rv.ParentRecordId,
                        RegionVersionId = rv.RecordId
                    })
                .ToArray();

            _websiteVersionComponents = _componentVersions
                .Select(v => 
                    new WebsiteVersionComponentRecord
                    {
                        WebsiteVersionId = 1,
                        ComponentId = v.ParentRecordId,
                        ComponentVersionId = v.RecordId
                    })
                .ToArray();

            _websiteVersionDataTypes = _dataTypeVersions
                .Select(v => 
                    new WebsiteVersionDataTypeRecord
                    {
                        WebsiteVersionId = 1,
                        DataTypeVersionId = v.RecordId,
                        DataTypeId = v.ParentRecordId
                    })
                .ToArray();

            #endregion
        }

    }
}
