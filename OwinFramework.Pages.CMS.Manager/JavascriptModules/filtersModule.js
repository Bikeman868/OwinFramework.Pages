exported.filters = function() {
    Vue.filter("cms_formatDateTime", function (value) {
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

    Vue.filter("cms_formatDate", function (value) {
        if (value) {
            var date = new Date(value);

            var year = date.getFullYear() + "";

            var month = date.getMonth() + 1;
            if (month < 10) month = "0" + month;
            else month = month + "";

            var day = date.getDate();
            if (day < 10) day = "0" + day;
            else day = day + "";

            return year + "-" + month + "-" + day;
        }
        return "";
    });

    Vue.filter("cms_lowercase", function (value) {
        if (value == undefined) return "";
        return value.replace(/([a-z0-9]|(?=[A-Z]))([A-Z])/g, "$1 $2").toLowerCase();
    })

    Vue.filter("cms_formatUserUrn", function (value) {
        var name = value || "";
        if (value) exported.userStore.retrieveRecord(value, function(user) { name = user.name });
        return name;
    });

    Vue.filter("cms_lookupWebsiteVersionId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }
        var websiteVersionName = "website version #" + id;
        exported.websiteVersionStore.retrieveRecord(
            id,
            function (websiteVersion) {
                websiteVersionName = websiteVersion.displayName;
            });
        return websiteVersionName;
    });

    Vue.filter("cms_lookupScenarioName", function (value) {
        if (value == undefined) { return "unsegmented users"; }
        var scenarioName = "'" + value + "' scenario";
        exported.segmentScenarioStore.retrieveRecord(
            value,
            function (scenario) {
                scenarioName = scenario.displayName;
            });
        return scenarioName;
    });

    Vue.filter("cms_lookupElementVersionId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }
        return "element version #" + id;
    });

    Vue.filter("cms_lookupElementId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }
        return "element #" + id;
    });

    Vue.filter("cms_lookupPageId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }

        var pageName = "page #" + id;
        exported.pageStore.retrieveRecord(
            id,
            function (page) {
                pageName = page.displayName;
            });
        return pageName;
    });

    Vue.filter("cms_lookupLayoutId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }

        var layoutName = "layout #" + id;
        //exported.layoutStore.retrieveRecord(
        //    id,
        //    function (layout) {
        //        layoutName = layout.displayName;
        //    });
        return layoutName;
    });

    Vue.filter("cms_lookupRegionId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }

        var regionName = "region #" + id;
        //exported.regionStore.retrieveRecord(
        //    id,
        //    function (region) {
        //        regionName = region.displayName;
        //    });
        return regionName;
    });

    Vue.filter("cms_lookupComponentId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }

        var componentName = "component #" + id;
        //exported.componentStore.retrieveRecord(
        //    id,
        //    function (component) {
        //        componentName = component.displayName;
        //    });
        return componentName;
    });
}