exported.init = function() {
    Vue.filter("cms_formatDate", function(value) {
        if (value) {
            var date = new Date(value);

            var year = date.getFullYear() + "";

            var month = date.getMonth() + 1;
            if (month < 10) month = "0" + month;
            else month = month + "";

            var day = date.getDate();
            if (day < 10) day = "0" + day;
            else day = day + "";

            var hours = date.getHours();
            if (hours < 10) hours = "0" + hours;
            else hours = hours + "";

            var minutes = date.getMinutes();
            if (minutes < 10) minutes = "0" + minutes;
            else minutes = minutes + "";

            return year + "-" + month + "-" + day + " " + hours + ":" + minutes;
        }
        return "";
    });

    Vue.filter("cms_formatUserUrn", function(value) {
        if (value) {
            return "user " + value.match(/[^:]*$/g); // TODO: Look up user display name
        }
        return "";
    });

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
                    ns.cmsmanager.viewStore.viewSelectChanged(e.target, this._context, this._managerContext);
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
            { name: "environments", displayName: "Environments", selected: true }
        ]);
    topAreaSelector.show(topContext, topContext);

    var bottomAreaSelector = areaSelectorVm(
        "cms_bottom_area_selector",
        [
                { name: "pages", displayName: "Pages", selected: true },
                { name: "layouts", displayName: "Layouts" },
                { name: "regions", displayName: "Regions" },
                { name: "components", displayName: "Components" },
                { name: "dataScopes", displayName: "Scopes" },
                { name: "dataTypes", displayName: "Types" }
        ]);
    bottomAreaSelector.show(bottomContext, topContext);

    exported.viewStore.showEnvironmentSelector(bottomContext, topContext);
    exported.viewStore.showPageSelector(bottomContext, topContext);
    exported.viewStore.showPageEditor(bottomContext, topContext);
    exported.viewStore.showEnvironmentEditor(bottomContext, topContext);
    exported.viewStore.showDispatcherLog(bottomContext, topContext);
}