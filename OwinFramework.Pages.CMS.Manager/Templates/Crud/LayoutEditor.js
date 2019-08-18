exported.layout_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            namePattern: "",
            displayNamePattern: "",
            cssPattern: "",
            websiteVersion: {},
            scenario: {},

            layoutMode: "view",
            errors: [],
            originalLayout: {},
            editingLayout: {},
            currentLayout: {},

            layoutVersionMode: "view",
            versionErrors: [],
            originalLayoutVersion: {},
            editingLayoutVersion: {},
            currentLayoutVersion: {},
            compareLayoutVersions: [],

            modalDialogTitle: "",
            modalDialogMessage: "",
            modalDialogButtons: [{ caption: "OK" }],
            modalDialogVisible: false
        },
        created: function() {
            var vm = this;
            vm.namePattern = exported.validation.namePattern.source;
            vm.displayNamePattern = exported.validation.displayNamePattern.source;
            vm.cssPattern = exported.validation.cssPattern.source;
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._unsubscribeLayoutId = context.subscribe("layoutId", function (layoutId) {
                    if (layoutId == undefined) {
                        vm.currentLayout = null;
                    } else {
                        exported.layoutStore.retrieveRecord(
                            layoutId,
                            function(layout) {
                                vm.currentLayout = layout;
                                vm.showLayoutVersion();
                            });
                    }
                });
                vm._unsubscribeWebsiteVersionId = managerContext.subscribe("websiteVersionId", function (value) {
                    exported.websiteVersionStore.retrieveRecord(
                        value,
                        function(websiteVersion) {
                            vm.websiteVersion = websiteVersion;
                            vm.showLayoutVersion();
                        });
                });
                vm._unsubscribeScenario = managerContext.subscribe("segmentationScenario", function (scenario) {
                    vm.scenario = scenario;
                    vm.showLayoutVersion();
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
                if (vm._unsubscribeLayoutId != undefined) {
                    vm._unsubscribeLayoutId();
                    vm._unsubscribeLayoutId = null;
                }
            },
            showModalDialog(title, message, buttons) {
                this.modalDialogTitle = title;
                this.modalDialogMessage = message;
                this.modalDialogButtons = buttons;
                this.modalDialogVisible = true;
            },
            newLayout: function() {
                var vm = this;
                vm.errors = [];
                vm.editingLayout = exported.layoutStore.blankRecord();
                vm.layoutMode = "new";
            },
            editLayout: function() {
                var vm = this;
                vm.errors = [];
                vm.editingLayout = exported.layoutStore.cloneForEditing(vm.currentLayout);
                Object.assign(vm.originalLayout, vm.editingLayout);
                vm.layoutMode = "edit";
            },
            deleteLayout: function() {
                var vm = this;
                vm.layoutMode = "delete";
                vm.showModalDialog(
                    "Delete " + vm.currentLayout.displayName + " layout",
                    "Are you sure you wan to permenantly delete this layout and all versions of it from all versions of the website?",
                    [
                        {
                            caption: "Delete",
                            onclick: function () {
                                exported.layoutStore.deleteRecord(
                                    vm.currentLayout.recordId,
                                    function () {
                                        vm.currentLayout = null;
                                        vm.layoutMode = "view";
                                    });
                            }
                        },
                        {
                            caption: "Cancel",
                            onclick: function () {
                                vm.layoutMode = "view";
                            }
                        }
                    ]);
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.layoutStore.updateRecord(
                        vm.originalLayout,
                        vm.editingLayout,
                        function() { vm.layoutMode = "view"; },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                if (vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.errors = ["You must select a website version before creating a new layout"];
                } else {
                    vm.validate();
                    if (vm.errors.length === 0) {
                        exported.layoutStore.createRecord(
                            vm.editingLayout,
                            function() { vm.layoutMode = "view"; },
                            function(msg) { vm.errors = [msg] },
                            {
                                websiteVersionId: vm.websiteVersion.recordId
                            });
                    }
                }
            },
            cancelChanges: function() {
                this.layoutMode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                vm.editingLayout.displayName = exported.validation.displayName(vm.editingLayout.displayName, "display name", errors);
                vm.editingLayout.name = exported.validation.name(vm.editingLayout.name, "name", errors);
                vm.errors = errors;
            },
            //
            //===============================================================================================================
            //
            showLayoutVersion: function () {
                var vm = this;
                vm.compareLayoutVersions = [];
                if (vm.currentLayout == undefined || vm.currentLayout.recordId == undefined || vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.currentLayoutVersion = null;
                } else {
                    exported.layoutVersionStore.getWebsiteLayoutVersion(
                        vm.websiteVersion.recordId,
                        vm.scenario ? vm.scenario.name : null,
                        vm.currentLayout.recordId,
                        function(layoutVersion) { vm.currentLayoutVersion = layoutVersion; },
                        function(msg) { vm.currentLayoutVersion = null; });
                }
            },
            chooseLayoutVersion: function () {
                var vm = this;
                vm.layoutVersionMode = "choose";
            },
            copySelectedLayoutVersion: function (layoutVersionId) {
                var vm = this;
                vm.versionErrors = [];
                exported.layoutVersionStore.retrieveRecord(
                    layoutVersionId,
                    function (layoutVersion) {
                        vm.editingLayoutVersion = Object.assign(exported.layoutVersionStore.blankRecord(), layoutVersion);
                        vm.layoutVersionMode = "new";
                    });
            },
            newLayoutVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                if (vm.currentLayoutVersion == undefined) {
                    if (vm.currentLayout == undefined) {
                        vm.versionErrors = ["You must select a layout to create a new layout version"];
                        return;
                    }
                    if (vm.scenario == undefined) {
                        vm.editingLayoutVersion = exported.layoutVersionStore.blankRecord();
                        vm.editingLayoutVersion.parentRecordId = vm.currentLayout.recordId;
                    } else {
                        exported.layoutVersionStore.getWebsiteLayoutVersion(
                            vm.websiteVersion.recordId,
                            null,
                            vm.currentLayout.recordId,
                            function (layoutVersion) {
                                vm.editingLayoutVersion = Object.assign(exported.layoutVersionStore.blankRecord(), layoutVersion);
                            });
                    }
                } else {
                    vm.editingLayoutVersion = Object.assign(exported.layoutVersionStore.blankRecord(), vm.currentLayoutVersion);
                }
                vm.layoutVersionMode = "new";
            },
            editLayoutVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                vm.editingLayoutVersion = exported.layoutVersionStore.cloneForEditing(vm.currentLayoutVersion);
                Object.assign(vm.originalLayoutVersion, vm.editingLayoutVersion);
                vm.layoutVersionMode = "edit";
            },
            deleteLayoutVersion: function (layoutVersionId, ondelete, oncancel) {
                var vm = this;
                exported.layoutVersionStore.retrieveRecord(
                    layoutVersionId,
                    function (layoutVersion) {
                        vm.showModalDialog(
                            "Delete " + layoutVersion.displayName + " version of the " + vm.currentLayout.displayName + " layout",
                            "Are you sure you wan to permenantly delete this version the layout. This will remove the layout from any versions of the website that use this version of the layout?",
                            [
                                {
                                    caption: "Delete",
                                    onclick: function () {
                                        exported.layoutVersionStore.deleteRecord(layoutVersionId, ondelete);
                                    }
                                },
                                {
                                    caption: "Cancel",
                                    onclick: oncancel
                                }
                            ]);
                    });
            },
            deleteCurrentLayoutVersion: function () {
                var vm = this;
                vm.layoutVersionMode = "delete";
                vm.deleteLayoutVersion(
                    vm.currentLayoutVersion.recordId,
                    function () {
                        vm.currentLayoutVersion = null;
                        vm.layoutVersionMode = "view";
                    },
                    function () {
                        vm.layoutVersionMode = "view";
                    });
            },
            deleteChooseLayoutVersion: function (e) {
                var vm = this;
                vm.deleteLayoutVersion(e.versionId, e.onsuccess);
            },
            saveVersionChanges: function () {
                var vm = this;
                vm.validateVersion();
                if (vm.versionErrors.length === 0) {
                    exported.layoutVersionStore.updateRecord(
                        vm.originalLayoutVersion,
                        vm.editingLayoutVersion,
                        function () { vm.layoutVersionMode = "view"; },
                        function (msg) { vm.versionErrors = [msg] });
                }
            },
            createNewVersion: function () {
                var vm = this;
                if (vm.websiteVersion.recordId == undefined) {
                    vm.versionErrors = ["You must select a website version before creating a new version of the layout"];
                } else {
                    vm.validateVersion();
                    if (vm.versionErrors.length === 0) {
                        exported.layoutVersionStore.createRecord(
                            vm.editingLayoutVersion,
                            function() {
                                 vm.layoutVersionMode = "view";
                                 vm.showLayoutVersion();
                            },
                            function(msg) {
                                vm.versionErrors = [msg];
                            },
                            {
                                websiteVersionId: vm.websiteVersion.recordId,
                                scenario: vm.scenario ? vm.scenario.name : null,
                                layoutId: vm.currentLayout.recordId
                            });
                    }
                }
            },
            selectLayoutVersion: function (layoutVersionId) {
                var vm = this;
                vm.selectedLayoutVersionId = layoutVersionId;
            },
            toggleLayoutVersion: function (layoutVersionId, isSelected) {
                var vm = this;
                if (isSelected) {
                    exported.layoutVersionStore.retrieveRecord(
                        layoutVersionId,
                        function (layoutVersion) {
                            vm.compareLayoutVersions.push(layoutVersion);
                        })
                } else {
                    for (i = 0; i < vm.compareLayoutVersions.length; i++) {
                        if (vm.compareLayoutVersions[i].recordId === layoutVersionId) {
                            vm.compareLayoutVersions.splice(i, 1);
                            i--;
                        }
                    }
                }
            },
            updateLayoutVersion: function () {
                var vm = this;
                exported.versionsService.assignElementVersion(
                    {
                        type: vm.currentLayout.recordType,
                        id: vm.currentLayout.recordId,
                        versionId: vm.selectedLayoutVersionId,
                        websiteVersionId: vm.websiteVersion.recordId,
                        scenario: vm.scenario ? vm.scenario.name : null
                    },
                    function () {
                        vm.layoutVersionMode = "view";
                        vm.showLayoutVersion();
                    });
            },
            clearLayoutVersion: function () {
                var vm = this;
                vm.selectedLayoutVersionId = null;
                vm.currentLayoutVersion = null;
            },
            cancelVersionChanges: function () {
                var vm = this;
                vm.layoutVersionMode = "view";
            },
            layoutInheritChanged: function (inherit) {
                var vm = this;
                if (inherit) {
                    vm.editingLayoutVersion.layoutName = null;
                    vm.editingLayoutVersion.layoutId = null;
                    vm.editingLayoutVersion.layoutZones.splice(0, vm.editingLayoutVersion.layoutZones.length);
                }
            },
            validateVersion: function () {
                var vm = this;
                var errors = [];
                if (vm.editingLayoutVersion.assetDeployment) {
                    vm.editingLayoutVersion.assetDeployment = exported.validation.assetDeployment(vm.editingLayoutVersion.assetDeployment, "asset deployment", errors);
                    if (vm.editingLayoutVersion.assetDeployment === "PerModule") {
                        vm.editingLayoutVersion.moduleName = exported.validation.name(vm.editingLayoutVersion.moduleName, "module name", errors);
                    }
                }
                if (vm.editingLayoutVersion.layoutZones) {
                    for (let i = 0; i < vm.editingLayoutVersion.layoutZones.length; i++) {
                        var layoutZone = vm.editingLayoutVersion.layoutZones[i];
                        layoutZone.zone = exported.validation.name(layoutZone.zone, "zone name", errors);
                        if (layoutZone.regionId)
                            layoutZone.regionId = exported.validation.id(layoutZone.regionId, "zone region id", errors);
                        if (layoutZone.layoutId)
                            layoutZone.layoutId = exported.validation.id(layoutZone.layoutId, "zone layout id", errors);
                        if (layoutZone.contentType) {
                            layoutZone.contentType = exported.validation.elementType(layoutZone.contentType, "zone content type", errors);
                            if (layoutZone.contentType === "Html") {
                                layoutZone.contentName = exported.validation.name(layoutZone.contentName, "zone html localization name", errors);
                                layoutZone.contentValue = exported.validation.html(layoutZone.contentValue, "zone html", errors);
                            } else if (layoutZone.contentType === "Template") {
                                layoutZone.contentName = exported.validation.path(layoutZone.contentName, "zone template path", errors);
                            } else {
                                layoutZone.contentName = exported.validation.nameRef(layoutZone.contentName, "zone content name", errors);
                            }
                        }
                    }
                }
                vm.versionErrors = errors;
            }
        }
    });
}