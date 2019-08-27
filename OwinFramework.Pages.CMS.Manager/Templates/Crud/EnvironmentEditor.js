exported.environment_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            mode: "view",
            namePattern: exported.validation.namePattern.source,
            displayNamePattern: exported.validation.displayNamePattern.source,
            urlPattern: exported.validation.urlPattern.source,
            idPattern: exported.validation.idPattern.source,
            errors: [],
            originalEnvironment: {},
            editingEnvironment: {},
            currentEnvironment: {},
            websiteVersion: {}
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._context = context;
                vm._unsubscribeEnvironmentId = context.subscribe("environmentId", function (environmentId) {
                    if (environmentId == undefined) {
                        vm.currentEnvironment = null;
                        vm.websiteVersion = {};
                    } else {
                        exported.environmentStore.retrieveRecord(
                            environmentId,
                            function(environment) {
                                vm.currentEnvironment = environment;
                                vm.updateWebsiteVersion();
                            });
                    }
                });
                this.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeEnvironmentId != undefined) {
                    vm._unsubscribeEnvironmentId();
                    vm._unsubscribeEnvironmentId = null;
                }
            },
            newEnvironment: function() {
                var vm = this;
                vm.errors = [];
                vm.editingEnvironment = exported.environmentStore.blankRecord();
                vm.mode = "new";
            },
            editEnvironment: function() {
                var vm = this;
                vm.errors = [];
                vm.originalEnvironment = exported.environmentStore.cloneRecord(vm.currentEnvironment);
                vm.editingEnvironment = exported.environmentStore.cloneRecord(vm.originalEnvironment);
                vm.mode = "edit";
            },
            deleteEnvironment: function() {
                var vm = this;
                vm.mode = "delete";
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.environmentStore.updateRecord(
                        vm.originalEnvironment,
                        vm.editingEnvironment,
                        function() {
                            vm.mode = "view";
                            vm._context.selected("websiteVersionId", vm.editingEnvironment.recordId);
                            vm.updateWebsiteVersion();
                        },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.environmentStore.createRecord(
                        vm.editingEnvironment,
                        function() {
                            vm.mode = "view";
                            vm._context.selected("environmentId", vm.editingEnvironment.recordId);
                            vm.updateWebsiteVersion();
                        },
                        function(msg) { vm.errors = [msg]},
                        {});
                }
            },
            confirmDelete: function() {
                var vm = this;
                exported.environmentStore.deleteRecord(
                    vm.currentEnvironment.recordId,
                    function() {
                        vm.currentEnvironment = null;
                        vm.mode = "view";
                    });
            },
            cancelChanges: function() {
                this.mode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                vm.editingEnvironment.displayName = exported.validation.displayName(vm.editingEnvironment.displayName, "display name", errors);
                vm.editingEnvironment.name = exported.validation.name(vm.editingEnvironment.name, "name", errors);
                vm.editingEnvironment.baseUrl = exported.validation.url(vm.editingEnvironment.baseUrl, "base url", errors);
                vm.editingEnvironment.websiteVersionId = exported.validation.id(vm.editingEnvironment.websiteVersionId, "website version id", errors);
                vm.errors = errors;
            },
            updateWebsiteVersion: function () {
                var vm = this;
                var websiteVersionId = vm.currentEnvironment.recordId;
                vm._context.selected("websiteVersionId", websiteVersionId);
                exported.websiteVersionStore.retrieveRecord(
                    websiteVersionId,
                    function (websiteVersion) { vm.websiteVersion = websiteVersion });
            }
        }
    });
}