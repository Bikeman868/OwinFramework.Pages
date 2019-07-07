exported.components = function() {
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
            "    <option v-for=\"websiteVersion in websiteVersions\" v-bind:value=\"websiteVersion.websiteVersionId\" v-bind:selected=\"websiteVersion.websiteVersionId==websiteVersionId\">" +
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
            exported.websiteVersionStore.getWebsiteVersions(
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
            "    <option v-for=\"page in pages\" v-bind:value=\"page.elementId\" v-bind:selected=\"page.elementId==pageId\">" +
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
                                    if (vm.exclude[j] == response[i].elementId) excluded = true;
                                }
                                if (excluded) {
                                    response.splice(i, 1);
                                    i--;
                                }
                            }
                        }
                        if (vm.allowNone) {
                            response.unshift({
                                elementId: null,
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
            "    <option v-for=\"layout in layouts\" v-bind:value=\"layout.elementId\" v-bind:selected=\"layout.elementId==layoutId\">" +
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
                { elementId: 7, displayName: "Layout 1" },
                { elementId: 8, displayName: "Layout 2" },
                { elementId: 9, displayName: "Layout 3" },
                { elementId: 10, displayName: "Layout 4" }
            ];
            //exported.layoutStore.retrieveAllRecords(
            //    function (response) {
            //        vm.layouts = response;
            //    });
            vm.layouts.unshift({
                elementId: "",
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
            "</div>",
        data: function () {
            return {
                layout: {}
            }
        },
        created: function () {
            var vm = this;
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
            "</div>",
        data: function () {
            return {
                layout: {}
            }
        },
        created: function () {
            var vm = this;
        },
        methods: {
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
                cssPattern: exported.validation.permissionPattern.source
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
                default: "Title"
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
            "  <label class=\"cms_field\">{{label}}</label>" +
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
}