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

    var selectView = function (container, templatePath, elementId, buildViewModel, childContext, parentContext) {
        if (currentViewModel[container] != undefined) {
            currentViewModel[container].hide();
        }
        var viewModel = viewModels[elementId];
        if (viewModel != undefined) {
            viewModel.show(childContext, parentContext);
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
                viewModel.show(childContext, parentContext);
            }
        );
    }

    // Selectors
    var showPageSelector = function(childContext, parentContext) {
        selectView("cms_select_view", "/cms/manager/PageSelector", "cms_page_selector", exported.page_selector_vm, childContext, parentContext);
    }
    var showEnvironmentSelector = function(childContext, parentContext) {
        selectView("cms_select_view", "/cms/manager/EnvironmentSelector", "cms_environment_selector", exported.environment_selector_vm, childContext, parentContext);
    }
    var showWebsiteVersionSelector = function(childContext, parentContext) {
        selectView("cms_select_view", "/cms/manager/WebsiteVersionListSelector", "cms_website_version_list_selector", exported.website_version_selector_vm, childContext, parentContext);
    }

    // Editors
    var showPageEditor = function(context) {
         selectView("cms_edit_view", "/cms/manager/PageEditor", "cms_page_editor", exported.page_editor_vm, context);
    }

    // Tools
    var showDispatcherLog = function(context) {
         selectView("cms_tool_view", "/cms/manager/DispatcherLog", "cms_dispatcher_log", exported.dispatcher_log_vm, context);
    }

    var viewSelectChanged = function(e, childContext, parentContext) {
        var viewName = (e.value || e.options[e.selectedIndex].value);
        if (viewName === "environments") showEnvironmentSelector(childContext, parentContext);
        else if (viewName === "versions") showWebsiteVersionSelector(childContext, parentContext);
        else if (viewName === "pages") showPageSelector(childContext, parentContext);
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
