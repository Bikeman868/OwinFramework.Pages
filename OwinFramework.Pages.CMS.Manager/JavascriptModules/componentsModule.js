exported.components = function() {
    Vue.component("websiteversiondropdown",
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
            "<div>" +
            "  <label class=\"cms_field\">{{label}}</label>" +
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

    Vue.component("pagedropdown",
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
            "<div>" +
            "  <label class=\"cms_field\">{{label}}</label>" +
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

    Vue.component("assetdeploymentdropdown",
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
            "<div>" +
            "  <label class=\"cms_field\">{{label}}</label>" +
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

    Vue.component("layoutdropdown",
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
            "<div>" +
            "  <label class=\"cms_field\">{{label}}</label>" +
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

}