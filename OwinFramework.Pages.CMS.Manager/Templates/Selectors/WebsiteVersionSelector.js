exported.website_version_selector_vm = function(eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            versions: []
        },
        methods: {
            show: function (childContext, parentContext) {
                var vm = this;
                if (childContext != undefined) vm._childContext = childContext;
                if (parentContext != undefined) vm._parentContext = parentContext;
                if (vm._parentContext == undefined) vm._parentContext = vm._childContext;
                vm.visible = true;
            },
            hide: function () { this.visible = false; },
            selectVersion: function (websiteVersionId) {
                var vm = this;
                vm._childContext.selected("websiteVersionId", websiteVersionId);
            }
        },
        created: function() {
            this.versions = [
                { elementId: 1, displayName: "Sprint 223" },
                { elementId: 2, displayName: "Sprint 224" },
                { elementId: 3, displayName: "Sprint 225" },
            ];
        }
    });
}