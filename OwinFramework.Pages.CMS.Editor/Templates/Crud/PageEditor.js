var page_editor = new Vue({
    el: "#cms_page_editor",
    data: {
        page: {
            title: "page title",
            description: "description"
        }
    },
    methods: {
        saveChanges: function () {
            ns.cmseditor.pagesData.updatePage(this.page);
        }
    }
})