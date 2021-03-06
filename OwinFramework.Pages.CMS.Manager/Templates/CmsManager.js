﻿exported.init = function() {
    exported.buildDataStores();
    exported.buildValidation();
    exported.buildFilters();
    exported.buildGenericComponents();
    exported.buildDisplayOnlyComponents();
    exported.buildFieldEditorComponents();
    exported.buildElementEditorComponents();
    exported.buildViewStore();

    var areaSelectorVm = function (eId, areas) {
        return new Vue({
            el: "#" + eId,
            data: {
                areas: areas
            },
            methods: {
                show: function(context, managerContext) {
                    this._context = context;
                    this._managerContext = managerContext;
                },
                selectArea: function(e) {
                    exported.viewStore.viewSelectChanged(e.target, this._context, this._managerContext);
                }
            }
        });
    }

    var topContext = exported.selectionContext.create();
    var bottomContext = exported.selectionContext.create();

    var topAreaSelector = areaSelectorVm(
        "cms_top_area_selector",
        [
            { name: "versions", displayName: "Website versions" },
            { name: "environments", displayName: "Environments", selected: true },
            { name: "scenarios", displayName: "A/B test scenarios" }
    ]);
    topAreaSelector.show(topContext, topContext);

    var bottomAreaSelector = areaSelectorVm(
        "cms_bottom_area_selector",
        [
            { name: "pages", displayName: "Pages", selected: true },
            { name: "layouts", displayName: "Layouts" },
            { name: "regions", displayName: "Regions" },
            { name: "components", displayName: "Components" },
            { name: "modules", displayName: "Modules" },
            { name: "dataScopes", displayName: "Scopes" },
            { name: "dataTypes", displayName: "Types" }
        ]);
    bottomAreaSelector.show(bottomContext, topContext);

    exported.viewStore.showEnvironmentSelector(bottomContext, topContext);
    exported.viewStore.showPageSelector(bottomContext, topContext);
    exported.viewStore.showPageEditor(bottomContext, topContext);
    exported.viewStore.showEnvironmentEditor(bottomContext, topContext);
    exported.viewStore.showUpdateNotifierLog(bottomContext, topContext);
    exported.viewStore.showContextDisplay(bottomContext, topContext);
}