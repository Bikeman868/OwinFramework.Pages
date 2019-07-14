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
            "    <option v-for=\"websiteVersion in websiteVersions\" v-bind:value=\"websiteVersion.recordId\" v-bind:selected=\"websiteVersion.recordId==websiteVersionId\">" +
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
            "    <option v-for=\"layout in layouts\" v-bind:value=\"layout.recordId\" v-bind:selected=\"layout.recordId==layoutId\">" +
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
            //    });
            vm.layouts.unshift({
                recordId: "",
                displayName: "Defined in code"
            });
            vm.recalculate(true);
        },
        methods: {
            selectLayout: function (e) {
                var vm = this;
                var layoutId = parseInt(e.target.value);
                if (isNaN(layoutId)) layoutId = null;
                vm.selectedLayoutId = layoutId;
                vm.$emit("layout-id-changed", vm.selectedLayoutId);
                vm.recalculate();
            },
            inputLayoutName: function (e) {
                var vm = this;
                vm.editedLayoutName = e.target.value;
                vm.$emit("layout-name-changed", vm.editedLayoutName);
                vm.recalculate();
            },
            changeInherit: function(e) {
                var vm = this;
                if (e.target.checked) {
                    vm.selectedLayoutId = null;
                    vm.editedLayoutName = null;
                    vm.recalculate();
                } else {
                    vm.inherit = false;
                }
            },
            recalculate: function(suppressNotification) {
                var vm = this;
                var oldValue = vm.inherit;
                vm.inherit = vm.selectedLayoutId == undefined && (vm.editedLayoutName == undefined || vm.editedLayoutName.length === 0);
                if (!suppressNotification && oldValue !== vm.inherit)
                    vm.$emit("inherit-changed", vm.inherit);
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
        template:
            "<div>" +
            "  <select v-model=\"mode\">" +
            "    <option value=\"0\">Default content</option>" +
            "    <option value=\"1\">CMS region</option>" +
            "    <option value=\"2\">CMS layout</option>" +
            "    <option value=\"3\">CMS component</option>" +
            "    <option value=\"4\">Named element</option>" +
            "    <option value=\"5\">Html</option>" +
            "  </select>" +
            "  <p v-if=\"mode==1\">-region-choosing-component-</p>" +
            "  <p v-if=\"mode==2\">-layout-choosing-component-</p>" +
            "  <p v-if=\"mode==3\">-component-choosing-component-</p>" +
            "  <div v-if=\"mode==4\">" +
            "    <select v-model=\"layoutZone.contentType\">" +
            "      <option value=\"Region\">Region name</option>" +
            "      <option value=\"Layout\">Layout name</option>" +
            "      <option value=\"Component\">Component name</option>" +
            "      <option value=\"Template\">Template path</option>" +
            "    </select>" +
            "    <input v-if=\"layoutZone.contentType=='Template'\" type=\"text\" v-model=\"layoutZone.contentName\" placeholder=\"/path/to/template\" :pattern=\"namePattern\">" +
            "    <input v-else type=\"text\" v-model=\"layoutZone.contentName\" placeholder=\"element_name\" :pattern=\"pathPattern\">" +
            "  </div>" +
            "  <div v-if=\"mode==5\">" +
            "    <textarea class=\"cms_field__html\" v-model=\"layoutZone.contentValue\" placeholder=\"<div></div>\" :pattern=\"htmlPattern\"></textarea>" +
            "  </div>" +
            "</div>",
        data: function () {
            return {
                mode: 0,
                namePattern: exported.validation.namePattern.source,
                pathPattern: exported.validation.pathPattern.source,
                htmlPattern: exported.validation.htmlPattern.source
            }
        },
        created: function () {
            if (this.layoutZone.regionId != undefined) {
                this.mode = 1;
            } else if (this.layoutZone.layoutId != undefined) {
                this.mode = 2;
            } else if (this.layoutZone.contentType === "Html") {
                this.mode = 4;
            } else if (this.layoutZone.contentType.length > 0) {
                this.mode = 3;
            } else {
                this.mode = 0;
            }
        },
        methods: {
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
                required: true,
                type: String
            },
            layoutZones: {
                required: true,
                type: Array
            }
        },
        template:
            "<div class=\"cms_field\">" +
            "  <label>{{label}}</label>" +
            "  <table>" +
            "    <tr><th>Zone</th><th>Contents</th></tr>" +
            "    <tr v-for=\"zone in zones\">" +
            "      <td>{{zone.name}}</td>" +
            "      <td><cms-layout-zone-editor :layout-zone=\"zone.layoutZone\"></cms-layout-zone-editor></td>" +
            "    </tr>" +
            "  </table>" +
            "</div>",
        data: function () {
            return {
                zones: []
            }
        },
        created: function () {
            var zoneNames = this.zoneNesting.replace(/[(),]/g, " ").split(" ");
            for (let i = 0; i < zoneNames.length; i++) {
                var zoneName = zoneNames[i].trim();
                if (zoneName.length > 0) {
                    var zone = {
                        name: zoneName,
                        layoutZone: {
                            zone: zoneName,
                            regionId: null,
                            layoutId: null,
                            contentType: "",
                            contentName: "",
                            contentValue: ""
                        }
                    };
                    for (var j = 0; j < this.layoutZones.length; j++) {
                        if (this.layoutZones[j].zone === zoneName)
                            zone.layoutZone = this.layoutZones[j];
                    }
                    this.zones.push(zone);
                }
            }
        },
        methods: {
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
            addRoute: function() {
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
            "    <tr class=\"cms_history_summary\"><th></th><th></th><th></th></tr>" +
            "    <tr class=\"cms_history_summary\" v-for=\"summary in summaries\">" +
            "      <td class=\"cms_history__when\">{{summary.when}}</td>" +
            "      <td class=\"cms_history__identity\">{{summary.identity}}</td>" +
            "      <td class=\"cms_history__changes\">{{summary.changes}}</td>" +
            "    </tr>" +
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
                exported.historyService.period(
                    { type: vm.recordType, id: vm.recordId },
                    function(result) {
                        vm.summaries = result.summaries;
                    });
            }
        }
    });
}