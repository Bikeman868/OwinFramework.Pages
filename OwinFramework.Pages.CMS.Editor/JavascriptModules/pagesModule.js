var pagesData = function() {
    var pages = [];

    var getPage = function (pageId, onsuccess) {
        ns.cmseditor.crudService.getPage({ pageId: pageId }, onsuccess);
    }

    var newPage = function (page, onsuccess) {
        ns.cmseditor.crudService.newPage(page, onsuccess);
    }

    var updatePage = function (page, onsuccess) {
        ns.cmseditor.crudService.updatePage(page, onsuccess);
    }

    var deletePage = function (page, onsuccess) {
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

exported.pagesData = pagesData;