exported.components = function() {
    Vue.component("websiteversiondropdown",
    {
        props: {
            websiteVersionId: {
                required: false
            }
        },
        template:
            "<select class=\"cms_field__website_version\" v-on:change=\"selectWebsiteVersion($event)\">" +
            "  <option v-for=\"websiteVersion in websiteVersions\" v-bind:value=\"websiteVersion.websiteVersionId\" v-bind:selected=\"websiteVersion.websiteVersionId==websiteVersionId\">" +
            "    {{websiteVersion.displayName}}" +
            "  </option>" +
            "</select>",
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
                var dropDown = e.target;
                var websiteVersionId = dropDown.value || dropDown.options[dropDown.selectedIndex].value;
                websiteVersionId = parseInt(websiteVersionId);
                vm.$emit("website-version-selected", websiteVersionId);
            }
        }
    });

    Vue.component("pagedropdown",
    {
        props: {
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
            "<select class=\"cms_field__page\" v-on:change=\"selectPage($event)\">" +
            "  <option v-for=\"page in pages\" v-bind:value=\"page.elementId\" v-bind:selected=\"page.elementId==pageId\">" +
            "    {{page.displayName}}" +
            "  </option>" +
            "</select>",
        data: function () {
            return {
                pages: []
            }
        },
        created: function () {
            var vm = this;
            exported.pageStore.retrieveAllRecords(
                function (response) {
                    if (response != undefined && vm.exclude != undefined) {
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
                    vm.pages = response;
                });
        },
        methods: {
            selectPage: function (e) {
                var vm = this;
                var dropDown = e.target;
                var pageId = dropDown.value || dropDown.options[dropDown.selectedIndex].value;
                pageId = parseInt(pageId);
                vm.$emit("page-selected", pageId);
            }
        }
    });

    Vue.component("assetdeploymentdropdown",
    {
        props: {
            assetDeployment: {
                required: true,
                type: String
            }
        },
        template:
            "<select class=\"cms_field__asset_deployment\" v-on:change=\"selectAssetDeployment($event)\">" +
            "  <option value=\"Inherit\" v-bind:selected=\"assetDeployment==='Inherit'\">Inherit from parent</option>" +
            "  <option value=\"InPage\" v-bind:selected=\"assetDeployment==='InPage'\">Inline within page</option>" +
            "  <option value=\"PerPage\" v-bind:selected=\"assetDeployment==='PerPage'\">Page specific asset</option>" +
            "  <option value=\"PerModule\" v-bind:selected=\"assetDeployment==='PerModule'\">Defined by module</option>" +
            "  <option value=\"PerWebsite\" v-bind:selected=\"assetDeployment==='PerWebsite'\">Website asset</option>" +
            "</select>",
        methods: {
            selectAssetDeployment: function (e) {
                var vm = this;
                var dropDown = e.target;
                var assetDeployment = dropDown.value || dropDown.options[dropDown.selectedIndex].value;
                vm.$emit("asset-deployment-selected", assetDeployment);
            }
        }
    });
}