exported.component_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            components: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                exported.componentStore.retrieveAllRecords(
                    function (components) { vm.components = components; });
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
            },
            selectComponent: function(componentId) {
                var vm = this;
                vm._context.selected("componentId", componentId);
            }
        }
    });
}