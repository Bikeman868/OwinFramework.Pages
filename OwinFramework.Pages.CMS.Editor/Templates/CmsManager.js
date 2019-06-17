var cms_manager = new Vue({
    el: "#cms_manager",
    data: {
    },
    created: function () {
        var vm = this;
        ns.cmseditor.viewStore.pageEditor(function(e) {
            
        });
    },
    methods: {
    }
})