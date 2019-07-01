exported.page_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            pages: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                exported.pageStore.getAllPages(
                    function (pages) { vm.pages = pages; });
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
            },
            selectPage: function(pageId) {
                var vm = this;
                vm._context.selected("pageId", pageId);
            }
        }
    });
}