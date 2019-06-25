﻿exported.environment_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            environments: []
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
            selectEnvironment: function (environmentId) {
                var vm = this;
                vm._childContext.selected("environmentId", environmentId);
            }
        },
        created: function() {
            this.environments = [
                { elementId: 1, displayName: "Production" },
                { elementId: 2, displayName: "Staging" },
                { elementId: 3, displayName: "Test" },
                { elementId: 4, displayName: "Integration" }
            ];
        }
    });
}