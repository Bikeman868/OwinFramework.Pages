exported.selectionContext = function () {
    var create = function() {
        var ids = {};

        var subscribe = function(name, onchange) {
            var id = ids[name];
            if (id == undefined) {
                id = { subscribers:[], value: null };
                ids[name] = id;
            }
            id.subscribers.push(onchange);
            onchange(id.value);
            return function() {
                id.subscribers = id.subscribers.filter(function(e) { return e !== onchange; });
            };
        }

        var selected = function(name, value) {
            var id = ids[name];
            if (id == undefined) {
                id = { subscribers: [], value: null };
                ids[name] = id;
            }
            id.value = value;
            for (let i = 0; i < id.subscribers.length; i++) {
                id.subscribers[i](value);
            }
        }

        return {
            subscribe: subscribe,
            selected: selected
        }
    }

    return {
        create: create
    }
}();

exported.viewStore = function () {
    var viewModels = {};
    var currentViewModel = {};

    var selectView = function (container, templatePath, elementId, buildViewModel, context, managerContext) {
        if (currentViewModel[container] != undefined) {
            currentViewModel[container].hide();
        }
        var viewModel = viewModels[elementId];
        if (viewModel != undefined) {
            viewModel.show(context, managerContext);
            currentViewModel[container] = viewModel;
            return;
        }
        var viewContainer = document.getElementById(container);
        ns.templates.templateLibrary.getTemplate(
            viewContainer,
            templatePath,
            function () {
                viewModel = buildViewModel(elementId);
                viewModels[elementId] = viewModel;
                currentViewModel[container] = viewModel;
                viewModel.show(context, managerContext);
            }
        );
    }

    // Selectors
    var showPageSelector = function(context, managerContext) {
        selectView("cms_select_view", "/cms/manager/PageSelector", "cms_page_selector", exported.page_selector_vm, context, managerContext);
    }
    var showEnvironmentSelector = function(context, managerContext) {
        selectView("cms_select_view", "/cms/manager/EnvironmentSelector", "cms_environment_selector", exported.environment_selector_vm, context, managerContext);
    }
    var showWebsiteVersionSelector = function (context, managerContext) {
        selectView("cms_select_view", "/cms/manager/WebsiteVersionListSelector", "cms_website_version_list_selector", exported.website_version_selector_vm, context, managerContext);
    }

    // Editors
    var showPageEditor = function (context, managerContext) {
        selectView("cms_edit_view", "/cms/manager/PageEditor", "cms_page_editor", exported.page_editor_vm, context, managerContext);
    }

    // Tools
    var showDispatcherLog = function (context, managerContext) {
        selectView("cms_tool_view", "/cms/manager/DispatcherLog", "cms_dispatcher_log", exported.dispatcher_log_vm, context, managerContext);
    }

    var viewSelectChanged = function(e, context, managerContext) {
        var viewName = (e.value || e.options[e.selectedIndex].value);
        if (viewName === "environments") showEnvironmentSelector(context, managerContext);
        else if (viewName === "versions") showWebsiteVersionSelector(context, managerContext);
        else if (viewName === "pages") showPageSelector(context, managerContext);
        else if (viewName === "layouts");
        else if (viewName === "regions");
        else if (viewName === "components");
        else if (viewName === "dataScopes");
        else if (viewName === "dataTypes");
    }

    return {
        showEnvironmentSelector: showEnvironmentSelector,
        showWebsiteVersionSelector: showWebsiteVersionSelector,
        showPageSelector: showPageSelector,

        showPageEditor: showPageEditor,

        showDispatcherLog: showDispatcherLog,

        viewSelectChanged: viewSelectChanged
    }
}();
