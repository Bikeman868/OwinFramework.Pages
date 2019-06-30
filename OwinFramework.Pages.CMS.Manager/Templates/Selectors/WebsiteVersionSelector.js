exported.website_version_selector_vm = function(eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            websiteVersions: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                vm.refresh();
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeDispatcher != undefined) {
                    vm._unsubscribeDispatcher();
                    vm._unsubscribeDispatcher = null;
                }
            },
            refresh: function () {
                var vm = this;
                exported.websiteVersionStore.getWebsiteVersions(
                    function (response) {
                        vm.websiteVersions = response;
                    });
            },
            selectWebsiteVersion: function (websiteVersionId) {
                var vm = this;
                vm._context.selected("websiteVersionId", websiteVersionId);
            },
            selectDropdown: function (e) {
                var vm = this;
                var dropdown = e.target;
                var websiteVersionId = (dropdown.value || dropdown.options[dropdown.selectedIndex].value);
                if (websiteVersionId != undefined) {
                    vm.selectWebsiteVersion(parseInt(websiteVersionId));
                }
            }
        }
    });
}