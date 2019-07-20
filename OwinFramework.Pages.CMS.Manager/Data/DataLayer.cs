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

            AddChangesToLiveUpdate(new EnvironmentRecord().RecordType, environmentId, changes);

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
                        RecordType = new EnvironmentRecord().RecordType,
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

            AddChangesToLiveUpdate(new WebsiteVersionRecord().RecordType, websiteVersionId, changes);

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
                        RecordType = new WebsiteVersionRecord().RecordType,
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

            AddChangesToLiveUpdate(new PageRecord().RecordType, pageId, changes);

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
                        RecordType = new PageRecord().RecordType,
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

            AddChangesToLiveUpdate(new PageVersionRecord().RecordType, pageVersionId, changes);

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionRoutes(string identity, long pageVersionId, IEnumerable<PageRouteRecord> routes)
        {
            var result = _databaseUpdater.UpdatePageVersionRoutes(identity, pageVersionId, routes);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.ChildListChanges.Add(
                    new RecordChildrenReference
                    {
                        RecordType = new PageVersionRecord().RecordType,
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
                        RecordType = new PageVersionRecord().RecordType,
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
                        RecordType = new PageVersionRecord().RecordType,
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
                        RecordType = new PageVersionRecord().RecordType,
                        ElementId = pageVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId)
        {
            var result = _databaseUpdater.AddPageToWebsiteVersion(identity, pageId, version, websiteVersionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = new PageRecord().RecordType,
                        ElementId = pageId,
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = 0 // TODO: find correct value
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId)
        {
            var result = _databaseUpdater.AddPageToWebsiteVersion(identity, pageVersionId, websiteVersionId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = new PageRecord().RecordType,
                        ElementId = 0, // TODO: find correct value
                        OldElementVersionId = 0, // TODO: find correct value
                        NewElementVersionId = pageVersionId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.RemovePageFromWebsite(string identity, long pageId, long websiteVersionId)
        {
            var result = _databaseUpdater.RemovePageFromWebsite(identity, pageId, websiteVersionId);
            if (!result.Success) return result;
            lock (_liveUpdateLock)
            {
                _nextMessage.WebsiteVersionChanges.Add(
                    new WebsiteVersionChange
                    {
                        WebsiteVersionId = websiteVersionId,
                        ElementType = new PageRecord().RecordType,
                        ElementId = pageId,
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
                            PropertyValue = property.PropertyValue
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

        T[] IDatabaseReader.GetWebsitePages<T>(long websiteVersionId, string userSegment, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate)
        {
            return _databaseReader.GetWebsitePages(websiteVersionId, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsitePages<T>(string websiteVersionName, string userSegment, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate)
        {
            return _databaseReader.GetWebsitePages(websiteVersionName, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(long websiteVersionId, string userSegment, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteLayouts(websiteVersionId, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(string websiteVersionName, string userSegment, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteLayouts(websiteVersionName, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(long websiteVersionId, string userSegment, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteRegions(websiteVersionId, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(string websiteVersionName, string userSegment, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteRegions(websiteVersionName, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(long websiteVersionId, string userSegment, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteDataTypes(websiteVersionId, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(string websiteVersionName, string userSegment, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteDataTypes(websiteVersionName, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(long websiteVersionId, string userSegment, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteComponents(websiteVersionId, userSegment, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(string websiteVersionName, string userSegment, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteComponents(websiteVersionName, userSegment, map, predicate);
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

        HistoryPeriodRecord IDatabaseReader.GetHistory(string recordType, long id, string bookmark)
        {
            return _databaseReader.GetHistory(recordType, id, bookmark);
        }

        HistoryEventRecord[] IDatabaseReader.GetHistorySummary(long summaryId)
        {
            return _databaseReader.GetHistorySummary(summaryId);
        }

        #endregion
    }
}
