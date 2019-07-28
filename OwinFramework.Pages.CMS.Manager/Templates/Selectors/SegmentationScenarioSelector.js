exported.segmentation_scenario_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            scenarios: []
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
            },
            refresh: function () {
                var vm = this;
                exported.segmentScenarioStore.retrieveAllRecords(
                    function(response) {
                        vm.scenarios = response;
                        vm.scenarios.unshift(null);
                    });
            },
            selectScenario: function (name) {
                var vm = this;
                var scenario = null;
                if (name) {
                    for (let i = 1; i < vm.scenarios.length; i++) {
                        if (vm.scenarios[i].name === name)
                            scenario = vm.scenarios[i];
                    }
                }
                vm._context.selected("segmentationScenario", scenario);
            }
        }
    });
}