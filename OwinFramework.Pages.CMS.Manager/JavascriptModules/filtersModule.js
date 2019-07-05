﻿exported.filters = function() {
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

    Vue.filter("cms_formatUserUrn", function (value) {
        if (value) {
            return "user " + value.match(/[^:]*$/g); // TODO: Look up user display name
        }
        return "";
    });

    Vue.filter("cms_lookupElementVersionId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }

        var elementName = "element version #" + id;
        //exported.pageStore.retrieveRecord(
        //    id,
        //    function (page) {
        //        pageName = page.displayName;
        //    });
        return elementName;
    });

    Vue.filter("cms_lookupElementId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }

        var elementName = "element #" + id;
        //exported.pageStore.retrieveRecord(
        //    id,
        //    function (page) {
        //        pageName = page.displayName;
        //    });
        return elementName;
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
        //exported.pageStore.retrieveRecord(
        //    id,
        //    function (page) {
        //        pageName = page.displayName;
        //    });
        return layoutName;
    });

    Vue.filter("cms_lookupRegionId", function (value) {
        var id = parseInt(value);
        if (isNaN(id)) { return ""; }

        var regionName = "region #" + id;
        //exported.pageStore.retrieveRecord(
        //    id,
        //    function (page) {
        //        pageName = page.displayName;
        //    });
        return regionName;
    });
}