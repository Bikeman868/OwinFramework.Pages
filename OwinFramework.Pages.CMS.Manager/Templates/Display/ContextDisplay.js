exported.context_display_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            websiteVersion: {},
            currentUserSegment: {}
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._context = context;
                vm._unsubscribeSegment = context.subscribe("userSegment", function (segment) {vm.currentUserSegment = segment;});
                vm._unsubscribeWebsiteVersionId = managerContext.subscribe("websiteVersionId", function (value) {
                    exported.websiteVersionStore.retrieveRecord(
                        value,
                        function (websiteVersion) {
                            vm.websiteVersion = websiteVersion;
                        });
                });
                this.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeSegment != undefined) {
                    vm._unsubscribeSegment();
                    vm._unsubscribeSegment = null;
                }
                if (vm._unsubscribeWebsiteVersionId != undefined) {
                    vm._unsubscribeWebsiteVersionId();
                    vm._unsubscribeWebsiteVersionId = null;
                }
            }
        }
    });
}