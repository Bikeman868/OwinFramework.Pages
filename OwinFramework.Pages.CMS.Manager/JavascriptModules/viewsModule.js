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

    var selectView = function (container, templatePath, elementId, buildViewModel, context, topContext) {
        if (currentViewModel[container] != undefined) {
            currentViewModel[container].hide();
        }
        var viewModel = viewModels[elementId];
        if (viewModel != undefined) {
            viewModel.show(context, topContext);
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
                viewModel.show(context, topContext);
            }
        );
    }

    // Selectors
    var showWebsiteVersionSelector = function (context, topContext) {
        selectView("cms_top_select_view", "/cms/manager/WebsiteVersionSelector", "cms_website_version_selector", exported.website_version_selector_vm, topContext, topContext);
    }
    var showEnvironmentSelector = function (context, topContext) {
        selectView("cms_top_select_view", "/cms/manager/EnvironmentSelector", "cms_environment_selector", exported.environment_selector_vm, topContext, topContext);
    }
    var showPageSelector = function(context, topContext) {
        selectView("cms_bottom_select_view", "/cms/manager/PageSelector", "cms_page_selector", exported.page_selector_vm, context, topContext);
    }
    var showLayoutSelector = function (context, topContext) {
        selectView("cms_bottom_select_view", "/cms/manager/LayoutSelector", "cms_layout_selector", exported.layout_selector_vm, context, topContext);
    }
    var showRegionSelector = function (context, topContext) {
        selectView("cms_bottom_select_view", "/cms/manager/RegionSelector", "cms_region_selector", exported.region_selector_vm, context, topContext);
    }
    var showComponentSelector = function (context, topContext) {
        selectView("cms_bottom_select_view", "/cms/manager/ComponentSelector", "cms_component_selector", exported.component_selector_vm, context, topContext);
    }
    var showDataScopeSelector = function (context, topContext) {
        selectView("cms_bottom_select_view", "/cms/manager/DataScopeSelector", "cms_data_scope_selector", exported.scope_selector_vm, context, topContext);
    }
    var showDataTypeSelector = function (context, topContext) {
        selectView("cms_bottom_select_view", "/cms/manager/DataTypeSelector", "cms_data_type_selector", exported.type_selector_vm, context, topContext);
    }

    // Editors
    var showEnvironmentEditor = function (context, topContext) {
        selectView("cms_top_edit_view", "/cms/manager/EnvironmentEditor", "cms_environment_editor", exported.environment_editor_vm, topContext, topContext);
    }
    var showWebsiteVersionEditor = function (context, topContext) {
        selectView("cms_top_edit_view", "/cms/manager/WebsiteVersionEditor", "cms_website_version_editor", exported.website_version_editor_vm, topContext, topContext);
    }
    var showPageEditor = function (context, topContext) {
        selectView("cms_bottom_edit_view", "/cms/manager/PageEditor", "cms_page_editor", exported.page_editor_vm, context, topContext);
    }
    var showLayoutEditor = function (context, topContext) {
        selectView("cms_bottom_edit_view", "/cms/manager/LayoutEditor", "cms_layout_editor", exported.layout_editor_vm, context, topContext);
    }
    var showRegionEditor = function (context, topContext) {
        selectView("cms_bottom_edit_view", "/cms/manager/RegionEditor", "cms_region_editor", exported.region_editor_vm, context, topContext);
    }
    var showComponentEditor = function (context, topContext) {
        selectView("cms_bottom_edit_view", "/cms/manager/ComponentEditor", "cms_component_editor", exported.component_editor_vm, context, topContext);
    }
    var showDataScopeEditor = function (context, topContext) {
        selectView("cms_bottom_edit_view", "/cms/manager/DataScopeEditor", "cms_data_scope_editor", exported.data_scope_editor_vm, context, topContext);
    }
    var showDataTypeEditor = function (context, topContext) {
        selectView("cms_bottom_edit_view", "/cms/manager/DataTypeEditor", "cms_data_type_editor", exported.data_type_editor_vm, context, topContext);
    }

    // Tools
    var showDispatcherLog = function (context, topContext) {
        selectView("cms_tool_view", "/cms/manager/DispatcherLog", "cms_dispatcher_log", exported.dispatcher_log_vm, context, topContext);
    }

    var viewSelectChanged = function(e, context, managerContext) {
        var viewName = (e.value || e.options[e.selectedIndex].value);
        if (viewName === "environments") {
            showEnvironmentSelector(managerContext, managerContext);
            showEnvironmentEditor(managerContext, managerContext);
        }
        else if (viewName === "versions") {
            showWebsiteVersionSelector(managerContext, managerContext);
            showWebsiteVersionEditor(managerContext, managerContext);
        }
        else if (viewName === "pages") {
            showPageSelector(context, managerContext);
            showPageEditor(context, managerContext);
        }
        else if (viewName === "layouts") {
            showLayoutSelector(context, managerContext);
        }
        else if (viewName === "regions") {
            showRegionSelector(context, managerContext);
        }
        else if (viewName === "components") {
            showComponentSelector(context, managerContext);
        }
        else if (viewName === "dataScopes") {
            showDataScopeSelector(context, managerContext);
        }
        else if (viewName === "dataTypes") {
            showDataTypeSelector(context, managerContext);
        }
    }

    return {
        showEnvironmentEditor: showEnvironmentEditor,
        showWebsiteVersionEditor: showWebsiteVersionEditor,
        showEnvironmentSelector: showEnvironmentSelector,
        showWebsiteVersionSelector: showWebsiteVersionSelector,
        showPageSelector: showPageSelector,
        showLayoutSelector: showLayoutSelector,
        showRegionSelector: showRegionSelector,
        showComponentSelector: showComponentSelector,
        showDataScopeSelector: showDataScopeSelector,
        showDataTypeSelector: showDataTypeSelector,

        showPageEditor: showPageEditor,
        showLayoutEditor: showLayoutEditor,
        showRegionEditor: showRegionEditor,
        showComponentEditor: showComponentEditor,
        showDataScopeEditor: showDataScopeEditor,
        showDataTypeEditor: showDataTypeEditor,

        showDispatcherLog: showDispatcherLog,

        viewSelectChanged: viewSelectChanged
    }
}();
