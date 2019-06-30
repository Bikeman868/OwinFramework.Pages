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

    return {
        copyObject: copyObject,
        findChanges: findChanges,
        doSuccessGet: doSuccessGet,
        doFailGet: doFailGet
    }
}();

var environmentStore = function () {
    var environmentList = null;
    var environmentsById = {};

    var environmentProperties = [
        "name", "displayName", "description", "createdBy", "createdWhen", "environmentId", "baseUrl", "websiteVersionId"];

    var add = function(environment){
        if (environmentList != undefined) environmentList.push(environment);
        environmentsById[environment.environmentId] = environment;
    }

    var remove = function(environmentId) {
        delete environmentsById[environmentId];
        if (environmentList != undefined) {
            var index = -1;
            environmentList.forEach(function (e, i) { if (e.environmentId === environmentId) index = i; });
            if (index >= 0) environmentList.splice(index, 1);
        }
    }

    var getEnvironments = function (onSuccess, onFail) {
        if (environmentList == undefined) {
            ns.cmsmanager.listService.environments(
                {},
                function (response) {
                    if (response != undefined) {
                        environmentList = response;
                        for (let i = 0; i < response.length; i++) {
                            environmentsById[response[i].environmentId] = response[i];
                        }
                    }
                    dataUtilities.doSuccessGet("list of environments", response, onSuccess, onFail);
                },
                null,
                function(ajax) { dataUtilities.doFailGet("list of environments", ajax, onFail); });
        } else {
            if (onSuccess != undefined) onSuccess(environmentList);
        }
    }

    var getEditableEnvironment = function (environment) {
        return dataUtilities.copyObject(environmentProperties, environment);
    }

    var getNewEnvironment = function () {
        return {};
    }

    var createEnvironment = function (environment, onsuccess, onfail) {
        ns.cmsmanager.crudService.createEnvironment(
            { body: environment },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the environment might not have been created");
                } else if (response.environmentId == undefined) {
                    if (onfail != undefined) onfail("The server failed to return an ID for the new environment");
                } else {
                    add(response);
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function (ajax) {
                if (onfail != undefined) {
                    onfail("Failed to create new environment");
                }
            });
    }

    var retrieveEnvironment = function (environmentId, onsuccess) {
        if (environmentId == undefined) return;
        var environment = environmentsById[environmentId];
        if (environment == undefined) {
            ns.cmsmanager.crudService.retrieveEnvironment(
                { id: environmentId },
                function (response) {
                    add(response);
                    if (onsuccess != undefined) onsuccess(response);
                });
        } else {
            if (onsuccess != undefined) onsuccess(environment);
        }
    }

    var updateEnvironment = function (originalEnvironment, updatedEnvironment, onsuccess, onfail) {
        var changes = dataUtilities.findChanges(environmentProperties, originalEnvironment, updatedEnvironment);
        if (changes.length === 0) {
            if (onsuccess != undefined) onsuccess(originalEnvironment);
            return;
        }
        ns.cmsmanager.crudService.updateEnvironment(
            {
                id: updatedEnvironment.environmentId,
                body: changes
            },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the environment might not have been updated");
                } else {
                    var cached = environmentsById[updatedEnvironment.environmentId];
                    if (cached != undefined) {
                        dataUtilities.copyObject(environmentProperties, response, cached);
                    }
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function (ajax) {
                if (onfail != undefined) {
                    onfail("Failed to update the environment");
                }
            });
    }

    var deleteEnvironment = function (environmentId, onsuccess, onfail) {
        ns.cmsmanager.crudService.deleteEnvironment(
            { id: environmentId },
            function (response) {
                remove(environmentId);
                if (onsuccess != undefined) onsuccess(response);
            },
            null,
            onfail);
    }

    var dispatcherUnsubscribe = exported.dispatcher.subscribe(function (message) {
        if (message.propertyChanges != undefined) {
            for (let i = 0; i < message.propertyChanges.length; i++) {
                var propertyChange = message.propertyChanges[i];
                if (propertyChange.elementType === "Environment") {
                    var environment = environmentsById[propertyChange.id];
                    if (environment != undefined) {
                        environment[propertyChange.name] = propertyChange.value;
                    }
                }
            }
        }
        if (message.deletedElements != undefined) {
            for (let i = 0; i < message.deletedElements.length; i++) {
                var deletedElement = message.deletedElements[i];
                if (deletedElement.elementType === "Environment") {
                    remove(deletedElement.id);
                }
            }
        }
        if (message.newElements != undefined) {
            for (let i = 0; i < message.newElements.length; i++) {
                var newElement = message.newElements[i];
                if (newElement.elementType === "Environment") {
                    retrieveEnvironment(newElement.id);
                }
            }
        }
    });

    return {
        getEditableEnvironment: getEditableEnvironment,
        getNewEnvironment: getNewEnvironment,

        createEnvironment: createEnvironment,
        retrieveEnvironment: retrieveEnvironment,
        updateEnvironment: updateEnvironment,
        deleteEnvironment: deleteEnvironment,

        getEnvironments: getEnvironments
    };
}();
exported.environmentStore = environmentStore;

var websiteVersionStore = function () {
    var websiteVersionList = null;
    var websiteVersionsById = {};

    var websiteVersionProperties = [
        "name", "displayName", "description", "createdBy", "createdWhen", "websiteVersionId"];

    var add = function (websiteVersion) {
        if (websiteVersionList != undefined) websiteVersionList.push(websiteVersion);
        websiteVersionsById[websiteVersion.websiteVersionId] = websiteVersion;
    }

    var remove = function (websiteVersionId) {
        delete websiteVersionsById[websiteVersionId];
        if (websiteVersionList != undefined) {
            var index = -1;
            websiteVersionList.forEach(function (v, i) { if (v.websiteVersionId === websiteVersionId) index = i; });
            if (index >= 0) websiteVersionList.splice(index, 1);
        }
    }

    var getEditableWebsiteVersion = function (websiteVersion) {
        return dataUtilities.copyObject(websiteVersionProperties, websiteVersion);
    }

    var getNewWebsiteVersion = function () {
        return {};
    }

    var createWebsiteVersion = function (websiteVersion, onsuccess, onfail) {
        ns.cmsmanager.crudService.createWebsiteVersion(
            { body: websiteVersion },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the website version might not have been created");
                } else if (response.websiteVersionId == undefined) {
                    if (onfail != undefined) onfail("The server failed to return an ID for the new website version");
                } else {
                    add(response);
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function (ajax) {
                if (onfail != undefined) {
                    onfail("Failed to create new website version");
                }
            });
    }

    var retrieveWebsiteVersion = function (websiteVersionId, onsuccess) {
        if (websiteVersionId == undefined) return;
        var websiteVersion = websiteVersionsById[websiteVersionId];
        if (websiteVersion == undefined) {
            ns.cmsmanager.crudService.retrieveWebsiteVersion(
                { id: websiteVersionId },
                function (response) {
                    add(response);
                    if (onsuccess != undefined) onsuccess(response);
                });
        } else {
            if (onsuccess != undefined) onsuccess(websiteVersion);
        }
    }

    var updateWebsiteVersion = function (originalWebsiteVersion, updatedWebsiteVersion, onsuccess, onfail) {
        var changes = dataUtilities.findChanges(websiteVersionProperties, originalWebsiteVersion, updatedWebsiteVersion);
        if (changes.length === 0) {
            if (onsuccess != undefined) onsuccess(originalWebsiteVersion);
            return;
        }
        ns.cmsmanager.crudService.updateWebsiteVersion(
            {
                id: updatedWebsiteVersion.websiteVersionId,
                body: changes
            },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was received from the server, the websiteVersion might not have been updated");
                } else {
                    var cached = websiteVersionsById[updatedWebsiteVersion.websiteVersionId];
                    if (cached != undefined) {
                        dataUtilities.copyObject(websiteVersionProperties, response, cached);
                    }
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function (ajax) {
                if (onfail != undefined) {
                    onfail("Failed to update the websiteVersion");
                }
            });
    }

    var deleteWebsiteVersion = function (websiteVersionId, onsuccess, onfail) {
        ns.cmsmanager.crudService.deleteWebsiteVersion(
            { id: websiteVersionId },
            function (response) {
                remove(websiteVersionId);
                if (onsuccess != undefined) onsuccess(response);
            },
            null,
            onfail);
    }

    var getWebsiteVersions = function (onSuccess, onFail) {
        if (websiteVersionList == undefined) {
            ns.cmsmanager.listService.websiteVersions(
                {},
                function (response) {
                    if (response != undefined) {
                        websiteVersionList = response;
                        for (let i = 0; i < response.length; i++) {
                            websiteVersionsById[response[i].websiteVersionId] = response[i];
                        }
                    }
                    dataUtilities.doSuccessGet("list of website versions", response, onSuccess, onFail);
                },
                null,
                function (ajax) { dataUtilities.doFailGet("list of website versions", ajax, onFail); });
        } else {
            if (onSuccess != undefined) onSuccess(websiteVersionList);
        }
    }

    var getPages = function (websiteVersionId, onSuccess, onFail) {
        ns.cmsmanager.listService.websiteVersionPages(
            { id: websiteVersionId },
            function (response) { dataUtilities.doSuccessGet("list of vebsite version pages", response, onSuccess, onFail); },
            null,
            function (ajax) { dataUtilities.doFailGet("list of vebsite version pages", ajax, onFail); });
    }

    var dispatcherUnsubscribe = exported.dispatcher.subscribe(function (message) {
        if (message.propertyChanges != undefined) {
            for (let i = 0; i < message.propertyChanges.length; i++) {
                var propertyChange = message.propertyChanges[i];
                if (propertyChange.elementType === "WebsiteVersion") {
                    var websiteVersion = websiteVersionsById[propertyChange.id];
                    if (websiteVersion != undefined) {
                        websiteVersion[propertyChange.name] = propertyChange.value;
                    }
                }
            }
        }
        if (message.deletedElements != undefined) {
            for (let i = 0; i < message.deletedElements.length; i++) {
                var deletedElement = message.deletedElements[i];
                if (deletedElement.elementType === "WebsiteVersion") {
                    remove(deletedElement.id);
                }
            }
        }
        if (message.newElements != undefined) {
            for (let i = 0; i < message.newElements.length; i++) {
                var newElement = message.newElements[i];
                if (newElement.elementType === "WebsiteVersion") {
                    retrieveWebsiteVersion(newElement.id);
                }
            }
        }
    });

    return {
        getEditableWebsiteVersion: getEditableWebsiteVersion,
        getNewWebsiteVersion: getNewWebsiteVersion,

        createWebsiteVersion: createWebsiteVersion,
        retrieveWebsiteVersion: retrieveWebsiteVersion,
        updateWebsiteVersion: updateWebsiteVersion,
        deleteWebsiteVersion: deleteWebsiteVersion,

        getWebsiteVersions: getWebsiteVersions,
        getPages: getPages
    };
}();
exported.websiteVersionStore = websiteVersionStore;

var pageStore = function () {
    var pages = {};

    var pageProperties = [
        "name", "displayName", "description", "createdBy", "createdWhen", "elementType", "elementId"];

    var getAllPages = function (onSuccess, onFail) {
        ns.cmsmanager.listService.allPages(
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
        ns.cmsmanager.crudService.createPage(
            { body: page, websiteversionid: websiteVersionId }, 
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
            function(ajax) {
                if (onfail != undefined) {
                    onfail("Failed to create new page");
                }
            });
    }

    var retrievePage = function (pageId, onsuccess) {
        if (pageId == undefined) return;
        var page = pages[pageId];
        if (page == undefined) {
            ns.cmsmanager.crudService.retrievePage(
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
        ns.cmsmanager.crudService.updatePage(
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
            function(ajax) {
                if (onfail != undefined) {
                    onfail("Failed to update the page");
                }
            });
    }

    var deletePage = function (pageId, onsuccess, onfail) {
        ns.cmsmanager.crudService.deletePage(
            { id: pageId },
            function (response) {
                delete pages[pageId];
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
        ns.cmsmanager.crudService.createPage(
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
            ns.cmsmanager.crudService.retrievePage(
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
        ns.cmsmanager.crudService.updatePage(
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
        ns.cmsmanager.crudService.deletePage(
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