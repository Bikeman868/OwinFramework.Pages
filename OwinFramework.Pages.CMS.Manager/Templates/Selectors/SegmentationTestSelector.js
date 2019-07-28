exported.segmentation_test_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            tests: []
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
                exported.segmentTestStore.retrieveAllRecords(
                    function (response) { vm.tests = response; });
            },
            selectTest: function (name) {
                var vm = this;
                var test = null;
                for (let i = 0; i < vm.tests.length; i++) {
                    if (vm.tests[i].name === name)
                        test = vm.tests[i];
                }
                vm._context.selected("segmentationTest", test);
            }
        }
    });
}