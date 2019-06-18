exported.viewStore = function() {
    var views = {};
    var viewModels = {};

    var getView = function (templatePath, elementId, buildViewModel, onSuccess)
    {
        var view = views[elementId];
        if (view != undefined) {
            onSuccess(view);
            return;
        }

        var parent = document.getElementById("cms_views");
        ns.templates.templateLibrary.getTemplate(
            parent,
            templatePath,
            function() {
                view = document.getElementById(elementId);
                views[elementId] = view;
                viewModels[elementId] = buildViewModel();
                onSuccess(view);
            }
        );
    }

    var pageEditor = function (onSuccess) {
        getView("/cms/editor/PageEditor", "cms_page_editor", exported.page_editor_vm, onSuccess);
    }

    return {
        pageEditor: pageEditor
    }
}();
