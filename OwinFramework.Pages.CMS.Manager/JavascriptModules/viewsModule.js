﻿exported.viewStore = function() {
    var viewModels = {};
    var currentViewModel = {};

    var selectView = function (container, templatePath, elementId, buildViewModel) {
        if (currentViewModel[container] != undefined) {
            currentViewModel[container].visible = false;
        }
        var viewModel = viewModels[elementId];
        if (viewModel != undefined) {
            viewModel.visible = true;
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
                viewModel.visible = true;
            }
        );
    }

    // Selectors
    var pageSelector = function () { selectView("cms_select_view", "/cms/manager/PageSelector", "page_selector", exported.page_selector_vm); }
    var environmentSelector = function () { selectView("cms_select_view", "/cms/manager/EnvironmentSelector", "environment_selector", exported.environment_selector_vm); }

    // Editors
    var pageEditor = function () { selectView("cms_edit_view", "/cms/manager/PageEditor", "page_editor", exported.page_editor_vm); }

    // Tools
    var dispatcherLog = function () { selectView("cms_tool_view", "/cms/manager/DispatcherLog", "log_dispatcher", exported.dispatcher_log_vm); }

    var viewSelectChanged = function(e) {
        var viewName = (e.value || e.options[e.selectedIndex].value);
        if (viewName === "environments") environmentSelector();
        else if (viewName === "versions");
        else if (viewName === "pages") pageSelector();
        else if (viewName === "layouts");
        else if (viewName === "regions");
        else if (viewName === "components");
        else if (viewName === "dataScopes");
        else if (viewName === "dataTypes");
    }

    return {
        environmentSelector: environmentSelector,
        pageSelector: pageSelector,

        pageEditor: pageEditor,

        dispatcherLog: dispatcherLog,

        viewSelectChanged: viewSelectChanged
    }
}();
