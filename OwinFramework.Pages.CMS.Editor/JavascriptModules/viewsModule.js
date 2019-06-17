var viewStore = function() {
    var views = {};

    var getView = function(templatePath, elementId, onSuccess)
    {
        var view = views[elementId];
        if (view != undefined) {
            onSuccess(view);
            return;
        }

        var parent = document.getElementById("cms_view");
        ns.templates.templateLibrary.getTemplate(parent, templatePath, function() {
            view = document.getElementById(elementId);
            views[elementId] = view;
            onSuccess(view);
        });
    }

    var pageEditor = function (onSuccess) {
        getView("/cms/editor/PageEditor", "cms_page_editor", onSuccess);
    }

    return {
        pageEditor: pageEditor
    }
}();

exported.viewStore = viewStore;