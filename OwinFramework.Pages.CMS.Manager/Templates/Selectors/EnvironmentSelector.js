﻿exported.environment_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            environments: []
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                if (context != undefined) vm._context = context;
                if (managerContext != undefined) vm._managerContext = managerContext;
                if (vm._managerContext == undefined) vm._managerContext = vm._context;
                vm.refresh();
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
            },
            refresh: function () {
                var vm = this;
                exported.environmentStore.retrieveAllRecords(
                    function (response) {
                        vm.environments = response;
                    });
            },
            selectEnvironment: function (environmentId) {
                var vm = this;
                var websiteVersionId = null;
                for (let i = 0; i < vm.environments.length; i++) {
                    var environment = vm.environments[i];
                    if (environment.recordId === environmentId) {
                        websiteVersionId = environment.websiteVersionId;
                    }
                }
                if (websiteVersionId != undefined)
                    vm._context.selected("websiteVersionId", websiteVersionId);
                vm._context.selected("environmentId", environmentId);
            }
        }
    });
}