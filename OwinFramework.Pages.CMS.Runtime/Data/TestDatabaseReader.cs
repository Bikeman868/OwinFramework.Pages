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
                    RecordId = elementId++,
                    Name = "example_region_1",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    RecordId = elementId++,
                    Name = "example_region_2",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _layouts = new []
            {
                new LayoutRecord 
                {
                    RecordId = elementId++,
                    Name = "page_layout",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _regionVersions = new []
            {
                new RegionVersionRecord
                {
                    ParentRecordId = _regions[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    LayoutName = "layouts:col_2_left_fixed"
                },
                new RegionVersionRecord
                {
                    ParentRecordId = _regions[1].RecordId,
                    RecordId = elementVersionId++,
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
                    ParentRecordId = _layouts[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 2,
                    AssetDeployment = AssetDeployment.Inherit,
                    ZoneNesting = "header,main,footer",
                    LayoutZones = new []
                    {
                        new LayoutZoneRecord
                        {
                            ZoneName = "header",
                            RegionId = _regions[0].RecordId
                        },
                        new LayoutZoneRecord
                        {
                            ZoneName = "footer",
                            RegionId = _regions[1].RecordId
                        }
                    }
                }
            };

            _pages = new []
            {
                new PageRecord 
                {
                    RecordId = elementId++,
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    Name = "page_1"
                },
                new PageRecord 
                {
                    RecordId = elementId++,
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    Name = "page_2"
                }
            };

            _pageVersions = new []
            {
                new PageVersionRecord
                {
                    ParentRecordId = _pages[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    LayoutId = _layouts[0].RecordId,
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
                    ParentRecordId = _pages[1].RecordId,
                    RecordId = elementVersionId++,
                    Version = 1,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    LayoutId = _layouts[0].RecordId,
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
                    ParentRecordId = _pages[0].RecordId,
                    RecordId = elementVersionId++,
                    Version = 2,
                    LayoutId = _layouts[0].RecordId,
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
                    ParentRecordId = _pages[1].RecordId,
                    RecordId = elementVersionId++,
                    Version = 2,
                    LayoutId = _layouts[0].RecordId,
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
                    RecordId = websiteVersionId++,
                    Name = "1.0",
                    Description = "First version",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                },
                new WebsiteVersionRecord
                {
                    RecordId = websiteVersionId++,
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
                    RecordId = 1,
                    Name = "prod",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    DisplayName = "Production",
                    BaseUrl = "http://sample1.localhost/",
                    WebsiteVersionId = _websiteVersions[0].RecordId
                },
                new EnvironmentRecord
                {
                    RecordId = 1,
                    Name = "stage",
                    CreatedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    DisplayName = "Staging",
                    BaseUrl = "http://staging.sample1.localhost/",
                    WebsiteVersionId = _websiteVersions[1].RecordId
                }
            };

            _websiteVersionPages = new []
            {
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[0].RecordId,
                    PageId = _pageVersions[0].ParentRecordId,
                    PageVersionId = _pageVersions[0].RecordId
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[0].RecordId,
                    PageId = _pageVersions[1].ParentRecordId,
                    PageVersionId = _pageVersions[1].RecordId
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[1].RecordId,
                    PageId = _pageVersions[2].ParentRecordId,
                    PageVersionId = _pageVersions[2].RecordId
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[1].RecordId,
                    PageId = _pageVersions[3].ParentRecordId,
                    PageVersionId = _pageVersions[3].RecordId
                }
            };

            _websiteVersionLayouts = new []
            {
                new WebsiteVersionLayoutRecord
                {
                    WebsiteVersionId = _websiteVersions[0].RecordId,
                    LayoutId = _layoutVersions[0].ParentRecordId,
                    LayoutVersionId = _layoutVersions[0].RecordId
                },
                new WebsiteVersionLayoutRecord
                {
                    WebsiteVersionId = _websiteVersions[1].RecordId,
                    LayoutId = _layoutVersions[1].ParentRecordId,
                    LayoutVersionId = _layoutVersions[1].RecordId
                }
            };

            _websiteVersionRegions = new []
            {
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[0].RecordId,
                    RegionId = _regionVersions[0].ParentRecordId,
                    RegionVersionId = _regionVersions[0].RecordId
                },
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[0].RecordId,
                    RegionId = _regionVersions[1].ParentRecordId,
                    RegionVersionId = _regionVersions[1].RecordId
                },
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[1].RecordId,
                    RegionId = _regionVersions[0].ParentRecordId,
                    RegionVersionId = _regionVersions[0].RecordId
                },
                new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = _websiteVersions[1].RecordId,
                    RegionId = _regionVersions[1].ParentRecordId,
                    RegionVersionId = _regionVersions[1].RecordId
                }
            };

            _websiteVersionDataTypes = new WebsiteVersionDataTypeRecord [0];
            _websiteVersionComponents = new WebsiteVersionComponentRecord [0];
        }
    }
}
