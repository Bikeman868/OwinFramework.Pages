exported.user_segment_display_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            currentUserSegment: {}
        },
        methods: {
            show: function (context, managerContext) {
                var vm = this;
                vm._context = context;
                vm._unsubscribeSegment = context.subscribe("userSegment", function (segment) {vm.currentUserSegment = segment;});
                this.visible = true;
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeSegment != undefined) {
                    vm._unsubscribeSegment();
                    vm._unsubscribeSegment = null;
                }
            }
        }
    });
}