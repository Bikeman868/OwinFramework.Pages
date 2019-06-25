exported.page_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
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
        },
        methods: {
            show: function (context) {
                var vm = this;
                vm._context = context;
                vm._unsubscribe = context.subscribe("pageId", function (pageId) {
                    if (pageId == undefined) {
                        vm.currentPage = null;
                    } else {
                        ns.cmsmanager.pageStore.retrievePage(
                            pageId,
                            function (page) { vm.currentPage = page; });
                    }
                });
                this.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribe != undefined) {
                    vm._unsubscribe();
                    vm._unsubscribe = null;
                }
            },
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