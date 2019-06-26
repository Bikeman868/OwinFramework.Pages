exported.website_version_selector_vm = function(eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            versions: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                vm.visible = true;
            },
            hide: function () { this.visible = false; },
            selectVersion: function (websiteVersionId) {
                var vm = this;
                vm._context.selected("websiteVersionId", websiteVersionId);
            },
            selectDropdown: function (e) {
                var vm = this;
                var dropdown = e.target;
                var websiteVersionId = (dropdown.value || dropdown.options[dropdown.selectedIndex].value);
                if (websiteVersionId != undefined) {
                    vm.selectVersion(parseInt(websiteVersionId));
                }
            }
        },
        created: function() {
            this.versions = [
                { elementId: 1, displayName: "Sprint 223" },
                { elementId: 2, displayName: "Sprint 224" },
                { elementId: 3, displayName: "Sprint 225" },
                { elementId: 4, displayName: "Sprint 226" },
                { elementId: 5, displayName: "Sprint 227" }
            ];
        }
    });
}