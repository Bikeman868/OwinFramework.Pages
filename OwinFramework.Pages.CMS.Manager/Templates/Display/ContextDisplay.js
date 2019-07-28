exported.context_display_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            websiteVersion: {},
            scenario: {}
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._context = context;
                vm._unsubscribeScenario = context.subscribe("segmentationScenario", function (scenario) { vm.scenario = scenario; });
                vm._unsubscribeWebsiteVersionId = managerContext.subscribe("websiteVersionId", function (value) {
                    exported.websiteVersionStore.retrieveRecord(
                        value,
                        function (websiteVersion) {
                            vm.websiteVersion = websiteVersion;
                        });
                });
                this.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeScenario != undefined) {
                    vm._unsubscribeScenario();
                    vm._unsubscribeScenario = null;
                }
                if (vm._unsubscribeWebsiteVersionId != undefined) {
                    vm._unsubscribeWebsiteVersionId();
                    vm._unsubscribeWebsiteVersionId = null;
                }
            }
        }
    });
}