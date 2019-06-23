exported.page_editor_vm = function() {
    return new Vue({
        el: "#cms_page_editor",
        data: {
            visible: true,
            mode: "view",
            namePattern: "",
            displayNamePattern: "",
            errors: [],
            originalPage: {},
            editingPage: {},
            currentPage: {}
        },
        created: function() {
            var vm = this;
            vm.namePattern = exported.validation.namePattern.source;
            vm.displayNamePattern = exported.validation.displayNamePattern.source;
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
                        function(msg) { vm.errors = [msg]});
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
                exported.validation.displayName(vm.editingPage.displayName, errors);
                exported.validation.name(vm.editingPage.name, errors);
                vm.errors = errors;
            }
        }
    });
}