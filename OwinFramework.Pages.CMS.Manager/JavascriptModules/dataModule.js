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

    return {
        copyObject: copyObject,
        findChanges: findChanges
    }
}();

var websiteStore = function() {
    return{
    };
}();

var websiteVersionStore = function () {
    var getPages = function (websiteVersionId, onsuccess, onfail) {
        ns.cmsmanager.listService.websiteVersionPages(
            { id: websiteVersionId },
            function (response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No response was returned by the server");
                } else {
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function (ajax) {
                if (onfail != undefined) {
                    onfail("Failed to retrieve a list of website pages");
                }
            });
    }

    return {
        getPages: getPages
    };
}();

var pageStore = function () {
    var pages = {};

    var pageProperties = [
        "name", "displayName", "description", "createdBy", "createdWhen", "elementType", "elementId"];

    var getAllPages = function(onsuccess, onfail) {
        ns.cmsmanager.listService.allPages(
            {},
            function(response) {
                if (response == undefined) {
                    if (onfail != undefined) onfail("No pages were returned by the server");
                } else {
                    if (onsuccess != undefined) onsuccess(response);
                }
            },
            null,
            function(ajax) {
                if (onfail != undefined) {
                    onfail("Failed to retrieve a list of pages");
                }
            });
    }

    var getEditablePage = function (page) {
        return dataUtilities.copyObject(pageProperties, page);
    }

    var getNewPage = function() {
        return {};
    }

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

exported.websiteStore = websiteStore;
exported.websiteVersionStore = websiteVersionStore;
exported.pageStore = pageStore;
exported.pageVersionStore = pageVersionStore;