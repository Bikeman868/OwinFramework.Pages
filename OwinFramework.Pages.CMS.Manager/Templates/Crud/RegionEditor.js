﻿exported.region_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            namePattern: exported.validation.namePattern.source,
            displayNamePattern: exported.validation.displayNamePattern.source,
            titlePattern: exported.validation.titlePattern.source,
            urlPathPattern: exported.validation.urlPathPattern.source,
            cssPattern: exported.validation.cssPattern.source,
            permissionPattern: exported.validation.permissionPattern.source,
            pathPattern: exported.validation.pathPattern.source,
            websiteVersion: {},
            scenario: {},

            regionMode: "view",
            errors: [],
            originalRegion: {},
            editingRegion: {},
            currentRegion: {},

            regionVersionMode: "view",
            versionErrors: [],
            originalRegionVersion: {},
            editingRegionVersion: {},
            currentRegionVersion: {},
            compareRegionVersions: [],

            modalDialogTitle: "",
            modalDialogMessage: "",
            modalDialogButtons: [{ caption: "OK" }],
            modalDialogVisible: false
        },
        computed: {
            editingZoneNesting: function () {
                if (this.editingRegionVersion.layoutName == undefined || this.editingRegionVersion.layoutName.length === 0) {
                    if (this.editingRegionVersion.layoutId == undefined) {
                        // TODO: Lookup the master region layout
                        return "master1,master2";
                    } else {
                        // TODO: Look up in the layoutStore
                        if (this.editingRegionVersion.layoutId === 7) return "main";
                        if (this.editingRegionVersion.layoutId === 8) return "left,right";
                        if (this.editingRegionVersion.layoutId === 9) return "header,body,footer";
                        return "";
                    }
                } else {
                    return null;
                }
            }
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._unsubscribeRegionId = context.subscribe("regionId", function (regionId) {
                    if (regionId == undefined) {
                        vm.currentRegion = null;
                    } else {
                        exported.regionStore.retrieveRecord(
                            regionId,
                            function(region) {
                                vm.currentRegion = region;
                                vm.showRegionVersion();
                            });
                    }
                });
                vm._unsubscribeWebsiteVersionId = managerContext.subscribe("websiteVersionId", function (value) {
                    exported.websiteVersionStore.retrieveRecord(
                        value,
                        function(websiteVersion) {
                            vm.websiteVersion = websiteVersion;
                            vm.showRegionVersion();
                        });
                });
                vm._unsubscribeScenario = managerContext.subscribe("segmentationScenario", function (scenario) {
                    vm.scenario = scenario;
                    vm.showRegionVersion();
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
                if (vm._unsubscribeRegionId != undefined) {
                    vm._unsubscribeRegionId();
                    vm._unsubscribeRegionId = null;
                }
            },
            showModalDialog(title, message, buttons) {
                this.modalDialogTitle = title;
                this.modalDialogMessage = message;
                this.modalDialogButtons = buttons;
                this.modalDialogVisible = true;
            },
            newRegion: function() {
                var vm = this;
                vm.errors = [];
                vm.editingRegion = exported.regionStore.blankRecord();
                vm.regionMode = "new";
            },
            editRegion: function() {
                var vm = this;
                vm.errors = [];
                vm.originalRegion = exported.regionStore.cloneRecord(vm.currentRegion);
                vm.editingRegion = exported.regionStore.cloneRecord(vm.originalRegion);
                vm.regionMode = "edit";
            },
            deleteRegion: function() {
                var vm = this;
                vm.regionMode = "delete";
                vm.showModalDialog(
                    "Delete " + vm.currentRegion.displayName + " region",
                    "Are you sure you wan to permenantly delete this region and all versions of it from all versions of the website?",
                    [
                        {
                            caption: "Delete",
                            onclick: function () {
                                exported.regionStore.deleteRecord(
                                    vm.currentRegion.recordId,
                                    function () {
                                        vm.currentRegion = null;
                                        vm.regionMode = "view";
                                    });
                            }
                        },
                        {
                            caption: "Cancel",
                            onclick: function () {
                                vm.regionMode = "view";
                            }
                        }
                    ]);
            },
            saveChanges: function() {
                var vm = this;
                vm.validate();
                if (vm.errors.length === 0) {
                    exported.regionStore.updateRecord(
                        vm.originalRegion,
                        vm.editingRegion,
                        function() { vm.regionMode = "view"; },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                if (vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.errors = ["You must select a website version before creating a new region"];
                } else {
                    vm.validate();
                    if (vm.errors.length === 0) {
                        exported.regionStore.createRecord(
                            vm.editingRegion,
                            function() { vm.regionMode = "view"; },
                            function(msg) { vm.errors = [msg] },
                            {
                                websiteVersionId: vm.websiteVersion.recordId
                            });
                    }
                }
            },
            cancelChanges: function() {
                this.regionMode = "view";
            },
            validate: function() {
                var vm = this;
                var errors = [];
                vm.editingRegion.displayName = exported.validation.displayName(vm.editingRegion.displayName, "display name", errors);
                vm.editingRegion.name = exported.validation.name(vm.editingRegion.name, "name", errors);
                vm.errors = errors;
            },
            //
            //===============================================================================================================
            //
            showRegionVersion: function () {
                var vm = this;
                vm.compareRegionVersions = [];
                if (vm.currentRegion == undefined || vm.currentRegion.recordId == undefined || vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.currentRegionVersion = null;
                } else {
                    exported.regionVersionStore.getWebsiteRegionVersion(
                        vm.websiteVersion.recordId,
                        vm.scenario ? vm.scenario.name : null,
                        vm.currentRegion.recordId,
                        function(regionVersion) { vm.currentRegionVersion = regionVersion; },
                        function(msg) { vm.currentRegionVersion = null; });
                }
            },
            chooseRegionVersion: function () {
                var vm = this;
                vm.regionVersionMode = "choose";
            },
            copySelectedRegionVersion: function (regionVersionId) {
                var vm = this;
                vm.versionErrors = [];
                exported.regionVersionStore.retrieveRecord(
                    regionVersionId,
                    function (regionVersion) {
                        vm.editingRegionVersion = exported.regionVersionStore.cloneRecord(regionVersion);
                        vm.regionVersionMode = "new";
                    });
            },
            newRegionVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                if (vm.currentRegionVersion == undefined) {
                    if (vm.currentRegion == undefined) {
                        vm.versionErrors = ["You must select a region to create a new region version"];
                        return;
                    }
                    if (vm.scenario == undefined) {
                        vm.editingRegionVersion = exported.regionVersionStore.blankRecord();
                        vm.editingRegionVersion.parentRecordId = vm.currentRegion.recordId;
                    } else {
                        exported.regionVersionStore.getWebsiteRegionVersion(
                            vm.websiteVersion.recordId,
                            null,
                            vm.currentRegion.recordId,
                            function (regionVersion) {
                                vm.editingRegionVersion = exported.regionVersionStore.cloneRecord(regionVersion);
                            });
                    }
                } else {
                    vm.editingRegionVersion = Object.assign(exported.regionVersionStore.blankRecord(), vm.currentRegionVersion);
                }
                vm.regionVersionMode = "new";
            },
            editRegionVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                vm.originalRegionVersion = exported.regionVersionStore.cloneRecord(vm.currentRegionVersion);
                vm.editingRegionVersion = exported.regionVersionStore.cloneRecord(vm.originalRegionVersion);
                vm.regionVersionMode = "edit";
            },
            deleteRegionVersion: function (regionVersionId, ondelete, oncancel) {
                var vm = this;
                exported.regionVersionStore.retrieveRecord(
                    regionVersionId,
                    function (regionVersion) {
                        vm.showModalDialog(
                            "Delete " + regionVersion.displayName + " version of the " + vm.currentRegion.displayName + " region",
                            "Are you sure you wan to permenantly delete this version the region. This will remove the region from any versions of the website that use this version of the region?",
                            [
                                {
                                    caption: "Delete",
                                    onclick: function () {
                                        exported.regionVersionStore.deleteRecord(regionVersionId, ondelete);
                                    }
                                },
                                {
                                    caption: "Cancel",
                                    onclick: oncancel
                                }
                            ]);
                    });
            },
            deleteCurrentRegionVersion: function () {
                var vm = this;
                vm.regionVersionMode = "delete";
                vm.deleteRegionVersion(
                    vm.currentRegionVersion.recordId,
                    function () {
                        vm.currentRegionVersion = null;
                        vm.regionVersionMode = "view";
                    },
                    function () {
                        vm.regionVersionMode = "view";
                    });
            },
            deleteChooseRegionVersion: function (e) {
                var vm = this;
                vm.deleteRegionVersion(e.versionId, e.onsuccess);
            },
            saveVersionChanges: function () {
                var vm = this;
                vm.validateVersion();
                if (vm.versionErrors.length === 0) {
                    exported.regionVersionStore.updateRecord(
                        vm.originalRegionVersion,
                        vm.editingRegionVersion,
                        function () { vm.regionVersionMode = "view"; },
                        function (msg) { vm.versionErrors = [msg] });
                }
            },
            createNewVersion: function () {
                var vm = this;
                if (vm.websiteVersion.recordId == undefined) {
                    vm.versionErrors = ["You must select a website version before creating a new version of the region"];
                } else {
                    vm.validateVersion();
                    if (vm.versionErrors.length === 0) {
                        exported.regionVersionStore.createRecord(
                            vm.editingRegionVersion,
                            function() {
                                 vm.regionVersionMode = "view";
                                 vm.showRegionVersion();
                            },
                            function(msg) {
                                vm.versionErrors = [msg];
                            },
                            {
                                websiteVersionId: vm.websiteVersion.recordId,
                                scenario: vm.scenario ? vm.scenario.name : null,
                                regionId: vm.currentRegion.recordId
                            });
                    }
                }
            },
            selectRegionVersion: function (regionVersionId) {
                var vm = this;
                vm.selectedRegionVersionId = regionVersionId;
            },
            toggleRegionVersion: function (regionVersionId, isSelected) {
                var vm = this;
                if (isSelected) {
                    exported.regionVersionStore.retrieveRecord(
                        regionVersionId,
                        function (regionVersion) {
                            vm.compareRegionVersions.push(regionVersion);
                        })
                } else {
                    for (i = 0; i < vm.compareRegionVersions.length; i++) {
                        if (vm.compareRegionVersions[i].recordId === regionVersionId) {
                            vm.compareRegionVersions.splice(i, 1);
                            i--;
                        }
                    }
                }
            },
            updateRegionVersion: function () {
                var vm = this;
                exported.versionsService.assignElementVersion(
                    {
                        type: vm.currentRegion.recordType,
                        id: vm.currentRegion.recordId,
                        versionId: vm.selectedRegionVersionId,
                        websiteVersionId: vm.websiteVersion.recordId,
                        scenario: vm.scenario ? vm.scenario.name : null
                    },
                    function () {
                        vm.regionVersionMode = "view";
                        vm.showRegionVersion();
                    });
            },
            clearRegionVersion: function () {
                var vm = this;
                vm.selectedRegionVersionId = null;
                vm.currentRegionVersion = null;
            },
            cancelVersionChanges: function () {
                var vm = this;
                vm.regionVersionMode = "view";
            },
            layoutInheritChanged: function (inherit) {
                var vm = this;
                if (inherit) {
                    vm.editingRegionVersion.layoutName = null;
                    vm.editingRegionVersion.layoutId = null;
                    vm.editingRegionVersion.layoutZones.splice(0, vm.editingRegionVersion.layoutZones.length);
                }
            },
            validateVersion: function () {
                var vm = this;
                var errors = [];
                vm.editingRegionVersion.title = exported.validation.title(vm.editingRegionVersion.title, "title", errors);
                if (vm.editingRegionVersion.canonicalUrl) {
                    vm.editingRegionVersion.canonicalUrl = exported.validation.urlPath(vm.editingRegionVersion.canonicalUrl, "canonical url path", errors);
                }
                if (vm.editingRegionVersion.routes) {
                    for (let i = 0; i < vm.editingRegionVersion.routes.length; i++) {
                        vm.editingRegionVersion.routes[i].path = exported.validation.urlPath(vm.editingRegionVersion.routes[i].path, "route path", errors);
                    }
                }
                if (vm.editingRegionVersion.assetDeployment) {
                    vm.editingRegionVersion.assetDeployment = exported.validation.assetDeployment(vm.editingRegionVersion.assetDeployment, "asset deployment", errors);
                    if (vm.editingRegionVersion.assetDeployment === "PerModule") {
                        vm.editingRegionVersion.moduleName = exported.validation.name(vm.editingRegionVersion.moduleName, "module name", errors);
                    }
                }
                if (vm.editingRegionVersion.layoutName) {
                    vm.editingRegionVersion.layoutName = exported.validation.name(vm.editingRegionVersion.layoutName, "layout name", errors);
                    if (vm.editingRegionVersion.layoutId) errors.push("You cannot specify a layout name and a layout ID for the region");
                } else {
                    if (vm.editingRegionVersion.layoutId)
                        vm.editingRegionVersion.layoutId = exported.validation.id(vm.editingRegionVersion.layoutId, "layout id", errors);
                }
                if (vm.editingRegionVersion.layoutZones) {
                    for (let i = 0; i < vm.editingRegionVersion.layoutZones.length; i++) {
                        var layoutZone = vm.editingRegionVersion.layoutZones[i];
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
                if (vm.editingRegionVersion.bodyStyle) {
                    vm.editingRegionVersion.bodyStyle = exported.validation.css(vm.editingRegionVersion.bodyStyle, "body style", errors);
                }
                var masterRegionId = vm.editingRegionVersion.masterRegionId;
                while (masterRegionId != undefined) {
                    if (masterRegionId === vm.editingRegionVersion.recordId) {
                        errors.push("Master regions must be in a tree-like heirachy, you cannot create loops");
                        masterRegionId = null;
                    } else {
                        masterRegionId = null;
                        // exported.regionStore.retrieveRecord(masterRegionId, function(region) { masterRegionId = region.recordId; });
                        // TODO: The masterRegionId is a property of the region version which is specific to each website version
                    }
                }
                vm.versionErrors = errors;
            }
        }
    });
}