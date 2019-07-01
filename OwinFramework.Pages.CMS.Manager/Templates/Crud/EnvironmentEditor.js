exported.environment_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            mode: "view",
            namePattern: "",
            displayNamePattern: "",
            urlPattern: "",
            idPattern: "",
            errors: [],
            originalEnvironment: {},
            editingEnvironment: {},
            currentEnvironment: {},
            websiteVersion: {}
        },
        created: function() {
            var vm = this;
            vm.namePattern = exported.validation.namePattern.source;
            vm.displayNamePattern = exported.validation.displayNamePattern.source;
            vm.urlPattern = exported.validation.urlPattern.source;
            vm.idPattern = exported.validation.idPattern.source;
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._context = context;
                vm._unsubscribeEnvironmentId = context.subscribe("environmentId", function (environmentId) {
                    if (environmentId == undefined) {
                        vm.currentEnvironment = null;
                        vm.websiteVersion = null;
                    } else {
                        exported.environmentStore.retrieveEnvironment(
                            environmentId,
                            function(environment) {
                                 vm.currentEnvironment = environment;
                                 exported.websiteVersionStore.retrieveWebsiteVersion(
                                     environment.websiteVersionId,
                                     function(v) {vm.websiteVersion = v});
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
                vm.editingEnvironment = exported.environmentStore.getNewEnvironment();
                vm.mode = "new";
            },
            editEnvironment: function() {
                var vm = this;
                vm.errors = [];
                vm.editingEnvironment = exported.environmentStore.getEditableEnvironment(vm.currentEnvironment);
                Object.assign(vm.originalEnvironment, vm.editingEnvironment);
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
                    exported.environmentStore.updateEnvironment(
                        vm.originalEnvironment,
                        vm.editingEnvironment,
                        function() {
                             vm.mode = "view";
                             vm._context.selected("websiteVersionId", vm.editingEnvironment.websiteVersionId);
                        },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.environmentStore.createEnvironment(
                        vm.editingEnvironment,
                        function() {
                            vm.mode = "view";
                            vm._context.selected("websiteVersionId", vm.editingEnvironment.websiteVersionId);
                        },
                        function(msg) { vm.errors = [msg]});
                }
            },
            confirmDelete: function() {
                var vm = this;
                exported.environmentStore.deleteEnvironment(
                    vm.currentEnvironment.environmentId,
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
            websiteVersionSelected: function(websiteVersionId) {
                this.editingEnvironment.websiteVersionId = websiteVersionId;
            }
        }
    });
}