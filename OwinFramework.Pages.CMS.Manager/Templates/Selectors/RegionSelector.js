exported.region_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            regions: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                exported.regionStore.retrieveAllRecords(
                    function (regions) { vm.regions = regions; });
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
            },
            selectRegion: function(regionId) {
                var vm = this;
                vm._context.selected("regionId", regionId);
            }
        }
    });
}