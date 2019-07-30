var dataUtilities = function() {
    var copyObject = function (props, arrayProps, from, to) {
        to = to || {};
        props.forEach(function (prop) {
            if (typeof(from[prop]) === "object") {
                to[prop] = Object.assign({}, from[prop]);
            } else {
                to[prop] = from[prop];
            }
        });
        arrayProps.forEach(function (prop) {
            to[prop] = [];
            if (from[prop] != undefined) {
                from[prop].forEach(function (e) {
                    if (typeof (e) === "object")
                        to[prop].push(Object.assign({}, e));
                    else
                        to[prop].push(e);
                });
            }
        });
        return to;
    }

    var findChanges = function (props, arrayProps, original, modified) {
        var changes = [];
        if (original == undefined) {
            props.forEach(function(prop) {
                var value = modified[prop];
                if (value != undefined)
                    changes.push({ name: prop, value: value });
            });
        } else {
            props.forEach(function (prop) {
                var value = modified[prop];
                if (original[prop] !== value)
                    if (value == undefined || typeof(value) !== "object")
                        changes.push({ name: prop, value: value });
            });
        }
        return changes;
    }

    var doSuccessGet = function(recordType, response, onSuccess, onFail) {
        if (response == undefined) {
            if (onFail != undefined) onFail("No " + recordType + " was returned by the server");
        } else {
            if (onSuccess != undefined) onSuccess(response);
        }
    }

    var doFailGet = function(recordType, ajax, onFail) {
        if (onFail != undefined) {
            onFail("Failed to retrieve a " + recordType);
        }
    }

    var newStore = function (store) {
        store.list = null;
        store.dictionary = {};
        store.dispatcherUnsubscribe = null;
        store.mixin = store.mixin || {},
        store.name = store.name || recordType,
        store.listName = store.listName || "list of " + name,
        store.arrayFields = store.arrayFields || [];

        store.dispose = function() {
            if (store.dispatcherUnsubscribe != null) store.dispatcherUnsubscribe();
        };

        store.add = function(record) {
            if (store.list != undefined) store.list.push(record);
            store.dictionary[record[store.idField]] = record;
        };

        store.remove = function(id) {
            delete store.dictionary[id];
            if (store.list != undefined) {
                var index = -1;
                store.list.forEach(function (e, i) { if (e != undefined && e[store.idField] === id) index = i; });
                if (index >= 0) store.list.splice(index, 1);
            }
        };

        store.setAll = function(records) {
            if (records == null) return;
            store.list = records;
            store.dictionary = {};
            for (let i = 0; i < records.length; i++) {
                if (records[i] != undefined && records[i][store.idField] != undefined)
                    store.dictionary[records[i][store.idField]] = records[i];
            }
        };

        store.getAll = function () { return store.list; };

        store.set = function(record) {
            if (record == undefined) return;
            var id = record[store.idField];
            if (id == undefined) return;
            var original = store.dictionary[id];
            if (original == undefined) {
                store.add(record);
            } else {
                copyObject(store.fields, store.arrayFields, record, original);
            }
        };
        
        store.get = function (id) { return store.dictionary[id]; }

        store.createRecord = function (newRecord, onSuccess, onFail, params) {
            if (store.mixin.createRecord == undefined) return;
            store.mixin.createRecord.call(store,
                newRecord,
                function (response) {
                    if (response == undefined) {
                        if (onFail != undefined) onFail("No response was received from the server, the " + store.name + " might not have been created");
                    } else if (response[store.idField] == undefined) {
                        if (onFail != undefined) onFail("The server failed to return an ID for the new " + store.name);
                    } else {
                        store.add(response);
                        if (onSuccess != undefined) onSuccess(response);
                    }
                },
                function(ajax) {
                    if (ajax.status === 403) onFail("You do not have permission to create " + store.name);
                    if (onFail != undefined) onFail("Failed to create a new " + store.name);
                },
                params);
        }

        store.retrieveAllRecords = function(onSuccess, onFail) {
            if (store.mixin.retrieveAllRecords == undefined) return;
            var records = store.getAll();
            if (records == undefined) {
                store.mixin.retrieveAllRecords.call(store,
                    function(response) {
                        if (response == undefined) {
                            if (onFail != undefined) onFail("No " + store.listName + " was returned by the server");
                        } else {
                            store.setAll(response);
                            if (onSuccess != undefined) onSuccess(response);
                        }
                    }, 
                    function(ajax) {
                        if (ajax.status === 403) onFail("You do not have permission to retrieve " + store.listName);
                        if (onFail != undefined) onFail("Failed to retrieve a " + store.listName);
                    });
            } else {
                if (onSuccess != undefined) onSuccess(records);
            }
        }

        store.retrieveRecord = function(id, onSuccess, onFail) {
            if (store.mixin.retrieveRecord == undefined) return;
            if (id == undefined) return;
            var record = store.get(id);
            if (record == undefined) {
                store.mixin.retrieveRecord.call(store,
                    id,
                    function (response) {
                        store.set(response);
                        if (onSuccess != undefined) onSuccess(response);
                    },
                    function(ajax) {
                        if (ajax.status === 403) onFail("You do not have permission to retrieve " + store.name + " " + id);
                        if (onFail != undefined) onFail("Failed to retrieve " + store.name + " " + id);
                    });
            } else {
                if (onSuccess != undefined) onSuccess(record);
            }
        }

        store.updateRecord = function (originalRecord, updatedRecord, onSuccess, onFail) {
            if (store.mixin.updateRecord == undefined) return;
            var changes = findChanges(store.fields, store.arrayFields, originalRecord, updatedRecord);
            if (changes.length === 0 && store.arrayFields.length === 0) {
                if (onSuccess != undefined) onSuccess(originalRecord);
            } else {
                var id = originalRecord[store.idField];
                if (updatedRecord[store.idField] !== id) {
                    if (onFail != undefined) {
                        onFail("Original and updated " + store.name + " must have the same id");
                    }
                    return;
                }
                store.mixin.updateRecord.call(store,
                    originalRecord,
                    updatedRecord,
                    changes,
                    function(response) {
                        if (response == undefined) {
                            if (onFail != undefined) onFail("No response was received from the server, the " + store.name + " might not have been updated");
                        } else {
                            store.set(response);
                            if (onSuccess != undefined) onSuccess(response);
                        }
                    },
                    function (ajax) {
                        if (onFail != undefined) {
                            if (ajax.status === 403) onFail("You do not have permission to update " + store.name);
                            else onFail("Failed to update " + store.name);
                        }
                    });
            }
        }

        store.deleteRecord = function(id, onSuccess, onFail) {
            if (store.mixin.deleteRecord == undefined) return;
            store.mixin.deleteRecord(
                id,
                function(response) {
                    store.remove(id);
                    if (onSuccess != undefined) onSuccess(response);
                },
                function(ajax) {
                    if (onFail != undefined) {
                        if (ajax.status === 403) onFail("You do not have permission to delete " + store.name + " " + id);
                        else onFail("Failed to delete " + store.name + " " + id);
                    }
                });
        }

        store.cloneForEditing = function(original) {
            return copyObject(store.fields, store.arrayFields, original);
        }

        store.blankRecord = function() {
             return {};
        }

        if (store.recordType != undefined) {
            store.dispatcherUnsubscribe = exported.dispatcher.subscribe(function(message) {
                if (message.propertyChanges != undefined) {
                    for (let i = 0; i < message.propertyChanges.length; i++) {
                        var propertyChange = message.propertyChanges[i];
                        if (propertyChange.recordType === store.recordType) {
                            var record = store.get(propertyChange.id);
                            if (record != undefined) {
                                record[propertyChange.name] = propertyChange.value;
                            }
                        }
                    }
                }
                if (message.deletedRecords != undefined) {
                    for (let i = 0; i < message.deletedRecords.length; i++) {
                        var deletedElement = message.deletedRecords[i];
                        if (deletedElement.recordType === store.recordType) {
                            store.remove(deletedElement.id);
                        }
                    }
                }
                if (message.newRecords != undefined && store.mixin.retrieveRecord != undefined) {
                    for (let i = 0; i < message.newRecords.length; i++) {
                        var newRecord = message.newRecords[i];
                        if (newRecord.recordType === store.recordType) {
                            store.mixin.retrieveRecord.call(store, newRecord.id, function (r) { store.set(r); });
                        }
                    }
                }
                if (message.childListChanges != undefined) {
                    for (let i = 0; i < message.childListChanges.length; i++) {
                        var childListChange = message.childListChanges[i];
                        if (childListChange.recordType === store.recordType) {
                            // TODO: refresh child list
                        }
                    }
                }
            });
        }

        return store;
    }

    return {
        copyObject: copyObject,
        findChanges: findChanges,
        doSuccessGet: doSuccessGet,
        doFailGet: doFailGet,
        newStore: newStore
    }
}();

exported.environmentStore = dataUtilities.newStore({
    recordType: "Environment",
    name: "environment",
    idField: "recordId",
    fields: ["name", "displayName", "description", "createdBy", "createdWhen", "recordId", "baseUrl", "websiteVersionId"],
    mixin: {
        createRecord: function(environment, onSuccess, onFail, params) {
            exported.crudService.createEnvironment({ body: environment }, onSuccess, onFail);
        },
        retrieveRecord: function(environmentId, onSuccess, onFail) {
            exported.crudService.retrieveEnvironment({ id: environmentId }, onSuccess, null, onFail);
        },
        retrieveAllRecords: function(onSuccess, onFail) {
            exported.listService.environments({}, onSuccess, null, onFail);
        },
        updateRecord: function(originalEnvironment, updatedEnvironment, changes, onSuccess, onFail) {
            exported.crudService.updateEnvironment(
            {
                id: updatedEnvironment.recordId,
                body: changes
            },
            onSuccess, null, onFail);
        },
        deleteRecord: function(environmentId, onSuccess, onFail) {
            exported.crudService.deleteEnvironment({ id: environmentId }, onSuccess, null, onFail);
        }
    }
});

exported.websiteVersionStore = dataUtilities.newStore({
    recordType: "WebsiteVersion",
    name: "website version",
    idField: "recordId",
    fields: ["name", "displayName", "description", "createdBy", "createdWhen", "recordId"],
    mixin: {
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
        exported.listService.websiteVersionPages(
            { id: websiteVersionId },
            function (response) { dataUtilities.doSuccessGet("list of vebsite version pages", response, onSuccess, onFail); },
            null,
            function (ajax) { dataUtilities.doFailGet("list of vebsite version pages", ajax, onFail); });
    }
});

exported.pageStore = dataUtilities.newStore({
    recordType: "Page",
    name: "page",
    listName: "list of pages",
    idField: "recordId",
    fields: ["name", "displayName", "description", "createdBy", "createdWhen", "recordId"],
    mixin: {
        createRecord: function(page, onSuccess, onFail, params) {
            exported.crudService.createPage({ body: page, websiteversionid: params.websiteVersionId }, onSuccess, onFail);
        },
        retrieveRecord: function(pageId, onSuccess, onFail) {
            exported.crudService.retrievePage({ id: pageId }, onSuccess, null, onFail);
        },
        retrieveAllRecords: function(onSuccess, onFail) {
            exported.listService.allPages({}, onSuccess, null, onFail);
        },
        updateRecord: function(originalPage, updatedPage, changes, onSuccess, onFail) {
            if (changes.length > 0) {
                exported.crudService.updatePage(
                    {
                        id: updatedPage.recordId,
                        body: changes
                    },
                    onSuccess, null, onFail);
            }
        },
        deleteRecord: function(pageId, onSuccess, onFail) {
            exported.crudService.deletePage({ id: pageId }, onSuccess, null, onFail);
        }
    }
});

exported.pageVersionStore = dataUtilities.newStore({
    recordType: "PageVersion",
    name: "page version",
    listName: "list of page versions",
    idField: "recordId",
    fields: [
        "name", "displayName", "description", "createdBy", "createdWhen",
        "recordId", "parentRecordId", "version", "moduleName", "assetDeployment", "masterPageId",
        "layoutName", "layoutId", "canonicalUrl", "title", "bodyStyle", "permission", "assetPath"],
    arrayFields: [
        "routes", "layoutZones", "components"],
    mixin: {
        createRecord: function (pageVersion, onSuccess, onFail, params) {
            exported.crudService.createPageVersion({ body: pageVersion, websiteVersionId: params.websiteVersionId }, onSuccess, onFail);
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
            if (updatedPageVersion.routes != undefined) {
                exported.crudService.updatePageVersionRoutes(
                    {
                        id: updatedPageVersion.recordId,
                        body: updatedPageVersion.routes
                    },
                    function (response) {
                        originalPageVersion.routes = updatedPageVersion.routes;
                        if (onSuccess != undefined) onSuccess(response);
                    },
                    null,
                    onFail);
            }
            if (updatedPageVersion.layoutZones != null) {
                exported.crudService.updatePageVersionZones(
                    {
                        id: updatedPageVersion.recordId,
                        body: updatedPageVersion.layoutZones
                    },
                    function (response) {
                        originalPageVersion.layoutZones = updatedPageVersion.layoutZones;
                        if (onSuccess != undefined) onSuccess(response);
                    },
                    null,
                    onFail);
            }
            if (updatedPageVersion.components != undefined) {
                exported.crudService.updatePageVersionComponents(
                    {
                        id: updatedPageVersion.recordId,
                        body: updatedPageVersion.components
                    },
                    function (response) {
                        originalPageVersion.components = updatedPageVersion.components;
                        if (onSuccess != undefined) onSuccess(response);
                    },
                    null,
                    onFail);
            }
        },
        deleteRecord: function (pageVersionId, onSuccess, onFail) {
            exported.crudService.deletePage({ id: pageVersionId }, onSuccess, null, onFail);
        }
    },
    getPageVersions: function (pageId, onSuccess, onFail) {
        var store = this;
        exported.listService.pageVersions(
            { id: pageId },
            function (response) { dataUtilities.doSuccessGet("list of page versions", response, onSuccess, onFail); },
            null,
            function (ajax) { dataUtilities.doFailGet("list of page versions", ajax, onFail); });
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

exported.userStore = dataUtilities.newStore({
    recordType: "User",
    name: "user",
    listName: "list of users",
    idField: "userUrn",
    fields: ["name", "userUrn"],
    mixin: {
        retrieveRecord: function (userUrn, onSuccess, onFail) {
            onSuccess({userUrn: userUrn, name: "bikeman"});
        },
        retrieveAllRecords: function (onSuccess, onFail) {
            onSuccess([]);
        }
    }
});

exported.segmentTestStore = dataUtilities.newStore({
    recordType: "SegmentationTest",
    name: "segmentation test",
    listName: "list of segmentation tests",
    idField: "name",
    fields: ["name", "displayName", "description", "start", "end", "environment", "pages", "map"],
    mixin: {
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

exported.segmentScenarioStore = dataUtilities.newStore({
    recordType: "SegmentationScenario",
    name: "segmentation scenario",
    listName: "list of segmentation scenarios",
    idField: "name",
    fields: ["name", "displayName", "description"],
    mixin: {
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

