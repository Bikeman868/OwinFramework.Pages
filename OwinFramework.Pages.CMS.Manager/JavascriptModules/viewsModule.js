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

    var root = create();

    return {
        root: root,
        create: create
    }
}();

exported.viewStore = function () {
    var viewModels = {};
    var currentViewModel = {};

    var selectView = function (container, templatePath, elementId, buildViewModel, context) {
        if (currentViewModel[container] != undefined) {
            currentViewModel[container].hide();
        }
        var viewModel = viewModels[elementId];
        if (viewModel != undefined) {
            viewModel.show(context);
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
                viewModel.show(context);
            }
        );
    }

    // Selectors
    var showPageSelector = function (context) { selectView("cms_select_view", "/cms/manager/PageSelector", "page_selector", exported.page_selector_vm, context); }
    var showEnvironmentSelector = function (context) { selectView("cms_select_view", "/cms/manager/EnvironmentSelector", "environment_selector", exported.environment_selector_vm, context); }

    // Editors
    var showPageEditor = function (context) { selectView("cms_edit_view", "/cms/manager/PageEditor", "page_editor", exported.page_editor_vm, context); }

    // Tools
    var showDispatcherLog = function (context) { selectView("cms_tool_view", "/cms/manager/DispatcherLog", "log_dispatcher", exported.dispatcher_log_vm, context); }

    var viewSelectChanged = function(e, context) {
        var viewName = (e.value || e.options[e.selectedIndex].value);
        if (viewName === "environments") showEnvironmentSelector(context);
        else if (viewName === "versions");
        else if (viewName === "pages") showPageSelector(context);
        else if (viewName === "layouts");
        else if (viewName === "regions");
        else if (viewName === "components");
        else if (viewName === "dataScopes");
        else if (viewName === "dataTypes");
    }

    return {
        showEnvironmentSelector: showEnvironmentSelector,
        showPageSelector: showPageSelector,

        showPageEditor: showPageEditor,

        showDispatcherLog: showDispatcherLog,

        viewSelectChanged: viewSelectChanged
    }
}();
