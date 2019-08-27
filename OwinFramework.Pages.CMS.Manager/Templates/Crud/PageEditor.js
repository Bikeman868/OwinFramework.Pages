exported.page_editor_vm = function (eId) {
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

            pageMode: "view",
            errors: [],
            originalPage: {},
            editingPage: {},
            currentPage: {},

            pageVersionMode: "view",
            versionErrors: [],
            originalPageVersion: {},
            editingPageVersion: {},
            currentPageVersion: {},
            comparePageVersions: [],

            modalDialogTitle: "",
            modalDialogMessage: "",
            modalDialogButtons: [{ caption: "OK" }],
            modalDialogVisible: false
        },
        computed: {
            editingZoneNesting: function () {
                if (this.editingPageVersion.layoutName == undefined || this.editingPageVersion.layoutName.length === 0) {
                    if (this.editingPageVersion.layoutId == undefined) {
                        // TODO: Lookup the master page layout
                        return "master1,master2";
                    } else {
                        // TODO: Look up in the layoutStore
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
            showModalDialog(title, message, buttons) {
                this.modalDialogTitle = title;
                this.modalDialogMessage = message;
                this.modalDialogButtons = buttons;
                this.modalDialogVisible = true;
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
                vm.originalPage = exported.pageStore.cloneRecord(vm.currentPage);
                vm.editingPage = exported.pageStore.cloneRecord(vm.originalPage);
                vm.pageMode = "edit";
            },
            deletePage: function() {
                var vm = this;
                vm.pageMode = "delete";
                vm.showModalDialog(
                    "Delete " + vm.currentPage.displayName + " page",
                    "Are you sure you wan to permenantly delete this page and all versions of it from all versions of the website?",
                    [
                        {
                            caption: "Delete",
                            onclick: function () {
                                exported.pageStore.deleteRecord(
                                    vm.currentPage.recordId,
                                    function () {
                                        vm.currentPage = null;
                                        vm.pageMode = "view";
                                    });
                            }
                        },
                        {
                            caption: "Cancel",
                            onclick: function () {
                                vm.pageMode = "view";
                            }
                        }
                    ]);
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
                vm.comparePageVersions = [];
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
            copySelectedPageVersion: function (pageVersionId) {
                var vm = this;
                vm.versionErrors = [];
                exported.pageVersionStore.retrieveRecord(
                    pageVersionId,
                    function (pageVersion) {
                        vm.editingPageVersion = exported.pageVersionStore.cloneRecord(pageVersion);
                        vm.pageVersionMode = "new";
                    });
            },
            newPageVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                if (vm.currentPageVersion == undefined) {
                    if (vm.currentPage == undefined) {
                        vm.versionErrors = ["You must select a page to create a new page version"];
                        return;
                    }
                    if (vm.scenario == undefined) {
                        vm.editingPageVersion = exported.pageVersionStore.blankRecord();
                        vm.editingPageVersion.parentRecordId = vm.currentPage.recordId;
                    } else {
                        exported.pageVersionStore.getWebsitePageVersion(
                            vm.websiteVersion.recordId,
                            null,
                            vm.currentPage.recordId,
                            function (pageVersion) {
                                vm.editingPageVersion = exported.pageVersionStore.cloneRecord(pageVersion);
                            });
                    }
                } else {
                    vm.editingPageVersion = Object.assign(exported.pageVersionStore.blankRecord(), vm.currentPageVersion);
                }
                vm.pageVersionMode = "new";
            },
            editPageVersion: function () {
                var vm = this;
                vm.versionErrors = [];
                vm.originalPageVersion = exported.pageVersionStore.cloneRecord(vm.currentPageVersion);
                vm.editingPageVersion = exported.pageVersionStore.cloneRecord(vm.originalPageVersion);
                vm.pageVersionMode = "edit";
            },
            deletePageVersion: function (pageVersionId, ondelete, oncancel) {
                var vm = this;
                exported.pageVersionStore.retrieveRecord(
                    pageVersionId,
                    function (pageVersion) {
                        vm.showModalDialog(
                            "Delete " + pageVersion.displayName + " version of the " + vm.currentPage.displayName + " page",
                            "Are you sure you wan to permenantly delete this version the page. This will remove the page from any versions of the website that use this version of the page?",
                            [
                                {
                                    caption: "Delete",
                                    onclick: function () {
                                        exported.pageVersionStore.deleteRecord(pageVersionId, ondelete);
                                    }
                                },
                                {
                                    caption: "Cancel",
                                    onclick: oncancel
                                }
                            ]);
                    });
            },
            deleteCurrentPageVersion: function () {
                var vm = this;
                vm.pageVersionMode = "delete";
                vm.deletePageVersion(
                    vm.currentPageVersion.recordId,
                    function () {
                        vm.currentPageVersion = null;
                        vm.pageVersionMode = "view";
                    },
                    function () {
                        vm.pageVersionMode = "view";
                    });
            },
            deleteChoosePageVersion: function (e) {
                var vm = this;
                vm.deletePageVersion(e.versionId, e.onsuccess);
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
                if (vm.websiteVersion.recordId == undefined) {
                    vm.versionErrors = ["You must select a website version before creating a new version of the page"];
                } else {
                    vm.validateVersion();
                    if (vm.versionErrors.length === 0) {
                        exported.pageVersionStore.createRecord(
                            vm.editingPageVersion,
                            function() {
                                 vm.pageVersionMode = "view";
                                 vm.showPageVersion();
                            },
                            function(msg) {
                                vm.versionErrors = [msg];
                            },
                            {
                                websiteVersionId: vm.websiteVersion.recordId,
                                scenario: vm.scenario ? vm.scenario.name : null,
                                pageId: vm.currentPage.recordId
                            });
                    }
                }
            },
            selectPageVersion: function (pageVersionId) {
                var vm = this;
                vm.selectedPageVersionId = pageVersionId;
            },
            togglePageVersion: function (pageVersionId, isSelected) {
                var vm = this;
                if (isSelected) {
                    exported.pageVersionStore.retrieveRecord(
                        pageVersionId,
                        function (pageVersion) {
                            vm.comparePageVersions.push(pageVersion);
                        })
                } else {
                    for (i = 0; i < vm.comparePageVersions.length; i++) {
                        if (vm.comparePageVersions[i].recordId === pageVersionId) {
                            vm.comparePageVersions.splice(i, 1);
                            i--;
                        }
                    }
                }
            },
            updatePageVersion: function () {
                var vm = this;
                exported.versionsService.assignElementVersion(
                    {
                        type: vm.currentPage.recordType,
                        id: vm.currentPage.recordId,
                        versionId: vm.selectedPageVersionId,
                        websiteVersionId: vm.websiteVersion.recordId,
                        scenario: vm.scenario ? vm.scenario.name : null
                    },
                    function () {
                        vm.pageVersionMode = "view";
                        vm.showPageVersion();
                    });
            },
            clearPageVersion: function () {
                var vm = this;
                vm.selectedPageVersionId = null;
                vm.currentPageVersion = null;
            },
            cancelVersionChanges: function () {
                var vm = this;
                vm.pageVersionMode = "view";
            },
            layoutInheritChanged: function (inherit) {
                var vm = this;
                if (inherit) {
                    vm.editingPageVersion.layoutName = null;
                    vm.editingPageVersion.layoutId = null;
                    vm.editingPageVersion.layoutZones.splice(0, vm.editingPageVersion.layoutZones.length);
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
                                layoutZone.contentValue = exported.validation.html(layoutZone.contentValue, "zone html", errors);
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