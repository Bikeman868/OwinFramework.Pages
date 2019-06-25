exported.page_selector_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            websiteVersionId: 1,
            pages: []
        },
        methods: {
            show: function (childContext, parentContext) {
                var vm = this;
                vm.visible = true;
                if (childContext != undefined) vm._childContext = childContext;
                if (parentContext != undefined) vm._parentContext = parentContext;
                if (vm._parentContext == undefined) vm._parentContext = vm._childContext;
                vm._unsubscribeWebsiteVersionId = vm._parentContext.subscribe("websiteVersionId", function (value) {
                    vm.websiteVersionId = value;
                    vm.refresh();
                });
                vm._unsubscribeDispatcher = exported.dispatcher.subscribe(function(message) {
                    if ((message.websiteVersionChanges && message.websiteVersionChanges.length > 0) ||
                        (message.newElements && message.newElements.length > 0) ||
                        (message.deletedElements && message.deletedElements.length > 0)) {
                        vm.refresh();
                    }
                });
            },
            hide: function() {
                var vm = this;
                vm.visible = false;
                if (vm._unsubscribeWebsiteVersionId != undefined) {
                    vm._unsubscribeWebsiteVersionId();
                    vm._unsubscribeWebsiteVersionId = null;
                }
                if (vm._unsubscribeDispatcher != undefined) {
                    vm._unsubscribeDispatcher();
                    vm._unsubscribeDispatcher = null;
                }
            },
            refresh: function() {
                var vm = this;
                if (vm.websiteVersionId == undefined) {
                    vm.pages = [];
                } else {
                    exported.websiteVersionStore.getPages(
                        vm.websiteVersionId,
                        function (response) {
                            var pages = [];
                            for (var i = 0; i < response.length; i++) {
                                exported.pageStore.retrievePage(response[i].pageId, function(page) {
                                    pages.push(page);
                                });
                            }
                            vm.pages = pages;
                        });
                }
            },
            selectPage: function(pageId) {
                var vm = this;
                vm._childContext.selected("pageId", pageId);
            }
        }
    });
}