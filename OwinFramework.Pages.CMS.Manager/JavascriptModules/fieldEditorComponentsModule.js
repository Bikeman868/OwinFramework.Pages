exported.buildFieldEditorComponents = function() {
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
/*html*/`<table class="cms_selector">
  <tr><th>Ver</th><th>Name</th><th>Usage</th><th v-if="showCopyButton || showDeleteButton">Actions</th></tr>
  <tr v-for="version in versions" class="cms_selection" v-bind:class="{ cms_selected: version.isSelected }" @click="selectVersion(version)">
    <td>{{version.version}}</td>
    <td>{{version.name}}</td>
    <td><ul><li v-for="usage in version.usages">{{usage.websiteVersionId|cms_lookupWebsiteVersionId}}<span v-if="usage.scenario"> for the {{usage.scenario|cms_lookupScenarioName}}</span></li></ul></td>
    <td v-if="showCopyButton || showDeleteButton"><button v-if="showCopyButton" @click.stop="copyVersion(version)">Copy</button><button v-if="showDeleteButton" @click.stop="deleteVersion(version)">Delete</button></td>
  </tr>
</table>`,
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

    Vue.component("cms-multi-select-element-versions", {
        props: {
            elementType: {
                required: true,
                type: String
            },
            elementId: {
                required: true,
                type: Number
            },
            showDeleteButton: {
                required: false,
                type: Boolean,
                default: false
            }
        },
        watch: {
            elementType: "retrieveVersions",
            elementId: "retrieveVersions"
        },
        template:
/*html*/`<table class="cms_selector">
  <tr><th>Ver</th><th>Name</th><th>Usage</th><th v-if="showDeleteButton">Actions</th></tr>
  <tr v-for="version in versions" class="cms_selection" v-bind:class="{ cms_selected: version.isSelected }" @click="toggleVersion(version)">
    <td>{{version.version}}</td>
    <td>{{version.name}}</td>
    <td><ul><li v-for="usage in version.usages">{{usage.websiteVersionId|cms_lookupWebsiteVersionId}}<span v-if="usage.scenario"> for the {{usage.scenario|cms_lookupScenarioName}}</span></li></ul></td>
    <td v-if="showDeleteButton"><button v-if="showDeleteButton" @click.stop="deleteVersion(version)">Delete</button></td>
  </tr>
</table>`,
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
                            response.versions.forEach(function (v) { v.isSelected = false; });
                            vm.versions = response.versions;
                        });
                }
            },
            toggleVersion: function (version) {
                var vm = this;
                version.isSelected = !version.isSelected;
                vm.$emit("toggle-version", version.versionId, version.isSelected);
            },
            deleteVersion: function (version) {
                var vm = this;
                vm.$emit(
                    "delete-version",
                    {
                        versionId: version.versionId,
                        onsuccess: function() {
                            for (i = 0; i < vm.versions.length; i++) {
                                if (vm.versions[i] === version) {
                                    vm.versions.splice(i, 1);
                                    i--;
                                }
                            }
                        }
                    });
            }
        },
        mounted: function () { this.isActive = this.selected; }
    });

    Vue.component("cms-website-version-field-edior", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <select class="cms_field__website_version" @change="selectWebsiteVersion($event)">
    <option :value="null" :selected="websiteVersionId==undefined"></option> 
    <option v-for="websiteVersion in websiteVersions" :value="websiteVersion.recordId" :selected="websiteVersion.recordId==websiteVersionId">
      {{websiteVersion.displayName}}
    </option>
  </select>
</div>`,
        data: function () {
            return {
                websiteVersions: []
            }
        },
        created: function () {
            var vm = this;
            exported.websiteVersionStore.retrieveAllRecords(
                function (response) { vm.websiteVersions = response; });
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

    Vue.component("cms-page-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <select class="cms_field__page" @change="selectPage($event)">
    <option v-if="allowNone" :selected="pageId==undefined"></option>
    <option v-for="page in pages" :value="page.recordId" :selected="page.recordId==pageId">
      {{page.displayName}}
    </option>
  </select>
</div>`,
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

    Vue.component("cms-asset-deployment-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <select class="cms_field__asset_deployment" @change="selectAssetDeployment($event)">
    <option value="Inherit" v-bind:selected="assetDeployment==='Inherit'">{{inheritOption}}</option>
    <option value="InPage" v-bind:selected="assetDeployment==='InPage'">Inline within page</option>
    <option value="PerPage" v-bind:selected="assetDeployment==='PerPage'">Page specific asset</option>
    <option value="PerModule" v-bind:selected="assetDeployment==='PerModule'">Defined by module</option>
    <option value="PerWebsite" v-bind:selected="assetDeployment==='PerWebsite'">Website asset</option>
  </select>
  <input v-if="selectedAssetDeployment==='PerModule'" type="text" class="cms_field__module" 
    v-model="editedModuleName" placeholder="module_name" v-bind:pattern="namePattern" @input="inputModuleName">
</div>`,
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

    Vue.component("cms-layout-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <span v-if="inheritOption" class="cms_checkbox"><input type="checkbox" v-bind:checked="inherit" @change="changeInherit">{{inheritOption}}</span>
  <select v-if="!inherit" class="cms_field__layout" @change="selectLayout($event)">
    <option :value="null" :selected="selectedLayoutId==undefined">Defined in code</option>
    <option v-for="layout in layouts" :value="layout.recordId" :selected="layout.recordId==selectedLayoutId">
      {{layout.displayName}}
    </option>
  </select>
  <input v-if="!inherit && !selectedLayoutId" type="text" class="cms_field__layout_name" 
    v-model="editedLayoutName" placeholder="layout_name" v-bind:pattern="nameRefPattern" @input="inputLayoutName">
</div>`,
        data: function () {
            return {
                layouts: [],
                namePattern: exported.validation.namePattern.source,
                nameRefPattern: exported.validation.nameRefPattern.source,
                inherit: false,
                selectedLayoutId: this.layoutId,
                editedLayoutName: this.layoutName
            }
        },
        created: function () {
            var vm = this;
            exported.layoutStore.retrieveAllRecords(function (response) { vm.layouts = response; });
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

    Vue.component("cms-region-field-editor", {
        props: {
            label: {
                required: false,
                type: String,
                default: "Region"
            },
            regionId: {
                required: false,
                type: Number
            },
            regionName: {
                required: false,
                type: String
            },
            inheritOption: {
                required: false,
                type: String,
                default: "Inherit parent region"
            }
        },
        template:
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <span v-if="inheritOption" class="cms_checkbox"><input type="checkbox" v-bind:checked="inherit" @change="changeInherit">{{inheritOption}}</span>
  <select v-if="!inherit" class="cms_field__region" @change="selectRegion($event)">
    <option :value="null" :selected="selectedRegionId==undefined">Defined in code</option>
    <option v-for="region in regions" v-bind:value="region.recordId" v-bind:selected="region.recordId==selectedRegionId">
      {{region.displayName}}
    </option>
  </select>
  <input v-if="!inherit && !selectedRegionId" type="text" class="cms_field__region_name" 
    v-model="editedRegionName" placeholder="region_name" v-bind:pattern="nameRefPattern" @input="inputRegionName">
</div>`,
        data: function () {
            return {
                regions: [],
                namePattern: exported.validation.namePattern.source,
                nameRefPattern: exported.validation.nameRefPattern.source,
                inherit: false,
                selectedRegionId: this.regionId,
                editedRegionName: this.regionName
            }
        },
        created: function () {
            var vm = this;
            exported.regionStore.retrieveAllRecords(function (response) { vm.regions = response; });
            vm.inherit = vm.selectedRegionId == undefined && (vm.editedRegionName == undefined || vm.editedRegionName.length === 0);
        },
        methods: {
            selectRegion: function (e) {
                var vm = this;
                var regionId = parseInt(e.target.value);
                if (isNaN(regionId)) regionId = null;
                vm.selectedRegionId = regionId;
                vm.$emit("region-id-changed", vm.selectedRegionId);
            },
            inputRegionName: function (e) {
                var vm = this;
                vm.editedRegionName = e.target.value;
                vm.$emit("region-name-changed", vm.editedRegionName);
            },
            changeInherit: function (e) {
                var vm = this;
                vm.inherit = e.target.checked;
                vm.$emit("inherit-changed", vm.inherit);
                if (!vm.inherit) vm.$emit("region-id-changed", vm.selectedRegionId);
            }
        }
    });

    Vue.component("cms-component-field-editor", {
        props: {
            label: {
                required: false,
                type: String,
                default: "Component"
            },
            componentId: {
                required: false,
                type: Number
            },
            componentName: {
                required: false,
                type: String
            },
            inheritOption: {
                required: false,
                type: String,
                default: "Inherit parent component"
            }
        },
        template:
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <span v-if="inheritOption" class="cms_checkbox"><input type="checkbox" v-bind:checked="inherit" @change="changeInherit">{{inheritOption}}</span>
  <select v-if="!inherit" class="cms_field__component" @change="selectComponent($event)">
    <option :value="null" :selected="selectedComponentId==undefined">Defined in code</option>
    <option v-for="component in components" v-bind:value="component.recordId" v-bind:selected="component.recordId==selectedComponentId">
      {{component.displayName}}
    </option>
  </select>
  <input v-if="!inherit && !selectedComponentId" type="text" class="cms_field__component_name" 
    v-model="editedComponentName" placeholder="component_name" :pattern="nameRefPattern" @input="inputComponentName">
</div>`,
        data: function () {
            return {
                components: [],
                namePattern: exported.validation.namePattern.source,
                nameRefPattern: exported.validation.nameRefPattern.source,
                inherit: false,
                selectedComponentId: this.componentId,
                editedComponentName: this.componentName
            }
        },
        created: function () {
            var vm = this;
            vm.components = [
                { recordId: 7, displayName: "Component 1" },
                { recordId: 8, displayName: "Component 2" },
                { recordId: 9, displayName: "Component 3" },
                { recordId: 10, displayName: "Component 4" }
            ];
            //exported.componentStore.retrieveAllRecords(function (response) { vm.components = response; });
            vm.inherit = vm.selectedComponentId == undefined && (vm.editedComponentName == undefined || vm.editedComponentName.length === 0);
        },
        methods: {
            selectComponent: function (e) {
                var vm = this;
                var componentId = parseInt(e.target.value);
                if (isNaN(componentId)) componentId = null;
                vm.selectedComponentId = componentId;
                vm.$emit("component-id-changed", vm.selectedComponentId);
            },
            inputComponentName: function (e) {
                var vm = this;
                vm.editedComponentName = e.target.value;
                vm.$emit("component-name-changed", vm.editedComponentName);
            },
            changeInherit: function (e) {
                var vm = this;
                vm.inherit = e.target.checked;
                vm.$emit("inherit-changed", vm.inherit);
                if (!vm.inherit) vm.$emit("component-id-changed", vm.selectedComponentId);
            }
        }
    });

    Vue.component("cms-layout-zone-editor", {
        props: {
            layoutZone: {
                required: true,
                type: Object
            },
            defaultOption: {
                required: false,
                type: String,
                default: "Default content"
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
/*html*/`<div>
  <select v-model="mode">
    <option value="0">{{defaultOption}}</option>
    <option value="1">CMS region</option>
    <option value="2">CMS layout</option>
    <option value="3">CMS component</option>
    <option value="4">Named element</option>
    <option value="5">Static HTML</option>
    <option value="6">HTML template</option>
  </select>
  <cms-region-field-editor v-if="mode==1" :region-id="layoutZone.regionId" :inherit-option="null"></cms-region-field-editor>
  <cms-layout-field-editor v-if="mode==2" :layout-id="layoutZone.layoutId" :inherit-option="null"></cms-layout-field-editor>
  <cms-component-field-editor v-if="mode==3" :component-id="layoutZone.componentId" :inherit-option="null"></cms-component-field-editor>
  <div v-if="mode==4">
    <select v-model="layoutZone.contentType">
      <option value="Region">Region name</option>
      <option value="Layout">Layout name</option>
      <option value="Component">Component name</option>
    </select>
    <input type="text" v-model="layoutZone.contentName" placeholder="package:element_name" :pattern="nameRefPattern">
  </div>
  <div v-if="mode==5">
    <label>Localizable asset name</label>
    <input type="text" v-model="layoutZone.contentName" placeholder="localizable_asset_name" :pattern="namePattern">
    <label>Default language Html</label>
    <textarea class="cms_field__html" v-model="layoutZone.contentValue" placeholder="<div></div>" :pattern="htmlPattern"></textarea>
  </div>
  <div v-if="mode==6">
    <cms-template-field-editor :templatePath="layoutZone.contentName" @template-path-changed="layoutZone.contentName=$event"></cms-template-field-editor>
  </div>
</div>`,
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

    Vue.component("cms-layout-zones-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <table v-if="mode==='fixed'">
    <tr><th>Zone</th><th>Contents</th></tr>
    <tr v-for="zone in zones">
      <td>{{zone.name}}</td>
      <td><cms-layout-zone-editor :layout-zone="zone.layoutZone" @default-changed="setZoneDefault($event)"></cms-layout-zone-editor></td>
    </tr>
  </table>
  <div v-else>
    <table>
      <tr><th>Zone</th><th>Contents</th><th>-</th></tr>
      <tr v-for="zone in zones">
        <td><input type="text" class="cms_field__name" placeholder="zone_name" :pattern="namePattern" v-model="zone.name"></td>
        <td><cms-layout-zone-editor :layout-zone="zone.layoutZone" @default-changed="setZoneDefault($event)"></cms-layout-zone-editor></td>
        <td><button @click="removeZone(zone.name)">-</button></td>
      </tr>
    </table>
    <button @click="addZone">+</button>
  </div>
</div>`,
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

    Vue.component("cms-zones-field-editor", {
        props: {
            label: {
                required: false,
                type: String,
                default: "Zone nesting"
            },
            nesting: {
                required: false,
                type: String
            },
            zones: {
                required: true,
                type: Array
            }
        },
        template:
 /*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
   <p>Nesting: {{editedNesting}}</p>
   <table>
     <tr><th>Zone</th><th>Nesting</th><th>Contents</th><th>-</th></tr>
     <tr v-for="zone in zones">
       <td><input type="text" class="cms_field__name" placeholder="zone_name" :pattern="namePattern" v-model="zone.zone"></td>
       <td>
         <span>{{zone.indent}}</span>
         <button @click="outdentZone(zone)">&lt;</button>
         <button @click="indentZone(zone)">&gt;</button>
       </td>
       <td><cms-layout-zone-editor :layout-zone="zone" default-option="Empty" @default-changed="setZoneDefault($event)"></cms-layout-zone-editor></td>
       <td>
         <button @click="moveZoneUp(zone)">^</button>
         <button @click="moveZoneDown(zone)">v</button>
         <button @click="removeZone(zone)">-</button>
       </td>
     </tr>
   </table>
   <button @click="addZone">+</button>
 </div>`,
        data: function () {
            return {
                editedNesting: this.nesting,
                namePattern: exported.validation.namePattern.source
            }
        },
        created: function () {
            var vm = this;
            var zone = "";
            var indent = 0;
            var zoneIndex = 0;
            var addZone = function () {
                if (zone.length > 0) {
                    for (let i = 0; i < vm.zones.length; i++) {
                        if (vm.zones[i].zone == zone) {
                            Vue.set(vm.zones[i], "indent", indent);
                            zone = "";
                            zoneIndex++;
                            return;
                        }
                    }
                    vm.zones.splice(zoneIndex, 0, {
                        zone: zone,
                        regionId: null,
                        layoutId: null,
                        contentType: "",
                        contentName: "",
                        contentValue: "",
                        indent: indent
                    });
                }
                zone = "";
                zoneIndex++;
            };
            for (let i = 0; i < vm.nesting.length; i++) {
                var c = vm.nesting.charAt(i);
                if (c == " ") continue;
                if (c == "(") {
                    addZone();
                    indent++;
                } else if (c == ")") {
                    addZone();
                    indent--;
                } else if (c == ",") {
                    addZone();
                } else {
                    zone += c;
                }
            }
            addZone();
        },
        methods: {
            setZoneDefault: function (e) {
                if (e.isDefault) {
                    e.layoutZone.regionId = null;
                    e.layoutZone.layoutId = null;
                    e.layoutZone.contentType = "";
                    e.layoutZone.contentName = "";
                    e.layoutZone.contentValue = "";
                }
            },
            setNesting: function () {
                var vm = this;
                var nesting = "";
                var indent = 0;
                for (let i = 0; i < vm.zones.length; i++) {
                    var zone = vm.zones[i];
                    if (zone.indent < indent) {
                        while (zone.indent < indent) {
                            nesting += ")";
                            indent--;
                        }
                    } else if (zone.indent > indent) {
                        while (zone.indent > indent) {
                            nesting += "(";
                            indent++;
                        }
                    } else {
                        if (i > 0) nesting += ",";
                    }
                    nesting += zone.zone;
                }
                while (indent-- > 0) nesting += ")";
                vm.editedNesting = nesting;
                this.$emit("zone-nesting-changed", nesting);
            },
            addZone: function () {
                this.zones.push({
                    zone: "zone_" + (this.zones.length + 1),
                    regionId: null,
                    layoutId: null,
                    contentType: "",
                    contentName: "",
                    contentValue: "",
                    indent: 0
                });
                this.setNesting();
            },
            removeZone: function (zone) {
                for (let i = 0; i < this.zones.length; i++) {
                    if (this.zones[i] === zone) {
                        this.zones.splice(i, 1);
                        this.setNesting();
                        break;
                    }
                }
            },
            indentZone: function (zone) {
                for (let i = 1; i < this.zones.length; i++) {
                    if (this.zones[i] === zone) {
                        if (zone.indent <= this.zones[i-1].indent)
                        zone.indent += 1
                        this.setNesting();
                        break;
                    }
                }
            },
            outdentZone: function (zone) {
                if (zone.indent > 0) {
                    zone.indent -= 1
                    this.setNesting();
                }
            },
            moveZoneUp: function (zone) {
                for (let i = 0; i < this.zones.length; i++) {
                    if (this.zones[i] === zone) {
                        var temp = this.zones[i - 1];
                        this.zones.splice(i - 1, 1);
                        this.zones.splice(i, 0, temp);
                        break;
                        this.setNesting();
                    }
                }
            },
            moveZoneDown: function (zone) {
                for (let i = 0; i < this.zones.length - 1; i++) {
                    if (this.zones[i] === zone) {
                        var temp = this.zones[i];
                        this.zones.splice(i, 1);
                        this.zones.splice(i + 1, 0, temp);
                        this.setNesting();
                        break;
                    }
                }
            }
        }
    });

    Vue.component("cms-routes-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <table>
    <tr><th>Priority</th><th>Url path</th><th>-</th></tr>
    <tr v-for="route in routes">
      <td><input type="text" class="cms_field__priority" placeholder="100" :pattern="idPattern" v-model="route.priority"></td>
      <td><input type="text" class="cms_field__url_path" placeholder="/content/page.html" :pattern="urlPathPattern" v-model="route.path"></td>
      <td><button @click="removeRoute(route.id)">-</button></td>
    </tr>
  </table>
  <button @click="addRoute">+</button>
</div>`,
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

    Vue.component("cms-data-scope-field-editor", {
        props: {},
        template: "<div>Data scope selector</div>",
        methods: {}
    });

    Vue.component("cms-data-type-field-editor", {
        props: {},
        template: "<div>Data type selector</div>",
        methods: {}
    });

    Vue.component("cms-data-scopes-field-editor", {
        props: {},
        template: "<div>Data scope list editor</div>",
        methods: {}
    });

    Vue.component("cms-data-types-field-editor", {
        props: {},
        template: "<div>Data type list editor</div>",
        methods: {}
    });

    Vue.component("cms-template-field-editor", {
        props: {
            label: {
                required: false,
                type: String,
                default: "Template path"
            },
            templatePath: {
                required: false,
                type: String,
                default: ""
            }
        },
        data: function () {
            return {
                pathPattern: exported.validation.pathPattern.source
            }
        },
        template:
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__template_path" placeholder="/path/to/template" :pattern="pathPattern" @input="inputTemplatePath" :value="templatePath">
</div>`,
        methods: {
            inputTemplatePath: function (e) {
                this.$emit("template-path-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-region-content-editor", {
            props: {
                regionVersion: {
                    required: true,
                    type: Object
                }
            },
            data: function () {
                return {
                    mode: this.calculateMode(),
                    namePattern: exported.validation.namePattern.source,
                    nameRefPattern: exported.validation.nameRefPattern.source,
                    pathPattern: exported.validation.pathPattern.source,
                    htmlPattern: exported.validation.htmlPattern.source
                }
            },
            computed: {
                zoneNesting: function () {
                    if (this.regionVersion.layoutName) {
                        return null;
                    } else {
                        if (this.regionVersion.layoutId == undefined) {
                            return "";
                        } else {
                            // TODO: Look up in the layoutStore
                            if (this.regionVersion.layoutId === 7) return "main";
                            if (this.regionVersion.layoutId === 8) return "left,right";
                            if (this.regionVersion.layoutId === 9) return "header,body,footer";
                            return "";
                        }
                    }
                }
            },
            watch: {
                regionVersion: function () {
                    this.mode = this.calculateMode();
                },
                mode: function (newMode, oldMode) {
                    if (oldMode === newMode) return;
                    this.regionVersion.componentId = null;
                    this.regionVersion.componentName = null;
                    this.regionVersion.layoutId = null;
                    this.regionVersion.layoutName = null;
                    this.regionVersion.layoutZones = [];
                    this.regionVersion.assetName = null;
                    this.regionVersion.regionTemplates = [];
                }
            },
            template:
/*html*/`<div>
    <select v-model="mode" >
        <option value="layout">Layout</option>
        <option value="component">Component</option>
        <option value="html">Html</option>
        <option value="template">Template</option>
    </select >
    <div v-if="mode === 'layout'">
        <cms-layout-field-editor
            label="Region layout"
            :inherit-option="null"
            :layout-id="regionVersion.layoutId"
            :layout-name="regionVersion.layoutName"
            @layout-id-changed="regionVersion.layoutId=$event"
            @layout-name-changed="regionVersion.layoutName=$event">
        </cms-layout-field-editor>
        <cms-layout-zones-field-editor
            label="Layout zone contents"
            :zone-nesting="zoneNesting"
            :layout-zones="regionVersion.layoutZones">
        </cms-layout-zones-field-editor>
    </div >
    <div v-if="mode === 'component'">
        <p>Component chooser</p>
    </div>
    <div v-if="mode === 'html'">
        <label>Localizable asset name</label>
        <input type="text" v-model="regionVersion.assetName" placeholder="localizable_asset_name" :pattern="namePattern">
        <label>Default language Html</label>
        <textarea class="cms_field__html" v-model="regionVersion.assetValue" placeholder="<div></div>" :pattern="htmlPattern"></textarea>
    </div >
    <div v-if="mode === 'template'">
        <p>Choose templates for each page area</p>
        <table>
          <tr><th>Page Area</th><th>Template</th></tr>
          <tr><td>Head</td><td><cms-template-field-editor :label="null"></cms-template-field-editor></td></tr>
          <tr><td>Title</td><td><cms-template-field-editor :label="null"></cms-template-field-editor></td></tr>
          <tr><td>Styles</td><td><cms-template-field-editor :label="null"></cms-template-field-editor></td></tr>
          <tr><td>Scripts</td><td><cms-template-field-editor :label="null"></cms-template-field-editor></td></tr>
          <tr><td>Body</td><td><cms-template-field-editor :label="null"></cms-template-field-editor></td></tr>
          <tr><td>Initialization</td><td><cms-template-field-editor :label="null"></cms-template-field-editor></td></tr>
        </table>
    </div>
</div>`,
            methods: {
                calculateMode: function () {
                    if (this.regionVersion.layoutId != undefined || this.regionVersion.layoutName) return "layout";
                    if (this.regionVersion.componentId != undefined || this.regionVersion.componentName) return "component";
                    if (this.regionVersion.regionTemplates && this.regionVersion.regionTemplates.length > 0) return "template";
                    return "html";
                }
            }
        });

    Vue.component("cms-html-tag-field-editor", {
        props: {
            label: {
                required: false,
                type: String,
                default: "Html tag"
            },
            htmlTag: {
                required: false,
                type: String,
                default: "div"
            },
            choices: {
                required: false,
                type: Array,
                default: function () {
                    return [
                        "", "div", "p", "ul", "li", "table", "tr", "td", "th",
                        "span", "pre", "main", "article", "aside", "header", "footer",
                        "nav", "form", "details", "summary"];
                }
            }
        },
        template: 
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <select class="cms_field__html_tag" @change="tagChanged($event)">
    <option v-for="choice in choices" :value="choice" :selected="choice==htmlTag">{{choice}}</option>
  </select>
</div>`,
        methods: {
            tagChanged: function(e) {
                this.$emit("html-tag-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-style-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__style" placeholder="font-family: arial; font-size: large;" :pattern="stylePattern" @input="inputStyle" :value="cssStyle">
</div>`,
            data: function () {
                return {
                    stylePattern: exported.validation.stylePattern.source
                }
            },
            methods: {
                inputStyle: function (e) {
                    this.$emit("css-style-changed", e.target.value);
                }
            }
        });

    Vue.component("cms-css-classes-field-editor", {
            props: {
                label: {
                    required: false,
                    type: String,
                    default: "Classes"
                },
                cssClasses: {
                    required: false,
                    type: String,
                    default: ""
                }
            },
            template:
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__classes" placeholder="my_container my_float_left" :pattern="classesPattern" @input="inputChanged" :value="cssClasses">
</div>`,
        data: function () {
            return {
                classesPattern: exported.validation.classesPattern.source
            }
        },
            methods: {
                inputChanged: function (e) {
                    this.$emit("css-classes-changed", e.target.value);
                }
            }
        });

    Vue.component("cms-permisson-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__permission" placeholder="content:editor" :pattern="permissionPattern" @input="inputPermission" :value="permission">
  <input type="text" class="cms_field__asset_path" placeholder="/user/profile/image" :pattern="pathPattern" @input="inputAssetPath" :value="assetPath">
</div>`,
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

    Vue.component("cms-title-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__title" :placeholder="placeholder" :pattern="titlePattern" @input="inputTitle" :value="title">
</div>`,
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

    Vue.component("cms-element-name-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__name" :placeholder="placeholder" :pattern="namePattern" @input="inputElementName" :value="elementName">
</div>`,
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

    Vue.component("cms-display-name-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__display_name" :placeholder="placeholder" :pattern="displayNamePattern" @input="inputDisplayName" :value="displayName">
</div>`,
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

    Vue.component("cms-url-path-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__url_path" :placeholder="placeholder" :pattern="urlPathPattern" @input="inputUrlPath" :value="urlPath">
</div>`,
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

    Vue.component("cms-url-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <input type="text" class="cms_field__url" :placeholder="placeholder" :pattern="urlPattern" @input="inputUrl" :value="url">
</div>`,
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

    Vue.component("cms-description-field-editor", {
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
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <textarea class="cms_field__description" :placeholder="placeholder" @input="inputDescription">{{description}}</textarea>
</div>`,
        methods: {
            inputDescription: function (e) {
                this.$emit("description-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-component-class-field-editor", {
        props: {
            label: {
                required: false,
                type: String,
                default: "Component C# class"
            },
            placeholder: {
                required: false,
                type: String,
                default: "my_package:my_component"
            },
            componentClass: {
                required: false,
                type: String,
                default: ""
            },
        },
        template:
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <select class="cms_field__component_class" @change="selectComponentClass($event)">
    <option :value="null" :selected="componentClass==undefined"></option> 
    <option v-for="component in components" :value="component.componentName" :selected="component.componentName==componentClass">
      {{component.displayName}}
    </option>
  </select>
  <input v-if="manual" type="text" class="cms_field__component_class" :placeholder="placeholder" :pattern="nameRefPattern" @input="inputClassName" :value="componentClass">
</div>`,
        data: function () {
            return {
                nameRefPattern: exported.validation.nameRefPattern.source,
                manual: false,
                components: []
            }
        },
        created: function () {
            var vm = this;
            exported.listService.allComponentClassess(
                { },
                function (response) {
                    vm.components = response;
                });
        },
        methods: {
            inputClassName: function (e) {
                this.$emit("component-class-changed", e.target.value);
            },
            selectComponentClass: function (e) {
                this.manual = !e.target.value;
                this.$emit("component-class-changed", e.target.value);
            }
        }
    });

    Vue.component("cms-element-properties-field-editor", {
        props: {
            label: {
                required: false,
                type: String,
                default: "Properties"
            },
            elementProperties: {
                required: false,
                type: Array
            }
        },
        template:
/*html*/`<div class="cms_field">
  <label v-if="label">{{label}}</label>
  <table>
      <tr><th>Name</th><th>Property</th><th>Regions</th><th>Description</th><th>-</th></tr>
      <tr v-for="property in elementProperties">
        <td>
            <input type="text" class="cms_field__display_name" placeholder="display name" v-model="property.displayName">
        </td>
        <td>
            <label>C# class property name</label>
            <input type="text" class="cms_field__name" placeholder="PropertyName" :pattern="csharpNamePattern" v-model="property.name">
            <label>C# class property type</label>
            <input type="text" class="cms_field__type" placeholder="System.String" :pattern="csharpTypeNamePattern" v-model="property.typeName">
        </td>
        <td>
            <cms-region-field-editor label="Display region" :region-name="property.displayRegionName" inherit-option="Default display"></cms-region-field-editor>
            <cms-region-field-editor label="Edit region" :region-name="property.editRegionName" inherit-option="Default editor"></cms-region-field-editor>
        </td>
        <td>
            <label>Property description</label>
            <textarea class="cms_field__description" placeholder="description">{{property.description}}</textarea>
        </td>
        <td>
            <button @click="removeProperty(property.name)">-</button>
        </td>
      </tr>
  </table>
  <button @click="addProperty">+</button>`,
        data: function () {
            return {
                regions: [],
                nameRefPattern: exported.validation.nameRefPattern.source,
                displayNamePattern: exported.validation.displayNamePattern.source,
                csharpNamePattern: exported.validation.csharpNamePattern.source,
                csharpTypeNamePattern: exported.validation.csharpTypeNamePattern.source
            }
        },
        created: function () {
            var vm = this;
            exported.regionStore.retrieveAllRecords(function (response) { vm.regions = response; });
        },
        methods: {
            addProperty: function () {
                this.elementProperties.push({
                    name: "",
                    displayName: "",
                    displayRegionName: "",
                    editRegionName: "",
                    description: ""
                });
            },
            removeProperty: function (name) {
                for (let i = 0; i < this.elementProperties.length; i++) {
                    if (this.elementProperties[i].name === name)
                        this.elementProperties.splice(i, 1);
                }
            }
        }
    });
}
