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

    var areaSelectorVm = new Vue({
        el: "#cms_area_selector",
        data: {
            areas: [
                { name: "versions", displayName: "Website versions" },
                { name: "environments", displayName: "Environments" },
                { name: "pages", displayName:"Pages", selected: true },
                { name: "layouts", displayName:"Layouts" },
                { name: "regions", displayName:"Regions" },
                { name: "components", displayName:"Components" },
                { name: "dataScopes", displayName:"Scopes" },
                { name: "dataTypes", displayName:"Types" }
            ]
        },
        methods: {
            show: function (context) { this._context = context; },
            selectArea: function (e) { ns.cmsmanager.viewStore.viewSelectChanged(e.target, this._context); }
        }
    });

    var managerContext = exported.selectionContext.create();
    var viewContext = exported.selectionContext.create();

    var websiteVersionSelectorVm = exported.website_version_selector_vm("cms_website_version_dropdown_selector");
    websiteVersionSelectorVm.show(managerContext, managerContext);

    areaSelectorVm.show(viewContext, managerContext);

    managerContext.selected("websiteVersionId", 1);

    exported.viewStore.showPageSelector(viewContext, managerContext);
    exported.viewStore.showPageEditor(viewContext, managerContext);
    exported.viewStore.showDispatcherLog(viewContext, managerContext);
}