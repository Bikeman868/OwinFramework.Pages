exported.viewStore = function() {
    var viewModels = {};
    var currentViewModel = {};

    var selectView = function (container, templatePath, elementId, buildViewModel) {
        if (currentViewModel[container] != undefined) {
            currentViewModel[container].hidden = true;
        }
        var viewModel = viewModels[elementId];
        if (viewModel != undefined) {
            viewModel.hidden = false;
            currentViewModel[container] = viewModel;
            return;
        }
        var viewContainer = document.getElementById(container);
        ns.templates.templateLibrary.getTemplate(
            viewContainer,
            templatePath,
            function () {
                viewModel = buildViewModel();
                viewModels[elementId] = viewModel;
                currentViewModel[container] = viewModel;
                viewModel.hidden = false;
            }
        );
    }

    return {
        pageEditor: function () { selectView("cms_edit_view", "/cms/manager/PageEditor", "page_editor", exported.page_editor_vm); },
        dispatcherLog: function () { selectView("cms_tool_view", "/cms/manager/DispatcherLog", "log_dispatcher", exported.dispatcher_log_vm); }
    }
}();
