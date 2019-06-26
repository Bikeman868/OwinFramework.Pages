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
                vm.visible = true;
            },
            hide: function () { this.visible = false; },
            selectEnvironment: function (environmentId) {
                var vm = this;
                vm._context.selected("environmentId", environmentId);
            }
        },
        created: function() {
            this.environments = [
                { elementId: 1, displayName: "Production" },
                { elementId: 2, displayName: "Staging" },
                { elementId: 3, displayName: "Test" },
                { elementId: 4, displayName: "Integration" }
            ];
        }
    });
}