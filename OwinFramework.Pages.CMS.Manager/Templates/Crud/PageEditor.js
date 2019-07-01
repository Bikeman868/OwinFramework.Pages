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
            show: function (context, managerContext) {
                var vm = this;
                vm._unsubscribePageId = context.subscribe("pageId", function (pageId) {
                    if (pageId == undefined) {
                        vm.currentPage = null;
                    } else {
                        exported.pageStore.retrievePage(
                            pageId,
                            function (page) { vm.currentPage = page; });
                    }
                });
                vm._unsubscribeWebsiteVersionId = managerContext.subscribe("websiteVersionId", function (value) {
                    vm.websiteVersionId = value;
                });
                this.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeWebsiteVersionId != undefined) {
                    vm._unsubscribeWebsiteVersionId();
                    vm._unsubscribeWebsiteVersionId = null;
                }
                if (vm._unsubscribePageId != undefined) {
                    vm._unsubscribePageId();
                    vm._unsubscribePageId = null;
                }
            },
            newPage: function() {
                var vm = this;
                vm.errors = [];
                vm.editingPage = exported.pageStore.getNewPage();
                vm.mode = "new";
            },
            editPage: function() {
                var vm = this;
                vm.errors = [];
                vm.editingPage = exported.pageStore.getEditablePage(vm.currentPage);
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
                    exported.pageStore.updatePage(
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
                    exported.pageStore.createPage(
                        vm.editingPage,
                        vm.websiteVersionId,
                        function() { vm.mode = "view"; },
                        function(msg) { vm.errors = [msg]});
                }
            },
            confirmDelete: function() {
                var vm = this;
                exported.pageStore.deletePage(
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
                exported.validation.displayName(vm.editingPage.displayName, "display name", errors);
                exported.validation.name(vm.editingPage.name, "name", errors);
                vm.errors = errors;
            }
        }
    });
}