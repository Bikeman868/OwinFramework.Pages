exported.component_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            namePattern: exported.validation.namePattern.source,
            displayNamePattern: exported.validation.displayNamePattern.source,
            stylePattern: exported.validation.stylePattern.source,
            classesPattern: exported.validation.classesPattern.source,
            websiteVersion: {},
            scenario: {},

            componentMode: "view",
            errors: [],
            originalComponent: {},
            editingComponent: {},
            currentComponent: {},

            componentVersionMode: "view",
            versionErrors: [],
            originalComponentVersion: {},
            editingComponentVersion: {},
            currentComponentVersion: {},
            compareComponentVersions: [],

            modalDialogTitle: "",
            modalDialogMessage: "",
            modalDialogButtons: [{ caption: "OK" }],
            modalDialogVisible: false
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._unsubscribeComponentId = context.subscribe("componentId", function (componentId) {
                    if (componentId == undefined) {
                        vm.currentComponent = null;
                    } else {
                        exported.componentStore.retrieveRecord(
                            componentId,
                            function(component) {
                                vm.currentComponent = component;
                                vm.showComponentVersion();
                            });
                    }
                });
                vm._unsubscribeWebsiteVersionId = managerContext.subscribe("websiteVersionId", function (value) {
                    exported.websiteVersionStore.retrieveRecord(
                        value,
                        function(websiteVersion) {
                            vm.websiteVersion = websiteVersion;
                            vm.showComponentVersion();
                        });
                });
                vm._unsubscribeScenario = managerContext.subscribe("segmentationScenario", function (scenario) {
                    vm.scenario = scenario;
                    vm.showComponentVersion();
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
                if (vm._unsubscribeWebsiteVersionId != undefined) {
                    vm._unsubscribeWebsiteVersionId();
                    vm._unsubscribeWebsiteVersionId = null;
                }
                if (vm._unsubscribeComponentId != undefined) {
                    vm._unsubscribeComponentId();
                    vm._unsubscribeComponentId = null;
                }
            },
            showModalDialog(title, message, buttons) {
                this.modalDialogTitle = title;
                this.modalDialogMessage = message;
                this.modalDialogButtons = buttons;
                this.modalDialogVisible = true;
            },
            newComponent: function() {
                var vm = this;
                vm.errors = [];
                vm.editingComponent = exported.componentStore.blankRecord();
                vm.componentMode = "new";
            },
            editComponent: function() {
                var vm = this;
                vm.errors = [];
                vm.originalComponent = exported.componentStore.cloneRecord(vm.currentComponent);
                vm.editingComponent = exported.componentStore.cloneRecord(vm.originalComponent);
                vm.componentMode = "edit";
            },
            deleteComponent: function() {
                var vm = this;
                vm.componentMode = "delete";
                vm.showModalDialog(
                    "Delete " + vm.currentComponent.displayName + " component",
                    "Are you sure you want to permenantly delete this component and all versions of it from all versions of the website?",
                    [
                        {
                            caption: "Delete",
                            onclick: function () {
                                exported.componentStore.deleteRecord(
                                    vm.currentComponent.recordId,
                                    function () {
                                        vm.currentComponent = null;
                                        vm.componentMode = "view";
                                    });
                            }
                        },
                        {
                            caption: "Cancel",
                            onclick: function () {
                                vm.componentMode = "view";
                            }
                        }
                    ]);
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.componentStore.updateRecord(
                        vm.originalComponent,
                        vm.editingComponent,
                        function() { vm.componentMode = "view"; },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                if (vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.errors = ["You must select a website version before creating a new component"];
                } else {
                    vm.validate();
                    if (vm.errors.length === 0) {
                        exported.componentStore.createRecord(
                            vm.editingComponent,
                            function() { vm.componentMode = "view"; },
                            function(msg) { vm.errors = [msg] },
                            {
                                websiteVersionId: vm.websiteVersion.recordId
                            });
                    }
                }
            },
            cancelChanges: function() {
                this.componentMode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                vm.editingComponent.displayName = exported.validation.displayName(vm.editingComponent.displayName, "display name", errors);
                vm.editingComponent.name = exported.validation.name(vm.editingComponent.name, "name", errors);
                vm.errors = errors;
            },
            //
            //===============================================================================================================
            //
            showComponentVersion: function () {
                var vm = this;
                vm.compareComponentVersions = [];
                if (vm.currentComponent == undefined || vm.currentComponent.recordId == undefined || vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.currentComponentVersion = null;
                } else {
                    exported.componentVersionStore.getWebsiteComponentVersion(
                        vm.websiteVersion.recordId,
                        vm.scenario ? vm.scenario.name : null,
                        vm.currentComponent.recordId,
                        function(componentVersion) { vm.currentComponentVersion = componentVersion; },
                        function(msg) { vm.currentComponentVersion = null; });
                }
            },
            chooseComponentVersion: function () {
                var vm = this;
                vm.componentVersionMode = "choose";
            },
            copySelectedComponentVersion: function (componentVersionId) {
                var vm = this;
                vm.versionErrors = [];
                exported.componentVersionStore.retrieveRecord(
                    componentVersionId,
                    function (componentVersion) {
                        vm.editingComponentVersion = exported.componentVersionStore.cloneRecord(componentVersion);
                        vm.componentVersionMode = "new";
                    });
            },
            newComponentVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                if (vm.currentComponentVersion == undefined) {
                    if (vm.currentComponent == undefined) {
                        vm.versionErrors = ["You must select a component to create a new component version"];
                        return;
                    }
                    if (vm.scenario == undefined) {
                        vm.editingComponentVersion = exported.componentVersionStore.blankRecord();
                        vm.editingComponentVersion.parentRecordId = vm.currentComponent.recordId;
                    } else {
                        exported.componentVersionStore.getWebsiteComponentVersion(
                            vm.websiteVersion.recordId,
                            null,
                            vm.currentComponent.recordId,
                            function (componentVersion) {
                                vm.editingComponentVersion = Exported.componentVersionStore.cloneRecord(componentVersion);
                            });
                    }
                } else {
                    vm.editingComponentVersion = exported.componentVersionStore.cloneRecord(vm.currentComponentVersion);
                }
                vm.componentVersionMode = "new";
            },
            editComponentVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                vm.originalComponentVersion = exported.componentVersionStore.cloneRecord(vm.currentComponentVersion);
                vm.editingComponentVersion = exported.componentVersionStore.cloneRecord(vm.originalComponentVersion);
                vm.componentVersionMode = "edit";
            },
            deleteComponentVersion: function (componentVersionId, ondelete, oncancel) {
                var vm = this;
                exported.componentVersionStore.retrieveRecord(
                    componentVersionId,
                    function (componentVersion) {
                        vm.showModalDialog(
                            "Delete " + componentVersion.displayName + " version of the " + vm.currentComponent.displayName + " component",
                            "Are you sure you wan to permenantly delete this version the component. This will remove the component from any versions of the website that use this version of the component?",
                            [
                                {
                                    caption: "Delete",
                                    onclick: function () {
                                        exported.componentVersionStore.deleteRecord(componentVersionId, ondelete);
                                    }
                                },
                                {
                                    caption: "Cancel",
                                    onclick: oncancel
                                }
                            ]);
                    });
            },
            deleteCurrentComponentVersion: function () {
                var vm = this;
                vm.componentVersionMode = "delete";
                vm.deleteComponentVersion(
                    vm.currentComponentVersion.recordId,
                    function () {
                        vm.currentComponentVersion = null;
                        vm.componentVersionMode = "view";
                    },
                    function () {
                        vm.componentVersionMode = "view";
                    });
            },
            deleteChooseComponentVersion: function (e) {
                var vm = this;
                vm.deleteComponentVersion(e.versionId, e.onsuccess);
            },
            saveVersionChanges: function () {
                var vm = this;
                vm.validateVersion();
                if (vm.versionErrors.length === 0) {
                    exported.componentVersionStore.updateRecord(
                        vm.originalComponentVersion,
                        vm.editingComponentVersion,
                        function () { vm.componentVersionMode = "view"; },
                        function (msg) { vm.versionErrors = [msg] });
                }
            },
            createNewVersion: function () {
                var vm = this;
                if (vm.websiteVersion.recordId == undefined) {
                    vm.versionErrors = ["You must select a website version before creating a new version of the component"];
                } else {
                    vm.validateVersion();
                    if (vm.versionErrors.length === 0) {
                        exported.componentVersionStore.createRecord(
                            vm.editingComponentVersion,
                            function() {
                                 vm.componentVersionMode = "view";
                                 vm.showComponentVersion();
                            },
                            function(msg) {
                                vm.versionErrors = [msg];
                            },
                            {
                                websiteVersionId: vm.websiteVersion.recordId,
                                scenario: vm.scenario ? vm.scenario.name : null,
                                componentId: vm.currentComponent.recordId
                            });
                    }
                }
            },
            selectComponentVersion: function (componentVersionId) {
                var vm = this;
                vm.selectedComponentVersionId = componentVersionId;
            },
            toggleComponentVersion: function (componentVersionId, isSelected) {
                var vm = this;
                if (isSelected) {
                    exported.componentVersionStore.retrieveRecord(
                        componentVersionId,
                        function (componentVersion) {
                            vm.compareComponentVersions.push(componentVersion);
                        })
                } else {
                    for (i = 0; i < vm.compareComponentVersions.length; i++) {
                        if (vm.compareComponentVersions[i].recordId === componentVersionId) {
                            vm.compareComponentVersions.splice(i, 1);
                            i--;
                        }
                    }
                }
            },
            updateComponentVersion: function () {
                var vm = this;
                exported.versionsService.assignElementVersion(
                    {
                        type: vm.currentComponent.recordType,
                        id: vm.currentComponent.recordId,
                        versionId: vm.selectedComponentVersionId,
                        websiteVersionId: vm.websiteVersion.recordId,
                        scenario: vm.scenario ? vm.scenario.name : null
                    },
                    function () {
                        vm.componentVersionMode = "view";
                        vm.showComponentVersion();
                    });
            },
            clearComponentVersion: function () {
                var vm = this;
                vm.selectedComponentVersionId = null;
                vm.currentComponentVersion = null;
            },
            cancelVersionChanges: function () {
                var vm = this;
                vm.componentVersionMode = "view";
            },
            componentInheritChanged: function (inherit) {
                var vm = this;
                if (inherit) {
                    vm.editingComponentVersion.componentName = null;
                    vm.editingComponentVersion.componentId = null;
                    vm.editingComponentVersion.zones.splice(0, vm.editingComponentVersion.zones.length);
                }
            },
            validateVersion: function () {
                var vm = this;
                var errors = [];
                if (vm.editingComponentVersion.assetDeployment) {
                    vm.editingComponentVersion.assetDeployment = exported.validation.assetDeployment(vm.editingComponentVersion.assetDeployment, "asset deployment", errors);
                    if (vm.editingComponentVersion.assetDeployment === "PerModule") {
                        vm.editingComponentVersion.moduleName = exported.validation.name(vm.editingComponentVersion.moduleName, "module name", errors);
                    }
                }
                if (vm.editingComponentVersion.zones) {
                    for (let i = 0; i < vm.editingComponentVersion.zones.length; i++) {
                        var componentZone = vm.editingComponentVersion.zones[i];
                        componentZone.zone = exported.validation.name(componentZone.zone, "zone name", errors);
                        if (componentZone.regionId)
                            componentZone.regionId = exported.validation.id(componentZone.regionId, "zone region id", errors);
                        if (componentZone.componentId)
                            componentZone.componentId = exported.validation.id(componentZone.componentId, "zone component id", errors);
                        if (componentZone.contentType) {
                            componentZone.contentType = exported.validation.elementType(componentZone.contentType, "zone content type", errors);
                            if (componentZone.contentType === "Html") {
                                componentZone.contentName = exported.validation.name(componentZone.contentName, "zone html localization name", errors);
                                componentZone.contentValue = exported.validation.html(componentZone.contentValue, "zone html", errors);
                            } else if (componentZone.contentType === "Template") {
                                componentZone.contentName = exported.validation.path(componentZone.contentName, "zone template path", errors);
                            } else {
                                componentZone.contentName = exported.validation.nameRef(componentZone.contentName, "zone content name", errors);
                            }
                        }
                    }
                }
                if (vm.editingComponentVersion.tag) {
                    vm.editingComponentVersion.tag = exported.validation.tag(vm.editingComponentVersion.tag, "tag", errors);
                }
                if (vm.editingComponentVersion.style) {
                    vm.editingComponentVersion.style = exported.validation.style(vm.editingComponentVersion.style, "style", errors);
                }
                if (vm.editingComponentVersion.classes) {
                    vm.editingComponentVersion.classes = exported.validation.classes(vm.editingComponentVersion.classes, "classes", errors);
                }
                if (vm.editingComponentVersion.nestingTag) {
                    vm.editingComponentVersion.nestingTag = exported.validation.tag(vm.editingComponentVersion.nestingTag, "nesting tag", errors);
                }
                if (vm.editingComponentVersion.nestingStyle) {
                    vm.editingComponentVersion.nestingStyle = exported.validation.style(vm.editingComponentVersion.nestingStyle, "nesting style", errors);
                }
                if (vm.editingComponentVersion.nestingClasses) {
                    vm.editingComponentVersion.nestingClasses = exported.validation.classes(vm.editingComponentVersion.nestingClasses, "nesting classes", errors);
                }
                vm.versionErrors = errors;
            }
        }
    });
}