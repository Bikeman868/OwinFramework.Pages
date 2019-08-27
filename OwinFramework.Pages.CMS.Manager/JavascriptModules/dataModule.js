exported.buildDataStores = function () {
    exported.environmentStore = ns.data.store.newStore({
        recordType: "Environment",
        name: "environment",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_environment" },
            { name: "displayName", type: String, default: "New environment" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true },
            { name: "baseUrl", type: String, default: function (r) { return "https://" + r.name + ".mycompany.com/" } },
            { name: "websiteVersionId", type: Number, allowNull: true }],
        crud: {
            createRecord: function (environment, onSuccess, onFail, params) {
                exported.crudService.createEnvironment({ body: environment }, onSuccess, onFail);
            },
            retrieveRecord: function (environmentId, onSuccess, onFail) {
                exported.crudService.retrieveEnvironment({ id: environmentId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.environments({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalEnvironment, updatedEnvironment, changes, onSuccess, onFail) {
                exported.crudService.updateEnvironment(
                    {
                        id: updatedEnvironment.recordId,
                        body: changes
                    },
                    onSuccess, null, onFail);
            },
            deleteRecord: function (environmentId, onSuccess, onFail) {
                exported.crudService.deleteEnvironment({ id: environmentId }, onSuccess, null, onFail);
            }
        }
    });

    exported.websiteVersionStore = ns.data.store.newStore({
        recordType: "WebsiteVersion",
        name: "website version",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_website_version" },
            { name: "displayName", type: String, default: "Website version" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true }],
        crud: {
            createRecord: function (websiteVersion, onSuccess, onFail, params) {
                exported.crudService.createWebsiteVersion({ body: websiteVersion }, onSuccess, onFail);
            },
            retrieveRecord: function (websiteVersionId, onSuccess, onFail) {
                exported.crudService.retrieveWebsiteVersion({ id: websiteVersionId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.websiteVersions({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalWebsiteVersion, updatedWebsiteVersion, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.crudService.updateWebsiteVersion(
                        {
                            id: updatedWebsiteVersion.recordId,
                            body: changes
                        },
                        onSuccess, null, onFail);
                }
            },
            deleteRecord: function (websiteVersionId, onSuccess, onFail) {
                exported.crudService.deleteWebsiteVersion({ id: websiteVersionId }, onSuccess, null, onFail);
            }
        },
        getWebsiteVersionPages: function (websiteVersionId, onSuccess, onFail) {
            var store = this;
            const description = "list of vebsite version pages";
            exported.listService.websiteVersionPages(
                { id: websiteVersionId },
                function (response) { store.handleGetSuccess(description, response, onSuccess, onFail); },
                null,
                function (ajax) { store.handleGetFail(description, ajax, onFail); });
        }
    });

    exported.pageStore = ns.data.store.newStore({
        recordType: "Page",
        name: "page",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_page" },
            { name: "displayName", type: String, default: "New page" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true }],
        crud: {
            createRecord: function (page, onSuccess, onFail, params) {
                exported.crudService.createPage({ body: page, websiteversionid: params.websiteVersionId }, onSuccess, onFail);
            },
            retrieveRecord: function (pageId, onSuccess, onFail) {
                exported.crudService.retrievePage({ id: pageId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.allPages({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalPage, updatedPage, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.crudService.updatePage(
                        {
                            id: updatedPage.recordId,
                            body: changes
                        },
                        onSuccess, null, onFail);
                }
            },
            deleteRecord: function (pageId, onSuccess, onFail) {
                exported.crudService.deletePage({ id: pageId }, onSuccess, null, onFail);
            }
        }
    });

    exported.pageVersionStore = ns.data.store.newStore({
        recordType: "PageVersion",
        name: "page version",
        listName: "list of page versions",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_page_version" },
            { name: "displayName", type: String, default: "New page version" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true },
            { name: "parentRecordId", type: Number, allowNull: true },
            { name: "version", type: Number, allowNull: true },
            { name: "moduleName", type: String, allowNull: true },
            { name: "assetDeployment", type: String, default: "Inherit" },
            { name: "masterPageId", type: Number, allowNull: true },
            { name: "layoutName", type: String, allowNull: true },
            { name: "layoutId", type: Number, allowNull: true },
            { name: "canonicalUrl", type: String, allowNull: true },
            { name: "title", type: String, allowNull: true },
            { name: "bodyStyle", type: String, allowNull: true },
            { name: "permission", type: String, allowNull: true },
            { name: "assetPath", type: String, allowNull: true },
            { name: "cacheCategory", type: String, allowNull: true },
            { name: "cachePriority", type: String, allowNull: true },
            { name: "routes", type: Array },
            { name: "layoutZones", type: Array },
            { name: "components", type: Array },
            { name: "dataScopes", type: Array },
            { name: "dataTypes", type: Array }],
        crud: {
            createRecord: function (pageVersion, onSuccess, onFail, params) {
                exported.crudService.createPageVersion({
                    body: pageVersion,
                    websiteVersionId: params.websiteVersionId,
                    scenario: params.scenario
                },
                    onSuccess,
                    onFail);
            },
            retrieveRecord: function (pageVersionId, onSuccess, onFail) {
                exported.crudService.retrievePageVersion({ id: pageVersionId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.allPageVersions({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalPageVersion, updatedPageVersion, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.crudService.updatePageVersion(
                        {
                            id: updatedPageVersion.recordId,
                            body: changes
                        },
                        onSuccess, null, onFail);
                }
            },
            deleteRecord: function (pageVersionId, onSuccess, onFail) {
                exported.crudService.deletePageVersion({ id: pageVersionId }, onSuccess, null, onFail);
            }
        },
        getPageVersions: function (pageId, onSuccess, onFail) {
            var store = this;
            const description = "list of page versions";
            exported.listService.pageVersions(
                { id: pageId },
                function (response) { store.handleGetSuccess(description, response, onSuccess, onFail); },
                null,
                function (ajax) { store.handleGetFail(description, ajax, onFail); });
        },
        getWebsitePageVersion: function (websiteVersionId, segmentationScenarioName, pageId, onSuccess, onFail) {
            var store = this;
            exported.listService.websitePageVersion(
                {
                    websiteVersionId: websiteVersionId,
                    scenario: segmentationScenarioName,
                    pageId: pageId
                },
                function (response) {
                    if (response != undefined) {
                        var pageVersionId = response.pageVersionId;
                        if (pageVersionId != undefined) {
                            store.retrieveRecord(pageVersionId, onSuccess, onFail);
                            return;
                        }
                    }
                    if (onFail != undefined) onFail("Failed to get page version for website version");
                },
                null,
                function (ajax) {
                    if (onFail != undefined) onFail("Failed to get page version for website version");
                });
        }
    });

    exported.layoutStore = ns.data.store.newStore({
        recordType: "Layout",
        name: "layout",
        listName: "list of layouts",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_layout" },
            { name: "displayName", type: String, default: "New layout" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true }],
        crud: {
            createRecord: function (layout, onSuccess, onFail, params) {
                exported.crudService.createLayout({ body: layout, websiteversionid: params.websiteVersionId }, onSuccess, onFail);
            },
            retrieveRecord: function (layoutId, onSuccess, onFail) {
                exported.crudService.retrieveLayout({ id: layoutId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.allLayouts({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalLayout, updatedLayout, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.crudService.updateLayout(
                        {
                            id: updatedLayout.recordId,
                            body: changes
                        },
                        onSuccess, null, onFail);
                }
            },
            deleteRecord: function (layoutId, onSuccess, onFail) {
                exported.crudService.deleteLayout({ id: layoutId }, onSuccess, null, onFail);
            }
        }
    });

    exported.layoutVersionStore = ns.data.store.newStore({
        recordType: "LayoutVersion",
        name: "layout version",
        listName: "list of layout versions",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_layout_version" },
            { name: "displayName", type: String, default: "New layout version" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true },
            { name: "parentRecordId", type: Number, allowNull: true },
            { name: "version", type: Number, allowNull: true },
            { name: "moduleName", type: String, allowNull: true },
            { name: "assetDeployment", type: String, default: "Inherit" },
            { name: "zoneNesting", type: String, default: "zone1,zone2" },
            { name: "tag", type: String, allowNull: true },
            { name: "style", type: String, allowNull: true },
            { name: "classes", type: String, allowNull: true },
            { name: "nestingTag", type: String, allowNull: true },
            { name: "nestingStyle", type: String, allowNull: true },
            { name: "nestingClasses", type: String, allowNull: true },
            { name: "zones", type: Array },
            { name: "components", type: Array },
            { name: "dataScopes", type: Array },
            { name: "dataTypes", type: Array }],
        crud: {
            createRecord: function (layoutVersion, onSuccess, onFail, params) {
                exported.crudService.createLayoutVersion({
                    body: layoutVersion,
                    websiteVersionId: params.websiteVersionId,
                    scenario: params.scenario
                },
                    onSuccess,
                    onFail);
            },
            retrieveRecord: function (layoutVersionId, onSuccess, onFail) {
                exported.crudService.retrieveLayoutVersion({ id: layoutVersionId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.allLayoutVersions({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalLayoutVersion, updatedLayoutVersion, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.crudService.updateLayoutVersion(
                        {
                            id: updatedLayoutVersion.recordId,
                            body: changes
                        },
                        onSuccess, null, onFail);
                }
            },
            deleteRecord: function (layoutVersionId, onSuccess, onFail) {
                exported.crudService.deleteLayoutVersion({ id: layoutVersionId }, onSuccess, null, onFail);
            }
        },
        getLayoutVersions: function (layoutId, onSuccess, onFail) {
            var store = this;
            const description = "list of layout versions";
            exported.listService.layoutVersions(
                { id: layoutId },
                function (response) { store.handleGetSuccess(description, response, onSuccess, onFail); },
                null,
                function (ajax) { store.handleGetFail(description, ajax, onFail); });
        },
        getWebsiteLayoutVersion: function (websiteVersionId, segmentationScenarioName, layoutId, onSuccess, onFail) {
            var store = this;
            exported.listService.websiteLayoutVersion(
                {
                    websiteVersionId: websiteVersionId,
                    scenario: segmentationScenarioName,
                    layoutId: layoutId
                },
                function (response) {
                    if (response != undefined) {
                        var layoutVersionId = response.layoutVersionId;
                        if (layoutVersionId != undefined) {
                            store.retrieveRecord(layoutVersionId, onSuccess, onFail);
                            return;
                        }
                    }
                    if (onFail != undefined) onFail("Failed to get layout version for website version");
                },
                null,
                function (ajax) {
                    if (onFail != undefined) onFail("Failed to get layout version for website version");
                });
        }
    });

    exported.regionStore = ns.data.store.newStore({
        recordType: "Region",
        name: "region",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_region" },
            { name: "displayName", type: String, default: "New region" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true }],
        crud: {
            createRecord: function (region, onSuccess, onFail, params) {
                exported.crudService.createRegion({ body: region, websiteversionid: params.websiteVersionId }, onSuccess, onFail);
            },
            retrieveRecord: function (regionId, onSuccess, onFail) {
                exported.crudService.retrieveRegion({ id: regionId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.allRegions({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalRegion, updatedRegion, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.crudService.updateRegion(
                        {
                            id: updatedRegion.recordId,
                            body: changes
                        },
                        onSuccess, null, onFail);
                }
            },
            deleteRecord: function (regionId, onSuccess, onFail) {
                exported.crudService.deleteRegion({ id: regionId }, onSuccess, null, onFail);
            }
        }
    });

    exported.regionVersionStore = ns.data.store.newStore({
        recordType: "RegionVersion",
        name: "region version",
        listName: "list of region versions",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "recordId", type: Number, isKey: true, allowNull: true },
            { name: "name", type: String, default: "new_region_version" },
            { name: "displayName", type: String, default: "New region version" },
            { name: "description", type: String, allowNull: true },
            { name: "createdBy", type: String, allowNull: true },
            { name: "createdWhen", type: Date, allowNull: true },
            { name: "parentRecordId", type: Number, allowNull: true },
            { name: "version", type: Number, allowNull: true },
            { name: "moduleName", type: String, allowNull: true },
            { name: "assetDeployment", type: String, default: "Inherit" },
            { name: "layoutName", type: String, allowNull: true },
            { name: "layoutId", type: Number, allowNull: true },
            { name: "componentName", type: String, allowNull: true },
            { name: "componentId", type: Number, allowNull: true },
            { name: "assetName", type: String, allowNull: true },
            { name: "assetValue", type: String, allowNull: true },
            { name: "repeatDataTypeId", type: Number, allowNull: true },
            { name: "repeatDataScopeId", type: Number, allowNull: true },
            { name: "repeatDataScopeName", type: String, allowNull: true },
            { name: "listDataScopeId", type: Number, allowNull: true },
            { name: "listDataScopeName", type: String, allowNull: true },
            { name: "listElementTag", type: String, allowNull: true },
            { name: "listElementStyle", type: String, allowNull: true },
            { name: "layoutZones", type: Array },
            { name: "regionTemplates", type: Array },
            { name: "components", type: Array },
            { name: "dataScopes", type: Array },
            { name: "dataTypes", type: Array }],
        crud: {
            createRecord: function (regionVersion, onSuccess, onFail, params) {
                exported.crudService.createRegionVersion({
                    body: regionVersion,
                    websiteVersionId: params.websiteVersionId,
                    scenario: params.scenario
                },
                    onSuccess,
                    onFail);
            },
            retrieveRecord: function (regionVersionId, onSuccess, onFail) {
                exported.crudService.retrieveRegionVersion({ id: regionVersionId }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.listService.allRegionVersions({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalRegionVersion, updatedRegionVersion, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.crudService.updateRegionVersion(
                        {
                            id: updatedRegionVersion.recordId,
                            body: changes
                        },
                        onSuccess, null, onFail);
                }
            },
            deleteRecord: function (regionVersionId, onSuccess, onFail) {
                exported.crudService.deleteRegionVersion({ id: regionVersionId }, onSuccess, null, onFail);
            }
        },
        getRegionVersions: function (regionId, onSuccess, onFail) {
            var store = this;
            const description = "list of region versions";
            exported.listService.regionVersions(
                { id: regionId },
                function (response) { store.handleGetSuccess(description, response, onSuccess, onFail); },
                null,
                function (ajax) { store.handleGetFail(description, ajax, onFail); });
        },
        getWebsiteRegionVersion: function (websiteVersionId, segmentationScenarioName, regionId, onSuccess, onFail) {
            var store = this;
            exported.listService.websiteRegionVersion(
                {
                    websiteVersionId: websiteVersionId,
                    scenario: segmentationScenarioName,
                    regionId: regionId
                },
                function (response) {
                    if (response != undefined) {
                        var regionVersionId = response.regionVersionId;
                        if (regionVersionId != undefined) {
                            store.retrieveRecord(regionVersionId, onSuccess, onFail);
                            return;
                        }
                    }
                    if (onFail != undefined) onFail("Failed to get region version for website version");
                },
                null,
                function (ajax) {
                    if (onFail != undefined) onFail("Failed to get region version for website version");
                });
        }
    });

    exported.userStore = ns.data.store.newStore({
        recordType: "User",
        name: "user",
        listName: "list of users",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "userUrn", type: String, isKey: true },
            { name: "name", type: String }],
        crud: {
            retrieveRecord: function (userUrn, onSuccess, onFail) {
                // TODO: Get users from back-end service
                onSuccess({ userUrn: userUrn, name: "bikeman" });
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                // TODO: Get users from back-end service
                onSuccess([]);
            }
        }
    });

    exported.segmentTestStore = ns.data.store.newStore({
        recordType: "SegmentationTest",
        name: "segmentation test",
        listName: "list of segmentation tests",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "name", type: String, default: "new_segment", isKey: true },
            { name: "displayName", type: String, default: "New segment" },
            { name: "description", type: String, allowNull: true },
            { name: "start", type: Date },
            { name: "end", type: Date },
            { name: "environment", type: String, default: "production" },
            { name: "pages", type: Array },
            { name: "map", type: Array }],
        crud: {
            createRecord: function (test, onSuccess, onFail, params) {
                exported.segmentTestingService.createTest({ body: test }, onSuccess, onFail);
            },
            retrieveRecord: function (name, onSuccess, onFail) {
                exported.segmentTestingService.retrieveTest({ name: name }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.segmentTestingService.allTests({}, onSuccess, null, onFail);
            },
            updateRecord: function (originalTest, updatedTest, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.segmentTestingService.updateTest({ body: updatedTest }, onSuccess, null, onFail);
                }
            },
            deleteRecord: function (name, onSuccess, onFail) {
                exported.segmentTestingService.deleteTest({ name: name }, onSuccess, null, onFail);
            }
        }
    });

    exported.segmentScenarioStore = ns.data.store.newStore({
        recordType: "SegmentationScenario",
        name: "segmentation scenario",
        listName: "list of segmentation scenarios",
        dispatcher: exported.dispatcher,
        fields: [
            { name: "name", type: String, default: "new_scenario", isKey: true },
            { name: "displayName", type: String, default: "New scenario" },
            { name: "description", type: String, allowNull: true }],
        crud: {
            createRecord: function (scenario, onSuccess, onFail, params) {
                exported.segmentTestingService.createScenario({ body: scenario }, onSuccess, onFail);
            },
            retrieveRecord: function (name, onSuccess, onFail) {
                exported.segmentTestingService.retrieveScenario({ name: name }, onSuccess, null, onFail);
            },
            retrieveAllRecords: function (onSuccess, onFail) {
                exported.segmentTestingService.allScenarios(
                    {},
                    function (response) {
                        response.unshift(null);
                        onSuccess(response);
                    },
                    null,
                    onFail);
            },
            updateRecord: function (originalScenario, updatedScenario, changes, onSuccess, onFail) {
                if (changes.length > 0) {
                    exported.segmentTestingService.updateScenario({ body: updatedScenario }, onSuccess, null, onFail);
                }
            },
            deleteRecord: function (name, onSuccess, onFail) {
                exported.segmentTestingService.deleteScenario({ name: name }, onSuccess, null, onFail);
            }
        }
    });
}