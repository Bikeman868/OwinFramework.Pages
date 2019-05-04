using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;

namespace Sample4.CmsData
{
    internal class StaticData: IDatabaseReader
    {
        private readonly List<WebsiteVersionRecord> _websiteVersions;
        private readonly List<WebsiteVersionPageRecord> _websiteVersionPages;

        private readonly List<PageRecord> _pages;
        private readonly List<PageVersionRecord> _pageVersions;

        private readonly List<LayoutRecord> _layouts;
        private readonly List<LayoutVersionRecord> _layoutVersions;

        private readonly List<RegionRecord> _regions;
        private readonly List<RegionVersionRecord> _regionVersions;

        private readonly List<ElementPropertyRecord> _properties;

        public StaticData()
        {
            var elementId = 1;
            var elementVersionId = 1;
            var propertyId = 1;
            var websiteVersionId = 1;

            _regions = new List<RegionRecord>
            {
                new RegionRecord
                {
                    Id = elementId++,
                    Name = "example_region_1",
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                },
                new RegionRecord
                {
                    Id = elementId++,
                    Name = "example_region_2",
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _regionVersions = new List<RegionVersionRecord>
            {
                new RegionVersionRecord
                {
                    Id = elementVersionId++,
                    Version = 1,
                    ElementId = _regions[0].Id,
                    LayoutName = "layouts:col_2_left_fixed"
                },
                new RegionVersionRecord
                {
                    Id = elementVersionId++,
                    Version = 1,
                    ElementId = _regions[1].Id,
                    RegionTemplates = new List<RegionTemplateRecord>
                    {
                        new RegionTemplateRecord{PageArea = PageArea.Body, TemplatePath = "/template1"}
                    }
                }
            };

            _layouts = new List<LayoutRecord>
            {
                new LayoutRecord 
                {
                    Id = elementId++,
                    Name = "page_layout",
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _layoutVersions = new List<LayoutVersionRecord>
            {
                new LayoutVersionRecord {
                    Id = elementVersionId++,
                    Version = 1,
                    ElementId = _layouts[0].Id,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "header,main,footer",
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "header",
                            ContentType = "html",
                            ContentName = "header",
                            ContentValue = "<h1>Header V1</h1>"
                        },
                        new LayoutRegionRecord
                        {
                            RegionName = "footer",
                            ContentType = "html",
                            ContentName = "footer",
                            ContentValue = "<h1>Footer V1</h1>"
                        }
                    }
                },
                new LayoutVersionRecord {
                    Id = elementVersionId++,
                    Version = 2,
                    ElementId = _layouts[0].Id,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "header,main,footer",
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "header",
                            RegionVersionId = _regionVersions[0].Id
                        },
                        new LayoutRegionRecord
                        {
                            RegionName = "footer",
                            RegionVersionId = _regionVersions[1].Id
                        }
                    }
                }
            };

            _pages = new List<PageRecord>
            {
                new PageRecord 
                {
                    Id = elementId++,
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    Name = "page_1"
                },
                new PageRecord 
                {
                    Id = elementId++,
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    Name = "page_2"
                }
            };

            _pageVersions = new List<PageVersionRecord>
            {
                new PageVersionRecord
                {
                    Id = elementVersionId++,
                    Version = 1,
                    ElementId = _pages[0].Id,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    LayoutVersionId = _layoutVersions[0].Id,
                    Title = "First CMS Page",
                    Routes = new List<PageRouteRecord>
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
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "main",
                            ContentType = "html",
                            ContentName = "cms-page1",
                            ContentValue = "<p>This is CMS page 1</p>"
                        }
                    }
                },
                new PageVersionRecord
                {
                    Id = elementVersionId++,
                    Version = 1,
                    ElementId = _pages[1].Id,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    LayoutVersionId = _layoutVersions[0].Id,
                    Title = "Second CMS Page",
                    Routes = new List<PageRouteRecord>
                    {
                        new PageRouteRecord
                        {
                            Path = "/cms/page2",
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
                            ContentValue = "<p>This is CMS page 2</p>"
                        }
                    }
                },
                new PageVersionRecord
                {
                    Id = elementVersionId++,
                    Version = 2,
                    ElementId = _pages[0].Id,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    LayoutVersionId = _layoutVersions[1].Id,
                    Title = "First CMS Page",
                    Routes = new List<PageRouteRecord>
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
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "main",
                            ContentType = "html",
                            ContentName = "cms-page1",
                            ContentValue = "<p>This is CMS page 1</p>"
                        }
                    }
                },
                new PageVersionRecord
                {
                    Id = elementVersionId++,
                    Version = 2,
                    ElementId = _pages[1].Id,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    LayoutVersionId = _layoutVersions[1].Id,
                    Title = "Second CMS Page",
                    Routes = new List<PageRouteRecord>
                    {
                        new PageRouteRecord
                        {
                            Path = "/cms/page2",
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
                            ContentValue = "<p>This is CMS page 2</p>"
                        }
                    }
                }
            };

            _properties = new List<ElementPropertyRecord> 
            { 
                new ElementPropertyRecord
                {
                    Id = propertyId++,
                    ElementVersionId = _pageVersions[0].Id,
                    Name = "StockTicker",
                    Value = "AMZN"
                }
            };

            _websiteVersions = new List<WebsiteVersionRecord>
            {
                new WebsiteVersionRecord
                {
                    Id = websiteVersionId++,
                    Name = "1.0",
                    Description = "First version",
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    BaseUrl = "http://sample1.localhost/"
                },
                new WebsiteVersionRecord
                {
                    Id = websiteVersionId++,
                    Name = "1.1",
                    Description = "Modified layout",
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow,
                    BaseUrl = "http://uat.sample1.localhost/"
                }
            };

            _websiteVersionPages = new List<WebsiteVersionPageRecord>
            {
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[0].Id,
                    PageVersionId = _pageVersions[0].Id
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[0].Id,
                    PageVersionId = _pageVersions[1].Id
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[1].Id,
                    PageVersionId = _pageVersions[2].Id
                },
                new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = _websiteVersions[1].Id,
                    PageVersionId = _pageVersions[3].Id
                }
            };
        }

        IList<T> IDatabaseReader.GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersions
                    .Select(map)
                    .ToList();

            return _websiteVersions
                .Where(predicate)
                .Select(map)
                .ToList();
        }

        IList<T> IDatabaseReader.GetWebsiteVersionPages<T>(
            long websiteVersionId, 
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionPages
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToList();

            return _websiteVersionPages
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToList();
        }

        IList<T> IDatabaseReader.GetWebsiteVersionPages<T>(
            string websiteVersionName, 
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => String.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteVersionPages(websiteVersion.Id, map, predicate);
        }

        IDictionary<string, string> IDatabaseReader.GetElementProperties(long elementVersionId)
        {
            return _properties
                .Where(p => p.ElementVersionId == elementVersionId)
                .ToDictionary(p => p.Name, p => p.Value);
        }

        IList<T> IDatabaseReader.GetElementVersions<T>(
            long elementId, 
            Func<ElementVersionRecordBase, T> map)
        {
            IEnumerable<ElementVersionRecordBase> elementVersions = null;

            if (elementVersions == null)
            {
                var page = _pages.FirstOrDefault(p => p.Id == elementId);
                if (page != null)
                {
                    elementVersions = _pageVersions.Where(pv => pv.ElementId == page.Id);
                }
            }

            if (elementVersions == null)
            {
                var layout = _layouts.FirstOrDefault(l => l.Id == elementId);
                if (layout != null)
                {
                    elementVersions = _layoutVersions.Where(lv => lv.ElementId == layout.Id);
                }
            }
            
            return elementVersions == null 
                ? new List<T>()
                : elementVersions.Select(map).ToList();
        }

        T IDatabaseReader.GetPage<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map)
        {
            var page = _pages.FirstOrDefault(p => p.Id == pageId);
            if (page == null) return default(T);

            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ElementId == page.Id && pv.Version == version);
            if (pageVersion == null) return default(T);

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayout<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layout = _layouts.FirstOrDefault(l => l.Id == layoutId);
            if (layout == null) return default(T);

            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.ElementId == layout.Id && lv.Version == version);
            if (layoutVersion == null) return default(T);

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var region = _regions.FirstOrDefault(r => r.Id == regionId);
            if (region == null) return default(T);

            var regionVersion = _regionVersions.FirstOrDefault(rv => rv.ElementId == region.Id && rv.Version == version);
            if (regionVersion == null) return default(T);

            return map(region, regionVersion);
        }

        T IDatabaseReader.GetPage<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.Id == pageVersionId);
            if (pageVersion == null) return default(T);

            var page = _pages.FirstOrDefault(p => p.Id == pageVersion.ElementId);
            if (page == null) return default(T);

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayout<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.Id == layoutVersionId);
            if (layoutVersion == null) return default(T);

            var layout = _layouts.FirstOrDefault(l => l.Id == layoutVersion.ElementId);
            if (layout == null) return default(T);

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var regionVersion = _regionVersions.FirstOrDefault(lv => lv.Id == regionVersionId);
            if (regionVersion == null) return default(T);

            var region = _regions.FirstOrDefault(r => r.Id == regionVersion.ElementId);
            if (region == null) return default(T);

            return map(region, regionVersion);
        }
    }
}
