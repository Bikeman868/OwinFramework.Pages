exported.segmentation_scenario_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            mode: "view",
            namePattern: "",
            displayNamePattern: "",
            errors: [],
            originalScenario: {},
            editingScenario: {},
            currentScenario: {}
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
                vm._unsubscribeScenario = context.subscribe("segmentationScenario", function (scenario) {
                    vm.currentScenario = scenario;
                });
                this.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeScenario != undefined) {
                    vm._unsubscribeScenario();
                    vm._unsubscribeScenario = null;
                }
            },
            newScenario: function() {
                var vm = this;
                vm.errors = [];
                vm.editingScenario = exported.segmentScenarioStore.blankRecord();
                vm.mode = "new";
            },
            editScenario: function() {
                var vm = this;
                vm.errors = [];
                vm.originalScenario = exported.segmentScenarioStore.cloneRecord(vm.currentScenario);
                vm.editingScenario = exported.segmentScenarioStore.cloneRecord(vm.originalScenario);
                vm.mode = "edit";
            },
            deleteScenario: function() {
                var vm = this;
                vm.mode = "delete";
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.segmentScenarioStore.updateRecord(
                        vm.originalScenario,
                        vm.editingScenario,
                        function() {
                            vm.mode = "view";
                        },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.segmentScenarioStore.createRecord(
                        vm.editingScenario,
                        function() {
                            vm.mode = "view";
                            vm._context.selected("segmentationScenario", vm.editingScenario);
                        },
                        function(msg) { vm.errors = [msg]},
                        {});
                }
            },
            confirmDelete: function() {
                var vm = this;
                exported.segmentScenarioStore.deleteRecord(
                    vm.currentScenario.name,
                    function() {
                        vm.currentScenario = null;
                        vm.mode = "view";
                    });
            },
            cancelChanges: function() {
                this.mode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                vm.editingScenario.displayName = exported.validation.displayName(vm.editingScenario.displayName, "display name", errors);
                vm.errors = errors;
            }
        }
    });
}