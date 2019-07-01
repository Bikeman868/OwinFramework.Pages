exported.page_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            namePattern: "",
            displayNamePattern: "",
            websiteVersion: {},

            pageMode: "view",
            errors: [],
            originalPage: {},
            editingPage: {},
            currentPage: {},

            pageVersionMode: "view",
            versionErrors: [],
            originalPageVersion: {},
            editingPageVersion: {},
            currentPageVersion: {}
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
                    exported.websiteVersionStore.retrieveWebsiteVersion(
                        value,
                        function (websiteVersion) { vm.websiteVersion = websiteVersion });
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
                vm.pageMode = "new";
            },
            editPage: function() {
                var vm = this;
                vm.errors = [];
                vm.editingPage = exported.pageStore.getEditablePage(vm.currentPage);
                Object.assign(vm.originalPage, vm.editingPage);
                vm.pageMode = "edit";
            },
            deletePage: function() {
                var vm = this;
                vm.pageMode = "delete";
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.pageStore.updatePage(
                        vm.originalPage,
                        vm.editingPage,
                        function() { vm.pageMode = "view"; },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                if (vm.websiteVersion == undefined) {
                    vm.errors = ["You must select a website version before creating a new page"];
                } else {
                    vm.validate();
                    if (vm.errors.length === 0) {
                        exported.pageStore.createPage(
                            vm.editingPage,
                            vm.websiteVersion.websiteVersionId,
                            function() { vm.pageMode = "view"; },
                            function(msg) { vm.errors = [msg] });
                    }
                }
            },
            confirmDelete: function() {
                var vm = this;
                exported.pageStore.deletePage(
                    vm.currentPage.elementId,
                    function() {
                        vm.currentPage = null;
                        vm.pageMode = "view";
                    });
            },
            cancelChanges: function() {
                this.pageMode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                exported.validation.displayName(vm.editingPage.displayName, "display name", errors);
                exported.validation.name(vm.editingPage.name, "name", errors);
                vm.errors = errors;
            },
            choosePageVersion: function () {
                var vm = this;
                vm.pageVersionMode = "choose";
            },
            newPageVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                vm.editingPageVersion = exported.pageStore.getNewPage();
                vm.pageVersionMode = "new";
            },
            editPageVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                vm.editingPageVersion = exported.pageStore.getEditablePage(vm.currentPageVersion);
                Object.assign(vm.originalPageVersion, vm.editingPageVersion);
                vm.pageVersionMode = "edit";
            },
            deletePageVersion: function () {
                var vm = this;
                vm.pageVersionMode = "delete";
            },
            saveVersionChanges: function () {
                var vm = this;
                vm.validate();
                if (vm.versionErrors.length === 0) {
                    exported.pageStore.updatePage(
                        vm.originalPageVersion,
                        vm.editingPageVersion,
                        function () { vm.pageVersionMode = "view"; },
                        function (msg) { vm.versionErrors = [msg] });
                }
            },
            createNewVersion: function () {
                var vm = this;
                if (vm.websiteVersion == undefined) {
                    vm.versionErrors = ["You must select a website version before creating a new page"];
                } else {
                    vm.validate();
                    if (vm.versionErrors.length === 0) {
                        exported.pageStore.createPage(
                            vm.editingPageVersion,
                            vm.websiteVersion.websiteVersionId,
                            function () { vm.pageVersionMode = "view"; },
                            function (msg) { vm.versionErrors = [msg] });
                    }
                }
            },
            confirmDeleteVersion: function () {
                var vm = this;
                exported.pageStore.deletePage(
                    vm.currentPageVersion.elementId,
                    function () {
                        vm.currentPageVersion = null;
                        vm.pageVersionMode = "view";
                    });
            },
            cancelVersionChanges: function () {
                this.pageVersionMode = "view";
            },
            validateVersion: function () {
                var vm = this;
                var errors = [];
                exported.validation.displayName(vm.editingPageVersion.displayName, "display name", errors);
                exported.validation.name(vm.editingPageVersion.name, "name", errors);
                vm.versionErrors = errors;
            }
        }
    });
}