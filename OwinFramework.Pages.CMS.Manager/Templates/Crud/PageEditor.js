﻿exported.page_editor_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            namePattern: "",
            displayNamePattern: "",
            titlePattern: "",
            urlPathPattern: "",
            cssPattern: "",
            permissionPattern: "",
            pathPattern: "",
            websiteVersion: {},
            scenario: {},

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
        computed: {
            editingZoneNesting: function () {
                // TODO: Look up in the layoutStore
                if (this.editingPageVersion.layoutName == undefined || this.editingPageVersion.layoutName.length === 0) {
                    if (this.editingPageVersion.layoutId == undefined) {
                        // TODO: Lookup the master page layout
                        return "master1,master2";
                    } else {
                        if (this.editingPageVersion.layoutId === 7) return "main";
                        if (this.editingPageVersion.layoutId === 8) return "left,right";
                        if (this.editingPageVersion.layoutId === 9) return "header,body,footer";
                        return "";
                    }
                } else {
                    return null;
                }
            }
        },
        created: function() {
            var vm = this;
            vm.namePattern = exported.validation.namePattern.source;
            vm.displayNamePattern = exported.validation.displayNamePattern.source;
            vm.titlePattern = exported.validation.titlePattern.source;
            vm.cssPattern = exported.validation.cssPattern.source;
            vm.urlPathPattern = exported.validation.urlPathPattern.source;
            vm.permissionPattern = exported.validation.permissionPattern.source;
            vm.pathPattern = exported.validation.pathPattern.source;
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._unsubscribePageId = context.subscribe("pageId", function (pageId) {
                    if (pageId == undefined) {
                        vm.currentPage = null;
                    } else {
                        exported.pageStore.retrieveRecord(
                            pageId,
                            function(page) {
                                vm.currentPage = page;
                                vm.showPageVersion();
                            });
                    }
                });
                vm._unsubscribeWebsiteVersionId = managerContext.subscribe("websiteVersionId", function (value) {
                    exported.websiteVersionStore.retrieveRecord(
                        value,
                        function(websiteVersion) {
                            vm.websiteVersion = websiteVersion;
                            vm.showPageVersion();
                        });
                });
                vm._unsubscribeScenario = managerContext.subscribe("segmentationScenario", function (scenario) {
                    vm.scenario = scenario;
                    vm.showPageVersion();
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
                if (vm._unsubscribePageId != undefined) {
                    vm._unsubscribePageId();
                    vm._unsubscribePageId = null;
                }
            },
            newPage: function() {
                var vm = this;
                vm.errors = [];
                vm.editingPage = exported.pageStore.blankRecord();
                vm.pageMode = "new";
            },
            editPage: function() {
                var vm = this;
                vm.errors = [];
                vm.editingPage = exported.pageStore.cloneForEditing(vm.currentPage);
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
                    exported.pageStore.updateRecord(
                        vm.originalPage,
                        vm.editingPage,
                        function() { vm.pageMode = "view"; },
                        function(msg) { vm.errors = [msg] });
                }
            },
            createNew: function() {
                var vm = this;
                if (vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.errors = ["You must select a website version before creating a new page"];
                } else {
                    vm.validate();
                    if (vm.errors.length === 0) {
                        exported.pageStore.createRecord(
                            vm.editingPage,
                            function() { vm.pageMode = "view"; },
                            function(msg) { vm.errors = [msg] },
                            {
                                websiteVersionId: vm.websiteVersion.recordId
                            });
                    }
                }
            },
            confirmDelete: function() {
                var vm = this;
                exported.pageStore.deleteRecord(
                    vm.currentPage.recordId,
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
                vm.editingPage.displayName = exported.validation.displayName(vm.editingPage.displayName, "display name", errors);
                vm.editingPage.name = exported.validation.name(vm.editingPage.name, "name", errors);
                vm.errors = errors;
            },
            //
            //===============================================================================================================
            //
            showPageVersion: function () {
                var vm = this;
                if (vm.currentPage == undefined || vm.currentPage.recordId == undefined || vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.currentPageVersion = null;
                } else {
                    exported.pageVersionStore.getWebsitePageVersion(
                        vm.websiteVersion.recordId,
                        vm.scenario ? vm.scenario.name : null,
                        vm.currentPage.recordId,
                        function(pageVersion) { vm.currentPageVersion = pageVersion; },
                        function(msg) { vm.currentPageVersion = null; });
                }
            },
            choosePageVersion: function () {
                var vm = this;
                vm.pageVersionMode = "choose";
            },
            newPageVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                if (vm.currentPageVersion == undefined) {
                    vm.editingPageVersion = exported.pageVersionStore.blankRecord();
                    vm.editingPageVersion.routes = [];
                    vm.editingPageVersion.layoutZones = [];
                } else {
                    vm.editingPageVersion = Object.assign(exported.pageVersionStore.blankRecord(), vm.currentPageVersion);
                }
                vm.pageVersionMode = "new";
            },
            editPageVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                vm.editingPageVersion = exported.pageVersionStore.cloneForEditing(vm.currentPageVersion);
                Object.assign(vm.originalPageVersion, vm.editingPageVersion);
                vm.pageVersionMode = "edit";
            },
            deletePageVersion: function () {
                var vm = this;
                vm.pageVersionMode = "delete";
            },
            saveVersionChanges: function () {
                var vm = this;
                vm.validateVersion();
                if (vm.versionErrors.length === 0) {
                    exported.pageVersionStore.updateRecord(
                        vm.originalPageVersion,
                        vm.editingPageVersion,
                        function () { vm.pageVersionMode = "view"; },
                        function (msg) { vm.versionErrors = [msg] });
                }
            },
            createNewVersion: function () {
                var vm = this;
                if (vm.websiteVersion == undefined || vm.websiteVersion.recordId == undefined) {
                    vm.versionErrors = ["You must select a website version before creating a new version of the page"];
                } else {
                    vm.validateVersion();
                    if (vm.versionErrors.length === 0) {
                        exported.pageVersionStore.createRecord(
                            vm.editingPageVersion,
                            function () { vm.pageVersionMode = "view"; },
                            function (msg) { vm.versionErrors = [msg] },
                            {
                                websiteVersionId: vm.websiteVersion.recordId,
                                scenario: vm.scenario ? vm.scenario.name : null,
                                pageId: vm.currentPage.recordId
                            });
                    }
                }
            },
            confirmDeleteVersion: function () {
                var vm = this;
                exported.pageVersionStore.deleteRecord(
                    vm.currentPageVersion.recordId,
                    function () {
                        vm.currentPageVersion = null;
                        vm.pageVersionMode = "view";
                    });
            },
            setPageVersion: function () {
                this.pageVersionMode = "view";
            },
            clearPageVersion: function () {
                this.pageVersionMode = "view";
            },
            cancelVersionChanges: function () {
                this.pageVersionMode = "view";
            },
            layoutInheritChanged: function (inherit) {
                if (inherit) {
                    this.editingPageVersion.layoutName = null;
                    this.editingPageVersion.layoutId = null;
                    this.editingPageVersion.layoutZones.splice(0, this.editingPageVersion.layoutZones.length);
                }
            },
            validateVersion: function () {
                var vm = this;
                var errors = [];
                vm.editingPageVersion.title = exported.validation.title(vm.editingPageVersion.title, "title", errors);
                if (vm.editingPageVersion.canonicalUrl) {
                    vm.editingPageVersion.canonicalUrl = exported.validation.urlPath(vm.editingPageVersion.canonicalUrl, "canonical url path", errors);
                }
                if (vm.editingPageVersion.routes) {
                    for (let i = 0; i < vm.editingPageVersion.routes.length; i++) {
                        vm.editingPageVersion.routes[i].path = exported.validation.urlPath(vm.editingPageVersion.routes[i].path, "route path", errors);
                    }
                }
                if (vm.editingPageVersion.assetDeployment) {
                    vm.editingPageVersion.assetDeployment = exported.validation.assetDeployment(vm.editingPageVersion.assetDeployment, "asset deployment", errors);
                    if (vm.editingPageVersion.assetDeployment === "PerModule") {
                        vm.editingPageVersion.moduleName = exported.validation.name(vm.editingPageVersion.moduleName, "module name", errors);
                    }
                }
                if (vm.editingPageVersion.layoutName) {
                    vm.editingPageVersion.layoutName = exported.validation.name(vm.editingPageVersion.layoutName, "layout name", errors);
                    if (vm.editingPageVersion.layoutId) errors.push("You cannot specify a layout name and a layout ID for the page");
                } else {
                    if (vm.editingPageVersion.layoutId)
                        vm.editingPageVersion.layoutId = exported.validation.id(vm.editingPageVersion.layoutId, "layout id", errors);
                }
                if (vm.editingPageVersion.layoutZones) {
                    for (let i = 0; i < vm.editingPageVersion.layoutZones.length; i++) {
                        var layoutZone = vm.editingPageVersion.layoutZones[i];
                        layoutZone.zone = exported.validation.name(layoutZone.zone, "zone name", errors);
                        if (layoutZone.regionId)
                            layoutZone.regionId = exported.validation.id(layoutZone.regionId, "zone region id", errors);
                        if (layoutZone.layoutId)
                            layoutZone.layoutId = exported.validation.id(layoutZone.layoutId, "zone layout id", errors);
                        if (layoutZone.contentType) {
                            layoutZone.contentType = exported.validation.elementType(layoutZone.contentType, "zone content type", errors);
                            if (layoutZone.contentType === "Html") {
                                layoutZone.contentName = exported.validation.name(layoutZone.contentName, "zone html localization name", errors);
                                layoutZone.contentValue = exported.validation.html(layoutZone.contentName, "zone html", errors);
                            } else if (layoutZone.contentType === "Template") {
                                layoutZone.contentName = exported.validation.path(layoutZone.contentName, "zone template path", errors);
                            } else {
                                layoutZone.contentName = exported.validation.nameRef(layoutZone.contentName, "zone content name", errors);
                            }
                        }
                    }
                }
                if (vm.editingPageVersion.bodyStyle) {
                    vm.editingPageVersion.bodyStyle = exported.validation.css(vm.editingPageVersion.bodyStyle, "body style", errors);
                }
                var masterPageId = vm.editingPageVersion.masterPageId;
                while (masterPageId != undefined) {
                    if (masterPageId === vm.editingPageVersion.recordId) {
                        errors.push("Master pages must be in a tree-like heirachy, you cannot create loops");
                        masterPageId = null;
                    } else {
                        masterPageId = null;
                        // exported.pageStore.retrieveRecord(masterPageId, function(page) { masterPageId = page.recordId; });
                        // TODO: The masterPageId is a property of the page version which is specific to each website version
                    }
                }
                vm.versionErrors = errors;
            }
        }
    });
}