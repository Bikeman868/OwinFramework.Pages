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
                var websiteVersionId = null;
                for (let i = 0; i < vm.environments.length; i++) {
                    var environment = vm.environments[i];
                    if (environment.environmentId === environmentId) {
                        websiteVersionId = environment.websiteVersionId;
                    }
                }
                if (websiteVersionId != undefined)
                    vm._context.selected("websiteVersionId", websiteVersionId);
                vm._context.selected("environmentId", environmentId);
            }
        }
    });
}