﻿using System;
using System.Collections.Generic;
using System.Threading;
using OwinFramework.Builder;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;

namespace OwinFramework.Pages.CMS.Editor.Data
{
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
                    DeletedElementVersions = new List<ElementVersionReference>(),
                    DeletedElements = new List<ElementReference>(),
                    NewElementVersions = new List<ElementVersionReference>(),
                    NewElements = new List<ElementReference>(),
                    PropertyChanges = new List<PropertyChange>(),
                    WebsiteVersionChanges = new List<WebsiteVersionChange>()
                };
            }

            if (message == null) return;

            if (message.DeletedElementVersions.Count > 0 ||
                message.DeletedElements.Count > 0 ||
                message.NewElementVersions.Count > 0 ||
                message.NewElements.Count > 0 ||
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

        CreateResult IDatabaseUpdater.CreatePage(PageRecord page)
        {
            var result = _databaseUpdater.CreatePage(page);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.NewElements.Add(
                    new ElementReference
                    {
                        ElementType = page.ElementType,
                        ElementId = result.NewRecordId
                    });
            }

            return result;
        }

        UpdateResult IDatabaseUpdater.UpdatePage(PageRecord page)
        {
            var oldPage = _databaseReader.GetPage(page.ElementId, (p, v) => p);

            var result = _databaseUpdater.UpdatePage(page);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                if (oldPage.Name != page.Name)
                {
                    _nextMessage.PropertyChanges.Add(
                        new PropertyChange
                        {
                            ElementType = page.ElementType,
                            Id = page.ElementId,
                            PropertyName = "name",
                            PropertyValue = page.Name
                        });
                }

                if (oldPage.DisplayName != page.DisplayName)
                {
                    _nextMessage.PropertyChanges.Add(
                        new PropertyChange
                        {
                            ElementType = page.ElementType,
                            Id = page.ElementId,
                            PropertyName = "displayName",
                            PropertyValue = page.DisplayName
                        });
                }

                if (oldPage.Description != page.Description)
                {
                    _nextMessage.PropertyChanges.Add(
                        new PropertyChange
                        {
                            ElementType = page.ElementType,
                            Id = page.ElementId,
                            PropertyName = "description",
                            PropertyValue = page.Description
                        });
                }

                if (oldPage.CreatedBy != page.CreatedBy)
                {
                    _nextMessage.PropertyChanges.Add(
                        new PropertyChange
                        {
                            ElementType = page.ElementType,
                            Id = page.ElementId,
                            PropertyName = "createdBy",
                            PropertyValue = page.CreatedBy
                        });
                }

                if (oldPage.CreatedWhen != page.CreatedWhen)
                {
                    _nextMessage.PropertyChanges.Add(
                        new PropertyChange
                        {
                            ElementType = page.ElementType,
                            Id = page.ElementId,
                            PropertyName = "createdWhen",
                            PropertyValue = page.CreatedWhen.ToString("r")
                        });
                }
            }

            return result;
        }

        DeleteResult IDatabaseUpdater.DeletePage(long pageId)
        {
            var result = _databaseUpdater.DeletePage(pageId);
            if (!result.Success) return result;

            lock (_liveUpdateLock)
            {
                _nextMessage.DeletedElements.Add(
                    new ElementReference
                    {
                        ElementType = new PageRecord().ElementType,
                        ElementId = pageId
                    });
            }

            return result;
        }

        #endregion

        #region IDatabaseReader

        T[] IDatabaseReader.GetEnvironments<T>(Func<EnvironmentRecord, T> map, Func<EnvironmentRecord, bool> predicate)
        {
            return _databaseReader.GetEnvironments(map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteVersions(map, predicate);
        }

        T[] IDatabaseReader.GetWebsitePages<T>(long websiteVersionId, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate)
        {
            return _databaseReader.GetWebsitePages(websiteVersionId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsitePages<T>(string websiteVersionName, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate)
        {
            return _databaseReader.GetWebsitePages(websiteVersionName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(long websiteVersionId, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteLayouts(websiteVersionId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(string websiteVersionName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteLayouts(websiteVersionName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(long websiteVersionId, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteRegions(websiteVersionId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(string websiteVersionName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteRegions(websiteVersionName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(long websiteVersionId, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteDataTypes(websiteVersionId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(string websiteVersionName, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteDataTypes(websiteVersionName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(long websiteVersionId, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteComponents(websiteVersionId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(string websiteVersionName, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            return _databaseReader.GetWebsiteComponents(websiteVersionName, map, predicate);
        }

        IDictionary<string, object> IDatabaseReader.GetElementPropertyValues(long elementVersionId)
        {
            return _databaseReader.GetElementPropertyValues(elementVersionId);
        }

        T[] IDatabaseReader.GetElementVersions<T>(long elementId, Func<ElementVersionRecordBase, T> map)
        {
            return _databaseReader.GetElementVersions(elementId, map);
        }

        T IDatabaseReader.GetPage<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map)
        {
            return _databaseReader.GetPage(pageId, version, map);
        }

        T IDatabaseReader.GetLayout<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            return _databaseReader.GetLayout(layoutId, version, map);
        }

        T IDatabaseReader.GetRegion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            return _databaseReader.GetRegion(regionId, version, map);
        }

        T IDatabaseReader.GetPage<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map)
        {
            return _databaseReader.GetPage(pageVersionId, map);
        }

        T IDatabaseReader.GetLayout<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            return _databaseReader.GetLayout(layoutVersionId, map);
        }

        T IDatabaseReader.GetRegion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            return _databaseReader.GetRegion(regionVersionId, map);
        }

        T IDatabaseReader.GetDataType<T>(long dataTypeVersionId, Func<DataTypeRecord, DataTypeVersionRecord, T> map)
        {
            return _databaseReader.GetDataType(dataTypeVersionId, map);
        }

        T IDatabaseReader.GetComponent<T>(long componentVersionId, Func<ComponentRecord, ComponentVersionRecord, T> map)
        {
            return _databaseReader.GetComponent(componentVersionId, map);
        }

        #endregion
    }
}