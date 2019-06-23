exported.page_selector_vm = function() {
    return new Vue({
        el: "#cms_page_selector",
        data: {
            visible: true,
            pages: []
        },
        methods: {
        },
        created: function () {
            var vm = this;
            exported.websiteVersionStore.getPages(
                1,
                function(response) {
                    vm.pages = response;
                });
        }
    });
}