exported.website_version_selector_vm = function(eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            versions: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                vm._unsubscribeDispatcher = exported.dispatcher.subscribe(function (message) {
                    if ((message.newElements && message.newElements.length > 0) ||
                        (message.deletedElements && message.deletedElements.length > 0)) {
                        vm.refresh(); // TODO: we only care about website versions being added/deleted
                    }
                });
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
                        vm.versions = response;
                    });
            },
            selectVersion: function (websiteVersionId) {
                var vm = this;
                vm._context.selected("websiteVersionId", websiteVersionId);
            },
            selectDropdown: function (e) {
                var vm = this;
                var dropdown = e.target;
                var websiteVersionId = (dropdown.value || dropdown.options[dropdown.selectedIndex].value);
                if (websiteVersionId != undefined) {
                    vm.selectVersion(parseInt(websiteVersionId));
                }
            }
        }
    });
}