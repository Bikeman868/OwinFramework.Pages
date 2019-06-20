exported.page_selector_vm = function() {
    return new Vue({
        el: "#cms_page_selector",
        data: {
            visible: true,
            pages: []
        },
        methods: {
        },
        created: function() {
            this.pages = [
                { displayName: "Page 1" },
                { displayName: "Page 2" },
                { displayName: "Page 3" },
                { displayName: "Page 4" }
            ];
        }
    });
}