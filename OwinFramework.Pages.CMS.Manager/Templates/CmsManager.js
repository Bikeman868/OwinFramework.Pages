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
        
    exported.viewStore.pageSelector();
    exported.viewStore.pageEditor();
    exported.viewStore.dispatcherLog();
}