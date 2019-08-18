exported.layout_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            layouts: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                exported.layoutStore.retrieveAllRecords(
                    function (layouts) { vm.layouts = layouts; });
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
            },
            selectLayout: function(layoutId) {
                var vm = this;
                vm._context.selected("layoutId", layoutId);
            }
        }
    });
}