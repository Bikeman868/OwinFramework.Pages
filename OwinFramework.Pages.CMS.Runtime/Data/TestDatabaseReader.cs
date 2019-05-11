using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.CMS.Runtime.Data
{
    internal class TestDatabaseReader: TestDatabaseReaderBase
    {
        public TestDatabaseReader()
        {
            var elementId = 1;
            var elementVersionId = 1;
            var propertyId = 1;
            var websiteVersionId = 1;
            var dataScopeId = 1;
            var dataTypeId = 1;
            var dataTypeVersionId = 1;

            _dataScopes = new DataScopeRecord[0];

            _dataTypes = new DataTypeRecord[0];

            _dataTypeVersions = new DataTypeVersionRecord[0];

            _components = new ComponentRecord[0];

            _componentVersions = new ComponentVersionRecord[0];

            _properties = new ElementPropertyRecord[0];

            _propertyValues = new ElementPropertyValueRecord[0];

            _regions = new []
            {
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "example_region_1",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    ElementId = elementId++,
                    Name = "example_region_2",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _layouts = new []
            {
                new LayoutRecord 
                {
                    ElementId = elementId++,
                    Name = "page_layout",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _regionVersions = new []
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
                    RegionTemplates = new []
                    {
                        new RegionTemplateRecord{PageArea = PageArea.Body, TemplatePath = "/template1"}
                    }
                }
            };

            _layoutVersions = new []
            {
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
                            ContentType = "html",
                            ContentName = "header",
                            ContentValue = "<h1>Header V1</h1>"
                        },
                        new LayoutZoneRecord
                        {
                            ZoneName = "footer",
                            ContentType = "html",
                            ContentName = "footer",
                            ContentValue = "<h1>Footer V1</h1>"
                        }
                    }
                },
                new LayoutVersionRecord {
                    ElementId = _layouts[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 2,
                    AssetDeployment = AssetDeployment.Inherit,
                    ZoneNesting = "header,main,footer",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "header",
                            RegionId = _regions[0].ElementId
                        },
                        new LayoutZoneRecord
                        {
                            ZoneName = "footer",
                            RegionId = _regions[1].ElementId
                        }
                    }
                }
            };

            _pages = new []
            {
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    Name = "page_1"
                },
                new PageRecord 
                {
                    ElementId = elementId++,
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    Name = "page_2"
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
                    Title = "First CMS Page",
                    Routes = new []
                    {
                        new PageRouteRecord
                        {
                            Path = "/cms/page1",
                            Priority = 200
                        },
                        new PageRouteRecord
                        {
                            Path = "/cms/page1_old_url",
                            Priority = -10
                        }
                    },
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "main",
                            ContentType = "html",
                            ContentName = "cms-page1",
                            ContentValue = "<p>This is CMS page 1</p>"
                        }
                    }
                },
                new PageVersionRecord
                {
                    ElementId = _pages[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    LayoutId = _layouts[0].ElementId,
                    Title = "Second CMS Page",
                    Routes = new []
                    {
                        new PageRouteRecord
                        {
                            Path = "/cms/page2",
                            Priority = 200
                        }
                    },
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "main",
                            ContentType = "html",
                            ContentName = "cms-page1",
                            ContentValue = "<p>This is CMS page 2</p>"
                        }
                    }
                },
                new PageVersionRecord
                {
                    ElementId = _pages[0].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 2,
                    LayoutId = _layouts[0].ElementId,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    Title = "First CMS Page",
                    Routes = new []
                    {
                        new PageRouteRecord
                        {
                            Path = "/cms/page1",
                            Priority = 200
                        },
                        new PageRouteRecord
                        {
                            Path = "/cms/page1_old_url",
                            Priority = -10
                        }
                    },
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "main",
                            ContentType = "html",
                            ContentName = "cms-page1",
                            ContentValue = "<p>This is CMS page 1</p>"
                        }
                    }
                },
                new PageVersionRecord
                {
                    ElementId = _pages[1].ElementId,
                    ElementVersionId = elementVersionId++,
                    Version = 2,
                    LayoutId = _layouts[0].ElementId,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    Title = "Second CMS Page",
                    Routes = new []
                    {
                        new PageRouteRecord
                        {
                            Path = "/cms/page2",
                            Priority = 200
                        }
                    },
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "main",
                            ContentType = "html",
                            ContentName = "cms-page1",
                            ContentValue = "<p>This is CMS page 2</p>"
                        }
                    }
                }
            };

            _websiteVersions = new []
            {
                new WebsiteVersionRecord
                {
                    WebsiteVersionId = websiteVersionId++,
                    Name = "1.0",
                    Description = "First version",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                },
                new WebsiteVersionRecord
                {
                    WebsiteVersionId = websiteVersionId++,
                    Name = "1.1",
                    Description = "Modified layout",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                }
            };

            _environments = new[]
            {
                new EnvironmentRecord
                {
                    EnvironmentId = 1,
                    Name = "prod",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    DisplayName = "Production",
                    BaseUrl = "http://sample1.localhost/",
                    WebsiteVersionId = _websiteVersions[0].WebsiteVersionId
                },
                new EnvironmentRecord
                {
                    EnvironmentId = 1,
                    Name = "stage",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    DisplayName = "Staging",
                    BaseUrl = "http://staging.sample1.localhost/",
                    WebsiteVersionId = _websiteVersions[1].WebsiteVersionId
                }
            };

            _websiteVersionPages = new []
            {
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[0].WebsiteVersionId,
                    PageId = _pageVersions[0].ElementId,
                    PageVersionId = _pageVersions[0].ElementVersionId
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[0].WebsiteVersionId,
                    PageId = _pageVersions[1].ElementId,
                    PageVersionId = _pageVersions[1].ElementVersionId
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[1].WebsiteVersionId,
                    PageId = _pageVersions[2].ElementId,
                    PageVersionId = _pageVersions[2].ElementVersionId
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[1].WebsiteVersionId,
                    PageId = _pageVersions[3].ElementId,
                    PageVersionId = _pageVersions[3].ElementVersionId
                }
            };

            _websiteVersionLayouts = new []
            {
                new WebsiteVersionLayoutRecord
                {
                    WebsiteVersionId = _websiteVersions[0].WebsiteVersionId,
                    LayoutId = _layoutVersions[0].ElementId,
                    LayoutVersionId = _layoutVersions[0].ElementVersionId
                },
                new WebsiteVersionLayoutRecord
                {
                    WebsiteVersionId = _websiteVersions[1].WebsiteVersionId,
                    LayoutId = _layoutVersions[1].ElementId,
                    LayoutVersionId = _layoutVersions[1].ElementVersionId
                }
            };

            _websiteVersionRegions = new []
            {
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[0].WebsiteVersionId,
                    RegionId = _regionVersions[0].ElementId,
                    RegionVersionId = _regionVersions[0].ElementVersionId
                },
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[0].WebsiteVersionId,
                    RegionId = _regionVersions[1].ElementId,
                    RegionVersionId = _regionVersions[1].ElementVersionId
                },
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[1].WebsiteVersionId,
                    RegionId = _regionVersions[0].ElementId,
                    RegionVersionId = _regionVersions[0].ElementVersionId
                },
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[1].WebsiteVersionId,
                    RegionId = _regionVersions[1].ElementId,
                    RegionVersionId = _regionVersions[1].ElementVersionId
                }
            };

            _websiteVersionDataTypes = new WebsiteVersionDataTypeRecord [0];
            _websiteVersionComponents = new WebsiteVersionComponentRecord [0];
        }
    }
}
