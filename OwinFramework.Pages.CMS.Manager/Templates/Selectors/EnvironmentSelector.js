exported.environment_selector_vm = function() {
    return new Vue({
        el: "#cms_environment_selector",
        data: {
            visible: true,
            environments: []
        },
        methods: {
        },
        created: function() {
            this.environments = [
                { displayName: "Production" },
                { displayName: "Staging" },
                { displayName: "Test" },
                { displayName: "Integration" }
            ];
        }
    });
}