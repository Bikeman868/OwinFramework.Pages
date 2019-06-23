exported.page_editor_vm = function() {
    return new Vue({
        el: "#cms_page_editor",
        data: {
            visible: true,
            mode: "view",
            errors: [],
            originalPage: {},
            editingPage: {},
            currentPage: {}
        },
        created: function() {
            var vm = this;
            ns.cmsmanager.pageStore.retrievePage(
                12,
                function(page) { vm.currentPage = page; });
        },
        methods: {
            newPage: function() {
                var vm = this;
                vm.errors = [];
                vm.editingPage = ns.cmsmanager.pageStore.getNewPage();
                vm.mode = "new";
            },
            editPage: function() {
                var vm = this;
                vm.errors = [];
                vm.editingPage = ns.cmsmanager.pageStore.getEditablePage(vm.currentPage);
                Object.assign(vm.originalPage, vm.editingPage);
                vm.mode = "edit";
            },
            deletePage: function() {
                var vm = this;
                vm.mode = "delete";
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    ns.cmsmanager.pageStore.updatePage(
                        vm.originalPage,
                        vm.editingPage,
                        function() { vm.mode = "view"; },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    ns.cmsmanager.pageStore.createPage(
                        vm.editingPage,
                        function() { vm.mode = "view"; },
                        function(msg) { vm.errors=[msg]});
                }
            },
            confirmDelete: function() {
                var vm = this;
                ns.cmsmanager.pageStore.deletePage(
                    vm.currentPage.elementId,
                    function() {
                        vm.currentPage = null;
                        vm.mode = "view";
                    });
            },
            cancelChanges: function() {
                this.mode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                if (vm.editingPage.displayName == undefined || vm.editingPage.displayName.length < 3)
                    errors.push("Page display name must be at least 3 characters");
                if (vm.editingPage.name == undefined || vm.editingPage.name.length < 2)
                    errors.push("Page name must be at least 2 characters");
                else {
                    var nameRegwx = new RegExp("^[a-zA-Z][0-9a-zA-Z_]*$");
                    if (!nameRegwx.test(vm.editingPage.name))
                        errors.push("Page name can only contain letters, numbers and underscore. The first character must be a letter");
                }
                vm.errors = errors;
            }
        }
    });
}