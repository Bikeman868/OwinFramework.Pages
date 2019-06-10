var page_editor = new Vue({
    el: "#cms_page_editor",
    data: {
        mode: "view",
        editingPage: {
            id: 1,
            title: "",
            description: ""
        },
        currentPage: {
            id: 1,
            title: "Home page",
            description: "The home page of the website"
        }
    },
    methods: {
        newPage: function () {
            var vm = this;
            vm.editingPage.id = 0;
            vm.editingPage.title = "";
            vm.editingPage.description = "";
            vm.mode = "new";
        },
        editPage: function() {
            var vm = this;
            vm.editingPage.id = vm.currentPage.id;
            vm.editingPage.title = vm.currentPage.title;
            vm.editingPage.description = vm.currentPage.description;
            vm.mode = "edit";
        },
        deletePage: function () {
            var vm = this;
            vm.editingPage.id = vm.currentPage.id;
            vm.editingPage.title = vm.currentPage.title;
            vm.editingPage.description = vm.currentPage.description;
            vm.mode = "delete";
        },
        saveChanges: function () {
            var vm = this;
            vm.currentPage.id = vm.editingPage.id;
            vm.currentPage.title = vm.editingPage.title;
            vm.currentPage.description = vm.editingPage.description;
            ns.cmseditor.pageStore.updatePage(
                vm.currentPage,
                function () { vm.mode = "view"; });
        },
        createNew: function() {
            var vm = this;
            ns.cmseditor.pageStore.newPage(
                vm.editingPage,
                function() { vm.mode = "view"; });
        },
        confirmDelete: function () {
            var vm = this;
            ns.cmseditor.pageStore.deletePage(
                vm.currentPage,
                function () { vm.mode = "view"; });
        },
        cancelChanges: function() {
            this.mode = "view";
        }
    }
})