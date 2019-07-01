var dataUtilities = function() {
    var copyObject = function (properties, from, to) {
        to = to || {};
        properties.forEach(function (prop) {
            to[prop] = from[prop];
        });
        return to;
    }

    var findChanges = function (properties, original, modified) {
        var changes = [];
        if (original == undefined) {
            properties.forEach(function (prop) {
                changes.push({ name: prop, value: modified[prop] });
            });
        } else {
            properties.forEach(function(prop) {
                if (original[prop] !== modified[prop])
                    changes.push({ name: prop, value: modified[prop] });
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

    var newStore = function (params) {
        var idField = params.idField;
        var fields = params.fields;
        var recordType = params.recordType;
        var methods = params.methods || {};
        var name = params.name || recordType;
        var listName = params.listName || "list of " + name;

        var list = null;
        var dictionary = {};
        var dispatcherUnsubscribe = null;

        var dispose = function() {
            if (dispatcherUnsubscribe != null) dispatcherUnsubscribe();
        };

        var add = function(record) {
            if (list != undefined) list.push(record);
            dictionary[record[idField]] = record;
        };

        var remove = function(id) {
            delete dictionary[id];
            if (list != undefined) {
                var index = -1;
                list.forEach(function(e, i) { if (e[idField] === id) index = i; });
                if (index >= 0) list.splice(index, 1);
            }
        };

        var setAll = function(records) {
            if (records == null) return;
            list = records;
            dictionary = {};
            for (let i = 0; i < records.length; i++) {
                dictionary[records[i][idField]] = records[i];
            }
        };

        var getAll = function() { return list; };

        var set = function(record) {
            if (record == undefined) return;
            var id = record[idField];
            if (id == undefined) return;
            var original = dictionary[id];
            if (original == undefined) {
                add(record);
            } else {
                copyObject(fields, record, original);
            }
        };
        
        var get = function(id) { return dictionary[id]; }

        var createRecord = function(newRecord, onSuccess, onFail) {
            methods.createRecord(
                newRecord,
                function (response) {
                    if (response == undefined) {
                        if (onFail != undefined) onFail("No response was received from the server, the " + name + " might not have been created");
                    } else if (response[idField] == undefined) {
                        if (onFail != undefined) onFail("The server failed to return an ID for the new " + name);
                    } else {
                        add(response);
                        if (onSuccess != undefined) onSuccess(response);
                    }
                },
                function(ajax) {
                    if (onFail != undefined) onFail("Failed to create a new " + name);                        
                });
        }

        var retrieveAllRecords = function(onSuccess, onFail) {
            var records = getAll();
            if (records == undefined) {
                methods.retrieveAllRecords(
                    function(response) {
                        if (response == undefined) {
                            if (onFail != undefined) onFail("No " + listName + " was returned by the server");
                        } else {
                            setAll(response);
                            if (onSuccess != undefined) onSuccess(response);
                        }
                    }, 
                    function(ajax) {
                        if (onFail != undefined) onFail("Failed to retrieve a " + listName);                        
                    });
            } else {
                if (onSuccess != undefined) onSuccess(records);
            }
        }

        var retrieveRecord = function(id, onSuccess, onFail) {
            if (id == undefined) return;
            var record = get(id);
            if (record == undefined) {
                methods.retrieveRecord( 
                    id,
                    function (response) {
                        set(response);
                        if (onSuccess != undefined) onSuccess(response);
                    },
                    function(ajax) {
                        if (onFail != undefined) onFail("Failed to retrieve " + name + " " + id);                        
                    });
            } else {
                if (onSuccess != undefined) onSuccess(record);
            }
        }

        var updateRecord = function(originalRecord, updatedRecord, onSuccess, onFail) {
            var changes = dataUtilities.findChanges(fields, originalRecord, updatedRecord);
            if (changes.length === 0) {
                if (onSuccess != undefined) onSuccess(originalRecord);
            } else {
                var id = originalRecord[idField];
                if (updatedRecord[idField] !== id) {
                    if (onFail != undefined) {
                        onFail("Original and updated records must have the same id");
                    }
                    return;
                }
                methods.updateRecord(
                    id,
                    changes,
                    function(response) {
                        if (response == undefined) {
                            if (onFail != undefined) onFail("No response was received from the server, the " + name + " might not have been updated");
                        } else {
                            set(response);
                            if (onSuccess != undefined) onSuccess(response);
                        }
                    },
                    function(ajax) {
                        if (onFail != undefined) onFail("Failed to update " + name);                        
                    });
            }
        }

        var deleteRecord = function(id, onSuccess, onFail) {
            methods.deleteRecord(
                id,
                function(response) {
                    remove(id);
                    if (onSuccess != undefined) onSuccess(response);
                },
                function(ajax) {
                    if (onFail != undefined) {
                        if (ajax.status === 403) onFail("You do not have permission to delete " + name + " " + id);
                        else onFail("Failed to delete " + name + " " + id);
                    }
                });
        }

        var cloneForEditing = function(original) {
            return dataUtilities.copyObject(fields, original);
        }

        var blankRecord = function() {
             return {};
        }

        if (recordType != undefined) {
            dispatcherUnsubscribe = exported.dispatcher.subscribe(function(message) {
                if (message.propertyChanges != undefined) {
                    for (let i = 0; i < message.propertyChanges.length; i++) {
                        var propertyChange = message.propertyChanges[i];
                        if (propertyChange.elementType === recordType) {
                            var record = get(propertyChange.id);
                            if (record != undefined) {
                                record[propertyChange.name] = propertyChange.value;
                            }
                        }
                    }
                }
                if (message.deletedElements != undefined) {
                    for (let i = 0; i < message.deletedElements.length; i++) {
                        var deletedElement = message.deletedElements[i];
                        if (deletedElement.elementType === recordType) {
                            remove(deletedElement.id);
                        }
                    }
                }
                if (message.newElements != undefined && methods.retrieveRecord != undefined) {
                    for (let i = 0; i < message.newElements.length; i++) {
                        var newElement = message.newElements[i];
                        if (newElement.elementType === recordType) {
                            methods.retrieveRecord(newElement.id, function(r) { set(r); });
                        }
                    }
                }
            });
        }

        return {
            idField: idField,
            fields: fields,
            dispose: dispose,
            createRecord: createRecord,
            retrieveAllRecords: retrieveAllRecords,
            retrieveRecord: retrieveRecord,
            updateRecord: updateRecord,
            deleteRecord: deleteRecord,
            cloneForEditing: cloneForEditing,
            blankRecord: blankRecord
        };
    }

    return {
        copyObject: copyObject,
        findChanges: findChanges,
        doSuccessGet: doSuccessGet,
        doFailGet: doFailGet,
        newStore: newStore
    }
}();

var environmentStore = dataUtilities.newStore({
    recordType: "Environment",
    name: "environment",
    idField: "environmentId",
    fields: ["name", "displayName", "description", "createdBy", "createdWhen", "environmentId", "baseUrl", "websiteVersionId"],
    methods: {
        createRecord: function(environment, onSuccess, onFail) {
            exported.crudService.createEnvironment({ body: environment }, onSuccess, onFail)
        },
        retrieveRecord: function(environmentId, onSuccess, onFail) {
            exported.crudService.retrieveEnvironment({ id: environmentId }, onSuccess, null, onFail);
        },
        retrieveAllRecords: function(onSuccess, onFail) {
            exported.listService.environments({}, onSuccess, null, onFail);
        },
        updateRecord: function(environmentId, changes, onSuccess, onFail) {
            exported.crudService.updateEnvironment(
            {
                id: environmentId,
                body: changes
            },
            onSuccess, null, onFail);
        },
        deleteRecord: function(environmentId, onSuccess, onFail) {
            exported.crudService.deleteEnvironment({ id: environmentId }, onSuccess, null, onFail);
        }
    }
});

exported.environmentStore = environmentStore;

var websiteVersionStore = dataUtilities.newStore({
    recordType: "WebsiteVersion",
    name: "website version",
    idField: "websiteVersionId",
    fields: ["name", "displayName", "description", "createdBy", "createdWhen", "websiteVersionId"],
    methods: {
        createRecord: function (websiteVersion, onSuccess, onFail) {
            exported.crudService.createWebsiteVersion({ body: websiteVersion }, onSuccess, onFail)
        },
        retrieveRecord: function (websiteVersionId, onSuccess, onFail) {
            exported.crudService.retrieveWebsiteVersion({ id: websiteVersionId }, onSuccess, null, onFail);
        },
        retrieveAllRecords: function (onSuccess, onFail) {
            exported.listService.websiteVersions({}, onSuccess, null, onFail);
        },
        updateRecord: function (websiteVersionId, changes, onSuccess, onFail) {
            exported.crudService.updateWebsiteVersion(
            {
                id: websiteVersionId,
                body: changes
            },
            onSuccess, null, onFail);
        },
        deleteRecord: function (websiteVersionId, onSuccess, onFail) {
            exported.crudService.deleteWebsiteVersion({ id: websiteVersionId }, onSuccess, null, onFail);
        }
    }
});

websiteVersionStore.getPages = function (websiteVersionId, onSuccess, onFail) {
    exported.listService.websiteVersionPages(
        { id: websiteVersionId },
        function (response) { dataUtilities.doSuccessGet("list of vebsite version pages", response, onSuccess, onFail); },
        null,
        function (ajax) { dataUtilities.doFailGet("list of vebsite version pages", ajax, onFail); });
}

exported.websiteVersionStore = websiteVersionStore;

var pageStore = function () {
    var pageList = null;
    var pagesById = {};

    var pageProperties = [
        "name", "displayName", "description", "createdBy", "createdWhen", "elementType", "elementId"];

    var add = function (page) {
        if (pageList != undefined) pageList.push(page);
        pagesById[page.environmentId] = page;
    }

    var remove = function (environmentId) {
        delete environmentsById[environmentId];
        if (environmentList != undefined) {
            var index = -1;
            environmentList.forEach(function (e, i) { if (e.environmentId === environmentId) index = i; });
            if (index >= 0) environmentList.splice(index, 1);
        }
    }

    var getAllPages = function (onSuccess, onFail) {
        exported.listService.allPages(
            {},
            function (response) { dataUtilities.doSuccess("list of pages", response, onSuccess, onFail); },
            null,
            function (ajax) { dataUtilities.doFail("list of pages", ajax, onFail); });
    }

    var getEditablePage = function (page) {
        return dataUtilities.copyObject(pageProperties, page);
    }

    var getNewPage = function() {
        return {};
    }

    var createPage = function (page, websiteVersionId, onsuccess, onfail) {
        exported.crudService.createPage(
            { body: page, websiteversionid: websiteVersionId }, 
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the page might not have been created");
                } else if (response.elementId == undefined) {
                    if (onfail != undefined) onfail("The server failed to return an ID for the new page");
                } else {
                    pagesById[response.elementId] = response;
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function(ajax) {
                if (onfail != undefined) {
                    onfail("Failed to create new page");
                }
            });
    }

    var retrievePage = function (pageId, onsuccess) {
        if (pageId == undefined) return;
        var page = pagesById[pageId];
        if (page == undefined) {
            exported.crudService.retrievePage(
                { id: pageId },
                function (response) {
                    pagesById[response.elementId] = response;
                    if (onsuccess != undefined) onsuccess(response);
                });
        } else {
            if (onsuccess != undefined) onsuccess(page);
        }
    }

    var updatePage = function (originalPage, updatedPage, onsuccess, onfail) {
        var changes = dataUtilities.findChanges(pageProperties, originalPage, updatedPage);
        if (changes.length === 0) {
            if (onsuccess != undefined) onsuccess(originalPage);
            return;
        }
        exported.crudService.updatePage(
            {
                id: updatedPage.elementId,
                body: changes
            },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the page might not have been updated");
                } else {
                    var cached = pagesById[updatedPage.elementId];
                    if (cached != undefined) {
                        dataUtilities.copyObject(pageProperties, response, cached);
                    }
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function(ajax) {
                if (onfail != undefined) {
                    onfail("Failed to update the page");
                }
            });
    }

    var deletePage = function (pageId, onsuccess, onfail) {
        exported.crudService.deletePage(
            { id: pageId },
            function (response) {
                delete pagesById[pageId];
                if (onsuccess != undefined) onsuccess(response);
            },
            null,
            onfail);
    }

    var dispatcherUnsubscribe = exported.dispatcher.subscribe(function(message) {
        if (message.propertyChanges != undefined) {
            for (let i = 0; i < message.propertyChanges.length; i++) {
                var propertyChange = message.propertyChanges[i];
                if (propertyChange.elementType === "Page") {
                    var page = pagesById[propertyChange.id];
                    if (page != undefined) {
                        page[propertyChange.name] = propertyChange.value;
                    }
                }
            }
        }
        if (message.deletedElements != undefined) {
            for (let i = 0; i < message.deletedElements.length; i++) {
                var deletedElement = message.deletedElements[i];
                if (deletedElement.elementType === "Page") {
                    delete pagesById[deletedElement.id];
                }
            }
        }
    });

    return {
        getEditablePage: getEditablePage,
        getNewPage: getNewPage,

        createPage: createPage,
        retrievePage: retrievePage,
        updatePage: updatePage,
        deletePage: deletePage,

        getAllPages: getAllPages
    }
}();
exported.pageStore = pageStore;

var pageVersionStore = function () {
    var pageVersions = {};

    var pageVersionProperties = ["name", "displayName", "description", "createdBy", "createdWhen",
        "elementVersionId", "elementId", "version", "moduleName", "assetDeployment", "masterPageId",
        "layoutName", "layoutVersionId", "canonicalUrl", "title", "bodyStyle", "permission", "assetPath"];

    var getEditablePageVersion = function (page) {
        return dataUtilities.copyObject(pageVersionProperties, page);
    }

    var getNewPageVersion = function () {
        return {};
    }
/*
    var createPage = function (page, onsuccess, onfail) {
        exported.crudService.createPage(
            { body: page },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the page might not have been created");
                } else if (response.elementId == undefined) {
                    if (onfail != undefined) onfail("The server failed to return an ID for the new page");
                } else {
                    pages[response.elementId] = response;
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function (ajax) {
                if (onfail != undefined) {
                    onfail("Failed to create new page");
                }
            });
    }

    var retrievePage = function (pageId, onsuccess) {
        if (pageId == undefined) return;
        var page = pages[pageId];
        if (page == undefined) {
            exported.crudService.retrievePage(
                { id: pageId },
                function (response) {
                    pages[response.elementId] = response;
                    if (onsuccess != undefined) onsuccess(response);
                });
        } else {
            if (onsuccess != undefined) onsuccess(page);
        }
    }

    var updatePage = function (originalPage, updatedPage, onsuccess, onfail) {
        var changes = dataUtilities.findChanges(pageProperties, originalPage, updatedPage);
        if (changes.length === 0) {
            if (onsuccess != undefined) onsuccess(originalPage);
            return;
        }
        exported.crudService.updatePage(
            {
                id: updatedPage.elementId,
                body: changes
            },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the page might not have been updated");
                } else {
                    var cached = pages[updatedPage.elementId];
                    if (cached != undefined) {
                        dataUtilities.copyObject(pageProperties, response, cached);
                    }
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function (ajax) {
                if (onfail != undefined) {
                    onfail("Failed to update the page");
                }
            });
    }

    var deletePage = function (pageId, onsuccess, onfail) {
        exported.crudService.deletePage(
            { id: pageId },
            function (response) {
                delete pages[pageId];
                if (onsuccess != undefined) onsuccess(response);
            },
            null,
            onfail);
    }

    var dispatcherUnsubscribe = exported.dispatcher.subscribe(function (message) {
        if (message.propertyChanges != undefined) {
            for (let i = 0; i < message.propertyChanges.length; i++) {
                var propertyChange = message.propertyChanges[i];
                if (propertyChange.elementType === "Page") {
                    var page = pages[propertyChange.id];
                    if (page != undefined) {
                        page[propertyChange.name] = propertyChange.value;
                    }
                }
            }
        }
        if (message.deletedElements != undefined) {
            for (let i = 0; i < message.deletedElements.length; i++) {
                var deletedElement = message.deletedElements[i];
                if (deletedElement.elementType === "Page") {
                    delete pages[deletedElement.id];
                }
            }
        }
    });
    */

    return {
        getEditablePageVersion: getEditablePageVersion,
        getNewPageVersion: getNewPageVersion,

        //createPageVersion: createPageVersion,
        //retrievePageVersion: retrievePageVersion,
        //updatePageVersion: updatePageVersion,
        //deletePageVersion: deletePageVersion,
    }
}();
exported.pageVersionStore = pageVersionStore;