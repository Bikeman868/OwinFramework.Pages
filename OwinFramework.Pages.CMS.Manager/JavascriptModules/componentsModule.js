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
}