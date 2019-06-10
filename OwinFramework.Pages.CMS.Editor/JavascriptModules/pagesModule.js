﻿var pageStore = function() {
    var pages = {};
    var pageVersions = {};

    var pageProperties = ["id", "title", "description"];
    var pageVersionProperties = ["id", "title", "description"];

    var copyObject = function (properties, from, to) {
        to = to || {};
        properties.forEach(function (prop) {
            to[prop] = from[prop];
        });
        return to;
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
        ns.cmseditor.crudService.createPage(
            page, 
            function (response) {
                pages[response.id] = response;
                if (onsuccess != undefined) onsuccess(response);
            });
    }

    var createPageVersion = function (pageVersion, onsuccess) {
        /*
        ns.cmseditor.crudService.createPageVersion(
            page,
            function (response) {
                pageVersions[response.id] = response;
                if (onsuccess != undefined) onsuccess(response);
            });
        */
    }

    var retrievePage = function (pageId, onsuccess) {
        var page = pages[pageId];
        if (page == undefined) {
            ns.cmseditor.crudService.retrievePage(
                { pageId: pageId },
                function (response) {
                    pages[response.id] = response;
                    if (onsuccess != undefined) onsuccess(response);
                });
        } else {
            if (onsuccess != undefined) onsuccess(page);
        }
    }

    var updatePage = function (page, onsuccess) {
        var original = pages[page.id];
        ns.cmseditor.crudService.updatePage(
            page, 
            function (response) {
                if (original != undefined) copyObject(pageProperties, response, original);
                if (onsuccess != undefined) onsuccess(response);
            });
    }

    var deletePage = function (page, onsuccess) {
        ns.cmseditor.crudService.deletePage(
            page,
            function (response) {
                delete pages[page.id];
                if (onsuccess != undefined) onsuccess(response);
            });
    }

    dispatcher.subscribe(function(message) {
        if (message.propertyChanges != undefined) {
            for (let i = 0; i < message.propertyChanges.length; i++) {
                var propertyChange = message.propertyChanges[i];
                if (propertyChange.elementType === "page") {
                    var page = pages[propertyChange.versionId];
                    if (page != undefined) {
                        page[propertyChange.name] = propertyChange.value;
                    }
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

exported.pageStore = pageStore;