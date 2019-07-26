exported.user_segment_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            userSegments: []
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
                exported.listService.userSegments(
                    {},
                    function (response) {
                        vm.userSegments = response;
                        vm.userSegments.unshift({
                            key: null,
                            name: "Everyone else",
                            description: "All other users"
                        });
                    });
            },
            selectUserSegment: function (key) {
                var vm = this;
                var segment = null;
                for (let i = 0; i < vm.userSegments.length; i++) {
                    if (vm.userSegments[i].key === key)
                        segment = vm.userSegments[i];
                }
                vm._context.selected("userSegment", segment);
            }
        }
    });
}