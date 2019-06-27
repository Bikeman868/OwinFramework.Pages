exported.environment_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            environments: []
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
                        vm.refresh(); // TODO: we only care about environments being added/deleted
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
                exported.environmentStore.getEnvironments(
                    function (response) {
                        vm.environments = response;
                    });
            },
            selectEnvironment: function (environmentId) {
                var vm = this;
                vm._context.selected("environmentId", environmentId);
            }
        }
    });
}