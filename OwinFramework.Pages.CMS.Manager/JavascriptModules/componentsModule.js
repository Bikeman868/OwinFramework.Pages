exported.components = function() {
    Vue.component("cms-tabs", {
        template:
            "<div>"+
            "    <ul class=\"cms_tabs\">" +
            "      <li v-for=\"tab in tabs\" :class=\"{ 'cms_is_active': tab.isActive }\">"+
            "          <a :href=\"tab.href\" @click=\"selectTab(tab)\">{{ tab.name }}</a>"+
            "      </li>"+
            "    </ul>"+
            "    <div class=\"cms_tabs_details\">"+
            "        <slot></slot>"+
            "    </div>"+
            "</div>",
        data: function() {
            return { tabs: [] };
        },
        created: function() {
            this.tabs = this.$children;
        },
        methods: {
            selectTab: function(selectedTab) {
                this.tabs.forEach(function(tab) { tab.isActive = (tab.name === selectedTab.name); });
            }
        }
    });

    Vue.component("cms-tab", {
        template:"<div v-show=\"isActive\"><slot></slot></div>",
        props: {
            name: { required: true },
            selected: { default: false}
        },
        data: function() {
            return {
                isActive: false
            };
        },
        computed: {
            href: function() { return "#" + this.name.toLowerCase().replace(/ /g, "-"); }
        },
        mounted: function() { this.isActive = this.selected; }
    });

    Vue.component("cms-choose-element-version", {
        props: {
            elementType: {
                required: true,
                type: String
            },
            elementId: {
                required: true,
                type: Number
            },
            selectedVersionId: {
                required: false,
                type: Number,
                defaultValue: 0
            },
            showCopyButton: {
                required: false,
                type: Boolean,
                default: true
            },
            showDeleteButton: {
                required: false,
                type: Boolean,
                default: false
            }
        },
        watch: {
            elementType: "retrieveVersions",
            elementId: "retrieveVersions",
            selectedVersionId: "retrieveVersions"
        },
        template:
            "<table class=\"cms_selector\">" +
            "  <tr><th>Ver</th><th>Name</th><th>Usage</th><th v-if=\"showCopyButton || showDeleteButton\">Actions</th></tr>" +
            "  <tr v-for=\"version in versions\" class=\"cms_selection\" v-bind:class=\"{ cms_selected: version.isSelected }\" @click=\"selectVersion(version)\">" +
            "    <td>{{version.version}}</td>" +
            "    <td>{{version.name}}</td>" +
            "    <td><ul><li v-for=\"usage in version.usages\">{{usage.websiteVersionId|cms_lookupWebsiteVersionId}}<span v-if=\"usage.scenario\"> in the {{usage.scenario|cms_lookupScenarioName}}</span></li></ul></td>" +
            "    <td v-if=\"showCopyButton || showDeleteButton\"><button v-if=\"showCopyButton\" @click.stop=\"copyVersion(version)\">Copy</button><button v-if=\"showDeleteButton\" @click.stop=\"deleteVersion(version)\">Delete</button></td>" +
            "  </tr>" +
            "</table>",
        data: function () {
            return {
                versions: []
            };
        },
        created: function () {
            this.retrieveVersions();
        },
        methods: {
            retrieveVersions: function () {
                var vm = this;
                if (vm.elementType == undefined || vm.elementId == undefined) {
                    vm.versions = [];
                } else {
                    exported.versionsService.getElementVersions(
                        { type: vm.elementType, id: vm.elementId },
                        function (response) {
                            response.versions.forEach(function (v) { v.isSelected = v.versionId === vm.selectedVersionId; });
                            vm.versions = response.versions;
                        });
                }
            },
            selectVersion: function (version) {
                var vm = this;
                vm.versions.forEach(function (v) { v.isSelected = v === version; });
                vm.$emit("select-version", version.versionId);
            },
            copyVersion: function (version) {
                var vm = this;
                vm.$emit("copy-version", version.versionId);
            },
            deleteVersion: function (version) {
                var vm = this;
                vm.$emit("delete-version", version.versionId);
            }
        },
        mounted: function () { this.isActive = this.selected; }
    });

    Vue.component("cms-website-version-field-edior",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Website version"
            },
            websiteVersionId: {
                required: false
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <select class=\"cms_field__website_version\" @change=\"selectWebsiteVersion($event)\">" +
            "    <option v-for=\"websiteVersion in websiteVersions\" :value=\"websiteVersion.recordId\" :selected=\"websiteVersion.recordId==websiteVersionId\">" +
            "      {{websiteVersion.displayName}}" +
            "    </option>" +
            "  </select>" +
            "</div>",
        data: function () {
            return {
                websiteVersions: []
            }
        },
        created: function () {
            var vm = this;
            exported.websiteVersionStore.retrieveAllRecords(
                function (response) {
                    response.unshift({recordId: null, displayName: ""});
                    vm.websiteVersions = response;
                });
        },
        methods: {
            selectWebsiteVersion: function (e) {
                var vm = this;
                var websiteVersionId = parseInt(e.target.value);
                if (isNaN(websiteVersionId)) websiteVersionId = null;
                vm.$emit("website-version-id-changed", websiteVersionId);
            }
        }
    });

    Vue.component("cms-page-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Page"
            },
            allowNone: {
                required: false,
                type: Boolean,
                default: true
            },
            pageId: {
                required: false,
                type: Number
            },
            exclude: {
                required: false,
                type: Array
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <select class=\"cms_field__page\" @change=\"selectPage($event)\">" +
            "    <option v-for=\"page in pages\" v-bind:value=\"page.recordId\" v-bind:selected=\"page.recordId==pageId\">" +
            "      {{page.displayName}}" +
            "    </option>" +
            "  </select>" +
            "</div>",
        data: function () {
            return {
                pages: []
            }
        },
        created: function () {
            var vm = this;
            exported.pageStore.retrieveAllRecords(
                function (response) {
                    if (response != undefined) {
                        if (vm.exclude != undefined) {
                            for (let i = 0; i < response.length; i++) {
                                var excluded = false;
                                for (let j = 0; j < vm.exclude.length; j++) {
                                    if (vm.exclude[j] == response[i].recordId) excluded = true;
                                }
                                if (excluded) {
                                    response.splice(i, 1);
                                    i--;
                                }
                            }
                        }
                        if (vm.allowNone) {
                            response.unshift({
                                recordId: null,
                                displayName: ""
                            });
                        }
                    }
                    vm.pages = response;
                });
        },
        methods: {
            selectPage: function (e) {
                var vm = this;
                var pageId = parseInt(e.target.value);
                if (isNaN(pageId)) pageId = null;
                vm.$emit("page-id-changed", pageId);
            }
        }
    });

    Vue.component("cms-asset-deployment-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Asset deployment"
            },
            assetDeployment: {
                required: true,
                type: String
            },
            moduleName: {
                required: false,
                type: String
            },
            inheritOption: {
                required: false,
                type: String,
                default: "Inherit from parent"
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <select class=\"cms_field__asset_deployment\" @change=\"selectAssetDeployment($event)\">" +
            "    <option value=\"Inherit\" v-bind:selected=\"assetDeployment==='Inherit'\">{{inheritOption}}</option>" +
            "    <option value=\"InPage\" v-bind:selected=\"assetDeployment==='InPage'\">Inline within page</option>" +
            "    <option value=\"PerPage\" v-bind:selected=\"assetDeployment==='PerPage'\">Page specific asset</option>" +
            "    <option value=\"PerModule\" v-bind:selected=\"assetDeployment==='PerModule'\">Defined by module</option>" +
            "    <option value=\"PerWebsite\" v-bind:selected=\"assetDeployment==='PerWebsite'\">Website asset</option>" +
            "  </select>" +
            "  <input v-if=\"selectedAssetDeployment==='PerModule'\" type=\"text\" class=\"cms_field__module\" " +
            "    v-model=\"editedModuleName\" placeholder=\"module_name\" v-bind:pattern=\"namePattern\" @input=\"inputModuleName\">" +
            "</div>",
        data: function () {
            return {
                namePattern: exported.validation.namePattern.source,
                selectedAssetDeployment: this.assetDeployment,
                editedModuleName: this.moduleName
            }
        },
        methods: {
            selectAssetDeployment: function (e) {
                var vm = this;
                vm.selectedAssetDeployment = e.target.value;
                vm.$emit("asset-deployment-changed", vm.selectedAssetDeployment);
            },
            inputModuleName: function (e) {
                var vm = this;
                vm.editedModuleName = e.target.value;
                vm.$emit("module-name-changed", vm.editedModuleName);
            }
        }
    });

    Vue.component("cms-layout-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Layout"
            },
            layoutId: {
                required: false,
                type: Number
            },
            layoutName: {
                required: false,
                type: String
            },
            inheritOption: {
                required: false,
                type: String,
                default: "Inherit master page layout"
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <span v-if=\"inheritOption\" class=\"cms_checkbox\"><input type=\"checkbox\" v-bind:checked=\"inherit\" @change=\"changeInherit\">{{inheritOption}}</span>" +
            "  <select v-if=\"!inherit\" class=\"cms_field__layout\" @change=\"selectLayout($event)\">" +
            "    <option v-for=\"layout in layouts\" v-bind:value=\"layout.recordId\" v-bind:selected=\"layout.recordId==selectedLayoutId\">" +
            "      {{layout.displayName}}" +
            "    </option>" +
            "  </select>" +
            "  <input v-if=\"!inherit && !selectedLayoutId\" type=\"text\" class=\"cms_field__layout_name\" " +
            "    v-model=\"editedLayoutName\" placeholder=\"layout_name\" v-bind:pattern=\"namePattern\" @input=\"inputLayoutName\">" +
            "</div>",
        data: function () {
            return {
                layouts: [],
                namePattern: exported.validation.namePattern.source,
                inherit: false,
                selectedLayoutId: this.layoutId,
                editedLayoutName: this.layoutName
            }
        },
        created: function () {
            var vm = this;
            vm.layouts = [
                { recordId: 7, displayName: "Layout 1" },
                { recordId: 8, displayName: "Layout 2" },
                { recordId: 9, displayName: "Layout 3" },
                { recordId: 10, displayName: "Layout 4" }
            ];
            //exported.layoutStore.retrieveAllRecords(
            //    function (response) {
            //        vm.layouts = response;
                    vm.layouts.unshift({
                        recordId: "",
                        displayName: "Defined in code"
                    });
            //    });
            vm.inherit = vm.selectedLayoutId == undefined && (vm.editedLayoutName == undefined || vm.editedLayoutName.length === 0);
        },
        methods: {
            selectLayout: function (e) {
                var vm = this;
                var layoutId = parseInt(e.target.value);
                if (isNaN(layoutId)) layoutId = null;
                vm.selectedLayoutId = layoutId;
                vm.$emit("layout-id-changed", vm.selectedLayoutId);
            },
            inputLayoutName: function (e) {
                var vm = this;
                vm.editedLayoutName = e.target.value;
                vm.$emit("layout-name-changed", vm.editedLayoutName);
            },
            changeInherit: function(e) {
                var vm = this;
                vm.inherit = e.target.checked;
                vm.$emit("inherit-changed", vm.inherit);
                if (!vm.inherit) vm.$emit("layout-id-changed", vm.selectedLayoutId);
            }
        }
    });

    Vue.component("cms-layout-zone-editor",
    {
        props: {
            layoutZone: {
                required: true,
                type: Object
            }
        },
        watch: {
            layoutZone: function() {
                 this.mode = this.calculateMode();
            },
            mode: function (newMode, oldMode) {
                if (oldMode === newMode) return;
                if (oldMode === "0")
                    this.$emit("default-changed", { isDefault: false, layoutZone: this.layoutZone });
                if (newMode === "0")
                    this.$emit("default-changed", { isDefault: true, layoutZone: this.layoutZone });
                this.layoutZone.regionId = null;
                this.layoutZone.layoutId = null;
                this.layoutZone.contentName = "";
                this.layoutZone.contentValue = "";
                if (newMode === "4") this.layoutZone.contentType = "Region";
                else if (newMode === "5") this.layoutZone.contentType = "Html";
                else if (newMode === "6") this.layoutZone.contentType = "Template";
                else this.layoutZone.contentType = "";
            }
        },
        template:
            "<div>" +
            "  <select v-model=\"mode\">" +
            "    <option value=\"0\">Default content</option>" +
            "    <option value=\"1\">CMS region</option>" +
            "    <option value=\"2\">CMS layout</option>" +
            "    <option value=\"3\">CMS component</option>" +
            "    <option value=\"4\">Named element</option>" +
            "    <option value=\"5\">Static HTML</option>" +
            "    <option value=\"6\">HTML template</option>" +
            "  </select>" +
            "  <p v-if=\"mode==1\">-region-choosing-component-</p>" +
            "  <p v-if=\"mode==2\">-layout-choosing-component-</p>" +
            "  <p v-if=\"mode==3\">-component-choosing-component-</p>" +
            "  <div v-if=\"mode==4\">" +
            "    <select v-model=\"layoutZone.contentType\">" +
            "      <option value=\"Region\">Region name</option>" +
            "      <option value=\"Layout\">Layout name</option>" +
            "      <option value=\"Component\">Component name</option>" +
            "    </select>" +
            "    <input type=\"text\" v-model=\"layoutZone.contentName\" placeholder=\"package:element_name\" :pattern=\"nameRefPattern\">" +
            "  </div>" +
            "  <div v-if=\"mode==5\">" +
            "    <label>Localizable asset name</label>" +
            "    <input type=\"text\" v-model=\"layoutZone.contentName\" placeholder=\"localizable_asset_name\" :pattern=\"namePattern\">" +
            "    <label>Default language Html</label>" +
            "    <textarea class=\"cms_field__html\" v-model=\"layoutZone.contentValue\" placeholder=\"<div></div>\" :pattern=\"htmlPattern\"></textarea>" +
            "  </div>" +
            "  <div v-if=\"mode==6\">" +
            "    <input type=\"text\" v-model=\"layoutZone.contentName\" placeholder=\"/path/to/template\" :pattern=\"pathPattern\">" +
            "  </div>" +
            "</div>",
        data: function () {
            return {
                mode: this.calculateMode(),
                namePattern: exported.validation.namePattern.source,
                nameRefPattern: exported.validation.nameRefPattern.source,
                pathPattern: exported.validation.pathPattern.source,
                htmlPattern: exported.validation.htmlPattern.source
            }
        },
        methods: {
            calculateMode: function () {
                if (this.layoutZone.regionId != undefined) return "1";
                if (this.layoutZone.layoutId != undefined) return "2";
                if (this.layoutZone.componentId != undefined) return "3";
                if (this.layoutZone.contentType === "Html") return "5";
                if (this.layoutZone.contentType === "Template") return "6";
                if (this.layoutZone.contentType.length > 0) return "4";
                return "0";
            }
        }
    });

    Vue.component("cms-layout-zones-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Layout zones"
            },
            zoneNesting: {
                required: false,
                type: String
            },
            layoutZones: {
                required: true,
                type: Array
            }
        },
        watch: {
            zoneNesting: "setZones",
            layoutZones: "setZones"
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <table v-if=\"mode==='fixed'\">" +
            "    <tr><th>Zone</th><th>Contents</th></tr>" +
            "    <tr v-for=\"zone in zones\">" +
            "      <td>{{zone.name}}</td>" +
            "      <td><cms-layout-zone-editor :layout-zone=\"zone.layoutZone\" @default-changed=\"setZoneDefault($event)\"></cms-layout-zone-editor></td>" +
            "    </tr>" +
            "  </table>" +
            "  <div v-else>" +
            "    <table>" +
            "      <tr><th>Zone</th><th>Contents</th><th>-</th></tr>" +
            "      <tr v-for=\"zone in zones\">" +
            "        <td><input type=\"text\" class=\"cms_field__name\" placeholder=\"zone_name\" :pattern=\"namePattern\" v-model=\"zone.name\"></td>" +
            "        <td><cms-layout-zone-editor :layout-zone=\"zone.layoutZone\" @default-changed=\"setZoneDefault($event)\"></cms-layout-zone-editor></td>" +
            "        <td><button @click=\"removeZone(zone.name)\">-</button></td>" +
            "      </tr>" +
            "    </table>" +
            "    <button @click=\"addZone\">+</button>" +
            "  </div>" +
            "</div>",
        data: function () {
            return {
                mode: "fixed",
                zones: [],
                namePattern: exported.validation.namePattern.source
            }
        },
        created: function () {
            this.setZones();
        },
        methods: {
            setZones: function () {
                var vm = this;
                if (vm.zoneNesting == undefined) {
                    vm.mode = "flexible";
                    vm.zones = [];
                } else {
                    this.mode = "fixed";
                    if (vm.zones == undefined) vm.zones = [];
                    var zoneNames = this.zoneNesting.replace(/[(),]/g, " ").split(" ");
                    var zoneIndex = 0;
                    for (let i = 0; i < zoneNames.length; i++) {
                        var zoneName = zoneNames[i].trim();
                        if (zoneName.length > 0) {
                            if (zoneIndex < vm.zones.length) {
                                vm.zones[zoneIndex].name = zoneName;
                            } else {
                                vm.zones.push({name: zoneName });
                            }
                            var isConfigured = false;
                            for (var j = 0; j < vm.layoutZones.length; j++) {
                                if (vm.layoutZones[j].zone === zoneName) {
                                    vm.zones[zoneIndex].layoutZone = vm.layoutZones[j];
                                    isConfigured = true;
                                }
                            }
                            if (!isConfigured) {
                                vm.zones[zoneIndex].layoutZone = {
                                    zone: zoneName,
                                    regionId: null,
                                    layoutId: null,
                                    contentType: "",
                                    contentName: "",
                                    contentValue: ""
                                }
                            }
                            zoneIndex++;
                        }
                    }
                    if (vm.zones.length > zoneIndex)
                        vm.zones.splice(zoneIndex, vm.zones.length - zoneIndex);
                    for (let i = 0; i < vm.layoutZones.length; i++) {
                        var exists = false;
                        for (let j = 0; j < zoneNames.length; j++) {
                            if (vm.layoutZones[i].zone === zoneNames[j]) {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists) {
                            vm.layoutZones.splice(i, 1);
                            i--;
                        }
                    }
                }
            },
            setZoneDefault: function (e) {
                var vm = this;
                if (e.isDefault) {
                    for (let i = 0; i < vm.layoutZones.length; i++) {
                        if (vm.layoutZones[i].zone === e.layoutZone.zone) {
                            vm.layoutZones.splice(i, 1);
                            break;
                        }
                    }
                } else {
                    vm.layoutZones.push(e.layoutZone);
                }
            },
            addZone: function () {
                this.zones.push({
                    zone: "",
                    layoutZone: {
                        regionId: null,
                        layoutId: null,
                        contentType: "Html",
                        contentName: "",
                        contentValue: ""
                    }
                });
            },
            removeZone: function (name) {
                for (let i = 0; i < this.zones.length; i++) {
                    if (this.zones[i].name === name)
                        this.zones.splice(i, 1);
                }
            }
        }
    });

    Vue.component("cms-routes-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Routes to this asset"
            },
            routes: {
                required: true,
                type: Array
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <table>" +
            "    <tr><th>Priority</th><th>Url path</th><th>-</th></tr>" +
            "    <tr v-for=\"route in routes\">" +
            "      <td><input type=\"text\" class=\"cms_field__priority\" placeholder=\"100\" :pattern=\"idPattern\" v-model=\"route.priority\"></td>" +
            "      <td><input type=\"text\" class=\"cms_field__url_path\" placeholder=\"/content/page.html\" :pattern=\"urlPathPattern\" v-model=\"route.path\"></td>" +
            "      <td><button @click=\"removeRoute(route.id)\">-</button></td>" +
            "    </tr>" +
            "  </table>" +
            "  <button @click=\"addRoute\">+</button>"+
            "</div>",
        data: function () {
            return {
                nextId: 1
            }
        },
        created: function () {
            this.idPattern = exported.validation.idPattern.source;
            this.urlPathPattern = exported.validation.urlPathPattern.source;
            for (let id = 0; id < this.routes.length; id++) {
                this.routes[id].id = id;
            }
            this.nextId = this.routes.length;
        },
        methods: {
            addRoute: function () {
                this.routes.push({ id: this.nextId });
                this.nextId = this.nextId + 1;
            },
            removeRoute: function(id) {
                for (let i = 0; i < this.routes.length; i++) {
                    if (this.routes[i].id === id)
                        this.routes.splice(i, 1);
                }
            }
        }
    });

    Vue.component("cms-style-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Style"
            },
            cssStyle: {
                required: false,
                type: String,
                default: ""
            }
        },
        template: 
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <input type=\"text\" class=\"cms_field__style\" placeholder=\"font-family: arial; font-size: large;\" :pattern=\"cssPattern\" @input=\"inputStyle\" :value=\"cssStyle\">" +
            "</div>",
        data: function () {
            return {
                cssPattern: exported.validation.cssPattern.source
            }
        },
        methods: {
            inputStyle: function(e) {
                this.$emit("css-style-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-permisson-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Required permission"
            },
            permission: {
                required: false,
                type: String,
                default: ""
            },
            assetPath: {
                required: false,
                type: String,
                default: ""
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <input type=\"text\" class=\"cms_field__permission\" placeholder=\"content:editor\" :pattern=\"permissionPattern\" @input=\"inputPermission\" :value=\"permission\">" +
            "  <input type=\"text\" class=\"cms_field__asset_path\" placeholder=\"/user/profile/image\" :pattern=\"pathPattern\" @input=\"inputAssetPath\" :value=\"assetPath\">" +
            "</div>",
        data: function () {
            return {
                editedStyle: this.style,
                permissionPattern: exported.validation.permissionPattern.source,
                pathPattern: exported.validation.pathPattern.source
            }
        },
        methods: {
            inputPermission: function (e) {
                this.$emit("permission-changed", e.target.value);
            },
            inputAssetPath: function (e) {
                this.$emit("asset-path-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-title-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Title"
            },
            placeholder: {
                required: false,
                type: String,
                default: "My page"
            },
            title: {
                required: false,
                type: String,
                default: ""
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <input type=\"text\" class=\"cms_field__title\" :placeholder=\"placeholder\" :pattern=\"titlePattern\" @input=\"inputTitle\" :value=\"title\">" +
            "</div>",
        data: function () {
            return {
                titlePattern: exported.validation.titlePattern.source
            }
        },
        methods: {
            inputTitle: function (e) {
                this.$emit("title-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-element-name-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Name"
            },
            placeholder: {
                required: false,
                type: String,
                default: "element_name"
            },
            elementName: {
                required: false,
                type: String,
                default: ""
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <input type=\"text\" class=\"cms_field__name\" :placeholder=\"placeholder\" :pattern=\"namePattern\" @input=\"inputElementName\" :value=\"elementName\">" +
            "</div>",
        data: function () {
            return {
                namePattern: exported.validation.namePattern.source
            }
        },
        methods: {
            inputElementName: function (e) {
                this.$emit("element-name-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-display-name-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Display name"
            },
            placeholder: {
                required: false,
                type: String,
                default: "Element name"
            },
            displayName: {
                required: false,
                type: String,
                default: ""
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <input type=\"text\" class=\"cms_field__display_name\" :placeholder=\"placeholder\" :pattern=\"displayNamePattern\" @input=\"inputDisplayName\" :value=\"displayName\">" +
            "</div>",
        data: function () {
            return {
                displayNamePattern: exported.validation.displayNamePattern.source
            }
        },
        methods: {
            inputDisplayName: function (e) {
                this.$emit("display-name-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-url-path-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Url path"
            },
            placeholder: {
                required: false,
                type: String,
                default: "/path/to/asset"
            },
            urlPath: {
                required: false,
                type: String,
                default: ""
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <input type=\"text\" class=\"cms_field__url_path\" :placeholder=\"placeholder\" :pattern=\"urlPathPattern\" @input=\"inputUrlPath\" :value=\"urlPath\">" +
            "</div>",
        data: function () {
            return {
                urlPathPattern: exported.validation.urlPathPattern.source
            }
        },
        methods: {
            inputUrlPath: function (e) {
                this.$emit("url-path-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-url-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Url"
            },
            placeholder: {
                required: false,
                type: String,
                default: "https://company.com/index.html"
            },
            url: {
                required: false,
                type: String,
                default: ""
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <input type=\"text\" class=\"cms_field__url\" :placeholder=\"placeholder\" :pattern=\"urlPattern\" @input=\"inputUrl\" :value=\"url\">" +
            "</div>",
        data: function () {
            return {
                urlPattern: exported.validation.urlPattern.source
            }
        },
        methods: {
            inputUrl: function (e) {
                this.$emit("url-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-description-field-editor",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "Description"
            },
            placeholder: {
                required: false,
                type: String,
                default: "optional description"
            },
            description: {
                required: false,
                type: String,
                default: ""
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <textarea class=\"cms_field__description\" :placeholder=\"placeholder\" @input=\"inputDescription\">{{description}}</textarea>" +
            "</div>",
        methods: {
            inputDescription: function (e) {
                this.$emit("description-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-history-summary",
    {
        props: {
            summary: {
                required: true,
                type: Object
            }
        },
        watch: {
            summary: "clearEvents"
        },
        template:
            "<tr class=\"cms_history_summary\">" +
            "  <td class=\"cms_history__when\">{{summary.when|cms_formatDate}}</td>" +
            "  <td class=\"cms_history__identity\">{{summary.identity|cms_formatUserUrn}}</td>" +
            "  <td v-if=\"events==undefined\" class=\"cms_history__changes\">" +
            "    {{summary.changes}}" +
            "    <button @click=\"retrieveEvents\">...</button>" +
            "  </td>" +
            "  <td v-else class=\"cms_history__changes\">" +
            "    <table class=\"cms_history_events\">" +
            "      <tr class=\"cms_history_events\" v-for=\"event in events\">" +
            "        <td class=\"cms_history__when\">{{event.when|cms_formatDateTime}}</td>" +
            "        <td class=\"cms_history__changes\">" +
            "          <div v-for=\"change in event.changes\">"+
            "            <span v-if=\"change.changeType==='Created'\">Created this {{event.recordType|cms_lowercase}}</span>" +
            "            <span v-if=\"change.changeType==='Deleted'\">Deleted this {{event.recordType|cms_lowercase}}</span>" +
            "            <span v-if=\"change.changeType==='Modified'\">Changed the {{change.field}} from \"{{change.oldValue}}\" to \"{{change.newValue}}\"</span>" +
            "            <span v-if=\"change.changeType==='ChildAdded'\">Added {{change.childType}} with ID={{change.childId}}</span>" +
            "            <span v-if=\"change.changeType==='ChildRemoved'\">Removed {{change.childType}} with ID={{change.childId}}</span>" +
            "          </div>" +
            "        </td>" +
            "      </tr>" +
            "    </table>" +
            "  </td>" +
            "</tr>",
        data: function () {
            return {
                events: null
            }
        },
        methods: {
            retrieveEvents: function () {
                var vm = this;
                exported.historyService.summary(
                { id: vm.summary.id },
                function(response) {
                    if (response != undefined) {
                        response.forEach(function(e) {
                            e.changes = JSON.parse(e.changeDetails);
                        });
                    }
                    vm.events = response;
                });
            },
            clearEvents: function() {
                this.events = null;
            }
        }
    });

    Vue.component("cms-history-period",
    {
        props: {
            label: {
                required: false,
                type: String,
                default: "History"
            },
            recordType: {
                required: true,
                type: String
            },
            recordId: {
                required: true,
                type: Number
            }
        },
        watch: {
            recordType: "loadData",
            recordId: "loadData"
        },
        template:
            "<div>" +
            "  <h2>{{label}}</h2>" +
            "  <table class=\"cms_history_period\">" +
            "    <tr class=\"cms_history_summary\">" +
            "      <th>When</th><th>Who</th><th>What</th>" +
            "    </tr>" +
            "    <cms-history-summary v-for=\"summary in summaries\" :key=\"summary.id\" :summary=\"summary\">" +
            "    </cms-history-summary>" +
            "  </table>" +
            "</div>",
        data: function () {
            return {
                summaries: []
            }
        },
        created: function() {
            this.loadData();
        },
        methods: {
            loadData: function () {
                var vm = this;
                vm.summaries = null;
                exported.historyService.period(
                    { type: vm.recordType, id: vm.recordId },
                    function(result) {
                        vm.summaries = result.summaries;
                    });
            }
        }
    });
}