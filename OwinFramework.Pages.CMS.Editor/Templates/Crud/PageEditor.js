var page_editor = function() {
    return new Vue({
        el: "#cms_page_editor",
        data: {
            mode: "view",
            originalPage: {},
            editingPage: {},
            currentPage: {}
        },
        created: function() {
            var vm = this;
            ns.cmseditor.pageStore.retrievePage(
                12,
                function(page) { vm.currentPage = page; });
        },
        methods: {
            newPage: function() {
                var vm = this;
                vm.editingPage = ns.cmseditor.pageStore.getNewPage();
                vm.mode = "new";
            },
            editPage: function() {
                var vm = this;
                vm.editingPage = ns.cmseditor.pageStore.getEditablePage(vm.currentPage);
                Object.assign(vm.originalPage, vm.editingPage);
                vm.mode = "edit";
            },
            deletePage: function() {
                var vm = this;
                vm.mode = "delete";
            },
            saveChanges: function() {
                var vm = this;
                ns.cmseditor.pageStore.updatePage(
                    vm.originalPage,
                    vm.editingPage,
                    function() { vm.mode = "view"; });
            },
            createNew: function() {
                var vm = this;
                ns.cmseditor.pageStore.createPage(
                    vm.editingPage,
                    function() { vm.mode = "view"; });
            },
            confirmDelete: function() {
                var vm = this;
                ns.cmseditor.pageStore.deletePage(
                    vm.currentPage.elementId,
                    function() {
                        vm.currentPage = null;
                        vm.mode = "view";
                    });
            },
            cancelChanges: function() {
                this.mode = "view";
            }
        }
    });
}