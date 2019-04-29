using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.CMS.Runtime.Data
{
    internal class TestDatabaseReader: IDatabaseReader
    {
        private List<VersionRecord> _versions;

        private List<PageRecord> _pages;
        private List<PageVersionRecord> _pageVersions;

        private List<LayoutRecord> _layouts;
        private List<LayoutVersionRecord> _layoutVersions;

        private List<ElementPropertyRecord> _properties;

        public TestDatabaseReader()
        {
            _versions = new List<VersionRecord>
            {
                new VersionRecord
                {
                    Id = 1,
                    Name = "1.0",
                    Description = "First version",
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            var elementId = 1;
            var elementVersionId = 1;
            var propertyId = 1;

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

            _layoutVersions = _layouts.Select(l => new LayoutVersionRecord
                {
                    Id = elementVersionId++,
                    VersionId = _versions[0].Id,
                    ElementId = l.Id,
                    Enabled = true,
                    AssetDeployment = AssetDeployment.Inherit,
                    RegionNesting = "header,main,footer",
                    LayoutRegions = new List<LayoutRegionRecord>
                    {
                        new LayoutRegionRecord
                        {
                            RegionName = "header",
                            ContentType = "html",
                            ContentName = "header",
                            ContentValue = "<h1>Header</h1>"
                        },
                        new LayoutRegionRecord
                        {
                            RegionName = "footer",
                            ContentType = "html",
                            ContentName = "footer",
                            ContentValue = "<h1>Footer</h1>"
                        }
                    }
                }).ToList();

            _pages = new List<PageRecord>
            {
                new PageRecord 
                {
                    Id = elementId++,
                    CraetedBy = "urn:user:1234",
                    CreatedWhen = DateTime.UtcNow
                }
            };

            _pageVersions = _pages.Select(p => new PageVersionRecord
                {
                    Id = elementVersionId++,
                    VersionId = _versions[0].Id,
                    ElementId = p.Id,
                    Enabled = true,
                    AssetDeployment = AssetDeployment.PerWebsite,
                    LayoutName = _layouts[0].Name,
                    CanonicalUrl = "http://sample1.localhost/cms/page1/",
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
                            Path = "/cms/page2",
                            Priority = 100
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
                }).ToList();

            _properties = new List<ElementPropertyRecord> 
            { 
                new ElementPropertyRecord
                {
                    Id = propertyId++,
                    ElementVersionId = _pageVersions[0].Id,
                    Name = "Title",
                    Value = "First CMS Page"
                }
            };
        }

        IList<T> IDatabaseReader.GetVersions<T>(Func<VersionRecord, T> map, Func<VersionRecord, bool> predicate)
        {
            if (predicate == null)
                return _versions.Select(map).ToList();
            return _versions.Where(predicate).Select(map).ToList();
        }

        IDictionary<string, string> IDatabaseReader.GetElementProperties(long elementVersionId)
        {
            return _properties.Where(p => p.ElementVersionId == elementVersionId).ToDictionary(p => p.Name, p => p.Value);
        }

        IList<T> IDatabaseReader.GetElementVersions<T>(long elementId, Func<VersionRecord, ElementVersionRecordBase, T> map)
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
                    elementVersions = _layoutVersions
                        .Where(lv => lv.ElementId == layout.Id);
                }
            }
            
            return elementVersions == null 
                ? new List<T>()
                : elementVersions
                    .Select(ev => map(_versions.FirstOrDefault(v => v.Id == ev.VersionId), ev))
                    .ToList();
        }

        T IDatabaseReader.GetPage<T>(long elementId, string versionName, Func<PageRecord, PageVersionRecord, T> map)
        {
            var version = _versions.FirstOrDefault(v => string.Equals(v.Name, versionName, StringComparison.OrdinalIgnoreCase));
            if (version == null) return default(T);

            var page = _pages.FirstOrDefault(p => p.Id == elementId);
            if (page == null) return default(T);

            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ElementId == page.Id && pv.VersionId == version.Id);
            if (pageVersion == null) return default(T);

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayout<T>(long elementId, string versionName, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var version = _versions.FirstOrDefault(v => string.Equals(v.Name, versionName, StringComparison.OrdinalIgnoreCase));
            if (version == null) return default(T);

            var layout = _layouts.FirstOrDefault(l => l.Id == elementId);
            if (layout == null) return default(T);

            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.ElementId == layout.Id && lv.VersionId == version.Id);
            if (layoutVersion == null) return default(T);

            return map(layout, layoutVersion);
        }

        IList<T> IDatabaseReader.GetPages<T>(
            string versionName, 
            Func<PageRecord, PageVersionRecord, T> map, 
            Func<PageRecord, PageVersionRecord, bool> predicate)
        {
            var version = _versions.FirstOrDefault(v => string.Equals(v.Name, versionName, StringComparison.OrdinalIgnoreCase));
            if (version == null) return new List<T>();

            var pages = _pageVersions
                .Where(pv => pv.VersionId == version.Id)
                .Select(pv => new 
                { 
                    PageVersion = pv, 
                    Page = _pages.FirstOrDefault(p => p.Id == pv.ElementId)
                });

            if (predicate != null) pages = pages.Where(o => predicate(o.Page, o.PageVersion));

            return pages.Select(o => map(o.Page, o.PageVersion)).ToList();
        }

        IList<T> IDatabaseReader.GetLayouts<T>(
            string versionName, 
            Func<LayoutRecord, LayoutVersionRecord, T> map, 
            Func<LayoutRecord, LayoutVersionRecord, bool> predicate)
        {
            var version = _versions.FirstOrDefault(v => string.Equals(v.Name, versionName, StringComparison.OrdinalIgnoreCase));
            if (version == null) return new List<T>();

            var layouts = _layoutVersions
                .Where(lv => lv.VersionId == version.Id)
                .Select(lv => new 
                { 
                    LayoutVersion = lv, 
                    Layout = _layouts.FirstOrDefault(l => l.Id == lv.ElementId)
                });

            if (predicate != null) layouts = layouts.Where(o => predicate(o.Layout, o.LayoutVersion));

            return layouts.Select(o => map(o.Layout, o.LayoutVersion)).ToList();
        }
    }
}
