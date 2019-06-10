var dispatcher_log = new Vue({
    el: "#cms_dispatcher_log",
    data: {
        messages: [
            {
                when: "2019-05-31",
                elapsed: "",
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
                var message = vm.messages[i];
                var updateDate = new Date(message.when);
                var secondsAgo = Math.trunc((date - updateDate) / 1000);
                if (isNaN(secondsAgo) && secondsAgo > 600) {
                    vm.messages.splice(i, 1);
                    i = i - 1;
                } else {
                    message.elapsed = secondsAgo + "s ago";
                }
            }
            setTimeout(vm.updateTimes, 1000);
        }
    },
    created: function () {
        var vm = this;
        vm.messages = [];
        dispatcher.subscribe(function (message) {
            var updateData =
            {
                when: message.when,
                elapsed: "",
                from: message.machine,
                changes: ""
            };
            if (message.propertyChanges && message.propertyChanges.length > 0) {
                updateData.changes += "Property changes. ";
            }
            if (message.websiteVersionChanges && message.websiteVersionChanges.length > 0) {
                updateData.changes += "Website version changes. ";
            }
            if (message.newElements && message.newElements.length > 0) {
                updateData.changes += "New elements. ";
            }
            if (message.deletedElements && message.deletedElements.length > 0) {
                updateData.changes += "Deleted elements. ";
            }
            if (message.newVersions && message.newVersions.length > 0) {
                updateData.changes += "New element versions. ";
            }
            if (message.deletedElementVersions && message.deletedElementVersions.length > 0) {
                updateData.changes += "Deleted element versions. ";
            }
            vm.messages.unshift(updateData);
        });
        this.updateTimes();
    }
})