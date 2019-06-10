var dispatcher_log = new Vue({
    el: "#cms_dispatcher_log",
    data: {
        messages: [
            {
                when: "2019-05-31",
                from: "SHVANMHALLIDAY",
                changes: "Description of changes"
            }
        ]
    },
    methods: {
        updateTimes: function () {
            var vm = this;
            var date = new Date();
            for (let i = 0; i < vm.messages.length; i++) {
                var updateDate = new Date(vm.messages[i].when);
                var secondsAgo = Math.trunc((date - updateDate) / 1000);
                if (isNaN(secondsAgo) && secondsAgo > 1200) {
                    vm.messages.splice(i, 1);
                    i = i - 1;
                } else {
                    vm.messages[i].elapsed = secondsAgo + "s ago";
                }
            }
            setTimeout(vm.updateTimes, 1000);
        }
    },
    created: function () {
        var vm = this;
        vm.messages = [];
        dispatcher.subscribe(function (update) {
            var updateData =
            {
                when: update.when,
                from: update.machine,
                changes: ""
            };
            if (update.propertyChanges && update.propertyChanges.length > 0) {
                updateData.changes += "Property changes. ";
            }
            if (update.websiteVersionChanges && update.websiteVersionChanges.length > 0) {
                updateData.changes += "Website version changes. ";
            }
            if (update.newElements && update.newElements.length > 0) {
                updateData.changes += "New elements. ";
            }
            if (update.deletedElements && update.deletedElements.length > 0) {
                updateData.changes += "Deleted elements. ";
            }
            if (update.newVersions && update.newVersions.length > 0) {
                updateData.changes += "New element versions. ";
            }
            if (update.deletedElementVersions && update.deletedElementVersions.length > 0) {
                updateData.changes += "Deleted element versions. ";
            }
            vm.messages.unshift(updateData);
        });
        this.updateTimes();
    }
})