exported.environment_selector_vm = function() {
    return new Vue({
        el: "#cms_environment_selector",
        data: {
            visible: true,
            environments: []
        },
        methods: {
            show: function (context) {
                var vm = this;
                vm.visible = true;
                vm._context = context;
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