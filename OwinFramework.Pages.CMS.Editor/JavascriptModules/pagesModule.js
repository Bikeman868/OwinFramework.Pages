var pageStore = function() {
    var pages = [];
    var pageVersions = [];

    var getPage = function (pageId, onsuccess) {
        // See if we already have the page first
        ns.cmseditor.crudService.getPage({ pageId: pageId }, onsuccess);
    }

    var newPage = function (page, onsuccess) {
        ns.cmseditor.crudService.newPage(page, onsuccess);
        // Add new page to the pages collection
    }

    var updatePage = function (page, onsuccess) {
        ns.cmseditor.crudService.updatePage(page, onsuccess);
    }

    var deletePage = function (page, onsuccess) {
        // Remove the page from the pages collection
        ns.cmseditor.crudService.deletePage(page, onsuccess);
    }

    return {
        pages: pages,
        getPage: getPage,
        newPage: newPage,
        updatePage: updatePage,
        deletePage: deletePage
    }
}();

exported.pageStore = pageStore;