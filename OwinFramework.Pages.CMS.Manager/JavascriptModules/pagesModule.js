exported.pageStore = function() {
    var pages = {};
    var pageVersions = {};

    var pageProperties = [
        "name", "displayName", "description", "createdBy", "createdWhen", "elementType", "elementId"];

    var pageVersionProperties = ["name", "displayName", "description", "createdBy", "createdWhen",
        "elementVersionId", "elementId", "version", "moduleName", "assetDeployment", "masterPageId",
        "layoutName", "layoutVersionId", "canonicalUrl", "title", "bodyStyle", "permission", "assetPath"];

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

    var getEditablePage = function (page) {
        return copyObject(pageProperties, page);
    }

    var getEditablePageVersion = function (pageVersion) {
        return copyObject(pageVersionProperties, pageVersion);
    }

    var getNewPage = function() {
        return {};
    }

    var getNewPageVersion = function () {
        return {};
    }

    var createPage = function (page, onsuccess) {
        ns.cmsmanager.crudService.createPage(
            { body: page }, 
            function (response) {
                pages[response.elementId] = response;
                if (onsuccess != undefined) onsuccess(response);
            });
    }

    var createPageVersion = function (pageVersion, onsuccess) {
        /*
        ns.cmsmanager.crudService.createPageVersion(
            page,
            function (response) {
                pageVersions[response.elementVersionId] = response;
                if (onsuccess != undefined) onsuccess(response);
            });
        */
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

    var updatePage = function (originalPage, updatedPage, onsuccess) {
        var changes = findChanges(pageProperties, originalPage, updatedPage);
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
                var cached = pages[updatedPage.elementId];
                if (cached != undefined) {
                    copyObject(pageProperties, response, cached);
                }
                if (onsuccess != undefined) onsuccess(response);
            });
    }

    var deletePage = function (pageId, onsuccess) {
        ns.cmsmanager.crudService.deletePage(
            { id: pageId },
            function (response) {
                delete pages[pageId];
                if (onsuccess != undefined) onsuccess(response);
            });
    }

    exported.dispatcher.subscribe(function(message) {
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

        getEditablePageVersion: getEditablePageVersion,
        getNewPageVersion: getNewPageVersion,

        //createPageVersion: createPageVersion,
        //retrievePageVersion: retrievePageVersion,
        //updatePageVersion: updatePageVersion,
        //deletePageVersion: deletePageVersion,
    }
}();
