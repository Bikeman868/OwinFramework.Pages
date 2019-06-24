exported.page_selector_vm = function() {
    return new Vue({
        el: "#cms_page_selector",
        data: {
            visible: true,
            websiteVersionId: 1,
            pages: []
        },
        methods: {
            show: function (context) {
                var vm = this;
                vm.visible = true;
                vm._context = context;
                vm._unsubscribe = context.subscribe("websiteVersionId", function (value) {
                    vm.websiteVersionId = value;
                    vm.refresh();
                });
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribe != undefined) {
                    vm._unsubscribe();
                    vm._unsubscribe = null;
                }
            },
            refresh: function() {
                var vm = this;
                if (vm.websiteVersionId == undefined) {
                    vm.pages = [];
                } else {
                    exported.websiteVersionStore.getPages(
                        vm.websiteVersionId,
                        function(response) { vm.pages = response; });
                }
            }
        }
    });
}