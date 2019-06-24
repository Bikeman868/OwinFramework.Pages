exported.environment_selector_vm = function() {
    return new Vue({
        el: "#cms_environment_selector",
        data: {
            visible: true,
            environments: []
        },
        methods: {
            show: function (context) { this.visible = true; },
            hide: function () { this.visible = false; },
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