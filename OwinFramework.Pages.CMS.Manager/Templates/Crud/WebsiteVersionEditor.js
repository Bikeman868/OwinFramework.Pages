exported.website_version_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            mode: "view", 
            namePattern: "",
            displayNamePattern: "",
            errors: [],
            originalWebsiteVersion: {},
            editingWebsiteVersion: {},
            currentWebsiteVersion: {}
        },
        created: function() {
            var vm = this;
            vm.namePattern = exported.validation.namePattern.source;
            vm.displayNamePattern = exported.validation.displayNamePattern.source;
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._context = context;
                vm._unsubscribeWebsiteVersionId = context.subscribe("websiteVersionId", function (websiteVersionId) {
                    if (websiteVersionId == undefined) {
                        vm.currentWebsiteVersion = null;
                    } else {
                        exported.websiteVersionStore.retrieveWebsiteVersion(
                            websiteVersionId,
                            function (websiteVersion) { vm.currentWebsiteVersion = websiteVersion; });
                    }
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
            },
            newWebsiteVersion: function() {
                var vm = this;
                vm.errors = [];
                vm.editingWebsiteVersion = exported.websiteVersionStore.getNewWebsiteVersion();
                vm.mode = "new";
            },
            editWebsiteVersion: function() {
                var vm = this;
                vm.errors = [];
                vm.editingWebsiteVersion = exported.websiteVersionStore.getEditableWebsiteVersion(vm.currentWebsiteVersion);
                Object.assign(vm.originalWebsiteVersion, vm.editingWebsiteVersion);
                vm.mode = "edit";
            },
            deleteWebsiteVersion: function() {
                var vm = this;
                vm.mode = "delete";
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.websiteVersionStore.updateWebsiteVersion(
                        vm.originalWebsiteVersion,
                        vm.editingWebsiteVersion,
                        function() {
                             vm.mode = "view";
                             vm._context.selected("websiteVersionId", vm.editingWebsiteVersion.websiteVersionId);
                        },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.websiteVersionStore.createWebsiteVersion(
                        vm.editingWebsiteVersion,
                        function() {
                            vm.mode = "view";
                            vm._context.selected("websiteVersionId", vm.editingWebsiteVersion.websiteVersionId);
                        },
                        function(msg) { vm.errors = [msg]});
                }
            },
            confirmDelete: function() {
                var vm = this;
                exported.websiteVersionStore.deleteWebsiteVersion(
                    vm.currentWebsiteVersion.websiteVersionId,
                    function() {
                        vm.currentWebsiteVersion = null;
                        vm.mode = "view";
                    });
            },
            cancelChanges: function() {
                this.mode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                vm.editingWebsiteVersion.displayName = exported.validation.displayName(vm.editingWebsiteVersion.displayName, "display name", errors);
                vm.editingWebsiteVersion.name = exported.validation.name(vm.editingWebsiteVersion.name, "name", errors);
                vm.errors = errors;
            }
        }
    });
}