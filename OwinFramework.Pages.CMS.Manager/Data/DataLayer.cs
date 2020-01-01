using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using OwinFramework.Builder;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;

namespace OwinFramework.Pages.CMS.Manager.Data
{
    using LiveUpdatePropertyChange = Runtime.Interfaces.LiveUpdate.PropertyChange;
    using DatabasePropertyChange = Runtime.Interfaces.Database.PropertyChange;

    internal class DataLayer : IDataLayer
    {
        private readonly ILiveUpdateSender _liveUpdateSender;
        private readonly IDatabaseReader _databaseReader;
        private readonly IDatabaseUpdater _databaseUpdater;
        private readonly object _liveUpdateLock = new object();
        private readonly Thread _liveUpdateThread;

        private MessageDto _nextMessage;

        public DataLayer(
            ILiveUpdateSender liveUpdateSender,
            IDatabaseReader databaseReader,
            IDatabaseUpdater databaseUpdater)
        {
            _liveUpdateSender = liveUpdateSender;
            _databaseReader = databaseReader;
            _databaseUpdater = databaseUpdater;

            _liveUpdateThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        FlushLiveUpdates();
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch
                    {
                    }
                }
            })
            {
                Name = "Flush live update",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };

            _liveUpdateThread.Start();
        }

        #region Live updates

        private void FlushLiveUpdates()
        {
            MessageDto message;

            lock (_liveUpdateLock)
            {
                message = _nextMessage;
                _nextMessage = new MessageDto
                {
                    DeletedRecords = new List<RecordReference>(),
                    NewRecords = new List<RecordReference>(),
                    PropertyChanges = new List<LiveUpdatePropertyChange>(),
                    WebsiteVersionChanges = new List<WebsiteVersionChange>(),
                    ChildListChanges = new List<RecordChildrenReference>()
                };
            }

            if (message == null) return;

            if (message.ChildListChanges.Count > 0 ||
                message.DeletedRecords.Count > 0 ||
                message.NewRecords.Count > 0 ||
                message.PropertyChanges.Count > 0 ||
                message.WebsiteVersionChanges.Count > 0)
            {
                message.WhenUtc = DateTime.UtcNow;
                message.MachineName = Environment.MachineName;
                message.UniqueId = Guid.NewGuid().ToShortString();

                _liveUpdateSender.Send(message);
            }
        }

        #endregion

        #region IDatabaseUpdater

        CreateResult IDatabaseUpdater.CreateEnvironment(string identity, EnvironmentRecord environment)
        {
            var result = _databaseUpdater.CreateEnvironment(identity, environment);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = environment.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateEnvironment(string identity, long environmentId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateEnvironment(identity, environmentId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(EnvironmentRecord.RecordTypeName, environmentId, changes);

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteEnvironment(string identity, long environmentId)
        {
            var result = _databaseUpdater.DeleteEnvironment(identity, environmentId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = EnvironmentRecord.RecordTypeName,
                        ElementId = environmentId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreateWebsiteVersion(string identity, WebsiteVersionRecord websiteVersion)
        {
            var result = _databaseUpdater.CreateWebsiteVersion(identity, websiteVersion);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = websiteVersion.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateWebsiteVersion(string identity, long websiteVersionId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateWebsiteVersion(identity, websiteVersionId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(WebsiteVersionRecord.RecordTypeName, websiteVersionId, changes);

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteWebsiteVersion(string identity, long websiteVersionId)
        {
            var result = _databaseUpdater.DeleteWebsiteVersion(identity, websiteVersionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = WebsiteVersionRecord.RecordTypeName,
                        ElementId = websiteVersionId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreatePage(string identity, PageRecord page)
        {
            var result = _databaseUpdater.CreatePage(identity, page);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = page.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdatePage(string identity, long pageId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdatePage(identity, pageId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(PageRecord.RecordTypeName, pageId, changes);

            return result;
        }

        DeleteResult IDatabaseUpdater.DeletePage(string identity, long pageId)
        {
            var result = _databaseUpdater.DeletePage(identity, pageId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = PageRecord.RecordTypeName,
                        ElementId = pageId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreatePageVersion(string identity, PageVersionRecord pageVersion)
        {
            var result = _databaseUpdater.CreatePageVersion(identity, pageVersion);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = pageVersion.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersion(string identity, long pageVersionId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdatePageVersion(identity, pageVersionId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(PageVersionRecord.RecordTypeName, pageVersionId, changes);

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionRoutes(string identity, long pageVersionId, IEnumerable<PageRouteRecord> routes)
        {
            foreach (var route in routes) route.PageVersionId = pageVersionId;

            var result = _databaseUpdater.UpdatePageVersionRoutes(identity, pageVersionId, routes);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = PageVersionRecord.RecordTypeName,
                        ElementId = pageVersionId,
                        ChildRecordType = "Route"
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionLayoutZones(string identity, long pageVersionId, IEnumerable<LayoutZoneRecord> layoutZones)
        {
            var result = _databaseUpdater.UpdatePageVersionLayoutZones(identity, pageVersionId, layoutZones);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = PageVersionRecord.RecordTypeName,
                        ElementId = pageVersionId,
                        ChildRecordType = "LayoutZone"
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionComponents(string identity, long pageVersionId, IEnumerable<ElementComponentRecord> components)
        {
            var result = _databaseUpdater.UpdatePageVersionComponents(identity, pageVersionId, components);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = PageVersionRecord.RecordTypeName,
                        ElementId = pageVersionId,
                        ChildRecordType = "Component"
                    });
            }

            return result;
        }

        DeleteResult IDatabaseUpdater.DeletePageVersion(string identity, long pageVersionId)
        {
            var result = _databaseUpdater.DeletePageVersion(identity, pageVersionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = PageVersionRecord.RecordTypeName,
                        ElementId = pageVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddPageToWebsiteVersion(identity, pageId, version, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = PageRecord.RecordTypeName,
                        ElementId = pageId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = 0 // TODO: find correct value
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddPageToWebsiteVersion(identity, pageVersionId, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = PageRecord.RecordTypeName,
                        ElementId = 0, // TODO: find correct value
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = pageVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.RemovePageFromWebsite(string identity, long pageId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.RemovePageFromWebsite(identity, pageId, websiteVersionId, scenario);
            if (!result.Success) return result;
            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = PageRecord.RecordTypeName,
                        ElementId = pageId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = null
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreateLayout(string identity, LayoutRecord layout)
        {
            var result = _databaseUpdater.CreateLayout(identity, layout);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = layout.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateLayout(string identity, long layoutId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateLayout(identity, layoutId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(LayoutRecord.RecordTypeName, layoutId, changes);

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteLayout(string identity, long layoutId)
        {
            var result = _databaseUpdater.DeleteLayout(identity, layoutId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = LayoutRecord.RecordTypeName,
                        ElementId = layoutId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreateLayoutVersion(string identity, LayoutVersionRecord layoutVersion)
        {
            var result = _databaseUpdater.CreateLayoutVersion(identity, layoutVersion);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = layoutVersion.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateLayoutVersion(string identity, long layoutVersionId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateLayoutVersion(identity, layoutVersionId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(LayoutVersionRecord.RecordTypeName, layoutVersionId, changes);

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateLayoutVersionZones(string identity, long layoutVersionId, IEnumerable<LayoutZoneRecord> layoutZones)
        {
            var result = _databaseUpdater.UpdateLayoutVersionZones(identity, layoutVersionId, layoutZones);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = LayoutVersionRecord.RecordTypeName,
                        ElementId = layoutVersionId,
                        ChildRecordType = "Zone"
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateLayoutVersionComponents(string identity, long layoutVersionId, IEnumerable<ElementComponentRecord> components)
        {
            var result = _databaseUpdater.UpdateLayoutVersionComponents(identity, layoutVersionId, components);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = LayoutVersionRecord.RecordTypeName,
                        ElementId = layoutVersionId,
                        ChildRecordType = "Component"
                    });
            }

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteLayoutVersion(string identity, long layoutVersionId)
        {
            var result = _databaseUpdater.DeleteLayoutVersion(identity, layoutVersionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = LayoutVersionRecord.RecordTypeName,
                        ElementId = layoutVersionId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreateRegion(string identity, RegionRecord region)
        {
            var result = _databaseUpdater.CreateRegion(identity, region);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = region.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateRegion(string identity, long regionId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateRegion(identity, regionId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(RegionRecord.RecordTypeName, regionId, changes);

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteRegion(string identity, long regionId)
        {
            var result = _databaseUpdater.DeleteRegion(identity, regionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = RegionRecord.RecordTypeName,
                        ElementId = regionId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreateRegionVersion(string identity, RegionVersionRecord regionVersion)
        {
            var result = _databaseUpdater.CreateRegionVersion(identity, regionVersion);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = regionVersion.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersion(string identity, long regionVersionId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateRegionVersion(identity, regionVersionId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(RegionVersionRecord.RecordTypeName, regionVersionId, changes);

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersionTemplates(string identity, long regionVersionId, IEnumerable<RegionTemplateRecord> templates)
        {
            var result = _databaseUpdater.UpdateRegionVersionTemplates(identity, regionVersionId, templates);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = RegionVersionRecord.RecordTypeName,
                        ElementId = regionVersionId,
                        ChildRecordType = "Template"
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersionLayoutZones(string identity, long regionVersionId, IEnumerable<LayoutZoneRecord> layoutZones)
        {
            var result = _databaseUpdater.UpdateRegionVersionLayoutZones(identity, regionVersionId, layoutZones);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = RegionVersionRecord.RecordTypeName,
                        ElementId = regionVersionId,
                        ChildRecordType = "Zone"
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersionComponents(string identity, long regionVersionId, IEnumerable<ElementComponentRecord> components)
        {
            var result = _databaseUpdater.UpdateRegionVersionComponents(identity, regionVersionId, components);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = RegionVersionRecord.RecordTypeName,
                        ElementId = regionVersionId,
                        ChildRecordType = "Component"
                    });
            }

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteRegionVersion(string identity, long regionVersionId)
        {
            var result = _databaseUpdater.DeleteRegionVersion(identity, regionVersionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = RegionVersionRecord.RecordTypeName,
                        ElementId = regionVersionId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreateComponent(string identity, ComponentRecord component)
        {
            var result = _databaseUpdater.CreateComponent(identity, component);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = component.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateComponent(string identity, long componentId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateComponent(identity, componentId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(ComponentRecord.RecordTypeName, componentId, changes);

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteComponent(string identity, long componentId)
        {
            var result = _databaseUpdater.DeleteComponent(identity, componentId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = ComponentRecord.RecordTypeName,
                        ElementId = componentId
                    });
            }

            return result;
        }

        CreateResult IDatabaseUpdater.CreateComponentVersion(string identity, ComponentVersionRecord componentVersion)
        {
            var result = _databaseUpdater.CreateComponentVersion(identity, componentVersion);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewRecords.Add(
                    new RecordReference
                    {
                        RecordType = componentVersion.RecordType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateComponentVersion(string identity, long componentVersionId, IEnumerable<DatabasePropertyChange> changes)
        {
            var result = _databaseUpdater.UpdateComponentVersion(identity, componentVersionId, changes);
            if (!result.Success) return result;

            AddChangesToLiveUpdate(ComponentVersionRecord.RecordTypeName, componentVersionId, changes);

            return result;
        }

        DeleteResult IDatabaseUpdater.DeleteComponentVersion(string identity, long componentVersionId)
        {
            var result = _databaseUpdater.DeleteComponentVersion(identity, componentVersionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedRecords.Add(
                    new RecordReference
                    {
                        RecordType = ComponentVersionRecord.RecordTypeName,
                        ElementId = componentVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdateComponentVersionProperties(string identity, long componentVersionId, IEnumerable<ElementPropertyRecord> componentProperties)
        {
            var result = _databaseUpdater.UpdateComponentVersionProperties(identity, componentVersionId, componentProperties);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = ComponentVersionRecord.RecordTypeName,
                        ElementId = componentVersionId,
                        ChildRecordType = "Property"
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddLayoutToWebsiteVersion(string identity, long layoutId, int version, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddLayoutToWebsiteVersion(identity, layoutId, version, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = LayoutRecord.RecordTypeName,
                        ElementId = layoutId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = 0 // TODO: find correct value
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddLayoutToWebsiteVersion(string identity, long layoutVersionId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddLayoutToWebsiteVersion(identity, layoutVersionId, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = LayoutRecord.RecordTypeName,
                        ElementId = 0, // TODO: find correct value
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = layoutVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.RemoveLayoutFromWebsite(string identity, long layoutId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.RemoveLayoutFromWebsite(identity, layoutId, websiteVersionId, scenario);
            if (!result.Success) return result;
            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = LayoutRecord.RecordTypeName,
                        ElementId = layoutId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = null
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddRegionToWebsiteVersion(string identity, long regionId, int version, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddRegionToWebsiteVersion(identity, regionId, version, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = RegionRecord.RecordTypeName,
                        ElementId = regionId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = 0 // TODO: find correct value
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddRegionToWebsiteVersion(string identity, long regionVersionId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddRegionToWebsiteVersion(identity, regionVersionId, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = RegionRecord.RecordTypeName,
                        ElementId = 0, // TODO: find correct value
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = regionVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.RemoveRegionFromWebsite(string identity, long regionId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.RemoveRegionFromWebsite(identity, regionId, websiteVersionId, scenario);
            if (!result.Success) return result;
            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = RegionRecord.RecordTypeName,
                        ElementId = regionId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = null
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddComponentToWebsiteVersion(string identity, long componentId, int version, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddComponentToWebsiteVersion(identity, componentId, version, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = ComponentRecord.RecordTypeName,
                        ElementId = componentId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = 0 // TODO: find correct value
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddComponentToWebsiteVersion(string identity, long componentVersionId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.AddComponentToWebsiteVersion(identity, componentVersionId, websiteVersionId, scenario);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = ComponentRecord.RecordTypeName,
                        ElementId = 0, // TODO: find correct value
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = componentVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.RemoveComponentFromWebsite(string identity, long componentId, long websiteVersionId, string scenario)
        {
            var result = _databaseUpdater.RemoveComponentFromWebsite(identity, componentId, websiteVersionId, scenario);
            if (!result.Success) return result;
            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = ComponentRecord.RecordTypeName,
                        ElementId = componentId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = null
                    });
            }

            return result;
        }

        #endregion

        #region Adding property changes to the live update log

        private readonly Dictionary<Type, TypeDefinition> _typeDefinitions = new Dictionary<Type, TypeDefinition>();

        private void AddChangesToLiveUpdate(string elementType, long elementId, IEnumerable<DatabasePropertyChange> changes)
        {
            lock (_liveUpdateLock)
            {
                _nextMessage.PropertyChanges.AddRange(
                    changes
                        .Select(property => new LiveUpdatePropertyChange
                        {
                            RecordType = elementType,
                            Id = elementId,
                            PropertyName = property.PropertyName,
                            PropertyValue = property.PropertyValue,
                            ArrayIndex = property.ArrayIndex,
                            PropertyArray = property.PropertyArray,
                            PropertyObject = property.PropertyObject
                        }));
            }
        }

        private void AddChangesToLiveUpdate(ElementRecordBase oldElement, ElementRecordBase newElement)
        {
            var typeDefinition = GetTypeDefinition(newElement.GetType());

            lock (_liveUpdateLock)
            {
                _nextMessage.PropertyChanges.AddRange(
                    typeDefinition.Properties
                        .Select(property => property.BuildChange(oldElement, newElement, newElement.RecordType, newElement.RecordId))
                        .Where(change => change != null));
            }
        }

        private TypeDefinition GetTypeDefinition(Type type)
        {
            lock (_typeDefinitions)
            {
                TypeDefinition typeDefinition;
                if (!_typeDefinitions.TryGetValue(type, out typeDefinition))
                {
                    typeDefinition = new TypeDefinition(type);
                    _typeDefinitions.Add(type, typeDefinition);
                }
                return typeDefinition;
            }
        }

        private class TypeDefinition
        {
            public readonly PropertyDefinition[] Properties;

            public TypeDefinition(Type type)
            {
                Properties = type
                    .GetProperties()
                    .Select(p => new
                    {
                        p, 
                        a = p
                            .GetCustomAttributes(true)
                            .Where(a => a is JsonPropertyAttribute)
                            .Cast<JsonPropertyAttribute>()
                            .FirstOrDefault()
                    })
                    .Where(o => !ReferenceEquals(o.a, null))
                    .Select(o => new PropertyDefinition
                    {
                        Property = o.p, 
                        Name = o.a.PropertyName
                    })
                    .ToArray();
            }
        }

        private class PropertyDefinition
        {
            public PropertyInfo Property;
            public string Name;

            public LiveUpdatePropertyChange BuildChange(
                object originalObject, 
                object newObject, 
                string elementType, 
                long elementId)
            {
                var originalValue = Property.GetValue(originalObject, null);
                var newValue = Property.GetValue(newObject, null);

                if (ReferenceEquals(originalValue, null) )
                {
                    if (ReferenceEquals(newValue, null))
                        return null;
                }
                else if (originalValue.Equals(newValue))
                    return null;

                return new LiveUpdatePropertyChange
                {
                    RecordType = elementType,
                    Id = elementId,
                    PropertyName = Name,
                    PropertyValue = ReferenceEquals(newValue, null) 
                        ? null 
                        : newValue.ToString()
                };
            }
        }

        #endregion

        #region IDatabaseReader MixIn

        T[] IDatabaseReader.GetEnvironments<T>(Func<EnvironmentRecord, T> map, Func<EnvironmentRecord, bool> predicate)
        {
            return _databaseReader.GetEnvironments(map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteVersions(map, predicate);
        }

        T[] IDatabaseReader.GetWebsitePages<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate)
        {
            return _databaseReader.GetWebsitePages(websiteVersionId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsitePages<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate)
        {
            return _databaseReader.GetWebsitePages(websiteVersionName, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteLayouts(websiteVersionId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteLayouts(websiteVersionName, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteRegions(websiteVersionId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteRegions(websiteVersionName, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteDataTypes(websiteVersionId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteDataTypes(websiteVersionName, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteComponents(websiteVersionId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteComponents(websiteVersionName, scenarioName, map, predicate);
        }

        IDictionary<string, object> IDatabaseReader.GetElementPropertyValues(long elementVersionId)
        {
            return _databaseReader.GetElementPropertyValues(elementVersionId);
        }

        T[] IDatabaseReader.GetElementVersions<T>(long elementId, Func<ElementVersionRecordBase, T> map)
        {
            return _databaseReader.GetElementVersions(elementId, map);
        }

        T IDatabaseReader.GetEnvironment<T>(long environmentId, Func<EnvironmentRecord, T> map)
        {
            return _databaseReader.GetEnvironment(environmentId, map);
        }

        T IDatabaseReader.GetWebsiteVersion<T>(long websiteVersionId, Func<WebsiteVersionRecord, T> map)
        {
            return _databaseReader.GetWebsiteVersion(websiteVersionId, map);
        }

        T IDatabaseReader.GetPageVersion<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map)
        {
            return _databaseReader.GetPageVersion(pageId, version, map);
        }

        T IDatabaseReader.GetLayoutVersion<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            return _databaseReader.GetLayoutVersion(layoutId, version, map);
        }

        T IDatabaseReader.GetRegionVersion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            return _databaseReader.GetRegionVersion(regionId, version, map);
        }

        T IDatabaseReader.GetDataTypeVersion<T>(long dataTypeId, int version, Func<DataTypeRecord, DataTypeVersionRecord, T> map)
        {
            return _databaseReader.GetDataTypeVersion(dataTypeId, version, map);
        }

        T IDatabaseReader.GetPageVersion<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map)
        {
            return _databaseReader.GetPageVersion(pageVersionId, map);
        }

        T IDatabaseReader.GetLayoutVersion<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            return _databaseReader.GetLayoutVersion(layoutVersionId, map);
        }

        T IDatabaseReader.GetRegionVersion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            return _databaseReader.GetRegionVersion(regionVersionId, map);
        }

        T IDatabaseReader.GetDataTypeVersion<T>(long dataTypeVersionId, Func<DataTypeRecord, DataTypeVersionRecord, T> map)
        {
            return _databaseReader.GetDataTypeVersion(dataTypeVersionId, map);
        }

        T IDatabaseReader.GetComponentVersion<T>(long componentVersionId, Func<ComponentRecord, ComponentVersionRecord, T> map)
        {
            return _databaseReader.GetComponentVersion(componentVersionId, map);
        }

        T[] IDatabaseReader.GetPages<T>(Func<PageRecord, T> map, Func<PageRecord, bool> predicate)
        {
            return _databaseReader.GetPages(map, predicate);
        }

        T[] IDatabaseReader.GetLayouts<T>(Func<LayoutRecord, T> map, Func<LayoutRecord, bool> predicate)
        {
            return _databaseReader.GetLayouts(map, predicate);
        }

        T[] IDatabaseReader.GetRegions<T>(Func<RegionRecord, T> map, Func<RegionRecord, bool> predicate)
        {
            return _databaseReader.GetRegions(map, predicate);
        }

        T[] IDatabaseReader.GetDataTypes<T>(Func<DataTypeRecord, T> map, Func<DataTypeRecord, bool> predicate)
        {
            return _databaseReader.GetDataTypes(map, predicate);
        }

        T[] IDatabaseReader.GetDataScopes<T>(Func<DataScopeRecord, T> map, Func<DataScopeRecord, bool> predicate)
        {
            return _databaseReader.GetDataScopes(map, predicate);
        }

        T[] IDatabaseReader.GetComponents<T>(Func<ComponentRecord, T> map, Func<ComponentRecord, bool> predicate)
        {
            return _databaseReader.GetComponents(map, predicate);
        }

        T[] IDatabaseReader.GetModules<T>(Func<ModuleRecord, T> map, Func<ModuleRecord, bool> predicate)
        {
            return _databaseReader.GetModules(map, predicate);
        }

        HistoryPeriodRecord IDatabaseReader.GetHistory(string recordType, long id, string bookmark)
        {
            return _databaseReader.GetHistory(recordType, id, bookmark);
        }

        HistoryEventRecord[] IDatabaseReader.GetHistorySummary(long summaryId)
        {
            return _databaseReader.GetHistorySummary(summaryId);
        }

        T[] IDatabaseReader.GetElementUsage<T>(long elementVersionId, Func<WebsiteVersionRecordBase, T> map, Func<WebsiteVersionRecordBase, bool> predicate)
        {
            return _databaseReader.GetElementUsage(elementVersionId, map, predicate);
        }

        T IDatabaseReader.GetPage<T>(long pageId, Func<PageRecord, T> map)
        {
            return _databaseReader.GetPage(pageId, map);
        }

        T IDatabaseReader.GetLayout<T>(long layoutId, Func<LayoutRecord, T> map)
        {
            return _databaseReader.GetLayout(layoutId, map);
        }

        T IDatabaseReader.GetRegion<T>(long regionId, Func<RegionRecord, T> map)
        {
            return _databaseReader.GetRegion(regionId, map);
        }

        T IDatabaseReader.GetComponent<T>(long componentId, Func<ComponentRecord, T> map)
        {
            return _databaseReader.GetComponent(componentId, map);
        }

        T IDatabaseReader.GetDataScope<T>(long dataScopeId, Func<DataScopeRecord, T> map)
        {
            return _databaseReader.GetDataScope(dataScopeId, map);
        }

        T IDatabaseReader.GetDataType<T>(long dataTypeId, Func<DataTypeRecord, T> map)
        {
            return _databaseReader.GetDataType(dataTypeId, map);
        }

        #endregion
    }
}
