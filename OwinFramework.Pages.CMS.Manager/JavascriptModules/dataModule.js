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

        var createRecord = function(newRecord, onSuccess, onFail, params) {
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
                },
                params);
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
        createRecord: function(environment, onSuccess, onFail, params) {
            exported.crudService.createEnvironment({ body: environment }, onSuccess, onFail);
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
        createRecord: function (websiteVersion, onSuccess, onFail, params) {
            exported.crudService.createWebsiteVersion({ body: websiteVersion }, onSuccess, onFail);
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

websiteVersionStore.getWebsiteVersionPages = function (websiteVersionId, onSuccess, onFail) {
    exported.listService.websiteVersionPages(
        { id: websiteVersionId },
        function (response) { dataUtilities.doSuccessGet("list of vebsite version pages", response, onSuccess, onFail); },
        null,
        function (ajax) { dataUtilities.doFailGet("list of vebsite version pages", ajax, onFail); });
}

exported.websiteVersionStore = websiteVersionStore;

var pageStore = dataUtilities.newStore({
    recordType: "Page",
    name: "page",
    listName: "list of pages",
    idField: "elementId",
    fields: ["name", "displayName", "description", "createdBy", "createdWhen", "elementType", "elementId"],
    methods: {
        createRecord: function(page, onSuccess, onFail, params) {
            exported.crudService.createPage({ body: page, websiteversionid: params.websiteVersionId }, onSuccess, onFail);
        },
        retrieveRecord: function(pageId, onSuccess, onFail) {
            exported.crudService.retrievePage({ id: pageId }, onSuccess, null, onFail);
        },
        retrieveAllRecords: function(onSuccess, onFail) {
            exported.listService.allPages({}, onSuccess, null, onFail);
        },
        updateRecord: function(pageId, changes, onSuccess, onFail) {
            exported.crudService.updatePage(
            {
                id: pageId,
                body: changes
            },
            onSuccess, null, onFail);
        },
        deleteRecord: function(pageId, onSuccess, onFail) {
            exported.crudService.deletePage({ id: pageId }, onSuccess, null, onFail);
        }
    }
});
exported.pageStore = pageStore;

var pageVersionStore = dataUtilities.newStore({
    recordType: "PageVersion",
    name: "page version",
    listName: "list of page versions",
    idField: "elementId",
    fields: [
        "name", "displayName", "description", "createdBy", "createdWhen",
        "elementVersionId", "elementId", "version", "moduleName", "assetDeployment", "masterPageId",
        "layoutName", "layoutVersionId", "canonicalUrl", "title", "bodyStyle", "permission", "assetPath"],
    methods: {
        createRecord: function (pageVersion, onSuccess, onFail, params) {
            exported.crudService.createPageVersion({ body: pageVersion, websiteVersionId: params.websiteVersionId }, onSuccess, onFail);
        },
        retrieveRecord: function (pageVersionId, onSuccess, onFail) {
            exported.crudService.retrievePageVersion({ id: pageVersionId }, onSuccess, null, onFail);
        },
        retrieveAllRecords: function (onSuccess, onFail) {
            exported.listService.allPageVersions({}, onSuccess, null, onFail);
        },
        updateRecord: function (pageVersionId, changes, onSuccess, onFail) {
            exported.crudService.updatePageVersion(
            {
                id: pageVersionId,
                body: changes
            },
            onSuccess, null, onFail);
        },
        deleteRecord: function (pageVersionId, onSuccess, onFail) {
            exported.crudService.deletePage({ id: pageVersionId }, onSuccess, null, onFail);
        }
    }
});

pageVersionStore.getPageVersions = function (pageId, onSuccess, onFail) {
    exported.listService.pageVersions(
        { id: pageId },
        function (response) { dataUtilities.doSuccessGet("list of page versions", response, onSuccess, onFail); },
        null,
        function (ajax) { dataUtilities.doFailGet("list of page versions", ajax, onFail); });
}

pageVersionStore.getWebsitePageVersion = function (websiteVersionId, pageId, onSuccess, onFail) {
    exported.listService.websitePageVersion(
        {
            websiteVersionId: websiteVersionId,
            pageId: pageId
        },
        function (response) {
            if (response != undefined) {
                var pageVersionId = response.pageVersionId;
                if (pageVersionId != undefined) {
                    pageVersionStore.retrieveRecord(pageVersionId, onSuccess, onFail);
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

// TODO: add these to the 'methods' collection in a way that gives them access to internal methods

exported.pageVersionStore = pageVersionStore;
